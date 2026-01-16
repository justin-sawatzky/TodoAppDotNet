using TodoApp.Generated;
using TodoApp.Models;
using TodoApp.Repositories;

namespace TodoApp.Services;

public class TodoTaskService : ITodoTaskService
{
    private readonly ITodoTaskRepository _todoTaskRepository;
    private readonly ITodoListRepository _todoListRepository;
    private readonly IUserRepository _userRepository;

    public TodoTaskService(ITodoTaskRepository todoTaskRepository, ITodoListRepository todoListRepository, IUserRepository userRepository)
    {
        _todoTaskRepository = todoTaskRepository;
        _todoListRepository = todoListRepository;
        _userRepository = userRepository;
    }

    private async Task ValidateUserAndListExistAsync(string userId, string listId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        var list = await _todoListRepository.GetListByIdAsync(userId, listId, cancellationToken);
        if (list == null)
        {
            throw new KeyNotFoundException($"List with ID {listId} not found");
        }
    }

    public async Task<ListTodoTasksResponseContent> ListTodoTasksAsync(string userId, string listId, int? maxResults, string? nextToken, bool? completed, CancellationToken cancellationToken)
    {
        await ValidateUserAndListExistAsync(userId, listId, cancellationToken);

        var (tasks, nextPageToken) = await _todoTaskRepository.GetTasksAsync(
            userId,
            listId,
            maxResults ?? 50,
            nextToken,
            completed,
            cancellationToken);

        return new ListTodoTasksResponseContent
        {
            Tasks = tasks.Select(MapToOutput).ToList(),
            NextToken = nextPageToken
        };
    }

    public async Task<TodoTaskOutput> CreateTodoTaskAsync(string userId, string listId, CreateTodoTaskRequestContent request, CancellationToken cancellationToken)
    {
        await ValidateUserAndListExistAsync(userId, listId, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ArgumentException("Task description is required");
        }

        var task = new TodoTask
        {
            UserId = userId,
            ListId = listId,
            TaskId = Guid.NewGuid().ToString(),
            Description = request.Description,
            Completed = request.Completed ?? false,
            Order = 0, // Will be set by repository
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Pass requested order to repository for atomic assignment
        int? requestedOrder = request.Order.HasValue ? (int)request.Order.Value : null;
        await _todoTaskRepository.CreateTaskAsync(task, requestedOrder, cancellationToken);

        return MapToOutput(task);
    }

    public async Task<TodoTaskOutput> GetTodoTaskAsync(string userId, string listId, string taskId, CancellationToken cancellationToken)
    {
        await ValidateUserAndListExistAsync(userId, listId, cancellationToken);

        var task = await _todoTaskRepository.GetTaskByIdAsync(userId, listId, taskId, cancellationToken);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        return MapToOutput(task);
    }

    public async Task<TodoTaskOutput> UpdateTodoTaskAsync(string userId, string listId, string taskId, UpdateTodoTaskRequestContent request, CancellationToken cancellationToken)
    {
        await ValidateUserAndListExistAsync(userId, listId, cancellationToken);

        var task = await _todoTaskRepository.GetTaskByIdAsync(userId, listId, taskId, cancellationToken);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            task.Description = request.Description;
        }

        if (request.Completed.HasValue)
        {
            task.Completed = request.Completed.Value;
        }

        if (request.Order.HasValue)
        {
            task.Order = (int)request.Order.Value;
        }

        task.UpdatedAt = DateTimeOffset.UtcNow;

        await _todoTaskRepository.UpdateTaskAsync(task, cancellationToken);

        return MapToOutput(task);
    }

    public async Task DeleteTodoTaskAsync(string userId, string listId, string taskId, CancellationToken cancellationToken)
    {
        await ValidateUserAndListExistAsync(userId, listId, cancellationToken);

        var task = await _todoTaskRepository.GetTaskByIdAsync(userId, listId, taskId, cancellationToken);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found");
        }

        await _todoTaskRepository.DeleteTaskAsync(userId, listId, taskId, cancellationToken);
    }

    public async Task<ReorderTodoTasksResponseContent> ReorderTodoTasksAsync(string userId, string listId, ReorderTodoTasksRequestContent request, CancellationToken cancellationToken)
    {
        await ValidateUserAndListExistAsync(userId, listId, cancellationToken);

        if (request.TaskOrders == null || request.TaskOrders.Count == 0)
        {
            throw new ArgumentException("Task orders are required");
        }

        var taskOrders = request.TaskOrders.ToDictionary(
            to => to.TaskId,
            to => (int)to.Order
        );

        var tasks = await _todoTaskRepository.ReorderTasksAsync(userId, listId, taskOrders, cancellationToken);

        return new ReorderTodoTasksResponseContent
        {
            Tasks = tasks.Select(MapToOutput).ToList()
        };
    }

    private static TodoTaskOutput MapToOutput(TodoTask task) => new()
    {
        UserId = task.UserId,
        ListId = task.ListId,
        TaskId = task.TaskId,
        Description = task.Description,
        Completed = task.Completed,
        Order = task.Order,
        CreatedAt = task.CreatedAt.ToUnixTimeSeconds(),
        UpdatedAt = task.UpdatedAt.ToUnixTimeSeconds()
    };
}
