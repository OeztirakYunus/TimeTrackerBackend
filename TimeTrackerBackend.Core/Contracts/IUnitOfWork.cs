using TimeTrackerBackend.Core.Contracts.Repository;
using System;
using System.Threading.Tasks;

namespace TimeTrackerBackend.Core.Contracts
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        //Alle Repos deklarieren. Format:
        //public IAddressRepository AddressRepository { get; }

        public ICompanyRepository CompanyRepository { get; }
        public IWorkDayRepository WorkDayRepository { get; }
        public IWorkMonthRepository WorkMonthRepository { get; }
        public IStampRepository StampRepository { get; }

        Task<int> SaveChangesAsync();
        Task DeleteDatabaseAsync();
        Task MigrateDatabaseAsync();
        Task CreateDatabaseAsync();
    }

}
