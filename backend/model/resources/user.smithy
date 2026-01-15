$version: "2"
namespace example.todoapp.resources

/// Represents a user in the TODO application
resource User {
    identifiers: { userId: UserId }
    properties: { 
        username: String,
        email: String,
        createdAt: Timestamp
    }
    create: CreateUser,
    read: GetUser,
    update: UpdateUser,
    delete: DeleteUser,
    list: ListUsers
}

/// User identifier
string UserId

/// Structure for creating a new user
structure CreateUserInput {
    @required
    username: String,
    
    @required
    email: String
}

/// Structure for updating a user
structure UpdateUserInput {
    @required
    @httpLabel
    userId: UserId,
    
    username: String,
    email: String
}

/// Structure for user response
structure UserOutput {
    @required
    userId: UserId,
    
    @required
    username: String,
    
    @required
    email: String,
    
    @required
    createdAt: Timestamp
}

/// Create a new user
@http(method: "POST", uri: "/users")
operation CreateUser {
    input: CreateUserInput,
    output: UserOutput,
    errors: [ValidationException, ConflictException]
}

/// Get a user by ID
@readonly
@http(method: "GET", uri: "/users/{userId}")
operation GetUser {
    input: GetUserInput,
    output: UserOutput,
    errors: [ValidationException, ResourceNotFoundException]
}

structure GetUserInput {
    @required
    @httpLabel
    userId: UserId
}

/// Update a user
@idempotent
@http(method: "PUT", uri: "/users/{userId}")
operation UpdateUser {
    input: UpdateUserInput,
    output: UserOutput,
    errors: [ValidationException, ResourceNotFoundException]
}

/// Delete a user
@idempotent
@http(method: "DELETE", uri: "/users/{userId}")
operation DeleteUser {
    input: DeleteUserInput,
    errors: [ValidationException, ResourceNotFoundException]
}

structure DeleteUserInput {
    @required
    @httpLabel
    userId: UserId
}

/// List all users
@readonly
@http(method: "GET", uri: "/users")
@paginated(inputToken: "nextToken", outputToken: "nextToken", pageSize: "maxResults")
operation ListUsers {
    input: ListUsersInput,
    output: ListUsersOutput
}

structure ListUsersInput {
    @httpQuery("maxResults")
    maxResults: Integer,
    
    @httpQuery("nextToken")
    nextToken: String
}

structure ListUsersOutput {
    @required
    users: UserList,
    
    nextToken: String
}

list UserList {
    member: UserOutput
}

/// Common error structures
@error("client")
@httpError(400)
structure ValidationException {
    @required
    message: String
}

@error("client")
@httpError(404)
structure ResourceNotFoundException {
    @required
    message: String
}

@error("client")
@httpError(409)
structure ConflictException {
    @required
    message: String
}