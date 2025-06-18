using TaskHandler.Application.Commands.UpdateTaskItem;
using TaskHandler.Domain.Enums;
using TaskStatus = TaskHandler.Domain.Enums.TaskStatus;

namespace TaskHandler.Api.Endpoints.Tasks;

public record UpdateTaskItemRequest(
    string Title,
    string Description,
    TaskStatus Status,
    TaskType TaskType,
    TaskPriority Priority,
    DateTime? CompletionDate);

public record UpdateTaskItemResponse(bool Succeeded, string Message = "");

public static class UpdateTaskItemEndpoint
{
    public static void MapUpdateTaskItemEndpoint(this IEndpointRouteBuilder app)
    {
        var taskGroup = app.MapGroup("/api/task")
            .WithTags("Tasks")
            .WithOpenApi();

        taskGroup.MapPut("/{id:guid}", async (Guid id, UpdateTaskItemRequest request, IMediator mediator) =>
        {
            var command = new UpdateTaskItemCommand(
                id,
                request.Title,
                request.Description,
                request.Status,
                request.TaskType,
                request.Priority,
                request.CompletionDate);

            var result = await mediator.Send(command);

            if (!result.Succeeded)
            {
                return Results.NotFound(new { message = "Task not found" });
            }

            return Results.Ok(new UpdateTaskItemResponse(result.Succeeded, "Task updated successfully"));
        })
        .WithName("UpdateTask")
        .Produces<UpdateTaskItemResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}