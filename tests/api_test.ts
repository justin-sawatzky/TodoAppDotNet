#!/usr/bin/env npx tsx
/**
 * TodoApp API Test Script
 *
 * A cross-platform test script that validates the backend API endpoints.
 * Uses native fetch (Node.js 18+) - no external dependencies needed at runtime.
 *
 * Usage:
 *   npx tsx tests/api_test.ts [--base-url URL]
 *   npm test (from tests directory)
 *
 * Default base URL: http://localhost:5247
 */

// Colors for terminal output
const Colors = {
  green: "\x1b[92m",
  red: "\x1b[91m",
  yellow: "\x1b[93m",
  blue: "\x1b[94m",
  reset: "\x1b[0m",
  bold: "\x1b[1m",
} as const;

interface TestResult {
  success: boolean;
  data?: unknown;
  error?: string;
}

class ApiTester {
  private baseUrl: string;
  private passed = 0;
  private failed = 0;
  private testUserId: string | null = null;
  private testListId: string | null = null;
  private testTaskId: string | null = null;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl.replace(/\/$/, "");
  }

  private log(message: string, color: string = Colors.reset): void {
    console.log(`${color}${message}${Colors.reset}`);
  }

  private logTest(name: string, passed: boolean, details?: string): void {
    if (passed) {
      this.passed++;
      console.log(`  ${Colors.green}✓ PASS${Colors.reset} ${name}`);
    } else {
      this.failed++;
      console.log(`  ${Colors.red}✗ FAIL${Colors.reset} ${name}`);
      if (details) {
        console.log(`         ${Colors.yellow}${details}${Colors.reset}`);
      }
    }
  }

  private async request(
    method: string,
    path: string,
    data?: object,
    expectedStatus = 200
  ): Promise<TestResult> {
    const url = `${this.baseUrl}${path}`;
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
    };

    try {
      const options: RequestInit = {
        method,
        headers,
      };

      if (data) {
        options.body = JSON.stringify(data);
      }

      const response = await fetch(url, options);
      const statusCode = response.status;

      let responseData: unknown = null;
      const text = await response.text();
      if (text) {
        try {
          responseData = JSON.parse(text);
        } catch {
          responseData = text;
        }
      }

      if (statusCode === expectedStatus) {
        return { success: true, data: responseData };
      }

      return {
        success: false,
        error: `Expected ${expectedStatus}, got ${statusCode}: ${JSON.stringify(responseData)}`,
      };
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Unknown error";
      if (message.includes("ECONNREFUSED")) {
        return {
          success: false,
          error: "Connection refused - is the server running?",
        };
      }
      return { success: false, error: message };
    }
  }

  // ==================== USER TESTS ====================

  private async testCreateUser(): Promise<boolean> {
    const timestamp = Date.now();
    const data = {
      username: `testuser_${timestamp}`,
      email: `test_${timestamp}@example.com`,
    };

    const result = await this.request("POST", "/users", data);

    if (result.success && typeof result.data === "object" && result.data) {
      const userData = result.data as Record<string, unknown>;
      this.testUserId = userData.userId as string;
      const hasRequired = ["userId", "username", "email", "createdAt"].every(
        (k) => k in userData
      );
      this.logTest("Create User", hasRequired && !!this.testUserId);
      return hasRequired;
    }

    this.logTest("Create User", false, result.error);
    return false;
  }

  private async testCreateUserValidationError(): Promise<boolean> {
    const data = { username: "ab", email: "invalid-email" };
    const result = await this.request("POST", "/users", data, 400);
    this.logTest("Create User - Validation Error (400)", result.success);
    return result.success;
  }

  private async testListUsers(): Promise<boolean> {
    const result = await this.request("GET", "/users");

    if (
      result.success &&
      typeof result.data === "object" &&
      result.data &&
      "users" in result.data
    ) {
      this.logTest("List Users", true);
      return true;
    }

    this.logTest("List Users", false, result.error);
    return false;
  }

  private async testGetUser(): Promise<boolean> {
    if (!this.testUserId) {
      this.logTest("Get User", false, "No test user created");
      return false;
    }

    const result = await this.request("GET", `/users/${this.testUserId}`);

    if (result.success && typeof result.data === "object" && result.data) {
      const userData = result.data as Record<string, unknown>;
      const hasRequired = ["userId", "username", "email"].every(
        (k) => k in userData
      );
      this.logTest("Get User", hasRequired);
      return hasRequired;
    }

    this.logTest("Get User", false, result.error);
    return false;
  }

  private async testGetUserNotFound(): Promise<boolean> {
    const result = await this.request(
      "GET",
      "/users/nonexistent-user-id",
      undefined,
      404
    );
    this.logTest("Get User - Not Found (404)", result.success);
    return result.success;
  }

  private async testUpdateUser(): Promise<boolean> {
    if (!this.testUserId) {
      this.logTest("Update User", false, "No test user created");
      return false;
    }

    const data = { username: `updated_${Date.now()}` };
    const result = await this.request(
      "PUT",
      `/users/${this.testUserId}`,
      data
    );
    this.logTest("Update User", result.success);
    return result.success;
  }

  private async testUpdateUserEmailConflict(): Promise<boolean> {
    if (!this.testUserId) {
      this.logTest("Update User - Email Conflict (409)", false, "No test user created");
      return false;
    }

    // Create a second user to conflict with
    const timestamp = Date.now();
    const secondUserData = {
      username: `conflictuser_${timestamp}`,
      email: `conflict_${timestamp}@example.com`,
    };

    const createResult = await this.request("POST", "/users", secondUserData);
    if (!createResult.success || !createResult.data) {
      this.logTest("Update User - Email Conflict (409)", false, "Could not create second user");
      return false;
    }

    const secondUserId = (createResult.data as Record<string, unknown>).userId as string;
    const secondUserEmail = (createResult.data as Record<string, unknown>).email as string;

    // Try to update first user's email to second user's email
    const updateData = { email: secondUserEmail };
    const result = await this.request(
      "PUT",
      `/users/${this.testUserId}`,
      updateData,
      409
    );

    // Cleanup: delete the second user
    await this.request("DELETE", `/users/${secondUserId}`);

    this.logTest("Update User - Email Conflict (409)", result.success);
    return result.success;
  }

  private async testGetUserByEmail(): Promise<boolean> {
    if (!this.testUserId) {
      this.logTest("Get User By Email", false, "No test user created");
      return false;
    }

    const userResult = await this.request("GET", `/users/${this.testUserId}`);
    if (!userResult.success || !userResult.data) {
      this.logTest("Get User By Email", false, "Could not get user");
      return false;
    }

    const email = (userResult.data as Record<string, unknown>).email as string;
    const result = await this.request(
      "GET",
      `/users/lookup?email=${encodeURIComponent(email)}`
    );
    this.logTest("Get User By Email", result.success);
    return result.success;
  }

  // ==================== TODO LIST TESTS ====================

  private async testCreateTodoList(): Promise<boolean> {
    if (!this.testUserId) {
      this.logTest("Create Todo List", false, "No test user created");
      return false;
    }

    const data = {
      name: `Test List ${Date.now()}`,
      description: "A test todo list",
    };

    const result = await this.request(
      "POST",
      `/users/${this.testUserId}/lists`,
      data
    );

    if (result.success && typeof result.data === "object" && result.data) {
      const listData = result.data as Record<string, unknown>;
      this.testListId = listData.listId as string;
      const hasRequired = ["listId", "name", "userId"].every(
        (k) => k in listData
      );
      this.logTest("Create Todo List", hasRequired && !!this.testListId);
      return hasRequired;
    }

    this.logTest("Create Todo List", false, result.error);
    return false;
  }

  private async testListTodoLists(): Promise<boolean> {
    if (!this.testUserId) {
      this.logTest("List Todo Lists", false, "No test user created");
      return false;
    }

    const result = await this.request(
      "GET",
      `/users/${this.testUserId}/lists`
    );

    if (
      result.success &&
      typeof result.data === "object" &&
      result.data &&
      "lists" in result.data
    ) {
      this.logTest("List Todo Lists", true);
      return true;
    }

    this.logTest("List Todo Lists", false, result.error);
    return false;
  }

  private async testGetTodoList(): Promise<boolean> {
    if (!this.testUserId || !this.testListId) {
      this.logTest("Get Todo List", false, "No test user/list created");
      return false;
    }

    const result = await this.request(
      "GET",
      `/users/${this.testUserId}/lists/${this.testListId}`
    );

    if (result.success && typeof result.data === "object" && result.data) {
      const listData = result.data as Record<string, unknown>;
      const hasRequired = ["listId", "name", "userId"].every(
        (k) => k in listData
      );
      this.logTest("Get Todo List", hasRequired);
      return hasRequired;
    }

    this.logTest("Get Todo List", false, result.error);
    return false;
  }

  private async testUpdateTodoList(): Promise<boolean> {
    if (!this.testUserId || !this.testListId) {
      this.logTest("Update Todo List", false, "No test user/list created");
      return false;
    }

    const data = { name: `Updated List ${Date.now()}` };
    const result = await this.request(
      "PUT",
      `/users/${this.testUserId}/lists/${this.testListId}`,
      data
    );
    this.logTest("Update Todo List", result.success);
    return result.success;
  }

  // ==================== TODO TASK TESTS ====================

  private async testCreateTodoTask(): Promise<boolean> {
    if (!this.testUserId || !this.testListId) {
      this.logTest("Create Todo Task", false, "No test user/list created");
      return false;
    }

    const data = {
      description: `Test Task ${Date.now()}`,
      completed: false,
      order: 1,
    };

    const result = await this.request(
      "POST",
      `/users/${this.testUserId}/lists/${this.testListId}/tasks`,
      data
    );

    if (result.success && typeof result.data === "object" && result.data) {
      const taskData = result.data as Record<string, unknown>;
      this.testTaskId = taskData.taskId as string;
      const hasRequired = ["taskId", "description", "completed"].every(
        (k) => k in taskData
      );
      this.logTest("Create Todo Task", hasRequired && !!this.testTaskId);
      return hasRequired;
    }

    this.logTest("Create Todo Task", false, result.error);
    return false;
  }

  private async testListTodoTasks(): Promise<boolean> {
    if (!this.testUserId || !this.testListId) {
      this.logTest("List Todo Tasks", false, "No test user/list created");
      return false;
    }

    const result = await this.request(
      "GET",
      `/users/${this.testUserId}/lists/${this.testListId}/tasks`
    );

    if (
      result.success &&
      typeof result.data === "object" &&
      result.data &&
      "tasks" in result.data
    ) {
      this.logTest("List Todo Tasks", true);
      return true;
    }

    this.logTest("List Todo Tasks", false, result.error);
    return false;
  }

  private async testGetTodoTask(): Promise<boolean> {
    if (!this.testUserId || !this.testListId || !this.testTaskId) {
      this.logTest("Get Todo Task", false, "No test user/list/task created");
      return false;
    }

    const result = await this.request(
      "GET",
      `/users/${this.testUserId}/lists/${this.testListId}/tasks/${this.testTaskId}`
    );

    if (result.success && typeof result.data === "object" && result.data) {
      const taskData = result.data as Record<string, unknown>;
      const hasRequired = ["taskId", "description", "completed"].every(
        (k) => k in taskData
      );
      this.logTest("Get Todo Task", hasRequired);
      return hasRequired;
    }

    this.logTest("Get Todo Task", false, result.error);
    return false;
  }

  private async testUpdateTodoTask(): Promise<boolean> {
    if (!this.testUserId || !this.testListId || !this.testTaskId) {
      this.logTest("Update Todo Task", false, "No test user/list/task created");
      return false;
    }

    const data = { completed: true };
    const result = await this.request(
      "PUT",
      `/users/${this.testUserId}/lists/${this.testListId}/tasks/${this.testTaskId}`,
      data
    );
    this.logTest("Update Todo Task", result.success);
    return result.success;
  }

  private async testReorderTasks(): Promise<boolean> {
    if (!this.testUserId || !this.testListId || !this.testTaskId) {
      this.logTest("Reorder Tasks", false, "No test user/list/task created");
      return false;
    }

    const data = {
      taskOrders: [{ taskId: this.testTaskId, order: 10 }],
    };

    const result = await this.request(
      "PUT",
      `/users/${this.testUserId}/lists/${this.testListId}/tasks/reorder`,
      data
    );
    this.logTest("Reorder Tasks", result.success);
    return result.success;
  }

  // ==================== CLEANUP TESTS ====================

  private async testDeleteTodoTask(): Promise<boolean> {
    if (!this.testUserId || !this.testListId || !this.testTaskId) {
      this.logTest("Delete Todo Task", false, "No test user/list/task created");
      return false;
    }

    const result = await this.request(
      "DELETE",
      `/users/${this.testUserId}/lists/${this.testListId}/tasks/${this.testTaskId}`
    );
    this.logTest("Delete Todo Task", result.success);
    return result.success;
  }

  private async testDeleteTodoList(): Promise<boolean> {
    if (!this.testUserId || !this.testListId) {
      this.logTest("Delete Todo List", false, "No test user/list created");
      return false;
    }

    const result = await this.request(
      "DELETE",
      `/users/${this.testUserId}/lists/${this.testListId}`
    );
    this.logTest("Delete Todo List", result.success);
    return result.success;
  }

  private async testDeleteUser(): Promise<boolean> {
    if (!this.testUserId) {
      this.logTest("Delete User", false, "No test user created");
      return false;
    }

    const result = await this.request("DELETE", `/users/${this.testUserId}`);
    this.logTest("Delete User", result.success);
    return result.success;
  }

  async runAllTests(): Promise<number> {
    console.log(`\n${Colors.bold}${"=".repeat(60)}${Colors.reset}`);
    console.log(`${Colors.bold}TodoApp API Test Suite${Colors.reset}`);
    console.log(`${Colors.bold}Base URL: ${this.baseUrl}${Colors.reset}`);
    console.log(`${Colors.bold}${"=".repeat(60)}${Colors.reset}\n`);

    // Check server connectivity
    this.log("Checking server connectivity...", Colors.blue);
    try {
      await fetch(`${this.baseUrl}/users`, { method: "GET" });
      this.log("Server is reachable\n", Colors.green);
    } catch {
      this.log(`Cannot connect to server at ${this.baseUrl}`, Colors.red);
      this.log("Make sure the backend is running", Colors.yellow);
      process.exit(1);
    }

    // User Tests
    this.log("▶ User Operations", Colors.blue);
    await this.testCreateUser();
    await this.testCreateUserValidationError();
    await this.testListUsers();
    await this.testGetUser();
    await this.testGetUserNotFound();
    await this.testUpdateUser();
    await this.testUpdateUserEmailConflict();
    await this.testGetUserByEmail();
    console.log();

    // Todo List Tests
    this.log("▶ Todo List Operations", Colors.blue);
    await this.testCreateTodoList();
    await this.testListTodoLists();
    await this.testGetTodoList();
    await this.testUpdateTodoList();
    console.log();

    // Todo Task Tests
    this.log("▶ Todo Task Operations", Colors.blue);
    await this.testCreateTodoTask();
    await this.testListTodoTasks();
    await this.testGetTodoTask();
    await this.testUpdateTodoTask();
    await this.testReorderTasks();
    console.log();

    // Cleanup Tests
    this.log("▶ Cleanup Operations", Colors.blue);
    await this.testDeleteTodoTask();
    await this.testDeleteTodoList();
    await this.testDeleteUser();
    console.log();

    // Summary
    const total = this.passed + this.failed;
    console.log(`${Colors.bold}${"=".repeat(60)}${Colors.reset}`);
    console.log(
      `${Colors.bold}Test Results: ${this.passed}/${total} passed${Colors.reset}`
    );

    if (this.failed === 0) {
      this.log("All tests passed! ✓", Colors.green);
      return 0;
    } else {
      this.log(`${this.failed} test(s) failed`, Colors.red);
      return 1;
    }
  }
}

// Parse command line arguments
function parseArgs(): string {
  const args = process.argv.slice(2);
  let baseUrl = "http://localhost:5247";

  for (let i = 0; i < args.length; i++) {
    if (args[i] === "--base-url" && args[i + 1]) {
      baseUrl = args[i + 1];
      i++;
    }
  }

  return baseUrl;
}

// Main
const baseUrl = parseArgs();
const tester = new ApiTester(baseUrl);
tester.runAllTests().then((exitCode) => process.exit(exitCode));
