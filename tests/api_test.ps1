#
# TodoApp API Test Script (PowerShell version)
# Works on Windows PowerShell and PowerShell Core (cross-platform)
#
# Usage: .\tests\api_test.ps1 [-BaseUrl URL]
# Default BaseUrl: http://localhost:5247
#

param(
    [string]$BaseUrl = "http://localhost:5247"
)

$ErrorActionPreference = "Continue"

$script:Passed = 0
$script:Failed = 0
$script:TestUserId = $null
$script:TestListId = $null
$script:TestTaskId = $null
$script:Timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

function Write-TestResult {
    param([string]$Name, [bool]$Passed, [string]$Details = "")
    
    if ($Passed) {
        $script:Passed++
        Write-Host "  " -NoNewline
        Write-Host "✓ PASS" -ForegroundColor Green -NoNewline
        Write-Host " $Name"
    } else {
        $script:Failed++
        Write-Host "  " -NoNewline
        Write-Host "✗ FAIL" -ForegroundColor Red -NoNewline
        Write-Host " $Name"
        if ($Details) {
            Write-Host "         $Details" -ForegroundColor Yellow
        }
    }
}

function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body = $null,
        [int]$ExpectedStatus = 200
    )
    
    $url = "$BaseUrl$Path"
    $headers = @{ "Content-Type" = "application/json" }
    
    try {
        $params = @{
            Uri = $url
            Method = $Method
            Headers = $headers
            UseBasicParsing = $true
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-WebRequest @params -ErrorAction Stop
        $statusCode = $response.StatusCode
        
        if ($statusCode -eq $ExpectedStatus) {
            if ($response.Content) {
                return @{ Success = $true; Data = ($response.Content | ConvertFrom-Json) }
            }
            return @{ Success = $true; Data = $null }
        }
        return @{ Success = $false; Error = "Expected $ExpectedStatus, got $statusCode" }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq $ExpectedStatus) {
            return @{ Success = $true; Data = $null }
        }
        return @{ Success = $false; Error = $_.Exception.Message }
    }
}

# ==================== USER TESTS ====================

function Test-CreateUser {
    $body = @{
        username = "testuser_$script:Timestamp"
        email = "test_$script:Timestamp@example.com"
    }
    
    $result = Invoke-ApiRequest -Method "POST" -Path "/users" -Body $body
    
    if ($result.Success -and $result.Data.userId) {
        $script:TestUserId = $result.Data.userId
        Write-TestResult "Create User" $true
        return $true
    }
    Write-TestResult "Create User" $false $result.Error
    return $false
}

function Test-CreateUserValidationError {
    $body = @{ username = "ab"; email = "invalid" }
    $result = Invoke-ApiRequest -Method "POST" -Path "/users" -Body $body -ExpectedStatus 400
    Write-TestResult "Create User - Validation Error (400)" $result.Success
    return $result.Success
}

function Test-ListUsers {
    $result = Invoke-ApiRequest -Method "GET" -Path "/users"
    
    if ($result.Success -and $null -ne $result.Data.users) {
        Write-TestResult "List Users" $true
        return $true
    }
    Write-TestResult "List Users" $false $result.Error
    return $false
}

function Test-GetUser {
    if (-not $script:TestUserId) {
        Write-TestResult "Get User" $false "No test user created"
        return $false
    }
    
    $result = Invoke-ApiRequest -Method "GET" -Path "/users/$script:TestUserId"
    
    if ($result.Success -and $result.Data.userId) {
        Write-TestResult "Get User" $true
        return $true
    }
    Write-TestResult "Get User" $false $result.Error
    return $false
}

function Test-GetUserNotFound {
    $result = Invoke-ApiRequest -Method "GET" -Path "/users/nonexistent-user-id" -ExpectedStatus 404
    Write-TestResult "Get User - Not Found (404)" $result.Success
    return $result.Success
}

function Test-UpdateUser {
    if (-not $script:TestUserId) {
        Write-TestResult "Update User" $false "No test user created"
        return $false
    }
    
    $body = @{ username = "updated_$script:Timestamp" }
    $result = Invoke-ApiRequest -Method "PUT" -Path "/users/$script:TestUserId" -Body $body
    Write-TestResult "Update User" $result.Success
    return $result.Success
}

# ==================== TODO LIST TESTS ====================

