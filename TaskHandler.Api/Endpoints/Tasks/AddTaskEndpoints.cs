using TaskHandler.Application.Commands.AddTaskItem;
using TaskHandler.Application.Commands.DeleteTaskItem;
using TaskHandler.Application.Commands.UpdateTaskItem;
using TaskHandler.Application.DTOs;
using TaskHandler.Application.Queries.GetTaskItems;
using TaskHandler.Domain.Enums;
using TaskStatus = TaskHandler.Domain.Enums.TaskStatus;
using Microsoft.AspNetCore.Mvc;

namespace TaskHandler.Api.Endpoints.Tasks;

public record UpdateTaskItemRequest(
    string Title,
    string Description,
    TaskStatus Status,
    TaskType TaskType,
    TaskPriority Priority,
    DateTime? CompletionDate);

public record UpdateTaskItemResponse(bool Succeeded, string Message = "");

public record GetTaskItemsRequest(Guid? UserId = null);

public record DeleteTaskItemRequest(Guid Id);

public record DeleteTaskItemResponse(bool Succeeded, string Message = "");

public record AddTaskItemRequest(
    Guid UserId,
    string Title,
    string Description,
    TaskType TaskType,
    TaskPriority Priority,
    DateTime? CompletionDate);

public record AddTaskItemResponse(bool succeeded, Guid id = default);

public static class AddTaskEndpoints
{
    public static void MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var taskGroup = app.MapGroup("/api/tasks")
            .WithTags("Tasks")
            .RequireAuthorization()
            .WithOpenApi();

        MapPutTaskEndpoints(app, taskGroup);
        MapGetTaskEndpoints(app, taskGroup);
        MapPostTaskEndpoints(app, taskGroup);
        MapDeleteTaskEndpoints(app, taskGroup);
    }

    private static void MapPutTaskEndpoints(this IEndpointRouteBuilder endpoints, RouteGroupBuilder group)
    {
        group.MapPut("/update",
                async ([FromQuery] Guid id, [FromBody] UpdateTaskItemRequest request,
                    [FromServices] IMediator mediator) =>
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

    private static void MapGetTaskEndpoints(this IEndpointRouteBuilder endpoints, RouteGroupBuilder group)
    {
        group.MapGet("/getAllTasksForUser",
                async ([AsParameters] GetTaskItemsRequest request, [FromServices] IMediator mediator) =>
                {
                    var query = new GetTaskItemsQuery(request.UserId);
                    var result = await mediator.Send(query);
                    return Results.Ok(result);
                })
            .WithName("GetAllTasks")
            .Produces<List<GetTasItemkDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static void MapPostTaskEndpoints(this IEndpointRouteBuilder endpoints, RouteGroupBuilder group)
    {
        group.MapPost("/addTaskItem", async ([FromBody] AddTaskItemRequest request, [FromServices] ISender mediator) =>
        {
            var command = new AddTaskItemCommand(
                request.UserId,
                request.Title,
                request.Description);

            var result = await mediator.Send(command);

            if (!result.succeeded)
            {
                return Results.BadRequest(new { message = "Can't add task" });
            }

            return Results.Ok(new { message = $"Task added successfully with id {result.taskId}" });
        });
    }

    private static void MapDeleteTaskEndpoints(this IEndpointRouteBuilder endpoints, RouteGroupBuilder group)
    {
        group.MapDelete("/delete", async ([FromQuery] Guid id, [FromServices] IMediator mediator) =>
            {
                var command = new DeleteTaskItemCommand(id);
                var result = await mediator.Send(command);

                if (!result.Succeeded)
                    return Results.BadRequest(new { message = "Can't delete task" });

                return Results.Ok(new DeleteTaskItemResponse(true, "Task deleted successfully"));
            })
            .RequireAuthorization()
            .Produces<DeleteTaskItemResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}