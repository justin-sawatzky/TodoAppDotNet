$version: "2"
namespace jsawatzky.todoapp.resources

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
    list: ListTodoTasks,
    collectionOperations: [ReorderTodoTasks]
}

/// Identifiers
string TaskId

/// Task description with validation constraints
@length(min: 1, max: 1000)
string TaskDescription

/// Task order with validation constraints
@range(min: 0, max: 999999)
integer TaskOrder

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

@readonly
@http(method: "GET", uri: "/users/{userId}/lists/{listId}/tasks")
@paginated(inputToken: "nextToken", outputToken: "nextToken", pageSize: "maxResults")
operation ListTodoTasks {
    input: ListTodoTasksInput,
    output: ListTodoTasksOutput,
    errors: [ValidationException, ResourceNotFoundException]
}

/// Task order entry for batch reorder
structure TaskOrderEntry {
    @required
    taskId: TaskId,
    
    @required
    order: TaskOrder
}

/// List of task order entries
list TaskOrderList {
    member: TaskOrderEntry
}

/// Input for batch reorder operation
structure ReorderTodoTasksInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId,
    
    @required
    taskOrders: TaskOrderList
}

/// Output for batch reorder operation
structure ReorderTodoTasksOutput {
    @required
    tasks: TodoTaskList
}

/// Batch reorder tasks within a list
@idempotent
@http(method: "PUT", uri: "/users/{userId}/lists/{listId}/tasks/reorder")
operation ReorderTodoTasks {
    input: ReorderTodoTasksInput,
    output: ReorderTodoTasksOutput,
    errors: [ValidationException, ResourceNotFoundException]
}
