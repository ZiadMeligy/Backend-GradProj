using System;
using GP_Server.Domain.Interfaces;
using GP_Server.Infrastructure.Repositories;

namespace GP_Server.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<T> GetRepository<T>() where T : class
        => new Repository<T>(_context);

    public async Task<int> CommitAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
        => _context.Dispose();
}
