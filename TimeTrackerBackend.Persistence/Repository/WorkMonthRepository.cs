
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
    using TimeTrackerBackend.Core.DataTransferObjects;

    public class WorkMonthRepository : Repository<WorkMonth>, IWorkMonthRepository
    {
        public WorkMonthRepository(ApplicationDbContext context) : base(context)
        {

        }
        override public Task<WorkMonth[]> GetAllAsync()
        {
            return _context.WorkMonths.ToArrayAsync();
        }

        private List<WorkDay>[] AddRecursive(int counter, int daysInMonth, DateTime date, List<WorkDay>[] workDays, List<WorkDay> workDaysAsQueue)
        {
            if(counter < daysInMonth)
            {
                if (workDays[counter] == null)
                {
                    workDays[counter] = new List<WorkDay>();
                        
                }
                if (workDaysAsQueue.FirstOrDefault() != null && workDaysAsQueue.FirstOrDefault().StartDate.Day == counter + 1)
                {
                    workDays[counter].Add(workDaysAsQueue.First());
                    workDaysAsQueue.Remove(workDaysAsQueue.First());
                    return AddRecursive(counter, daysInMonth, date, workDays, workDaysAsQueue);
                }
                else
                {
                    workDays[counter].Add(new WorkDay()
                    {
                        StartDate = new DateTime(date.Year, date.Month, counter + 1),
                        Id = Guid.Empty,
                        Stamps = new List<Stamp>(),
                        WorkedHours = 0,
                        WorkMonthId = Guid.Empty
                    });
                    counter = counter + 1;
                    return AddRecursive(counter, daysInMonth, date, workDays, workDaysAsQueue);
                }
            }

            return workDays;
        }

       
        public async Task<WorkMonthDto> GetByDate(DateTime date, string employeeId)
        {
            var workMonth = await _context.WorkMonths.Where(i => i.EmployeeId.Equals(employeeId)).Where(i => i.Date.Month.Equals(date.Month) && i.Date.Year.Equals(date.Year)).Include(i => i.WorkDays).FirstOrDefaultAsync();
            WorkMonthDto workMonthDto = new WorkMonthDto();
            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            if (workMonth != null)
            {
                workMonth.WorkDays = workMonth.WorkDays.OrderBy(i => i.StartDate).ToList();
                List<WorkDay> workDaysAsQueue = new List<WorkDay>(workMonth.WorkDays);
                List<WorkDay>[] workDays = new List<WorkDay>[daysInMonth];

                /*for (int i = 0; i < workDaysAsQueue.Count; i++)
                {
                    if (workDaysAsQueue.FirstOrDefault() != null && workDaysAsQueue.FirstOrDefault().StartDate.Day == i + 1)
                    {
                        workDays[i].Add(workDaysAsQueue.First());
                    }
                    else
                    {
                        workDays[i].Add(new WorkDay()
                        {
                            StartDate = new DateTime(date.Year, date.Month, i + 1),
                            Id = Guid.Empty,
                            Stamps = new List<Stamp>(),
                            WorkedHours = 0,
                            WorkMonthId = Guid.Empty
                        });
                    }
                }*/
                workDays = AddRecursive(0, daysInMonth, date, workDays, workDaysAsQueue);


                workMonthDto.WorkDays = workDays;
                workMonthDto.Date = workMonth.Date;
                workMonthDto.Employee = workMonth.Employee;
                workMonthDto.EmployeeId = workMonth.EmployeeId;
                workMonthDto.WorkedHours = workMonth.WorkedHours;
                //workMonth.WorkDays = workDays;
            }
            else
            {
                workMonthDto = new WorkMonthDto()
                {
                    WorkDays = new List<WorkDay>[daysInMonth]
                };
                for (int i = 0; i < workMonthDto.WorkDays.Count(); i++)
                {
                    workMonthDto.WorkDays[i] = new List<WorkDay>();
                }
            }

            foreach (var item in workMonthDto.WorkDays)
            {
                if(item != null && item.Count > 1)
                {
                    item.RemoveAll(i => i.Id.Equals(Guid.Empty));
                }
            }

       /*     foreach (var item in workMonthDto.WorkDays)
            {
                
                 //var workDayIndex = item.Select((workDay, index) => (workDay, index)).First(i => i.workDay.StartDate.Day.Equals(date.Day)).index;
                 var workDays = item;

                    var workDaysIncluded = new List<WorkDay>();
                    foreach (var workDayItem in workDays)
                    {
                        if (workDayItem.Id != Guid.Empty)
                        {
                            var workDayReturnItem = await _context.WorkDays.Where(i => i.Id.Equals(workDayItem.Id)).Include(i => i.Stamps).FirstOrDefaultAsync();
                            workDaysIncluded.Add(workDayReturnItem);
                        }
                    }

                if(!item.Any(i => i.Id.Equals(Guid.Empty)))
                {
                    var index = workMonthDto.WorkDays.Select((workDay, index) => (workDay, index)).First(i => i.Equals(item)).index;
                    workMonthDto.WorkDays.ToArray()[index] = workDaysIncluded;
                }
                
            }*/

            for (int i = 0; i < workMonthDto.WorkDays.Count(); i++)
            {
                //var workDayIndex = item.Select((workDay, index) => (workDay, index)).First(i => i.workDay.StartDate.Day.Equals(date.Day)).index;
                var workDays = workMonthDto.WorkDays[i];

                if(workDays != null) {
                    var workDaysIncluded = new List<WorkDay>();
                    foreach (var workDayItem in workDays)
                    {
                        if (workDayItem.Id != Guid.Empty)
                        {
                            var workDayReturnItem = await _context.WorkDays.Where(i => i.Id.Equals(workDayItem.Id)).Include(i => i.Stamps).FirstOrDefaultAsync();
                            workDaysIncluded.Add(workDayReturnItem);
                        }
                    }

                    if (!workMonthDto.WorkDays[i].Any(i => i.Id.Equals(Guid.Empty)))
                    {
                        workMonthDto.WorkDays.ToArray()[i] = workDaysIncluded;
                    }
                }
            }

            /*if(workMonthDto.WorkDays.Length > 0)
            {
                var workDayIndex = workMonth.WorkDays.Select((workDay, index) => (workDay, index)).First(i => i.workDay.StartDate.Day.Equals(date.Day)).index;
                var workDay = workMonth.WorkDays.ToArray()[workDayIndex];
                if (workDay.Id != Guid.Empty)
                {
                    workDay = await _context.WorkDays.Where(i => i.Id.Equals(workDay.Id)).Include(i => i.Stamps).FirstOrDefaultAsync();
                }
                workMonth.WorkDays.ToArray()[workDayIndex] = workDay;
            }*/
            return workMonthDto;
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
