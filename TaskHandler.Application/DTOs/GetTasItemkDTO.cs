using TaskHandler.Domain;
using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Enums;
using TaskStatus = TaskHandler.Domain.Enums.TaskStatus;

namespace TaskHandler.Application.DTOs;

public class GetTasItemkDTO
{
    public Guid? UserId { get; set; }
    public string? Title { get; set; } = "New Task";
    public string? Description { get; set; } = string.Empty;
    public TaskStatus? Status { get; set; } = TaskStatus.Pending;
    public TaskType? TaskType { get; set; } = Domain.Enums.TaskType.None;
    public TaskPriority? Priority { get; set; } = 0;
    public DateTime? CompletionDate { get; set; }
    
    public bool HasUserId => UserId != null;
}