$version: "2"
namespace example.todoapp

use aws.protocols#restJson1
use example.todoapp.resources#User
use example.todoapp.resources#TodoList
use example.todoapp.resources#TodoTask

/// A TODO list application service that manages users, todo lists, and tasks.
@restJson1
service TodoAppDotNet {
    version: "2026-01-09",
    resources: [User, TodoList, TodoTask]
}