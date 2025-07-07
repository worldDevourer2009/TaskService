using Microsoft.Extensions.Logging;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;
using TaskHandler.Domain.Services;

namespace TaskHandler.Infrastructure.Services;

public class TaskGroupService : ITaskGroupService
{
    private readonly ITaskGroupRepository _taskGroupRepository;
    private readonly ILogger<TaskGroupService> _logger;

    public TaskGroupService(ITaskGroupRepository taskGroupRepository, ILogger<TaskGroupService> logger)
    {
        _taskGroupRepository = taskGroupRepository;
        _logger = logger;
    }

    public async Task<bool> AddTaskGroup(TaskGroup taskGroup, CancellationToken cancellationToken = default)
    {
        try
        {
            await _taskGroupRepository.AddTaskGroup(taskGroup, cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding task group");
            return false;
        }
    }

    public async Task<TaskGroup?> GetTaskGroupById(Guid groupId, CancellationToken cancellationToken = default)
    {
        if (groupId == Guid.Empty)
        {
            _logger.LogWarning("Id is empty");
            return null;
        }
        
        try
        {
            return await _taskGroupRepository.GetTaskGroupById(groupId.ToString(), cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting task group by groupId");
            return null;
        }
    }

    public async Task<List<TaskGroup>?> GetAllTaskGroupsForUser(string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all task groups for user {Id}", userId);

        try
        {
            return await _taskGroupRepository.GetAllTaskGroupsForUser(userId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting all task groups for user {Id}", userId);
            return null;
        }
    }

    public async Task<bool> UpdateTaskGroup(string groupId, TaskGroup taskGroup, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating task group {TaskGroup}", taskGroup);

        try
        {
            return await _taskGroupRepository.UpdateTaskGroup(groupId, taskGroup, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating task group {TaskGroup}", taskGroup);
            return false;
        }
    }

    public async Task<bool> DeleteTaskGroup(string groupId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting task group {Id}", groupId);

        try
        {
            return await _taskGroupRepository.DeleteTaskGroup(groupId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting task group {Id}", groupId);
            return false;
        }
    }

    public async Task<bool> AddUserToTaskGroup(string groupId, string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding user {UserId} to task group {Id}", userId, groupId);

        try
        {
            var group = await TryGetGroup(groupId, cancellationToken);
            
            if (group == null)
            {
                _logger.LogWarning("Task group not found");
                return false;
            }

            group.AddUser(userId);
            return await _taskGroupRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding user {UserId} to task group {Id}", userId, groupId);
            return false;
        }
    }

    public async Task<bool> RemoveUserFromTaskGroup(string groupId, string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing user {UserId} from task group {Id}", userId, groupId);

        try
        {
            var group = await TryGetGroup(groupId, cancellationToken);

            if (group == null)
            {
                _logger.LogWarning("Task group not found");
                return false;
            }
            
            group.RemoveUser(userId);
            return await _taskGroupRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing user {UserId} from task group {Id}", userId, groupId);
            return false;
        }
    }

    public async Task<bool> IsUserInTaskGroup(string groupId, string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking if user {UserId} is in task group {Id}", userId, groupId);

        try
        {
            var group = await TryGetGroup(groupId, cancellationToken);
            
            if (group == null)
            {
                _logger.LogWarning("Task group not found");
                return false;
            }
            
            return group.UserIds.Contains(userId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error checking if user {UserId} is in task group {Id}", userId, groupId);
            return false;
        }
    }

    public async Task<bool> AddTaskToTaskGroup(string groupId, Guid taskId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding task {taskId} to task group {Id}", taskId, groupId);

        try
        {
            var group = await TryGetGroup(groupId, cancellationToken);
            
            if (group == null)
            {
                _logger.LogWarning("Task group not found");
                return false;
            }
            
            group.AddTask(taskId.ToString());
            return await _taskGroupRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding task {taskId} to task group {Id}", taskId, groupId);
            return false;
        }
    }

    public async Task<bool> RemoveTaskFromTaskGroup(string groupId, Guid taskId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing task {taskId} from task group {Id}", taskId, groupId);

        try
        {
            var group = await TryGetGroup(groupId, cancellationToken);
            
            if (group == null)
            {
                _logger.LogWarning("Task group not found");
                return false;
            }
            
            group.RemoveTask(taskId.ToString());
            return await _taskGroupRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing task {taskId} from task group {Id}", taskId, groupId);
            return false;
        }
    }

    public async Task<bool> IsTaskInTaskGroup(string groupId, Guid taskId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking if task {task} is in task group {Id}", taskId, groupId);

        try
        {
            var group = await TryGetGroup(groupId, cancellationToken);
            
            if (group == null)
            {
                _logger.LogWarning("Task group not found");
                return false;
            }
            
            return group.TaskIds.Contains(taskId.ToString());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error checking if task {task} is in task group {Id}", taskId, groupId);
            return false;
        }
    }

    private async Task<TaskGroup?> TryGetGroup(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            _logger.LogWarning("Id is null or empty");
            return null;
        }

        try
        {
            var group = await _taskGroupRepository.GetTaskGroupById(id, cancellationToken);
            return group;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting task group by id {Id}", id);
            return null;
        }
    }
}