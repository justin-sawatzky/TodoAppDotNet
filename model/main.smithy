$version: "2"

metadata suppressions = [
    {
        id: "MemberShouldReferenceResource",
        namespace: "jsawatzky.todoapp.resources"
    }
]

namespace jsawatzky.todoapp

use aws.protocols#restJson1
use jsawatzky.todoapp.resources#User
use jsawatzky.todoapp.resources#TodoList
use jsawatzky.todoapp.resources#TodoTask

/// A TODO list application service that manages users, todo lists, and tasks.
@restJson1
service TodoAppDotNet {
    version: "2026-01-09",
    resources: [User, TodoList, TodoTask]
}