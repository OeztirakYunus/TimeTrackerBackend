
namespace TimeTrackerBackend.Persistence.Repository
{
    using TimeTrackerBackend.Core.Contracts.Repository;
    using TimeTrackerBackend.Core.Entities;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class WorkDayRepository : Repository<WorkDay>, IWorkDayRepository
    {
        public WorkDayRepository(ApplicationDbContext context) : base(context)
        {

        }
        override public Task<WorkDay[]> GetAllAsync()
        {
            return _context.WorkDays.ToArrayAsync();
        }
        public async Task<WorkDay> GetDayForEmployee(Employee employee)
        {
            var currentDate = DateTime.Now;
            var workMonths = await _context.WorkMonths.Where(i => i.EmployeeId.Equals(employee.Id)).Include(i => i.WorkDays).ToArrayAsync();
            var workMonth = workMonths.Where(i => i.Date.Month.Equals(currentDate.Month) && i.Date.Year.Equals(currentDate.Year)).FirstOrDefault();
            var workDay = new WorkDay();
            if(workMonth != null)
            {
                workDay = workMonth.WorkDays.Where(i => i.Status == Core.Enums.Status.OPEN).FirstOrDefault();
                if(workDay == null) {
                    return new WorkDay();
                }
                workDay = await _context.WorkDays.Where(i => i.Id == workDay.Id).Include(i => i.Stamps).FirstOrDefaultAsync();
            }
            return workDay;
        }
    }
}
