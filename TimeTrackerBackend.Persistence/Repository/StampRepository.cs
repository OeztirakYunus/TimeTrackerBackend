
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
    using TimeTrackerBackend.Core.Enums;

    public class StampRepository : Repository<Stamp>, IStampRepository
    {
        public StampRepository(ApplicationDbContext context) : base(context)
        {

        }
        override public Task<Stamp[]> GetAllAsync()
        {
            return _context.Stamps.ToArrayAsync();
        }

        public async Task<Stamp[]> GetForDayForEmployee(Employee employee)
        {
            var currentDate = DateTime.Now;
            var workMonths = await _context.WorkMonths.Where(i => i.EmployeeId.Equals(employee.Id)).Include(i => i.WorkDays).ToArrayAsync();
            var workMonth = workMonths.Where(i => i.Date.Month.Equals(currentDate.Month) && i.Date.Year.Equals(currentDate.Year)).FirstOrDefault();
            var workDay = workMonth.WorkDays.Where(i => i.Date.Equals(currentDate.Date)).FirstOrDefault();
            var stamps = await _context.Stamps.Where(i => i.WorkDayId == workDay.Id).ToArrayAsync();

            return stamps;
        }

        private double GetWorkingHoursOfDay(Stamp firstStamp, Stamp lastStamp)
        {
            var workingHours = lastStamp.Time - firstStamp.Time;
            return workingHours.TotalHours;
        }
        private Stamp CreateStamp(TypeOfStamp typeOfStamp, DateTime currentDate, Guid? workDayId)
        {
            Stamp newStamp = new Stamp()
            {
                TypeOfStamp = typeOfStamp,
                Time = currentDate
            };

            if(workDayId != null)
            {
                newStamp.WorkDayId = workDayId;
            }

            return newStamp;
        }

        private WorkDay CreateWorkDayWithStamp(DateTime currentDate, Guid? workMonthId)
        {
            WorkDay newWorkDay = new WorkDay()
            {
                Date = currentDate,
                WorkedHours = 0,
                Stamps = new List<Stamp>()
                        {
                            CreateStamp(TypeOfStamp.Dienstbeginn, currentDate, null)
                        }
            };

            if(workMonthId != null)
            {
                newWorkDay.WorkMonthId = workMonthId;
            }

            return newWorkDay;
        }

        private WorkMonth CreateWorkMonthWithWorkDayAndStamp(ref WorkDay newReturnWorkDay, DateTime currentDate, string employeeId)
        {
            newReturnWorkDay = CreateWorkDayWithStamp(currentDate, null);

            WorkMonth newWorkMonth = new WorkMonth()
            {
                Date = currentDate,
                WorkedHours = 0,
                EmployeeId = employeeId,
                WorkDays = new List<WorkDay>() { newReturnWorkDay }
            };

            return newWorkMonth;
        }


        public async Task<WorkDay> TakeABreakAsync(Employee employee)
        {
            DateTime currentDate = DateTime.Now;
            var workMonth = await _context.WorkMonths.Where(i => i.EmployeeId.Equals(employee.Id)).Where(i => i.Date.Year.Equals(currentDate.Year) && i.Date.Month.Equals(currentDate.Month)).Include(i => i.WorkDays).FirstOrDefaultAsync();
            if(workMonth == null) throw new Exception("Kein Dienstbeginn vorhanden");
            WorkDay workDay = workMonth.WorkDays.Where(i => i.Date.Day.Equals(currentDate.Day)).FirstOrDefault();
            workDay = await _context.WorkDays.Where(i => i.Id == workDay.Id).Include(i => i.Stamps).FirstOrDefaultAsync();
            if(workDay != null)
            {
                if (workDay.Stamps.Any())
                {
                    var stamps = workDay.Stamps.OrderByDescending(i => i.Time);
                    var breakType = TypeOfStamp.Pause;
                    foreach (var item in stamps)
                    {
                        if(item.TypeOfStamp == TypeOfStamp.Pause)
                        {
                            breakType = TypeOfStamp.PauseEnde;
                            break;
                        }
                        else if(item.TypeOfStamp == TypeOfStamp.PauseEnde)
                        {
                            breakType = TypeOfStamp.Pause;
                            break;
                        }
                    }

                    Stamp newStamp = CreateStamp(breakType, currentDate, workDay.Id);
                    await AddAsync(newStamp);
                    workDay.Stamps.Add(newStamp);
                    return workDay;
                }
            }
            throw new Exception("Kein Dienstbeginn vorhanden");
        }

        public async Task<WorkDay> StampAsync(Employee employee)
        {
            DateTime currentDate = DateTime.Now;
            var workMonth = await _context.WorkMonths.Where(i => i.EmployeeId.Equals(employee.Id)).Where(i => i.Date.Year.Equals(currentDate.Year) && i.Date.Month.Equals(currentDate.Month)).Include(i => i.WorkDays).FirstOrDefaultAsync();
            var newReturnWorkDay = new WorkDay();

            if (workMonth != null)
            {
                var workDay = workMonth.WorkDays.Where(i => i.Date.Date.Equals(currentDate.Date)).FirstOrDefault();
                if (workDay != null) //Dienstende
                {
                    workDay = await _context.WorkDays.Where(i => i.Id == workDay.Id).Include(i => i.Stamps).FirstAsync();
                    var breakCount = workDay.Stamps.Where(i => i.TypeOfStamp == TypeOfStamp.Pause).Count();
                    var breakEndCount = workDay.Stamps.Where(i => i.TypeOfStamp == TypeOfStamp.PauseEnde).Count();

                    if (breakCount != breakEndCount) throw new Exception("Dienst darf nicht vor Pausenende beendet werden!");

                    //if(!workDay.Stamps.Any(i => i.TypeOfStamp == TypeOfStamp.Dienstende)) //Wenn schon Dienstende, kein Dienstende
                    //{
                    newReturnWorkDay = workDay;
                        Stamp newStamp = CreateStamp(TypeOfStamp.Dienstende, currentDate, workDay.Id);

                        var workingHours = GetWorkingHoursOfDay(workDay.Stamps.Where(i => i.TypeOfStamp == TypeOfStamp.Dienstbeginn).First(), newStamp);
                        workDay.WorkedHours = workingHours;
                        workMonth.WorkedHours += workingHours;
                        await AddAsync(newStamp);
                        await Update(workMonth);
                        await Update(workDay);
                    //}                   
                }   
                else //Dienstbeginn
                {
                    WorkDay newWorkDay = CreateWorkDayWithStamp(currentDate, workMonth.Id);
                    newReturnWorkDay = workDay;
                    await AddAsync(newWorkDay);
                }
            }
            else
            {
                WorkMonth newWorkMonth = CreateWorkMonthWithWorkDayAndStamp(ref newReturnWorkDay, currentDate, employee.Id);
                await AddAsync(newWorkMonth);
            }

            return newReturnWorkDay;
        }
    }
}

