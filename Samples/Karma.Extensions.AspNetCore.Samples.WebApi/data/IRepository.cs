using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Karma.Extensions.AspNetCore.Samples.WebApi.Data
{
  public interface IRepository<T, TId> where T : class
  {
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
  }
}