function Test-CreateTodoList {
    if (-not $script:TestUserId) {
        Write-TestResult "Create Todo List" $false "No test user created"
        return $false
    }
    
    $body = @{
        name = "Test List $script:Timestamp"
        description = "A test todo list"
    }
    
    $result = Invoke-ApiRequest -Method "POST" -Path "/users/$script:TestUserId/lists" -Body $body
    
    if ($result.Success -and $result.Data.listId) {
        $script:TestListId = $result.Data.listId
        Write-TestResult "Create Todo List" $true
        return $true
    }
    Write-TestResult "Create Todo List" $false $result.Error
    return $false
}

function Test-ListTodoLists {
    if (-not $script:TestUserId) {
        Write-TestResult "List Todo Lists" $false "No test user created"
        return $false
    }
    
    $result = Invoke-ApiRequest -Method "GET" -Path "/users/$script:TestUserId/lists"
    
    if ($result.Success -and $null -ne $result.Data.lists) {
        Write-TestResult "List Todo Lists" $true
        return $true
    }
    Write-TestResult "List Todo Lists" $false $result.Error
    return $false
}

function Test-GetTodoList {
    if (-not $script:TestUserId -or -not $script:TestListId) {
        Write-TestResult "Get Todo List" $false "No test user/list created"
        return $false
    }
    
    $result = Invoke-ApiRequest -Method "GET" -Path "/users/$script:TestUserId/lists/$script:TestListId"
    
    if ($result.Success -and $result.Data.listId) {
        Write-TestResult "Get Todo List" $true
        return $true
    }
    Write-TestResult "Get Todo List" $false $result.Error
    return $false
}

function Test-UpdateTodoList {
    if (-not $script:TestUserId -or -not $script:TestListId) {
        Write-TestResult "Update Todo List" $false "No test user/list created"
        return $false
    }
    
    $body = @{ name = "Updated List $script:Timestamp" }
    $result = Invoke-ApiRequest -Method "PUT" -Path "/users/$script:TestUserId/lists/$script:TestListId" -Body $body
    Write-TestResult "Update Todo List" $result.Success
    return $result.Success
}

# ==================== TODO TASK TESTS ====================

function Test-CreateTodoTask {
    if (-not $script:TestUserId -or -not $script:TestListId) {
        Write-TestResult "Create Todo Task" $false "No test user/list created"
        return $false
    }
    
    $body = @{
        description = "Test Task $script:Timestamp"
        completed = $false
        order = 1
    }
    
    $result = Invoke-ApiRequest -Method "POST" -Path "/users/$script:TestUserId/lists/$script:TestListId/tasks" -Body $body
    
    if ($result.Success -and $result.Data.taskId) {
        $script:TestTaskId = $result.Data.taskId
        Write-TestResult "Create Todo Task" $true
        return $true
    }
    Write-TestResult "Create Todo Task" $false $result.Error
    return $false
}

function Test-ListTodoTasks {
    if (-not $script:TestUserId -or -not $script:TestListId) {
        Write-TestResult "List Todo Tasks" $false "No test user/list created"
        return $false
    }
    
    $result = Invoke-ApiRequest -Method "GET" -Path "/users/$script:TestUserId/lists/$script:TestListId/tasks"
    
    if ($result.Success -and $null -ne $result.Data.tasks) {
        Write-TestResult "List Todo Tasks" $true
        return $true
    }
    Write-TestResult "List Todo Tasks" $false $result.Error
    return $false
}

function Test-GetTodoTask {
    if (-not $script:TestUserId -or -not $script:TestListId -or -not $script:TestTaskId) {
        Write-TestResult "Get Todo Task" $false "No test user/list/task created"
        return $false
    }
    
    $result = Invoke-ApiRequest -Method "GET" -Path "/users/$script:TestUserId/lists/$script:TestListId/tasks/$script:TestTaskId"
    
    if ($result.Success -and $result.Data.taskId) {
        Write-TestResult "Get Todo Task" $true
        return $true
    }
    Write-TestResult "Get Todo Task" $false $result.Error
    return $false
}

function Test-UpdateTodoTask {
    if (-not $script:TestUserId -or -not $script:TestListId -or -not $script:TestTaskId) {
        Write-TestResult "Update Todo Task" $false "No test user/list/task created"
        return $false
    }
    
    $body = @{ completed = $true }
    $result = Invoke-ApiRequest -Method "PUT" -Path "/users/$script:TestUserId/lists/$script:TestListId/tasks/$script:TestTaskId" -Body $body
    Write-TestResult "Update Todo Task" $result.Success
    return $result.Success
}

