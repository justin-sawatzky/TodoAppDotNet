$version: "2"
namespace example.todoapp.resources

/// Represents a TODO list that belongs to a user
resource TodoList {
    identifiers: { 
        userId: UserId,
        listId: ListId 
    }
    properties: {
        name: ListName,
        description: ListDescription,
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
        description: TaskDescription,
        completed: Boolean,
        order: TaskOrder,
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

/// List name with validation constraints
@length(min: 1, max: 100)
string ListName

/// List description with validation constraints
@length(min: 0, max: 500)
string ListDescription

/// Task description with validation constraints
@length(min: 1, max: 1000)
string TaskDescription

/// Task order with validation constraints
@range(min: 0, max: 999999)
integer TaskOrder

/// TodoList structures
structure CreateTodoListInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    name: ListName,
    
    description: ListDescription
}

structure UpdateTodoListInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId,
    
    name: ListName,
    description: ListDescription
}

structure TodoListOutput {
    @required
    userId: UserId,
    
    @required
    listId: ListId,
    
    @required
    name: ListName,
    
    description: ListDescription,
    
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
    description: TaskDescription,
    
    completed: Boolean = false,
    
    order: TaskOrder
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
    
    description: TaskDescription,
    completed: Boolean,
    order: TaskOrder
}

structure TodoTaskOutput {
    @required
    userId: UserId,
    
    @required
    listId: ListId,
    
    @required
    taskId: TaskId,
    
    @required
    description: TaskDescription,
    
    @required
    completed: Boolean,
    
    order: TaskOrder,
    
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
    maxResults: MaxResults,
    
    @httpQuery("nextToken")
    nextToken: NextToken
}

structure ListTodoListsOutput {
    @required
    lists: TodoListList,
    
    nextToken: NextToken
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
    maxResults: MaxResults,
    
    @httpQuery("nextToken")
    nextToken: NextToken,
    
    @httpQuery("completed")
    completed: Boolean
}

structure ListTodoTasksOutput {
    @required
    tasks: TodoTaskList,
    
    nextToken: NextToken
}

list TodoTaskList {
    member: TodoTaskOutput
}