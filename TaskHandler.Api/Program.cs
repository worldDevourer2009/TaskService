using Microsoft.EntityFrameworkCore;
using TaskHandler.Application.Interfaces;
using Serilog;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskHandler.Api.Exceptions.Handlers;
using TaskHandler.Api.Middleware;
using TaskHandler.Api.Services;
using TaskHandler.Application;
using TaskHandler.Application.Behaviors;
using TaskHandler.Application.Commands.Tasks;
using TaskHandler.Application.Configurations;
using TaskHandler.Application.Services;
using TaskHandler.Infrastructure;
using TaskHandler.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();

// Bind configurations

builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"));

builder.Services
    .AddOptions<InternalAuthSettings>()
    .Bind(builder.Configuration.GetSection("InternalAuthSettings"));

builder.Services
    .AddOptions<AuthSettings>()
    .Bind(builder.Configuration.GetSection("AuthSettings"));

builder.Services
    .AddOptions<KafkaSettings>()
    .Bind(builder.Configuration.GetSection("Kafka"));

// builder.Services
//     .AddHttpClient(internalAuth.ServiceClientId!, client =>
//     {
//         client.BaseAddress = new Uri(baseUri);
//         client.Timeout = TimeSpan.FromSeconds(30);
//         client.DefaultRequestHeaders.Add("Accept", "application/json");
//     })
//     .AddHttpMessageHandler<InternalAuthHandler>();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IPublicKeyService, PublicKeyService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var publicKeyService = builder.Services.BuildServiceProvider().GetRequiredService<IPublicKeyService>();
                return new[]
                {
                    publicKeyService.GetPublicKey()
                };
            },
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "test-issuer",
            ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "test-audience",
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token))
                {
                    context.Token = context.Request.Cookies["accessToken"];
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            }
        };
    })
    .AddJwtBearer("ServiceScheme", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var publicKeyService = builder.Services.BuildServiceProvider().GetRequiredService<IPublicKeyService>();
                return new[]
                {
                    publicKeyService.GetPublicKey()
                };
            },
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["AuthSettings:Issuer"] ?? "test-issuer",
            ValidAudience = builder.Configuration["AuthSettings:Audience"] ?? "test-audience",
        };
        options.Events = new JwtBearerEvents()
        {
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("ServiceSchemeAuth");

                var svc = ctx.Principal?.FindFirst("service_name")?.Value ?? "unknown";

                logger.LogError("Authentication failed: {0} from service {svc}", ctx.Exception.Message, svc);
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("ServiceSchemeAuthValidated");

                var svc = ctx.Principal?.FindFirst("service_name")?.Value ?? "unknown";

                logger.LogInformation("Token validated successfully from service {svc}", svc);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:5273")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
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

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddDataProtection();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<AddTaskItemCommand>();

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

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql");

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "TaskHandler API v1"); });
}

app.UseExceptionHandler();
app.UseCors("AllowAll");

app.UseMiddleware<PublicKeyMiddleware>();

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

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

public partial class Program;