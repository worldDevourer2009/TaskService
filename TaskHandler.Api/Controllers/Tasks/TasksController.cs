using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHandler.Application.Queries.GetTaskItems;
using System.Security.Claims;
using TaskHandler.Application.Commands.Tasks;
using TaskHandler.Shared.Tasks.DTOs.Tasks;

namespace TaskHandler.Api.Controllers.Tasks;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPut("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskDTO updateTaskDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("Invalid user token");
        }

        var command = new UpdateTaskItemCommand(
            updateTaskDto.Id,
            updateTaskDto.Title,
            updateTaskDto.Description,
            updateTaskDto.Status,
            updateTaskDto.TaskType,
            updateTaskDto.Priority,
            updateTaskDto.CompletionDate);

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result.Message);
    }

    [HttpGet("get-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTasks()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("Invalid user token");
        }

        var command = new GetTaskItemsQuery(userId.Value);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddTask([FromBody] AddTaskDTO addTaskDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("Invalid user token");
        }

        var command = new AddTaskItemCommand(userId.Value, addTaskDto.Title!, addTaskDto.Description!);

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }

        return Ok(result.Message);
    }

    [HttpDelete("delete/{taskId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTask(Guid taskId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("Invalid user token");
        }

        var command = new DeleteTaskItemCommand(taskId);
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest("Can't delete task. Task is already deleted");
        }

        return Ok("Task deleted successfully");
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                          User.FindFirst("sub") ?? User.FindFirst("userId");

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }
}