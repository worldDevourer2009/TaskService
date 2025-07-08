using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskHandler.Application.Interfaces;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;

namespace TaskHandler.Infrastructure.Repositories;

public class TaskGroupRepository : ITaskGroupRepository
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TaskGroupRepository> _logger;

    public TaskGroupRepository(IApplicationDbContext context, ILogger<TaskGroupRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskGroup?> GetTaskGroupById(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting task group by id {Id}", id);

        if (string.IsNullOrEmpty(id))
        {
            _logger.LogWarning("Id is null or empty");
            return null;
        }

        if (!Guid.TryParse(id, out var guid))
        {
            _logger.LogWarning("Id is not a guid");
            return null;
        }

        try
        {
            var group = await _context.TaskGroups
                .FirstOrDefaultAsync(x => x.Id == guid, cancellationToken);
            
            if (group == null)
            {
                throw new Exception("Task group not found");
            }
            
            return group;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting task group by id {Id}", id);
            return null;
        }
    }

    public async Task<List<TaskGroup>?> GetAllTaskGroupsForUser(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all task groups for user {Id}", id);

        try
        {
            var userIdJsonArray = JsonSerializer.Serialize(new[] { id });
            
            var taskGroups = await _context.TaskGroups
                .Where(x => EF.Functions.JsonContains(x.UserIds, userIdJsonArray))
                .ToListAsync(cancellationToken);
            
            _logger.LogInformation("Found {Count} task groups for user {user}", taskGroups.Count, id);
            
            return taskGroups;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting all task groups for user {Id}", id);
            return null;
        }
    }

    public async Task<bool> AddTaskGroup(TaskGroup taskGroup, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding task group {TaskGroup}", taskGroup);

        try
        {
            await _context.TaskGroups.AddAsync(taskGroup, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Task group added successfully");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding task group {TaskGroup}", taskGroup);
            return false;
        }
    }

    public async Task<bool> UpdateTaskGroup(string id, TaskGroup taskGroup, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating task group {TaskGroup}", taskGroup);
        
        if (!Guid.TryParse(id, out var guid))
        {
            _logger.LogWarning("Invalid GUID format for id {Id}", id);
            return false;
        }

        try
        {
            var taskGroupToUpdate = await _context.TaskGroups.FindAsync([guid], cancellationToken);
            
            if (taskGroupToUpdate == null)
            {
                throw new Exception("Task group not found");
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Task group updated successfully");
            
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating task group {TaskGroup}", taskGroup);
            return false;
        }
    }

    public async Task<bool> DeleteTaskGroup(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting task group {Id}", id);
        
        if (!Guid.TryParse(id, out var guid))
        {
            _logger.LogWarning("Invalid GUID format for id {Id}", id);
            return false;
        }

        try
        {
            var taskGroup = await _context.TaskGroups.FindAsync([guid], cancellationToken);

            if (taskGroup == null)
            {
                _logger.LogWarning("Task group not found");
                return false;
            }
            
            _context.TaskGroups.Remove(taskGroup);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Task group deleted successfully");
            
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting task group {Id}", id);
            return false;
        }
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellation = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellation);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while saving db");
            return false;
        }
    }
}