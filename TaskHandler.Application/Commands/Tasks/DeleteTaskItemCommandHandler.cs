using TaskHandler.Domain.Repositories;

namespace TaskHandler.Application.Commands.Tasks;

public record DeleteTaskItemCommand(Guid Id) : ICommand<DeleteTaskItemCommandResponse>;

public record DeleteTaskItemCommandResponse(bool Succeeded);

public class DeleteTaskItemCommandHandler : ICommandHandler<DeleteTaskItemCommand, DeleteTaskItemCommandResponse>
{
    private readonly ITaskRepository _context;

    public DeleteTaskItemCommandHandler(ITaskRepository context)
    {
        _context = context;
    }

    public async Task<DeleteTaskItemCommandResponse> Handle(DeleteTaskItemCommand request,
        CancellationToken cancellationToken)
    {
        await _context.DeleteTaskItemById(request.Id, cancellationToken);
        return new DeleteTaskItemCommandResponse(true);
    }
}