using Microsoft.EntityFrameworkCore;
using TaskHandler.Application.Interfaces;
using Serilog;
using System.Text.Json.Serialization;
using FluentValidation;
using TaskHandler.Api.Endpoints;
using TaskHandler.Api.Endpoints.Tasks;
using TaskHandler.Api.Exceptions.Handlers;
using TaskHandler.Application;
using TaskHandler.Application.Behaviors;
using TaskHandler.Infrastructure;
using TaskHandler.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(6000, listenOptions =>
    {
        listenOptions.UseHttps();
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

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

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
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql");

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "TaskHandler API v1");
    });
}

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapTaskEndpoints();
app.MapPostTaskEndpoints();
app.MapUpdateTaskItemEndpoint();
app.MapDeleteTaskItemEndpoints();
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