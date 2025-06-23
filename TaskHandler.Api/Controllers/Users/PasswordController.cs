using Microsoft.AspNetCore.Mvc;
using TaskHandler.Application.Commands.Passwords;

namespace TaskHandler.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
public class PasswordController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public PasswordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("request-reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestReset([FromBody] RequestPasswordResetDTO requestPasswordResetDTO)
    {
        try
        {
            var command = new RequestPasswordResetCommand(requestPasswordResetDTO.Email);
            await _mediator.Send(command);
            return Ok(new {message = "Password reset request sent"});
        }
        catch (Exception e)
        {
            return BadRequest(new {message = e.Message});
        }
    }

    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        try
        {
            var command = new ResetPasswordCommand(request.UserId, request.ResetToken, request.NewPassword);
            await _mediator.Send(command);
            return Ok(new {message = "Password reset"});       
        }
        catch (Exception e)
        {
            return BadRequest(new {message = e.Message});       
        }
    }
}

public record RequestPasswordResetDTO(string Email);
public record ResetPasswordDto(Guid UserId, string ResetToken, string NewPassword);