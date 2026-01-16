using Microsoft.AspNetCore.Mvc;
using TodoApp.Generated;
using TodoApp.Services;

namespace TodoApp.Controllers;

[ApiController]
[Route("users/{userId}/lists")]
public class TodoListsController : ControllerBase
{
    private readonly ITodoListService _todoListService;

    public TodoListsController(ITodoListService todoListService)
    {
        _todoListService = todoListService;
    }

    [HttpGet]
    public async Task<ActionResult<ListTodoListsResponseContent>> ListTodoLists(
        string userId,
        [FromQuery] int? maxResults,
        [FromQuery] string? nextToken,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoListService.ListTodoListsAsync(userId, maxResults, nextToken, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TodoListOutput>> CreateTodoList(
        string userId,
        [FromBody] CreateTodoListRequestContent request,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoListService.CreateTodoListAsync(userId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{listId}")]
    public async Task<ActionResult<TodoListOutput>> GetTodoList(
        string userId,
        string listId,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoListService.GetTodoListAsync(userId, listId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{listId}")]
    public async Task<ActionResult<TodoListOutput>> UpdateTodoList(
        string userId,
        string listId,
        [FromBody] UpdateTodoListRequestContent request,
        CancellationToken cancellationToken = default)
    {
        var result = await _todoListService.UpdateTodoListAsync(userId, listId, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{listId}")]
    public async Task<ActionResult> DeleteTodoList(
        string userId,
        string listId,
        CancellationToken cancellationToken = default)
    {
        await _todoListService.DeleteTodoListAsync(userId, listId, cancellationToken);
        return Ok();
    }
}
