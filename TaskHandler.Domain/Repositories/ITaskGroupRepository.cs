using TaskHandler.Domain.Entities;

namespace TaskHandler.Domain.Repositories;

public interface ITaskGroupRepository
{
    Task<TaskGroup?> GetTaskGroupById(string id, CancellationToken cancellationToken = default);
    Task<List<TaskGroup>?> GetAllTaskGroupsForUser(string id, CancellationToken cancellationToken = default);
    Task<bool> AddTaskGroup(TaskGroup taskGroup, CancellationToken cancellationToken = default);
    Task<bool> UpdateTaskGroup(string id, TaskGroup taskGroup, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskGroup(string id, CancellationToken cancellationToken = default);
    Task<bool> SaveChangesAsync(CancellationToken cancellation = default);
}