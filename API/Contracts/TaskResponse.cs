using Gestor.Tareas.Api.Rest.Domain.Entities;
using Gestor.Tareas.Api.Rest.Domain.Enums;

namespace Gestor.Tareas.Api.Rest.API.Contracts;

public sealed record TaskResponse(
    int Id,
    string Title,
    string? Description,
    TaskState Status,
    string StatusName,
    string Assignee,
    DateOnly DueDate)
{
    public static TaskResponse FromEntity(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.Status.ToString(),
        task.Assignee,
        task.DueDate);
}