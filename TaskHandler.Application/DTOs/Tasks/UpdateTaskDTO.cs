using TaskHandler.Domain.Enums;
using TaskStatus = TaskHandler.Domain.Enums.TaskStatus;

namespace TaskHandler.Application.DTOs.Tasks;

public class UpdateTaskDTO
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskType TaskType { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? CompletionDate { get; set; }
}