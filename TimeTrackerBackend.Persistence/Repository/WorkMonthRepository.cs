
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

        public async Task<WorkMonthDto> GetByDate(DateTime date, string employeeId)
        {
            var workMonth = await _context.WorkMonths.Where(i => i.EmployeeId.Equals(employeeId)).Where(i => i.Date.Month.Equals(date.Month) && i.Date.Year.Equals(date.Year)).Include(i => i.WorkDays).FirstOrDefaultAsync();
            return await GetAsDto(workMonth);
        }


        //Returns WorkMonth as DTO with Included Data
        public async Task<WorkMonthDto> GetAsDto(WorkMonth workMonth)
        {
            if(workMonth == null)
            {
                return new WorkMonthDto();
            }

            var date = workMonth.Date;
            var employeeId = workMonth.EmployeeId;
            workMonth = await _context.WorkMonths.Where(i => i.Id.Equals(workMonth.Id)).Include(i => i.WorkDays).FirstOrDefaultAsync();
            WorkMonthDto workMonthDto = new WorkMonthDto();
            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            if (workMonth != null)
            {
                workMonth.WorkDays = workMonth.WorkDays.OrderBy(i => i.StartDate).ToList();
                List<WorkDay> workDaysAsQueue = new List<WorkDay>(workMonth.WorkDays);
                List<WorkDay>[] workDays = new List<WorkDay>[daysInMonth];
                           
                workDays = AddRecursive(0, daysInMonth, date, workDays, workDaysAsQueue);
                var workDayDtos = await CreateWorkDayDto(workMonth, workDays, daysInMonth);
                

                workMonthDto.Id = workMonth.Id.ToString();
                workMonthDto.WorkDays = workDayDtos;
                workMonthDto.Date = workMonth.Date;
                workMonthDto.Employee = workMonth.Employee;
                workMonthDto.EmployeeId = workMonth.EmployeeId;
                workMonthDto.WorkedHours = workMonth.WorkedHours;
            }
            else //Create empty WorkMonthDto
            {
                var employee = await _context.Users.FindAsync(employeeId);
                if(employee == null)
                {
                    return new WorkMonthDto();
                }

                workMonthDto = new WorkMonthDto()
                {
                    EmployeeId = employeeId,
                    Employee = employee,
                    Date = date,
                    BreakHours = 0,
                    Id = Guid.Empty.ToString(),
                    WorkedHours = 0,
                    WorkDays = new List<WorkDayDto>[daysInMonth]
                };

                workMonthDto.WorkDays = await CreateEmptyWorkDay(workMonthDto.WorkDays, date, workMonthDto.EmployeeId);
                return workMonthDto;
            }

            //Delete empty WorkDays if WorkDay != null
            foreach (var item in workMonthDto.WorkDays)
            {
                if(item != null && item.Count > 1)
                {
                    item.RemoveAll(i => i.Id.Equals(Guid.Empty));
                }
            }

            workMonthDto = await IncludeStampsForDto(workMonthDto);
            workMonthDto = CalculateHours(workMonthDto);
         
            return workMonthDto;
        }

        //Creates List of WorkDay
        private List<WorkDay>[] AddRecursive(int counter, int daysInMonth, DateTime date, List<WorkDay>[] workDays, List<WorkDay> workDaysAsQueue)
        {
            if (counter < daysInMonth)
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

        private async Task<List<WorkDayDto>[]> CreateEmptyWorkDay(List<WorkDayDto>[] workDaysDto, DateTime date,string employeeId)
        {
            var vacations = await _context.Vacations.Where(i => i.EmployeeId == employeeId).Where(i => i.Status == Core.Enums.TypeOfVacation.Bestaetigt).ToListAsync();

            for (int i = 0; i < workDaysDto.Count(); i++)
            {
                workDaysDto[i] = new List<WorkDayDto>();
                WorkDayDto workDayDto = new WorkDayDto()
                {
                    Id = Guid.Empty,
                    StartDate = new DateTime(date.Year, date.Month, i + 1),
                    BreakHours = 0,
                    WorkedHours = 0,
                    EndDate = DateTime.MinValue
                };

                var vacationDay = vacations.Any(i => i.StartDate.Date <= workDayDto.StartDate.Date && i.EndDate.Date >= workDayDto.StartDate.Date);
                workDayDto.VacationDay = vacationDay;
                workDaysDto[i].Add(workDayDto);
            }

            return workDaysDto;
        }

        private async Task<List<WorkDayDto>[]> CreateWorkDayDto(WorkMonth workMonth, List<WorkDay>[] workDays, int daysInMonth)
        {
            List<WorkDayDto>[] workDayDtos = new List<WorkDayDto>[daysInMonth];
            var vacations = await _context.Vacations.Where(i => i.EmployeeId == workMonth.EmployeeId).Where(i => i.Status == Core.Enums.TypeOfVacation.Bestaetigt).ToListAsync();

            for (int i = 0; i < daysInMonth; i++)
            {
                workDayDtos[i] = new List<WorkDayDto>();
                foreach (var item in workDays[i])
                {
                    var vacationDay = vacations.Any(i => i.StartDate.Date <= item.StartDate.Date && i.EndDate.Date >= item.StartDate.Date);
                    workDayDtos[i].Add(WorkDayEntityToDto(item, vacationDay));
                }
            }

            return workDayDtos;
        }

        private async Task<WorkMonthDto> IncludeStampsForDto(WorkMonthDto workMonthDto)
        {
            for (int i = 0; i < workMonthDto.WorkDays.Count(); i++) //Include Stamps
            {
                //var workDayIndex = item.Select((workDay, index) => (workDay, index)).First(i => i.workDay.StartDate.Day.Equals(date.Day)).index;
                var workDays = workMonthDto.WorkDays[i];

                if (workDays != null)
                {
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
                        var workDaysIncludedDto = new List<WorkDay>();
                        var workDaysList = new WorkDayDto[workMonthDto.WorkDays.ToArray()[i].Count()];
                        Array.Copy(workMonthDto.WorkDays.ToArray()[i].ToArray(), workDaysList, workDaysList.Count());
                        // var workDaysList = workMonthDto.WorkDays.ToArray()[i];
                        workMonthDto.WorkDays.ToArray()[i].Clear();
                        for (int k = 0; k < workDaysIncluded.Count(); k++)
                        {
                            workMonthDto.WorkDays.ToArray()[i].Add(WorkDayEntityToDto(workDaysIncluded[k], workDaysList[k].VacationDay));
                        }
                    }
                }
            }

            return workMonthDto;
        }

        //Calculates WorkHours and BreakHours
        private WorkMonthDto CalculateHours(WorkMonthDto workMonthDto)
        {
            double workedHoursMonth = 0;
            double breakHoursMonth = 0;
            for (int i = 0; i < workMonthDto.WorkDays.Length; i++)
            {
                for (int j = 0; j < workMonthDto.WorkDays[i].Count; j++)
                {
                    double workedHoursDay = 0;
                    double breakHours = 0;
                    if (workMonthDto.WorkDays[i][j].Stamps != null && workMonthDto.WorkDays[i][j].Stamps.Count > 1)
                    {
                        if (workMonthDto.WorkDays[i][j].Stamps.Any(i => i.TypeOfStamp == Core.Enums.TypeOfStamp.Dienstbeginn) && workMonthDto.WorkDays[i][j].Stamps.Any(i => i.TypeOfStamp == Core.Enums.TypeOfStamp.Dienstende))
                        {
                            var startStampDateTime = workMonthDto.WorkDays[i][j].Stamps.Where(i => i.TypeOfStamp == Core.Enums.TypeOfStamp.Dienstbeginn).Select(i => i.Time).FirstOrDefault();
                            var endStampDateTime = workMonthDto.WorkDays[i][j].Stamps.Where(i => i.TypeOfStamp == Core.Enums.TypeOfStamp.Dienstende).Select(i => i.Time).FirstOrDefault();
                            workedHoursDay = (endStampDateTime - startStampDateTime).TotalHours;

                            Queue<Stamp> breakStamps = new Queue<Stamp>(workMonthDto.WorkDays[i][j].Stamps.Where(i => i.TypeOfStamp == Core.Enums.TypeOfStamp.Pause || i.TypeOfStamp == Core.Enums.TypeOfStamp.PauseEnde).OrderByDescending(i => i.Time));
                            for (int a = 0; a < breakStamps.Count; a++)
                            {
                                var breakEnd = breakStamps.Dequeue();
                                var breakBegin = breakStamps.Dequeue();

                                breakHours = (breakEnd.Time - breakBegin.Time).TotalHours;
                            }

                        }
                    }

                    workedHoursDay -= breakHours;
                    workedHoursMonth += workedHoursDay;
                    breakHoursMonth += breakHours;
                    workMonthDto.WorkDays[i][j].WorkedHours = workedHoursDay;
                    workMonthDto.WorkDays[i][j].BreakHours = breakHours;
                }
            }

            workMonthDto.BreakHours = breakHoursMonth;
            workMonthDto.WorkedHours = workedHoursMonth;

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

        private WorkDayDto WorkDayEntityToDto(WorkDay workDay, bool vacationDay)
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
                WorkMonthId = workDay.WorkMonthId
            };
            return workDayDto;
        }
    }
}
