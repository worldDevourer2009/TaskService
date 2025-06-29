using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskHandler.Domain;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Enums;
using TaskHandler.Domain.ValueObjects;
using TaskStatus = TaskHandler.Domain.Enums.TaskStatus;

namespace TaskHandler.Infrastructure.Persistence;

public interface IDataSeeder
{
    Task SeedAsync();
}

public class DataSeeder : IDataSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(AppDbContext dbContext, ILogger<DataSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            if (await _dbContext.TaskItems.AnyAsync())
            {
                _logger.LogInformation("Found task Items, cleaning");
                _dbContext.TaskItems.RemoveRange(_dbContext.TaskItems);
                await _dbContext.SaveChangesAsync();
            }
            
            await SeedTasks();

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedTasks()
    {
        if (!await _dbContext.TaskItems.AnyAsync())
        {
            var tasks = new List<TaskItem>();
            var userId = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var userId3 = Guid.NewGuid();

            var task1 = TaskItem.Create(userId);
            task1.SetTitle("Configure PostgreSQL");
            task1.SetDescription("Connect PostgreSQL and create migrations");
            task1.SetStatus(TaskStatus.Done);
            task1.SetPriority(TaskPriority.High);
            task1.TaskType = TaskType.Work;

            var task2 = TaskItem.Create(userId2);
            task2.SetTitle("Create API Controllers");
            task2.SetDescription("Create API Controllers for Task and User");
            task2.SetStatus(TaskStatus.InProgress);
            task2.SetPriority(TaskPriority.High);
            task2.TaskType = TaskType.Work;

            var task3 = TaskItem.Create(userId3);
            task3.SetTitle("Write tests");
            task3.SetDescription("make code with unit and integration tests");
            task3.SetStatus(TaskStatus.Pending);
            task3.SetPriority(TaskPriority.Medium);
            task3.TaskType = TaskType.Work;

            var task4 = TaskItem.Create(userId2);
            task4.SetTitle("Go buy groceries");
            task4.SetDescription("Milk, eggs, bread, cheese, etc.");
            task4.SetStatus(TaskStatus.Pending);
            task4.SetPriority(TaskPriority.Low);
            task4.TaskType = TaskType.Personal;

            var task5 = TaskItem.Create(userId2);
            task5.SetTitle("Learn .NET 9");
            task5.SetDescription("Look for new features in .NET 9");
            task5.SetStatus(TaskStatus.InProgress);
            task5.SetPriority(TaskPriority.High);
            task5.TaskType = TaskType.Studies;

            var task6 = TaskItem.Create(userId);
            task6.SetTitle("Learn 123 .NET 9");
            task6.SetDescription("Look 123 for new features in .NET 9");
            task6.SetStatus(TaskStatus.InProgress);
            task6.SetPriority(TaskPriority.Medium);
            task6.TaskType = TaskType.Studies;

            tasks.Add(task1);
            tasks.Add(task2);
            tasks.Add(task3);
            tasks.Add(task4);
            tasks.Add(task5);
            tasks.Add(task6);

            await _dbContext.TaskItems.AddRangeAsync(tasks);
            _logger.LogInformation("Seeded {Count} task items", tasks.Count);
        }
    }
}