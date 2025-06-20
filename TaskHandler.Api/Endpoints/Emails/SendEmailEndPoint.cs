using Microsoft.AspNetCore.Mvc;
using TaskHandler.Application.Commands.Emails;
using TaskHandler.Application.DTOs;

namespace TaskHandler.Api.Endpoints.Emails;

public static class SendEmailEndPoint
{
    public static void SetupEmailEndPoint(this IEndpointRouteBuilder app)
    {
        var emailGroup = app.MapGroup("/api/emails")
            .WithTags("Emails")
            .WithOpenApi();
        
        emailGroup.MapPost("", async ([FromBody] EmailRequestDTO email, ISender mediator) =>
        {
            if (string.IsNullOrEmpty(email.Email))
            {
                return Results.BadRequest("Email address is required");
            }
            
            var command = new SendEmailCommand(email.Email, email.Subject, email.Message, email.HtmlMessage, null);
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .WithName("SendEmail")
        .Produces<SendEmailCommandResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}