namespace TaskHandler.Application.DTOs.Tasks;

public class DeleteTaskForUserDTO
{
    public Guid UserId { get; set; }
    public Guid Id { get; set; }
}