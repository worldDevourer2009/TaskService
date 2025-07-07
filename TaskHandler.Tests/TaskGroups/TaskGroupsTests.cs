using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;
using TaskHandler.Infrastructure.Persistence;
using TaskHandler.Infrastructure.Repositories;
using TaskHandler.Infrastructure.Services;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace TaskHandler.Tests.TaskGroups;

public class TaskGroupsTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    private readonly ITaskGroupService _taskGroupService;
    private readonly TaskService _taskService;
    private readonly PostgreSqlContainer _container;
    private readonly Mock<ITaskGroupRepository> _mockRepository;
    private readonly Mock<ILogger<TaskGroupService>> _mockLogger;
    
    private TaskGroupService _service;
    private TaskGroupRepository _repository;

    private AppDbContext _dbContext;

    public TaskGroupsTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;

        _container = new PostgreSqlBuilder()
            .WithDatabase("notification_test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => { logging.ClearProviders(); });

            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((ctx, conf) =>
            {
                var settings = new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = _container.GetConnectionString(),
                };
                conf.AddInMemoryCollection(settings);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(_container.GetConnectionString(),
                        npgsql => npgsql.MigrationsAssembly("AuthService.Infrastructure")));

                services.AddLogging(log => log.AddConsole().SetMinimumLevel(LogLevel.Debug));
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            });
        });
        
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly("AuthService.Infrastructure"))
            .Options;

        _dbContext = new AppDbContext(optionsBuilder);
        await _dbContext.Database.EnsureCreatedAsync();

        var loggerMock = new Mock<ILogger<TaskGroupRepository>>();
        _repository = new TaskGroupRepository(_dbContext, loggerMock.Object);

        var serviceLoggerMock = new Mock<ILogger<TaskGroupService>>();
        _service = new TaskGroupService(_repository, serviceLoggerMock.Object);
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }

    [Fact]
    private async Task AddTaskToGroup_ReturnsTrue()
    {
        //Arrange
        _dbContext.TaskGroups.RemoveRange(_dbContext.TaskGroups);
        await _dbContext.SaveChangesAsync();
        
        var group = TaskGroup.Create("Test group", "Test description",new HashSet<string> { Guid.NewGuid().ToString() });
        
        //Act
        var result = await _service.AddTaskGroup(group);
        
        //Assert
        Assert.True(result);
    }

    [Fact]
    private async Task GetTaskGroupById_ReturnsNotNull()
    {
        //Arrange
        _dbContext.TaskGroups.RemoveRange(_dbContext.TaskGroups);
        await _dbContext.SaveChangesAsync();
        
        var group = TaskGroup.Create("Test group", "Test description", new HashSet<string>());
        
        //Act
        var result = await _service.AddTaskGroup(group);
        var groupById = await _service.GetTaskGroupById(group.Id);
        
        //Assert
        Assert.NotNull(groupById);   
    }
    
    [Fact]
    private async Task GetTaskGroupById_ReturnsNull()
    {
        //Arrange
        _dbContext.TaskGroups.RemoveRange(_dbContext.TaskGroups);
        await _dbContext.SaveChangesAsync();
        
        var id = Guid.NewGuid();
        
        //Act
        var result = await _service.GetTaskGroupById(id);
        
        //Assert
        Assert.Null(result);
    }
    
    [Fact]
    private async Task GetAllTaskGroupsForUser_ReturnsNotNull()
    {
        //Arrange
        _dbContext.TaskGroups.RemoveRange(_dbContext.TaskGroups);
        await _dbContext.SaveChangesAsync();
        
        var user = Guid.NewGuid().ToString();
        
        var group1 = TaskGroup.Create("Test group1", "Test description", new HashSet<string>() { user });
        var group2 = TaskGroup.Create("Test group2", "Test description", new HashSet<string>() { user });
        var group3 = TaskGroup.Create("Test group3", "Test description", new HashSet<string>() { user });
        
        var result1 = await _service.AddTaskGroup(group1);
        var result2 = await _service.AddTaskGroup(group2);
        var result3 = await _service.AddTaskGroup(group3);
        
        //Act
        var result = await _service.GetAllTaskGroupsForUser(user);
        
        //Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        
        Assert.NotNull(result);
        Assert.True(result.Count == 3);
    }

    [Fact]
    private async Task UpdateTaskGroup_ReturnsTrue()
    {
        //Arrange
        _dbContext.TaskGroups.RemoveRange(_dbContext.TaskGroups);
        await _dbContext.SaveChangesAsync();

        var user = Guid.NewGuid().ToString();
        var group1 = TaskGroup.Create("Test group1", "Test description", new HashSet<string>() { user });
        var group2 = TaskGroup.Create("Test group updated", "Test description", new HashSet<string>() { user });

        var addedResult = await _service.AddTaskGroup(group1);
        
        //Act
        var result = await _service.UpdateTaskGroup(group1.Id.ToString(), group2);
        
        //Assert
        Assert.True(result);
    }
    
    [Fact]
    private async Task DeleteTaskGroup_ReturnsTrue()
    {
        //Arrange
        _dbContext.TaskGroups.RemoveRange(_dbContext.TaskGroups);
        await _dbContext.SaveChangesAsync();

        var user = Guid.NewGuid().ToString();
        var group1 = TaskGroup.Create("Test group1", "Test description", new HashSet<string>() { user });
        
        var addedResult = await _service.AddTaskGroup(group1);
        
        //Act
        var result = await _service.DeleteTaskGroup(group1.Id.ToString());
        
        //Assert
        Assert.True(result);
    }
    
    [Fact]
    private async Task DeleteTaskGroup_ReturnsFalse()
    {
        //Arrange
        _dbContext.TaskGroups.RemoveRange(_dbContext.TaskGroups);
        await _dbContext.SaveChangesAsync();

        var user = Guid.NewGuid().ToString();
        var group1 = TaskGroup.Create("Test group1", "Test description", new HashSet<string>() { user });
        
        var addedResult = await _service.AddTaskGroup(group1);
        
        //Act
        var result = await _service.DeleteTaskGroup(Guid.NewGuid().ToString());
        
        //Assert
        Assert.False(result);
    }
    
    [Fact]
    private async Task AddUserToTaskGroup_ReturnsTrue()
    {
        //Arrange
        _dbContext.TaskGroups.RemoveRange(_dbContext.TaskGroups);
        await _dbContext.SaveChangesAsync();

        var user = Guid.NewGuid().ToString();
        var group1 = TaskGroup.Create("Test group1", "Test description", new HashSet<string>());
        
        var addedResult = await _service.AddTaskGroup(group1);
        
        //Act
        var result = await _service.AddUserToTaskGroup(group1.Id.ToString(), user);
        
        //Assert
        Assert.True(result);
    }
    
    [Fact]
    private async Task AddUserToTaskGroup_ReturnsFalse()
    {
        //Arrange
        _dbContext.TaskGroups.RemoveRange(_dbContext.TaskGroups);
        await _dbContext.SaveChangesAsync();
        
        var user = Guid.NewGuid().ToString();
        
        var group1 = TaskGroup.Create("Test group1", "Test description", new HashSet<string>());
        
        var addedResult = await _service.AddTaskGroup(group1);
        
        //Act
        var result = await _service.AddUserToTaskGroup(Guid.NewGuid().ToString(), user);
        
        //Assert
        Assert.False(result);
    }
    
    [Fact]
    private async Task RemoveUserFromTaskGroup_ReturnsTrue()
    {
        //Arrange
        _dbContext.TaskGroups.RemoveRange(_dbContext.TaskGroups);
        await _dbContext.SaveChangesAsync();
        
        var user = Guid.NewGuid().ToString();
        var group1 = TaskGroup.Create("Test group1", "Test description", new HashSet<string>() { user });
        
        var addedResult = await _service.AddTaskGroup(group1);
        
        //Act
        var result = await _service.RemoveUserFromTaskGroup(group1.Id.ToString(), user);
        
        //Assert
        Assert.True(result);
    }
    
    [Fact]
    private async Task RemoveUserFromTaskGroup_ReturnsFalse()
    {
        //Arrange
        _dbContext.TaskGroups.RemoveRange(_dbContext.TaskGroups);
        await _dbContext.SaveChangesAsync();

        var user = Guid.NewGuid().ToString();
        var group1 = TaskGroup.Create("Test group1", "Test description", new HashSet<string>());
        
        var addedResult = await _service.AddTaskGroup(group1);
        
        //Act
        var result = await _service.RemoveUserFromTaskGroup(group1.Id.ToString(), user);
        
        //Assert
        Assert.False(result);
    }
}