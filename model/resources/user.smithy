$version: "2"
namespace jsawatzky.todoapp.resources

/// Represents a user in the TODO application
resource User {
    identifiers: { userId: UserId }
    properties: { 
        username: Username,
        email: Email,
        createdAt: Timestamp
    }
    create: CreateUser,
    read: GetUser,
    update: UpdateUser,
    delete: DeleteUser,
    list: ListUsers,
    collectionOperations: [GetUserByEmail]
}

/// User identifier
string UserId

/// Username with validation constraints
@length(min: 3, max: 50)
@pattern("^[a-zA-Z0-9_-]+$")
string Username

/// Email with validation constraints
@length(min: 5, max: 254)
@pattern("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")
string Email

/// Pagination max results constraint
@range(min: 1, max: 100)
integer MaxResults

/// Pagination token constraint
@length(min: 1, max: 1000)
string NextToken

/// Structure for creating a new user
structure CreateUserInput {
    @required
    username: Username,
    
    @required
    email: Email
}

/// Structure for updating a user
structure UpdateUserInput {
    @required
    @httpLabel
    userId: UserId,
    
    username: Username,
    email: Email
}

structure GetUserInput {
    @required
    @httpLabel
    userId: UserId
}

structure DeleteUserInput {
    @required
    @httpLabel
    userId: UserId
}

structure ListUsersInput {
    @httpQuery("maxResults")
    maxResults: MaxResults,
    
    @httpQuery("nextToken")
    nextToken: NextToken
}

structure ListUsersOutput {
    @required
    users: UserList,
    
    nextToken: NextToken
}

/// Structure for user response
structure UserOutput {
    @required
    userId: UserId,
    
    @required
    username: Username,
    
    @required
    email: Email,
    
    @required
    createdAt: Timestamp
}

list UserList {
    member: UserOutput
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

/// Update a user
@idempotent
@http(method: "PUT", uri: "/users/{userId}")
operation UpdateUser {
    input: UpdateUserInput,
    output: UserOutput,
    errors: [ValidationException, ResourceNotFoundException, ConflictException]
}

/// Delete a user
@idempotent
@http(method: "DELETE", uri: "/users/{userId}")
operation DeleteUser {
    input: DeleteUserInput,
    errors: [ValidationException, ResourceNotFoundException]
}

/// List all users
@readonly
@http(method: "GET", uri: "/users")
@paginated(inputToken: "nextToken", outputToken: "nextToken", pageSize: "maxResults")
operation ListUsers {
    input: ListUsersInput,
    output: ListUsersOutput,
    errors: [ValidationException]
}

/// Get a user by email address
structure GetUserByEmailInput {
    @required
    @httpQuery("email")
    email: Email
}

/// Get a user by their email address
@readonly
@http(method: "GET", uri: "/users/lookup")
operation GetUserByEmail {
    input: GetUserByEmailInput,
    output: UserOutput,
    errors: [ValidationException, ResourceNotFoundException]
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