using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Specifications;

namespace TaskHandler.Domain.Repositories;

public interface ITaskRepository
{
    Task<TaskItem> GetTaskItemForUser(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<TaskItem> GetTaskItemById(Guid taskId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItem>> GetAllTasksForUser(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskItem>> GetAllTasks(CancellationToken cancellationToken = default);
    Task<bool> TryAddTaskItem(TaskItem taskItem, CancellationToken cancellationToken = default);
    Task DeleteTaskItemById(Guid taskId, CancellationToken cancellationToken = default);
    Task UpdateTaskItem(TaskItem taskItem, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetTasksBySpecification(ISpecification<TaskItem> specification,  CancellationToken cancellationToken = default);
}