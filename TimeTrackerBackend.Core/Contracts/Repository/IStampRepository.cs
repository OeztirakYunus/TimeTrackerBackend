namespace TimeTrackerBackend.Core.Contracts.Repository
{
    using TimeTrackerBackend.Core.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IStampRepository : IRepository<Stamp>
    {
        Task<WorkDay> StampAsync(Employee employee);
        Task<WorkDay> TakeABreakAsync(Employee employee);
        Task<Stamp[]> GetForDayForEmployee(Employee employee);
    }
}
