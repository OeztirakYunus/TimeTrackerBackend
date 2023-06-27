namespace TimeTrackerBackend.Core.Contracts.Repository
{
    using TimeTrackerBackend.Core.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IWorkMonthRepository : IRepository<WorkMonth>
    {
        Task<WorkMonth> GetByDate(DateTime date, string employeeId);
        Task<WorkMonth[]> GetByEmployeeId(string employeeId);
    }
}
