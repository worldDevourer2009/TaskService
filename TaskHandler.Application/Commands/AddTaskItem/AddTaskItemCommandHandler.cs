using TaskHandler.Domain.Entities;
using TaskHandler.Domain.Repositories;

namespace TaskHandler.Application.Commands.AddTaskItem;

public record AddTaskItemCommand(
    Guid userId,
    string title,
    string description) : ICommand<AddTaskItemCommandResult>;

public record AddTaskItemCommandResult(Guid Id);

public class AddTaskItemCommandHandler : ICommandHandler<AddTaskItemCommand, AddTaskItemCommandResult>
{
    private readonly ITaskRepository _context;

    public AddTaskItemCommandHandler(ITaskRepository context)
    {
        _context = context;
    }

    public async Task<AddTaskItemCommandResult> Handle(AddTaskItemCommand command, CancellationToken cancellationToken)
    {
        var newTask = TaskItem.Create(command.userId);
        newTask.Update(newTask);
        await _context.TryAddTaskItem(newTask, cancellationToken);
        return new AddTaskItemCommandResult(newTask.Id);
    }
}