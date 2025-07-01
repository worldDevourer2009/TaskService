using Microsoft.EntityFrameworkCore;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Specifications;
using TaskHandler.Infrastructure.Extensions;

namespace TaskHandler.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly IApplicationDbContext _context;

    public TaskRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem> GetTaskItemForUser(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _context.TaskItems.FindAsync([taskId, cancellationToken], cancellationToken: cancellationToken).AsTask();
        
        if (task == null)
        {
            throw new Exception("Task not found");
        }

        if (task.UserId != userId)
        {
            throw new Exception("Task not found for user");
        }
        
        return task;
    }

    public async Task<TaskItem> GetTaskItemById(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _context.TaskItems.FindAsync([taskId, cancellationToken], cancellationToken: cancellationToken).AsTask();

        if (task == null)
        {
            throw new Exception("Task not found");
        }
        
        return task;
    }

    public async Task<IEnumerable<TaskItem>> GetAllTasksForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var tasks = await _context.TaskItems.Where(tasks => tasks.UserId == userId).ToListAsync(cancellationToken);

        if (tasks.Count == 0)
        {
            throw new Exception("User has no tasks");
        }
        
        return tasks;
    }
    
    public async Task<IEnumerable<TaskItem>> GetAllTasks(CancellationToken cancellationToken = default)
    {
        var tasks = await _context.TaskItems.ToListAsync(cancellationToken);
        
        if (tasks.Count == 0)
        {
            throw new Exception("No tasks found");
        }
        
        return tasks;
    }

    public async Task<bool> TryAddTaskItem(TaskItem taskItem, CancellationToken cancellationToken = default)
    {
        var added = _context.TaskItems.TryAdd(taskItem);
        await _context.SaveChangesAsync(cancellationToken);
        return added;
    }

    public async Task DeleteTaskItemById(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _context.TaskItems.FindAsync([taskId, cancellationToken], cancellationToken: cancellationToken);
        
        if (task == null)
        {
            throw new Exception("Task not found");
        }
        
        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateTaskItem(TaskItem taskItem, CancellationToken cancellationToken = default)
    {
        var task = await _context.TaskItems.FindAsync([taskItem.Id], cancellationToken: cancellationToken);
        
        if (task == null)
        {
            throw new Exception("Task not found");
        }
        
        task.Update(taskItem);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TaskItem>> GetTasksBySpecification(ISpecification<TaskItem> specification, CancellationToken cancellationToken = default)
    {
        return await _context.TaskItems
            .Where(x => specification.IsSatisfiedBy(x)).ToListAsync(cancellationToken: cancellationToken);
    }
}