using TimeTrackerBackend.Core.Contracts;
using TimeTrackerBackend.Core.Contracts.Repository;
using TimeTrackerBackend.Core.Entities;
using TimeTrackerBackend.Persistence.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerBackend.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed;

        public UnitOfWork() : this(new ApplicationDbContext())
        {
        }
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            //Alle Repos initialisieren. Format:
            //AddressRepository = new AddressRepository(_context);
            CompanyRepository = new CompanyRepository(_context);
            WorkMonthRepository = new WorkMonthRepository(_context);
            WorkDayRepository = new WorkDayRepository(_context);
            StampRepository = new StampRepository(_context);
        }

        //Alle Repos deklarieren. Format:
        //public IAddressRepository AddressRepository { get; }
        public ICompanyRepository CompanyRepository { get; }
        public IWorkDayRepository WorkDayRepository { get; }
        public IWorkMonthRepository WorkMonthRepository { get; }
        public IStampRepository StampRepository { get; }


        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
        public async Task DeleteDatabaseAsync() => await _context.Database.EnsureDeletedAsync();
        public async Task MigrateDatabaseAsync() => await _context.Database.MigrateAsync();
        public async Task CreateDatabaseAsync() => await _context.Database.EnsureCreatedAsync();

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    await _context.DisposeAsync();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
