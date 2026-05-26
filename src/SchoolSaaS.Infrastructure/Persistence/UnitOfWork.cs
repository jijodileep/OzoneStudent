using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Infrastructure.Persistence;

public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
