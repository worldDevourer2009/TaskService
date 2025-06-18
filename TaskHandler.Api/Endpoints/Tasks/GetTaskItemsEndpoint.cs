using TaskHandler.Application.DTOs;
using TaskHandler.Application.Queries.GetTaskItems;

namespace TaskHandler.Api.Endpoints.Tasks;

public static class GetTaskItemsEndpoint
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var taskGroup = app.MapGroup("/api/task")
            .WithTags("Tasks")
            .WithOpenApi();

        taskGroup.MapGet("/getAllTasks", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetTaskItemsQuery());
                return Results.Ok(result);
            })
            .WithName("GetAllTasks")
            .Produces<List<GetTasItemkDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);


        taskGroup.MapGet("/getAllTasksForUser",
                async ([AsParameters] GetTaskItemsRequest request, IMediator mediator) =>
                {
                    if (request.UserId == null)
                    {
                        return Results.BadRequest("User id is required");
                    }
                    
                    var query = request;
                    var result = await mediator.Send(new GetTaskItemsQuery(query.UserId));
                    return Results.Ok(result);
                })
            .WithName("GetAllTasksForUser")
            .Produces<List<GetTasItemkDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}