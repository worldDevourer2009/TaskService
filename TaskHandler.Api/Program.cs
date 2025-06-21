using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using TaskHandler.Application.Interfaces;
using Serilog;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskHandler.Api.Endpoints.Emails;
using TaskHandler.Api.Endpoints.Tasks;
using TaskHandler.Api.Endpoints.Users;
using TaskHandler.Api.Exceptions.Handlers;
using TaskHandler.Api.Middleware;
using TaskHandler.Application;
using TaskHandler.Application.Behaviors;
using TaskHandler.Application.Events.Users;
using TaskHandler.Application.Services;
using TaskHandler.Domain.DomainsEvents.Users;
using TaskHandler.Domain.Services;
using TaskHandler.Infrastructure;
using TaskHandler.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(6000, listenOptions => { listenOptions.UseHttps(); });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                
                if (jti == null)
                {
                    context.Fail("Invalid token");
                }

                var redisService = context.HttpContext.RequestServices.GetRequiredService<IRedisService>();
                var isRevoked = await redisService.KeyExistsAsync($"revoked:{jti}");

                if (isRevoked)
                {
                    context.Fail("Invalid token");
                }
            },

            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Authentication failed: {Exception}", context.Exception.Message);
                return Task.CompletedTask;
            },

            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("access_token", out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"] ?? string.Empty)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});


builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("TaskHandler.Infrastructure")));

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IDomainDispatcher, DomainDispatcher>();

builder.Services.AddScoped<IDomainEventHandler<UserCreatedDomainEvent>, NewUserCreatedEventHandler>();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddDataProtection();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<TaskHandler.Application.Commands.AddTaskItem.AddTaskItemCommand>();

builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "TaskHandler API v1"); });
}

app.UseTokenRefresh();
app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseAuthorization();

app.MapControllers();
app.MapTaskEndpoints();
app.MapPostTaskEndpoints();
app.MapUpdateTaskItemEndpoint();
app.MapDeleteTaskItemEndpoints();
app.SetupEmailEndPoint();
app.MapHealthChecks("/health");
app.MapUserEndpoints();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();

    try
    {
        await context.Database.MigrateAsync();
        app.Logger.LogInformation("Database migration completed successfully");

        await seeder.SeedAsync();
        app.Logger.LogInformation("Seeding completed successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database");
    }
}

app.Logger.LogInformation("TaskHandler API starting up...");
app.Run();