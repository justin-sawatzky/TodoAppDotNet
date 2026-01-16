using TodoApp.Generated;
using TodoApp.Models;
using TodoApp.Repositories;

namespace TodoApp.Services;

public class TodoListService : ITodoListService
{
    private readonly ITodoListRepository _todoListRepository;
    private readonly IUserRepository _userRepository;

    public TodoListService(ITodoListRepository todoListRepository, IUserRepository userRepository)
    {
        _todoListRepository = todoListRepository;
        _userRepository = userRepository;
    }

    private async Task ValidateUserExistsAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }
    }

    public async Task<ListTodoListsResponseContent> ListTodoListsAsync(string userId, int? maxResults, string? nextToken, CancellationToken cancellationToken)
    {
        await ValidateUserExistsAsync(userId, cancellationToken);

        var (lists, nextPageToken) = await _todoListRepository.GetListsAsync(
            userId,
            maxResults ?? 50,
            nextToken,
            cancellationToken);

        return new ListTodoListsResponseContent
        {
            Lists = lists.Select(MapToOutput).ToList(),
            NextToken = nextPageToken
        };
    }

    public async Task<TodoListOutput> CreateTodoListAsync(string userId, CreateTodoListRequestContent request, CancellationToken cancellationToken)
    {
        await ValidateUserExistsAsync(userId, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("List name is required");
        }

        var list = new TodoList
        {
            UserId = userId,
            ListId = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _todoListRepository.CreateListAsync(list, cancellationToken);

        return MapToOutput(list);
    }

    public async Task<TodoListOutput> GetTodoListAsync(string userId, string listId, CancellationToken cancellationToken)
    {
        await ValidateUserExistsAsync(userId, cancellationToken);

        var list = await _todoListRepository.GetListByIdAsync(userId, listId, cancellationToken);
        if (list == null)
        {
            throw new KeyNotFoundException($"List with ID {listId} not found");
        }

        return MapToOutput(list);
    }

    public async Task<TodoListOutput> UpdateTodoListAsync(string userId, string listId, UpdateTodoListRequestContent request, CancellationToken cancellationToken)
    {
        await ValidateUserExistsAsync(userId, cancellationToken);

        var list = await _todoListRepository.GetListByIdAsync(userId, listId, cancellationToken);
        if (list == null)
        {
            throw new KeyNotFoundException($"List with ID {listId} not found");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            list.Name = request.Name;
        }

        if (request.Description != null)
        {
            list.Description = request.Description;
        }

        list.UpdatedAt = DateTimeOffset.UtcNow;

        await _todoListRepository.UpdateListAsync(list, cancellationToken);

        return MapToOutput(list);
    }

    public async Task DeleteTodoListAsync(string userId, string listId, CancellationToken cancellationToken)
    {
        await ValidateUserExistsAsync(userId, cancellationToken);

        var list = await _todoListRepository.GetListByIdAsync(userId, listId, cancellationToken);
        if (list == null)
        {
            throw new KeyNotFoundException($"List with ID {listId} not found");
        }

        await _todoListRepository.DeleteListAsync(userId, listId, cancellationToken);
    }

    private static TodoListOutput MapToOutput(TodoList list) => new()
    {
        UserId = list.UserId,
        ListId = list.ListId,
        Name = list.Name,
        Description = list.Description,
        CreatedAt = list.CreatedAt.ToUnixTimeSeconds(),
        UpdatedAt = list.UpdatedAt.ToUnixTimeSeconds()
    };
}
