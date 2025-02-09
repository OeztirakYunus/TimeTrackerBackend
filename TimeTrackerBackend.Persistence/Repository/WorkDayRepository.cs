
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
    using TimeTrackerBackend.Core.DataTransferObjects;

    public class WorkDayRepository : Repository<WorkDay>, IWorkDayRepository
    {
        public WorkDayRepository(ApplicationDbContext context) : base(context)
        {

        }
        override public Task<WorkDay[]> GetAllAsync()
        {
            return _context.WorkDays.ToArrayAsync();
        }
        public async Task<WorkDayDto> GetDayForEmployee(Employee employee)
        {
            var currentDate = DateTime.Now.ToUniversalTime();
            var workMonths = await _context.WorkMonths.Where(i => i.EmployeeId.Equals(employee.Id)).Include(i => i.WorkDays).ToArrayAsync();
            var workMonth = workMonths.Where(i => i.Date.Month.Equals(currentDate.Month) && i.Date.Year.Equals(currentDate.Year)).FirstOrDefault();
            var workDay = new WorkDay();
            var vacationDay = false;
            var illDay = false;

            if(workMonth != null)
            {
                workDay = workMonth.WorkDays.Where(i => i.Status == Core.Enums.Status.OPEN).FirstOrDefault();
                var vacation = await _context.Vacations.Where(i => i.EmployeeId == employee.Id).Where(i => i.StartDate.Date <= currentDate.Date && i.EndDate.Date >= currentDate.Date).Where(i => i.Status == Core.Enums.TypeOfVacation.Bestaetigt).FirstOrDefaultAsync();
                var illness = await _context.NotificationOfIllness.Where(i => i.EmployeeId == employee.Id).Where(i => i.StartDate.Date <= currentDate.Date && i.EndDate.Date >= currentDate.Date).Where(i => i.IsConfirmed).FirstOrDefaultAsync();

                if (vacation != null)
                {
                    vacationDay = true;
                }

                if (illness != null)
                {
                    illDay = true;
                }

                if (workDay == null)
                {
                    return WorkDayEntityToDto(new WorkDay(), vacationDay, illDay);
                }
                workDay = await _context.WorkDays.Where(i => i.Id == workDay.Id).Include(i => i.Stamps).FirstOrDefaultAsync();
                //workDay = await _context.WorkDays.Where(i => i.StartDate.Day == currentDate.Day).Include(i => i.Stamps).FirstOrDefaultAsync();

            }
            return WorkDayEntityToDto(workDay, vacationDay, illDay);
        }

        private WorkDayDto WorkDayEntityToDto(WorkDay workDay, bool vacationDay, bool illDay)
        {
            WorkDayDto workDayDto = new WorkDayDto
            {
                Id = workDay.Id,
                Stamps = workDay.Stamps,
                EndDate = workDay.EndDate,
                StartDate = workDay.StartDate,
                Status = workDay.Status,
                BreakHours = workDay.BreakHours,
                VacationDay = vacationDay,
                WorkedHours = workDay.WorkedHours,
                WorkMonth = workDay.WorkMonth,
                WorkMonthId = workDay.WorkMonthId,
                IllDay = illDay
            };
            return workDayDto;
        }
    }
}
