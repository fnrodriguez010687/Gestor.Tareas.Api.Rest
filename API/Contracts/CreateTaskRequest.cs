using System.ComponentModel.DataAnnotations;
using Gestor.Tareas.Api.Rest.Domain.Enums;

namespace Gestor.Tareas.Api.Rest.API.Contracts;

public sealed record CreateTaskRequest(
    [property: Required, StringLength(200, MinimumLength = 1)] string Title,
    [property: StringLength(2000)] string? Description,
    TaskState Status,
    [property: Required, StringLength(120, MinimumLength = 1)] string Assignee,
    DateOnly DueDate);