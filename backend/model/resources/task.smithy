$version: "2"
namespace example.todoapp.resources

/// Represents a TODO list that belongs to a user
resource TodoList {
    identifiers: { 
        userId: UserId,
        listId: ListId 
    }
    properties: {
        name: String,
        description: String,
        createdAt: Timestamp,
        updatedAt: Timestamp
    }
    create: CreateTodoList,
    read: GetTodoList,
    update: UpdateTodoList,
    delete: DeleteTodoList,
    list: ListTodoLists
}

/// Represents a TODO task within a list
resource TodoTask {
    identifiers: { 
        userId: UserId,
        listId: ListId,
        taskId: TaskId 
    }
    properties: {
        description: String,
        completed: Boolean,
        createdAt: Timestamp,
        updatedAt: Timestamp
    }
    create: CreateTodoTask,
    read: GetTodoTask,
    update: UpdateTodoTask,
    delete: DeleteTodoTask,
    list: ListTodoTasks
}

/// Identifiers
string ListId
string TaskId

/// TodoList structures
structure CreateTodoListInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    name: String,
    
    description: String
}

structure UpdateTodoListInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId,
    
    name: String,
    description: String
}

structure TodoListOutput {
    @required
    userId: UserId,
    
    @required
    listId: ListId,
    
    @required
    name: String,
    
    description: String,
    
    @required
    createdAt: Timestamp,
    
    @required
    updatedAt: Timestamp
}

/// TodoTask structures
structure CreateTodoTaskInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId,
    
    @required
    description: String,
    
    completed: Boolean = false
}

structure UpdateTodoTaskInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId,
    
    @required
    @httpLabel
    taskId: TaskId,
    
    description: String,
    completed: Boolean
}

structure TodoTaskOutput {
    @required
    userId: UserId,
    
    @required
    listId: ListId,
    
    @required
    taskId: TaskId,
    
    @required
    description: String,
    
    @required
    completed: Boolean,
    
    @required
    createdAt: Timestamp,
    
    @required
    updatedAt: Timestamp
}

/// TodoList operations
@http(method: "POST", uri: "/users/{userId}/lists")
operation CreateTodoList {
    input: CreateTodoListInput,
    output: TodoListOutput,
    errors: [ValidationException, ResourceNotFoundException]
}

@readonly
@http(method: "GET", uri: "/users/{userId}/lists/{listId}")
operation GetTodoList {
    input: GetTodoListInput,
    output: TodoListOutput,
    errors: [ValidationException, ResourceNotFoundException]
}

structure GetTodoListInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId
}

@idempotent
@http(method: "PUT", uri: "/users/{userId}/lists/{listId}")
operation UpdateTodoList {
    input: UpdateTodoListInput,
    output: TodoListOutput,
    errors: [ValidationException, ResourceNotFoundException]
}

@idempotent
@http(method: "DELETE", uri: "/users/{userId}/lists/{listId}")
operation DeleteTodoList {
    input: DeleteTodoListInput,
    errors: [ValidationException, ResourceNotFoundException]
}

structure DeleteTodoListInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId
}

@readonly
@http(method: "GET", uri: "/users/{userId}/lists")
@paginated(inputToken: "nextToken", outputToken: "nextToken", pageSize: "maxResults")
operation ListTodoLists {
    input: ListTodoListsInput,
    output: ListTodoListsOutput
}

structure ListTodoListsInput {
    @required
    @httpLabel
    userId: UserId,
    
    @httpQuery("maxResults")
    maxResults: Integer,
    
    @httpQuery("nextToken")
    nextToken: String
}

structure ListTodoListsOutput {
    @required
    lists: TodoListList,
    
    nextToken: String
}

list TodoListList {
    member: TodoListOutput
}

/// TodoTask operations
@http(method: "POST", uri: "/users/{userId}/lists/{listId}/tasks")
operation CreateTodoTask {
    input: CreateTodoTaskInput,
    output: TodoTaskOutput,
    errors: [ValidationException, ResourceNotFoundException]
}

@readonly
@http(method: "GET", uri: "/users/{userId}/lists/{listId}/tasks/{taskId}")
operation GetTodoTask {
    input: GetTodoTaskInput,
    output: TodoTaskOutput,
    errors: [ValidationException, ResourceNotFoundException]
}

structure GetTodoTaskInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId,
    
    @required
    @httpLabel
    taskId: TaskId
}

@idempotent
@http(method: "PUT", uri: "/users/{userId}/lists/{listId}/tasks/{taskId}")
operation UpdateTodoTask {
    input: UpdateTodoTaskInput,
    output: TodoTaskOutput,
    errors: [ValidationException, ResourceNotFoundException]
}

@idempotent
@http(method: "DELETE", uri: "/users/{userId}/lists/{listId}/tasks/{taskId}")
operation DeleteTodoTask {
    input: DeleteTodoTaskInput,
    errors: [ValidationException, ResourceNotFoundException]
}

structure DeleteTodoTaskInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId,
    
    @required
    @httpLabel
    taskId: TaskId
}

@readonly
@http(method: "GET", uri: "/users/{userId}/lists/{listId}/tasks")
@paginated(inputToken: "nextToken", outputToken: "nextToken", pageSize: "maxResults")
operation ListTodoTasks {
    input: ListTodoTasksInput,
    output: ListTodoTasksOutput
}

structure ListTodoTasksInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId,
    
    @httpQuery("maxResults")
    maxResults: Integer,
    
    @httpQuery("nextToken")
    nextToken: String,
    
    @httpQuery("completed")
    completed: Boolean
}

structure ListTodoTasksOutput {
    @required
    tasks: TodoTaskList,
    
    nextToken: String
}

list TodoTaskList {
    member: TodoTaskOutput
}