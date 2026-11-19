using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Gestor.Tareas.Api.Rest.Tests;

public sealed class TaskApiIntegrationTests : IClassFixture<TaskApiFactory>
{
    private readonly TaskApiFactory factory;

    public TaskApiIntegrationTests(TaskApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task CreateTask_ReturnsCreated()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tasks", new
        {
            title = "Tarea de integración",
            description = "Prueba real contra PostgreSQL",
            status = 1,
            assignee = "QA",
            dueDate = "2026-12-31"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, document.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Pendiente", document.RootElement.GetProperty("statusName").GetString());
    }

    [Fact]
    public async Task GetTasks_WithInvalidStatus_ReturnsBadRequest()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks?status=99");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTask_WithMissingId_ReturnsNotFound()
    {
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
