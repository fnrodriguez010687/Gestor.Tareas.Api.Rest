namespace Gestor.Tareas.Api.Rest.Application.Tasks;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}