namespace TimeTrackerBackend.Core.Contracts.Repository
{
    using TimeTrackerBackend.Core.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IWorkDayRepository : IRepository<WorkDay>
    {
        Task<WorkDay> GetDayForEmployee(Employee employee);
    }
}
