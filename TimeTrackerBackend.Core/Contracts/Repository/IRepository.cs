using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerBackend.Core.Contracts.Repository
{
    public interface IRepository<T>
    {
        Task<T> GetByIdAsync(Guid id);
        Task<T[]> GetAllAsync();
        Task AddAsync(T entity);
        Task AddAsync<E>(E entity);
        Task Update(T entity);
        Task Update<E>(E entity);
        Task Remove(Guid id);
    }
}
