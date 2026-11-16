namespace Gestor.Tareas.Api.Rest.Models;

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Pendiente;

    public string Assignee { get; set; } = string.Empty;

    public DateOnly DueDate { get; set; }
}