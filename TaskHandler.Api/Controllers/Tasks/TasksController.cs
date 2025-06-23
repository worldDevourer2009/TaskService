using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHandler.Application.Commands.AddTaskItem;
using TaskHandler.Application.Commands.DeleteTaskItem;
using TaskHandler.Application.Commands.UpdateTaskItem;
using TaskHandler.Application.DTOs.Tasks;
using TaskHandler.Application.Queries.GetTaskItems;

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
    public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskDTO updateTaskDto)
    {
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
            return BadRequest(new {message = result.Message});       
        }
        
        return Ok(result.Message);
    }

    [HttpGet("get-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTasks([FromQuery] Guid userId)
    {
        var command = new GetTaskItemsQuery(userId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddTask([FromBody] AddTaskForUserDTO addTaskForUserDto)
    {
        var command = new AddTaskItemCommand(addTaskForUserDto.UserId, addTaskForUserDto.Title, addTaskForUserDto.Description);
        
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }
        
        return Ok(result.Message);
    }

    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteTask([FromBody] DeleteTaskForUserDTO deleteTaskForUserDto)
    {
        var command = new DeleteTaskItemCommand(deleteTaskForUserDto.Id);
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest("Can't delete task. Task is already deleted");
        }
        
        return Ok("Task deleted successfully");
    }
}