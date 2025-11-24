using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Karma.Extensions.AspNetCore.Samples.WebApi.Data
{

  internal class Repository<T, TId> : IRepository<T, TId> where T : class
  {
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
      _context = context;
      _dbSet = context.Set<T>();
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
      _ = await _dbSet.AddAsync(entity, cancellationToken);
      _ = await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
      // Use FindAsync to retrieve the entity first, then remove it
      T? entity = await _dbSet.FindAsync([id], cancellationToken);

      if (entity is not null)
      {
        _ = _dbSet.Remove(entity);
        _ = await _context.SaveChangesAsync(cancellationToken);
      }
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
      // Use AsNoTracking for read-only operations to improve performance
      await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default) =>
      _dbSet.FindAsync([id], cancellationToken).AsTask();

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
      _ = _dbSet.Update(entity);
      _ = await _context.SaveChangesAsync(cancellationToken);
    }
  }
}