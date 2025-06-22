using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;

namespace TaskHandler.Application.Commands.AddTaskItem;

public record AddTaskItemCommand(
    Guid userId,
    string title,
    string description) : ICommand<AddTaskItemResponse>;

public record AddTaskItemResponse(bool succeeded, Guid taskId = default);

public class AddTaskItemCommandHandler : ICommandHandler<AddTaskItemCommand, AddTaskItemResponse>
{
    private readonly ITaskRepository _context;

    public AddTaskItemCommandHandler(ITaskRepository context)
    {
        _context = context;
    }

    public async Task<AddTaskItemResponse> Handle(AddTaskItemCommand command, CancellationToken cancellationToken)
    {
        var newTask = TaskItem.Create(command.userId);
        newTask.Update(newTask);
        var success = await _context.TryAddTaskItem(newTask, cancellationToken);
        return new AddTaskItemResponse(success, newTask.Id);
    }
}