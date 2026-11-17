using Gestor.Tareas.Api.Rest.Application.Tasks;

namespace Gestor.Tareas.Api.Rest.Infrastructure.Data;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}