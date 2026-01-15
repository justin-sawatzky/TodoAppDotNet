namespace TodoApp.DTOs;

public class CreateTodoTaskRequest
{
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public int? Order { get; set; }
}

public class UpdateTodoTaskRequest
{
    public string? Description { get; set; }
    public bool? Completed { get; set; }
    public int? Order { get; set; }
}

public class TodoTaskResponse
{
    public string UserId { get; set; } = string.Empty;
    public string ListId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public int Order { get; set; }
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
}

public class ListTodoTasksResponse
{
    public List<TodoTaskResponse> Tasks { get; set; } = new();
    public string? NextToken { get; set; }
}
