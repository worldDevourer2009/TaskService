using System.ComponentModel.DataAnnotations;
using TaskHandler.Domain.Enums;
using TaskStatus = TaskHandler.Domain.Enums.TaskStatus;

namespace TaskHandler.Application.DTOs.Tasks;

public class AddTaskForUserDTO
{
    public Guid UserId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskType TaskType { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? CompletionDate { get; set; }
}

public class AddTaskDTO
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}