function Test-ReorderTasks {
    if (-not $script:TestUserId -or -not $script:TestListId -or -not $script:TestTaskId) {
        Write-TestResult "Reorder Tasks" $false "No test user/list/task created"
        return $false
    }
    
    $body = @{
        taskOrders = @(
            @{ taskId = $script:TestTaskId; order = 10 }
        )
    }
    $result = Invoke-ApiRequest -Method "PUT" -Path "/users/$script:TestUserId/lists/$script:TestListId/tasks/reorder" -Body $body
    Write-TestResult "Reorder Tasks" $result.Success
    return $result.Success
}

# ==================== CLEANUP TESTS ====================

function Test-DeleteTodoTask {
    if (-not $script:TestUserId -or -not $script:TestListId -or -not $script:TestTaskId) {
        Write-TestResult "Delete Todo Task" $false "No test user/list/task created"
        return $false
    }
    
    $result = Invoke-ApiRequest -Method "DELETE" -Path "/users/$script:TestUserId/lists/$script:TestListId/tasks/$script:TestTaskId"
    Write-TestResult "Delete Todo Task" $result.Success
    return $result.Success
}

function Test-DeleteTodoList {
    if (-not $script:TestUserId -or -not $script:TestListId) {
        Write-TestResult "Delete Todo List" $false "No test user/list created"
        return $false
    }
    
    $result = Invoke-ApiRequest -Method "DELETE" -Path "/users/$script:TestUserId/lists/$script:TestListId"
    Write-TestResult "Delete Todo List" $result.Success
    return $result.Success
}

function Test-DeleteUser {
    if (-not $script:TestUserId) {
        Write-TestResult "Delete User" $false "No test user created"
        return $false
    }
    
    $result = Invoke-ApiRequest -Method "DELETE" -Path "/users/$script:TestUserId"
    Write-TestResult "Delete User" $result.Success
    return $result.Success
}

# ==================== MAIN ====================

function Main {
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor White
    Write-Host "TodoApp API Test Suite (PowerShell)" -ForegroundColor White
    Write-Host "Base URL: $BaseUrl" -ForegroundColor White
    Write-Host "============================================================" -ForegroundColor White
    Write-Host ""

    # Check server connectivity
    Write-ColorOutput "Checking server connectivity..." "Cyan"
    try {
        $null = Invoke-WebRequest -Uri "$BaseUrl/users" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        Write-ColorOutput "Server is reachable" "Green"
    }
    catch {
        Write-ColorOutput "Cannot connect to server at $BaseUrl" "Red"
        Write-ColorOutput "Make sure the backend is running" "Yellow"
        exit 1
    }
    Write-Host ""

    # User Tests
    Write-ColorOutput "▶ User Operations" "Cyan"
    Test-CreateUser | Out-Null
    Test-CreateUserValidationError | Out-Null
    Test-ListUsers | Out-Null
    Test-GetUser | Out-Null
    Test-GetUserNotFound | Out-Null
    Test-UpdateUser | Out-Null
    Write-Host ""

    # Todo List Tests
    Write-ColorOutput "▶ Todo List Operations" "Cyan"
    Test-CreateTodoList | Out-Null
    Test-ListTodoLists | Out-Null
    Test-GetTodoList | Out-Null
    Test-UpdateTodoList | Out-Null
    Write-Host ""

    # Todo Task Tests
    Write-ColorOutput "▶ Todo Task Operations" "Cyan"
    Test-CreateTodoTask | Out-Null
    Test-ListTodoTasks | Out-Null
    Test-GetTodoTask | Out-Null
    Test-UpdateTodoTask | Out-Null
    Test-ReorderTasks | Out-Null
    Write-Host ""

    # Cleanup Tests
    Write-ColorOutput "▶ Cleanup Operations" "Cyan"
    Test-DeleteTodoTask | Out-Null
    Test-DeleteTodoList | Out-Null
    Test-DeleteUser | Out-Null
    Write-Host ""

    # Summary
    $total = $script:Passed + $script:Failed
    Write-Host "============================================================" -ForegroundColor White
    Write-Host "Test Results: $($script:Passed)/$total passed" -ForegroundColor White
    
    if ($script:Failed -eq 0) {
        Write-Host "All tests passed! ✓" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "$($script:Failed) test(s) failed" -ForegroundColor Red
        exit 1
    }
}

Main
