using TaskHandler.Domain.Entities;

namespace TaskHandler.Domain.Services;

public interface ITaskGroupService
{
    Task<bool> AddTaskGroup(TaskGroup taskGroup, CancellationToken cancellationToken = default);
    Task<TaskGroup?> GetTaskGroupById(Guid groupId, CancellationToken cancellationToken = default);
    Task<List<TaskGroup>?> GetAllTaskGroupsForUser(string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateTaskGroup(string groupId, TaskGroup taskGroup, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskGroup(string groupId, CancellationToken cancellationToken = default);
    Task<bool> AddUserToTaskGroup(string groupId, string userId, CancellationToken cancellationToken = default);
    Task<bool> RemoveUserFromTaskGroup(string groupId, string userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserInTaskGroup(string groupId, string userId, CancellationToken cancellationToken = default);
    Task<bool> AddTaskToTaskGroup(string groupId, Guid taskId, CancellationToken cancellationToken = default);
    Task<bool> RemoveTaskFromTaskGroup(string groupId, Guid taskId, CancellationToken cancellationToken = default);
    Task<bool> IsTaskInTaskGroup(string groupId, Guid taskId, CancellationToken cancellationToken = default);
}