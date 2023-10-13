namespace TimeTrackerBackend.Core.Contracts.Repository
{
    using TimeTrackerBackend.Core.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TimeTrackerBackend.Core.DataTransferObjects;

    public interface IWorkDayRepository : IRepository<WorkDay>
    {
        Task<WorkDayDto> GetDayForEmployee(Employee employee);
    }
}
