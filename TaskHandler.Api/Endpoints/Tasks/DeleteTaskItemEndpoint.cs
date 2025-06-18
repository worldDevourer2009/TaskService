using TaskHandler.Application.Commands.DeleteTaskItem;

namespace TaskHandler.Api.Endpoints.Tasks;

public record DeleteTaskItemRequest(Guid Id);
public record DeleteTaskItemResponse(bool Succeeded, string Message = "");

public static class DeleteTaskItemEndpoint
{
    public static void MapDeleteTaskItemEndpoints(this IEndpointRouteBuilder app)
    {
        var taskGroup = app.MapGroup("/api/task")
            .WithTags("Tasks")
            .WithOpenApi();
        
        taskGroup.MapDelete("/{id:guid}", async (Guid id, ISender mediator) =>
        {
            var command = new DeleteTaskItemCommand(id);
            var result = await mediator.Send(command);

            return Results.Ok(new DeleteTaskItemResponse(result.Succeeded, "Task deleted successfully"));
        })
        .WithName("DeleteTask")
        .Produces<DeleteTaskItemResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}