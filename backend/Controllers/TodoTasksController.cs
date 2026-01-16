using Microsoft.AspNetCore.Mvc;
using TodoApp.Generated;
using TodoApp.Services;

namespace TodoApp.Controllers;

[ApiController]
[Route("users/{userId}/lists/{listId}/tasks")]
public class TodoTasksController : ControllerBase
{
    private readonly ITodoTaskService _todoTaskService;

    public TodoTasksController(ITodoTaskService todoTaskService)
    {
        _todoTaskService = todoTaskService;
    }

    [HttpGet]
    public async Task<ActionResult<ListTodoTasksResponseContent>> ListTodoTasks(
        string userId,
        string listId,
        [FromQuery] int? maxResults,
        [FromQuery] string? nextToken,
        [FromQuery] bool? completed,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoTaskService.ListTodoTasksAsync(userId, listId, maxResults, nextToken, completed, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TodoTaskOutput>> CreateTodoTask(
        string userId,
        string listId,
        [FromBody] CreateTodoTaskRequestContent request,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoTaskService.CreateTodoTaskAsync(userId, listId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{taskId}")]
    public async Task<ActionResult<TodoTaskOutput>> GetTodoTask(
        string userId,
        string listId,
        string taskId,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoTaskService.GetTodoTaskAsync(userId, listId, taskId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{taskId}")]
    public async Task<ActionResult<TodoTaskOutput>> UpdateTodoTask(
        string userId,
        string listId,
        string taskId,
        [FromBody] UpdateTodoTaskRequestContent request,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoTaskService.UpdateTodoTaskAsync(userId, listId, taskId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{taskId}")]
    public async Task<ActionResult> DeleteTodoTask(
        string userId,
        string listId,
        string taskId,
        CancellationToken cancellationToken = default)
    {
        await _todoTaskService.DeleteTodoTaskAsync(userId, listId, taskId, cancellationToken);
        return Ok();
    }

    [HttpPut("reorder")]
    public async Task<ActionResult<ReorderTodoTasksResponseContent>> ReorderTodoTasks(
        string userId,
        string listId,
        [FromBody] ReorderTodoTasksRequestContent request,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoTaskService.ReorderTodoTasksAsync(userId, listId, request, cancellationToken);
        return Ok(result);
    }
}
