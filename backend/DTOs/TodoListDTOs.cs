namespace TodoApp.DTOs;

public class CreateTodoListRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateTodoListRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class TodoListResponse
{
    public string UserId { get; set; } = string.Empty;
    public string ListId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
}

public class ListTodoListsResponse
{
    public List<TodoListResponse> Lists { get; set; } = new();
    public string? NextToken { get; set; }
}
