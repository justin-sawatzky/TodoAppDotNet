#!/bin/bash
#
# TodoApp API Test Script (Bash/curl version)
# Works on macOS, Linux, and Windows (Git Bash/WSL)
#
# Usage: ./tests/api_test.sh [BASE_URL]
# Default BASE_URL: http://localhost:5247
#

set -e

BASE_URL="${1:-http://localhost:5247}"
PASSED=0
FAILED=0
TEST_USER_ID=""
TEST_LIST_ID=""
TEST_TASK_ID=""
TIMESTAMP=$(date +%s)

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
BOLD='\033[1m'
NC='\033[0m' # No Color

log() {
    echo -e "$1"
}

log_test() {
    local name="$1"
    local passed="$2"
    local details="$3"
    
    if [ "$passed" = "true" ]; then
        ((PASSED++))
        echo -e "  ${GREEN}✓ PASS${NC} $name"
    else
        ((FAILED++))
        echo -e "  ${RED}✗ FAIL${NC} $name"
        if [ -n "$details" ]; then
            echo -e "         ${YELLOW}$details${NC}"
        fi
    fi
}

# Make HTTP request and capture response
# Usage: http_request METHOD PATH [DATA] [EXPECTED_STATUS]
http_request() {
    local method="$1"
    local path="$2"
    local data="$3"
    local expected_status="${4:-200}"
    
    local url="${BASE_URL}${path}"
    local response
    local http_code
    
    if [ -n "$data" ]; then
        response=$(curl -s -w "\n%{http_code}" -X "$method" \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$url" 2>/dev/null || echo -e "\n000")
    else
        response=$(curl -s -w "\n%{http_code}" -X "$method" \
            -H "Content-Type: application/json" \
            "$url" 2>/dev/null || echo -e "\n000")
    fi
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" = "$expected_status" ]; then
        echo "$body"
        return 0
    else
        echo "Expected $expected_status, got $http_code: $body"
        return 1
    fi
}

# Extract JSON field (simple extraction, works for flat JSON)
json_get() {
    local json="$1"
    local field="$2"
    echo "$json" | grep -o "\"$field\"[[:space:]]*:[[:space:]]*\"[^\"]*\"" | head -1 | sed 's/.*:.*"\([^"]*\)".*/\1/'
}

# ==================== USER TESTS ====================

test_create_user() {
    local data="{\"username\":\"testuser_${TIMESTAMP}\",\"email\":\"test_${TIMESTAMP}@example.com\"}"
    local result
    
    if result=$(http_request "POST" "/users" "$data"); then
        TEST_USER_ID=$(json_get "$result" "userId")
        if [ -n "$TEST_USER_ID" ]; then
            log_test "Create User" "true"
            return 0
        fi
    fi
    log_test "Create User" "false" "$result"
    return 1
}

test_create_user_validation_error() {
    local data='{"username":"ab","email":"invalid"}'
    
    if http_request "POST" "/users" "$data" "400" >/dev/null 2>&1; then
        log_test "Create User - Validation Error (400)" "true"
        return 0
    fi
    log_test "Create User - Validation Error (400)" "false"
    return 1
}

test_list_users() {
    local result
    
    if result=$(http_request "GET" "/users"); then
        if echo "$result" | grep -q '"users"'; then
            log_test "List Users" "true"
            return 0
        fi
    fi
    log_test "List Users" "false" "$result"
    return 1
}

test_get_user() {
    if [ -z "$TEST_USER_ID" ]; then
        log_test "Get User" "false" "No test user created"
        return 1
    fi
    
    local result
    if result=$(http_request "GET" "/users/${TEST_USER_ID}"); then
        if echo "$result" | grep -q '"userId"'; then
            log_test "Get User" "true"
            return 0
        fi
    fi
    log_test "Get User" "false" "$result"
    return 1
}

test_get_user_not_found() {
    if http_request "GET" "/users/nonexistent-user-id" "" "404" >/dev/null 2>&1; then
        log_test "Get User - Not Found (404)" "true"
        return 0
    fi
    log_test "Get User - Not Found (404)" "false"
    return 1
}

test_update_user() {
    if [ -z "$TEST_USER_ID" ]; then
        log_test "Update User" "false" "No test user created"
        return 1
    fi
    
    local data="{\"username\":\"updated_${TIMESTAMP}\"}"
    if http_request "PUT" "/users/${TEST_USER_ID}" "$data" >/dev/null 2>&1; then
        log_test "Update User" "true"
        return 0
    fi
    log_test "Update User" "false"
    return 1
}

# ==================== TODO LIST TESTS ====================

test_create_todo_list() {
    if [ -z "$TEST_USER_ID" ]; then
        log_test "Create Todo List" "false" "No test user created"
        return 1
    fi
    
    local data="{\"name\":\"Test List ${TIMESTAMP}\",\"description\":\"A test todo list\"}"
    local result
    
    if result=$(http_request "POST" "/users/${TEST_USER_ID}/lists" "$data"); then
        TEST_LIST_ID=$(json_get "$result" "listId")
        if [ -n "$TEST_LIST_ID" ]; then
            log_test "Create Todo List" "true"
            return 0
        fi
    fi
    log_test "Create Todo List" "false" "$result"
    return 1
}

test_list_todo_lists() {
    if [ -z "$TEST_USER_ID" ]; then
        log_test "List Todo Lists" "false" "No test user created"
        return 1
    fi
    
    local result
    if result=$(http_request "GET" "/users/${TEST_USER_ID}/lists"); then
        if echo "$result" | grep -q '"lists"'; then
            log_test "List Todo Lists" "true"
            return 0
        fi
    fi
    log_test "List Todo Lists" "false" "$result"
    return 1
}

test_get_todo_list() {
    if [ -z "$TEST_USER_ID" ] || [ -z "$TEST_LIST_ID" ]; then
        log_test "Get Todo List" "false" "No test user/list created"
        return 1
    fi
    
    local result
    if result=$(http_request "GET" "/users/${TEST_USER_ID}/lists/${TEST_LIST_ID}"); then
        if echo "$result" | grep -q '"listId"'; then
            log_test "Get Todo List" "true"
            return 0
        fi
    fi
    log_test "Get Todo List" "false" "$result"
    return 1
}

test_update_todo_list() {
    if [ -z "$TEST_USER_ID" ] || [ -z "$TEST_LIST_ID" ]; then
        log_test "Update Todo List" "false" "No test user/list created"
        return 1
    fi
    
    local data="{\"name\":\"Updated List ${TIMESTAMP}\"}"
    if http_request "PUT" "/users/${TEST_USER_ID}/lists/${TEST_LIST_ID}" "$data" >/dev/null 2>&1; then
        log_test "Update Todo List" "true"
        return 0
    fi
    log_test "Update Todo List" "false"
    return 1
}

# ==================== TODO TASK TESTS ====================

test_create_todo_task() {
    if [ -z "$TEST_USER_ID" ] || [ -z "$TEST_LIST_ID" ]; then
        log_test "Create Todo Task" "false" "No test user/list created"
        return 1
    fi
    
    local data="{\"description\":\"Test Task ${TIMESTAMP}\",\"completed\":false,\"order\":1}"
    local result
    
    if result=$(http_request "POST" "/users/${TEST_USER_ID}/lists/${TEST_LIST_ID}/tasks" "$data"); then
        TEST_TASK_ID=$(json_get "$result" "taskId")
        if [ -n "$TEST_TASK_ID" ]; then
            log_test "Create Todo Task" "true"
            return 0
        fi
    fi
    log_test "Create Todo Task" "false" "$result"
    return 1
}

test_list_todo_tasks() {
    if [ -z "$TEST_USER_ID" ] || [ -z "$TEST_LIST_ID" ]; then
        log_test "List Todo Tasks" "false" "No test user/list created"
        return 1
    fi
    
    local result
    if result=$(http_request "GET" "/users/${TEST_USER_ID}/lists/${TEST_LIST_ID}/tasks"); then
        if echo "$result" | grep -q '"tasks"'; then
            log_test "List Todo Tasks" "true"
            return 0
        fi
    fi
    log_test "List Todo Tasks" "false" "$result"
    return 1
}

test_get_todo_task() {
    if [ -z "$TEST_USER_ID" ] || [ -z "$TEST_LIST_ID" ] || [ -z "$TEST_TASK_ID" ]; then
        log_test "Get Todo Task" "false" "No test user/list/task created"
        return 1
    fi
    
    local result
    if result=$(http_request "GET" "/users/${TEST_USER_ID}/lists/${TEST_LIST_ID}/tasks/${TEST_TASK_ID}"); then
        if echo "$result" | grep -q '"taskId"'; then
            log_test "Get Todo Task" "true"
            return 0
        fi
    fi
    log_test "Get Todo Task" "false" "$result"
    return 1
}

test_update_todo_task() {
    if [ -z "$TEST_USER_ID" ] || [ -z "$TEST_LIST_ID" ] || [ -z "$TEST_TASK_ID" ]; then
        log_test "Update Todo Task" "false" "No test user/list/task created"
        return 1
    fi
    
    local data='{"completed":true}'
    if http_request "PUT" "/users/${TEST_USER_ID}/lists/${TEST_LIST_ID}/tasks/${TEST_TASK_ID}" "$data" >/dev/null 2>&1; then
        log_test "Update Todo Task" "true"
        return 0
    fi
    log_test "Update Todo Task" "false"
    return 1
}

test_reorder_tasks() {
    if [ -z "$TEST_USER_ID" ] || [ -z "$TEST_LIST_ID" ] || [ -z "$TEST_TASK_ID" ]; then
        log_test "Reorder Tasks" "false" "No test user/list/task created"
        return 1
    fi
    
    local data="{\"taskOrders\":[{\"taskId\":\"${TEST_TASK_ID}\",\"order\":10}]}"
    if http_request "PUT" "/users/${TEST_USER_ID}/lists/${TEST_LIST_ID}/tasks/reorder" "$data" >/dev/null 2>&1; then
        log_test "Reorder Tasks" "true"
        return 0
    fi
    log_test "Reorder Tasks" "false"
    return 1
}

# ==================== CLEANUP TESTS ====================

test_delete_todo_task() {
    if [ -z "$TEST_USER_ID" ] || [ -z "$TEST_LIST_ID" ] || [ -z "$TEST_TASK_ID" ]; then
        log_test "Delete Todo Task" "false" "No test user/list/task created"
        return 1
    fi
    
    if http_request "DELETE" "/users/${TEST_USER_ID}/lists/${TEST_LIST_ID}/tasks/${TEST_TASK_ID}" >/dev/null 2>&1; then
        log_test "Delete Todo Task" "true"
        return 0
    fi
    log_test "Delete Todo Task" "false"
    return 1
}

test_delete_todo_list() {
    if [ -z "$TEST_USER_ID" ] || [ -z "$TEST_LIST_ID" ]; then
        log_test "Delete Todo List" "false" "No test user/list created"
        return 1
    fi
    
    if http_request "DELETE" "/users/${TEST_USER_ID}/lists/${TEST_LIST_ID}" >/dev/null 2>&1; then
        log_test "Delete Todo List" "true"
        return 0
    fi
    log_test "Delete Todo List" "false"
    return 1
}

test_delete_user() {
    if [ -z "$TEST_USER_ID" ]; then
        log_test "Delete User" "false" "No test user created"
        return 1
    fi
    
    if http_request "DELETE" "/users/${TEST_USER_ID}" >/dev/null 2>&1; then
        log_test "Delete User" "true"
        return 0
    fi
    log_test "Delete User" "false"
    return 1
}

# ==================== MAIN ====================

main() {
    echo -e "\n${BOLD}============================================================${NC}"
    echo -e "${BOLD}TodoApp API Test Suite (Bash)${NC}"
    echo -e "${BOLD}Base URL: ${BASE_URL}${NC}"
    echo -e "${BOLD}============================================================${NC}\n"

    # Check server connectivity
    log "${BLUE}Checking server connectivity...${NC}"
    if ! curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}/users" | grep -q "200\|404"; then
        log "${RED}Cannot connect to server at ${BASE_URL}${NC}"
        log "${YELLOW}Make sure the backend is running${NC}"
        exit 1
    fi
    log "${GREEN}Server is reachable${NC}\n"

    # User Tests
    log "${BLUE}▶ User Operations${NC}"
    test_create_user || true
    test_create_user_validation_error || true
    test_list_users || true
    test_get_user || true
    test_get_user_not_found || true
    test_update_user || true
    echo

    # Todo List Tests
    log "${BLUE}▶ Todo List Operations${NC}"
    test_create_todo_list || true
    test_list_todo_lists || true
    test_get_todo_list || true
    test_update_todo_list || true
    echo

    # Todo Task Tests
    log "${BLUE}▶ Todo Task Operations${NC}"
    test_create_todo_task || true
    test_list_todo_tasks || true
    test_get_todo_task || true
    test_update_todo_task || true
    test_reorder_tasks || true
    echo

    # Cleanup Tests
    log "${BLUE}▶ Cleanup Operations${NC}"
    test_delete_todo_task || true
    test_delete_todo_list || true
    test_delete_user || true
    echo

    # Summary
    local total=$((PASSED + FAILED))
    echo -e "${BOLD}============================================================${NC}"
    echo -e "${BOLD}Test Results: ${PASSED}/${total} passed${NC}"
    
    if [ "$FAILED" -eq 0 ]; then
        echo -e "${GREEN}${BOLD}All tests passed! ✓${NC}"
        exit 0
    else
        echo -e "${RED}${BOLD}${FAILED} test(s) failed${NC}"
        exit 1
    fi
}

main
