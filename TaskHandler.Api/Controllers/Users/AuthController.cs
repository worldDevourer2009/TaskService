using Microsoft.AspNetCore.Mvc;
using TaskHandler.Application.Commands.Users;
using TaskHandler.Application.DTOs.User;

namespace TaskHandler.Api.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginUser([FromBody] UserLoginDTO userLoginDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userLoginDto.Email) || string.IsNullOrWhiteSpace(userLoginDto.Password))
            {
                return BadRequest(new {message = "Email or password is required"});
            }
            
            var command = new LoginUserCommand(userLoginDto.Email, userLoginDto.Password);
            var result = await _mediator.Send(command);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message});
            }
            
            return Ok(new  { message = result.Message});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(new {message = ex.Message});
        }
    }

    [HttpPost("sign-up")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignUpUser([FromBody] UserSingUpDTO userSignUpDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userSignUpDto.Name) || string.IsNullOrWhiteSpace(userSignUpDto.Email) ||
                string.IsNullOrWhiteSpace(userSignUpDto.Password))
            {
                return BadRequest(new {message = "Name, email or password is required"});
            }

            var command = new SignUpUserCommand(userSignUpDto.Name, userSignUpDto.Email, userSignUpDto.Password);
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message});
            }
            
            return Ok(new  { message = result.Message});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(new {message = ex.Message});
        }
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogoutUser([FromBody] UserLogoutDTO userLogoutDto)
    {
        if (userLogoutDto.UserId == Guid.Empty)
        {
            return BadRequest(new {message = "User id is required"});
        }
        
        var command = new UserLogoutCommand(userLogoutDto.UserId);
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(new { result.Message});
        }
        
        return Ok(result.Message); 
    }
}