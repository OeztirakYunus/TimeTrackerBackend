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
        Task<WorkDay> StampAsync(Employee employee, DateTime? dateTime = null);
        Task<WorkDay> TakeABreakAsync(Employee employee, DateTime? dateTime = null);
        Task<WorkDay> TakeABreakManuallyAsync(Employee employee, DateTime dateTime);
        Task<WorkDay> StampManuallyAsync(Employee employee, DateTime dateTime);
        Task<Stamp[]> GetForDayForEmployee(Employee employee);
    }
}
