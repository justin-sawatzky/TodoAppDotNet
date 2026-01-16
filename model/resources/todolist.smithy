$version: "2"
namespace jsawatzky.todoapp.resources

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

/// Identifiers
string ListId

/// List name with validation constraints
@length(min: 1, max: 100)
string ListName

/// List description with validation constraints
@length(min: 0, max: 500)
string ListDescription

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

structure GetTodoListInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId
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

structure DeleteTodoListInput {
    @required
    @httpLabel
    userId: UserId,
    
    @required
    @httpLabel
    listId: ListId
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

structure ListTodoListsOutput {
    @required
    lists: TodoListList,
    
    nextToken: NextToken
}

list TodoListList {
    member: TodoListOutput
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

@readonly
@http(method: "GET", uri: "/users/{userId}/lists")
@paginated(inputToken: "nextToken", outputToken: "nextToken", pageSize: "maxResults")
operation ListTodoLists {
    input: ListTodoListsInput,
    output: ListTodoListsOutput,
    errors: [ValidationException, ResourceNotFoundException]
}
