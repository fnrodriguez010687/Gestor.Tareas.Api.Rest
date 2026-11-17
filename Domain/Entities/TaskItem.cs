using Gestor.Tareas.Api.Rest.Domain.Enums;

namespace Gestor.Tareas.Api.Rest.Domain.Entities;

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskState Status { get; set; } = TaskState.Pendiente;

    public string Assignee { get; set; } = string.Empty;

    public DateOnly DueDate { get; set; }
}