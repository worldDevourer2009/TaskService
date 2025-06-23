using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;

namespace TaskHandler.Application.Commands.AddTaskItem;

public record AddTaskItemCommand(
    Guid UserId,
    string Title,
    string Description) : ICommand<AddTaskItemResponse>;

public record AddTaskItemResponse(bool Succeeded, string? Message = null);

public class AddTaskItemCommandHandler : ICommandHandler<AddTaskItemCommand, AddTaskItemResponse>
{
    private readonly ITaskRepository _context;

    public AddTaskItemCommandHandler(ITaskRepository context)
    {
        _context = context;
    }

    public async Task<AddTaskItemResponse> Handle(AddTaskItemCommand command, CancellationToken cancellationToken)
    {
        var newTask = TaskItem.Create(command.UserId);
        newTask.Update(newTask);
        var success = await _context.TryAddTaskItem(newTask, cancellationToken);

        if (!success)
        {
            return new AddTaskItemResponse(success, "Task creation failed");
        }
        
        return new AddTaskItemResponse(success, "Task created successfully");
    }
}