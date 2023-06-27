using TimeTrackerBackend.Core.Contracts.Repository;
using TimeTrackerBackend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeTrackerBackend.Persistence.Repository
{
    public class Repository<T> : IRepository<T>
    {
        protected ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public async Task AddAsync(T entity)
        {
            await AddAsync<T>(entity);
        }

        public async Task AddAsync<E>(E entity)
        {
            await _context.AddAsync(entity);
        }

        public virtual Task<T[]> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            var result = await _context.FindAsync(typeof(T), id);
            return (T)result;
        }

        public virtual async Task Remove(Guid id)
        {
            var result = await GetByIdAsync(id);
            _context.Remove(result);
        }

        public virtual async Task Update(T entity)
        {
            await Update<T>(entity);
        }

        public virtual async Task Update<E>(E entity)
        {
            await Task.Run(() =>
            {
                _context.Update(entity);
            });
        }
    }
}
