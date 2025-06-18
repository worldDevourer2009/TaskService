using FluentValidation;
using TaskHandler.Application.Commands.AddTaskItem;
using TaskHandler.Domain.Enums;
using TaskStatus = TaskHandler.Domain.Enums.TaskStatus;

namespace TaskHandler.Api.Endpoints.Tasks;

public record AddTaskItemRequest(
    Guid UserId,
    string Title,
    string Description,
    TaskType TaskType,
    TaskPriority Priority,
    DateTime? CompletionDate);

public class AddTaskItemValidator : AbstractValidator<AddTaskItemCommand>
{
    public AddTaskItemValidator()
    {
        RuleFor(item => item.title)
            .NotEmpty()
            .WithMessage("Title is required");

        RuleFor(item => item.description)
            .NotEmpty()
            .WithMessage("Description is required");
        
        RuleFor(item => item.userId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

public record AddTaskItemResponse(Guid Id);

public static class AddTaskItemEndpoint
{
    public static void MapPostTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var taskGroup = app.MapGroup("/api/task")
            .WithTags("Tasks")
            .WithOpenApi();

        taskGroup.MapPost("/addTaskItem", async (AddTaskItemRequest request, ISender mediator) =>
        {
            var command = new AddTaskItemCommand(
                request.UserId,
                request.Title,
                request.Description);

            var result = await mediator.Send(command);
            var response = new AddTaskItemResponse(result.Id);

            return Results.Ok(response);
        });
    }
}