using TaskHandler.Domain.Repositories;
using TaskHandler.Shared.Tasks.Enums;
using TaskStatus = TaskHandler.Shared.Tasks.Enums.TaskStatus;

namespace TaskHandler.Application.Commands.Tasks;

public record UpdateTaskItemCommand(
    Guid Id,
    string Title,
    string Description,
    TaskStatus Status,
    TaskType TaskType,
    TaskPriority Priority,
    DateTime? CompletionDate) : ICommand<UpdateTaskItemCommandResult>;

public record UpdateTaskItemCommandResult(bool Succeeded, string Message = "");

public class UpdateTaskItemCommandHandler : ICommandHandler<UpdateTaskItemCommand, UpdateTaskItemCommandResult>
{
    private readonly ITaskRepository _context;
    
    public UpdateTaskItemCommandHandler(ITaskRepository context)
    {
        _context = context;
    }
    
    public async Task<UpdateTaskItemCommandResult> Handle(UpdateTaskItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _context.GetTaskItemById(request.Id, cancellationToken);
            await _context.UpdateTaskItem(task, cancellationToken);
            return new UpdateTaskItemCommandResult(true, "Task updated successfully");
        }
        catch (Exception ex)
        {
            return new UpdateTaskItemCommandResult(false, ex.Message);
        }
    }
}