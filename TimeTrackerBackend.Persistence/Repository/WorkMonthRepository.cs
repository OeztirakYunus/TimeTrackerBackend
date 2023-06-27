
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
    using System.Collections;

    public class WorkMonthRepository : Repository<WorkMonth>, IWorkMonthRepository
    {
        public WorkMonthRepository(ApplicationDbContext context) : base(context)
        {

        }
        override public Task<WorkMonth[]> GetAllAsync()
        {
            return _context.WorkMonths.ToArrayAsync();
        }

        public async Task<WorkMonth> GetByDate(DateTime date, string employeeId)
        {
            var workMonth = await _context.WorkMonths.Where(i => i.EmployeeId.Equals(employeeId)).Where(i => i.Date.Month.Equals(date.Month) && i.Date.Year.Equals(date.Year)).Include(i => i.WorkDays).FirstOrDefaultAsync();
            if (workMonth != null)
            {
                workMonth.WorkDays = workMonth.WorkDays.OrderBy(i => i.Date).ToList();
                Queue<WorkDay> workDaysAsQueue = new Queue<WorkDay>(workMonth.WorkDays);
                var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                WorkDay[] workDays = new WorkDay[daysInMonth];

                for (int i = 0; i < workDays.Length; i++)
                {
                    if (workDaysAsQueue.FirstOrDefault() != null && workDaysAsQueue.FirstOrDefault().Date.Day == i + 1)
                    {
                        workDays[i] = workDaysAsQueue.Dequeue();
                    }
                    else
                    {
                        workDays[i] = new WorkDay()
                        {
                            Date = new DateTime(date.Year, date.Month, i + 1),
                            Id = Guid.Empty,
                            Stamps = new List<Stamp>(),
                            WorkedHours = 0,
                            WorkMonthId = Guid.Empty
                        };
                    }
                }

                workMonth.WorkDays = workDays;
            }
            else
            {
                workMonth = new WorkMonth()
                {
                    WorkDays = new List<WorkDay>()
                };
            }

            if(workMonth.WorkDays.Count > 0)
            {
                var workDayIndex = workMonth.WorkDays.Select((workDay, index) => (workDay, index)).First(i => i.workDay.Date.Day.Equals(date.Day)).index;
                var workDay = workMonth.WorkDays.ToArray()[workDayIndex];
                if (workDay.Id != Guid.Empty)
                {
                    workDay = await _context.WorkDays.Where(i => i.Id.Equals(workDay.Id)).Include(i => i.Stamps).FirstOrDefaultAsync();
                }
                workMonth.WorkDays.ToArray()[workDayIndex] = workDay;
            }
            return workMonth;
        }

        public override async Task Remove(Guid id)
        {
            var workMonth = await _context.WorkMonths.Where(i => i.Id.Equals(id)).Include(i => i.WorkDays).FirstOrDefaultAsync();
            var workDays = await _context.WorkDays.Where(i => i.WorkMonthId.Equals(workMonth.Id)).Include(i => i.Stamps).ToArrayAsync();
            foreach (var item in workDays)
            {
                _context.Stamps.RemoveRange(item.Stamps);
            }

            _context.WorkDays.RemoveRange(workMonth.WorkDays);
            _context.WorkMonths.Remove(workMonth);
        }

        public async Task<WorkMonth[]> GetByEmployeeId(string employeeId)
        {
            return await _context.WorkMonths.Where(i => employeeId == i.EmployeeId).ToArrayAsync();
        }
    }
}
