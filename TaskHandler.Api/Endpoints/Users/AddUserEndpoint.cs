using TaskHandler.Application.Commands.Users;
using Microsoft.AspNetCore.Mvc;

namespace TaskHandler.Api.Endpoints.Users;

public record SignUpUserRequest(string Name, string Email, string Password);
public record SignUpUserResponse(bool Succeeded, string Message = "");
public record LoginRequest(string Email, string Password);
public record LoginResponse(bool Succeeded, string Message);

public static class AddUserEndpoint
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var userGroup = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();
        
        MapUserPostEndpoint(app, userGroup);
        MapUserPutEndpoint(app, userGroup);
        MapUserDeleteEndpoint(app, userGroup);
        MapUserGetEndpoint(app, userGroup);
    }

    private static void MapUserPostEndpoint(this IEndpointRouteBuilder endpoints, RouteGroupBuilder group)
    {
        group.MapPost("auth/signUp", async ([FromBody] SignUpUserRequest request, [FromServices] ISender mediator) =>
        {
            var command = new SignUpUserCommand(request.Name, request.Email, request.Password);
            var result = await mediator.Send(command);

            return result.success ? Results.Ok(new SignUpUserResponse(result.success, "User created successfully")) 
                : Results.BadRequest(new SignUpUserResponse(result.success, result.message));
        });

        group.MapPost("auth/login", async ([FromBody] LoginRequest request, [FromServices] ISender mediator) =>
        {
            var command = new LoginUserCommand(request.Email, request.Password);
            var result = await mediator.Send(command);

            return result.success 
                ? Results.Ok(new LoginResponse(result.success, "User logged in successfully")) 
                : Results.BadRequest(new LoginResponse(result.success, "Failed to login. {" + result.message + "}"));
        });
    }

    private static void MapUserPutEndpoint(this IEndpointRouteBuilder endpoints, RouteGroupBuilder group)
    {
        
    }

    private static void MapUserDeleteEndpoint(this IEndpointRouteBuilder endpoints, RouteGroupBuilder group)
    {
        
    }

    private static void MapUserGetEndpoint(this IEndpointRouteBuilder endpoints, RouteGroupBuilder group)
    {
        
    }
}