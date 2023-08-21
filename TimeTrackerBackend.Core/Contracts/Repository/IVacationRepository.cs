
namespace TimeTrackerBackend.Contracts.Repository
{
    using TimeTrackerBackend.Core.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TimeTrackerBackend.Core.Contracts.Repository;

    public interface IVacationRepository : IRepository<Vacation>
    {
        public Task<Vacation[]> GetAllAsyncByEmployeeId(string id);
        public Task<Vacation[]> GetAllAsyncForCompany(Guid companyId);
        public Task ConfirmVacation(Vacation vacation);
        public Task RejectVacation(Vacation vacationId);
    }
}
