namespace example.todoapp;

// Smithy use
// use aws.protocols#restJson1
// use example.todoapp.resources#User
// use example.todoapp.resources#TodoList
// use example.todoapp.resources#TodoTask

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

public interface ITodoAppDotNet
{
}

[ApiController]
[Route("api/todo-app-dot-net")]
public class TodoAppDotNetController : ControllerBase, ITodoAppDotNet
{
}
