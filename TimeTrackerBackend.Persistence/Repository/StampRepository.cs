
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

        public override async Task Update(Stamp entityToUpdate)
        {
            var entity = await GetByIdAsync(entityToUpdate.Id);
            if (entity == null)
            {
                throw new ArgumentNullException("entityToUpdate");
            }

            var prevTime = entity.Time;
            entity.Time = entityToUpdate.Time;
            entity.Time = entity.Time.AddHours(2); //Aufgrund eines Fehlers, muss 2h hinzugefügt werden.

            if (entity.TypeOfStamp == TypeOfStamp.Dienstbeginn || entity.TypeOfStamp == TypeOfStamp.Dienstende)
            {
                var workDay = await _context.WorkDays.FindAsync(entity.WorkDayId);

                switch (entity.TypeOfStamp)
                {
                    case TypeOfStamp.Dienstbeginn:
                        workDay.StartDate = entity.Time;
                        break;
                    case TypeOfStamp.Dienstende:
                        workDay.EndDate = entity.Time;
                        break;
                    default:
                        break;
                }

                await Update(workDay);
            }

            var allStampsOfDay = await _context.Stamps.Where(i => i.WorkDayId == entity.WorkDayId && i.Id != entity.Id).ToArrayAsync();

            if (entity.TypeOfStamp == TypeOfStamp.Dienstbeginn && allStampsOfDay.Any(i => entity.Time > i.Time))
            {
                throw new Exception("Die Uhrzeit von Dienstbeginn darf nicht nach der Uhrzeit von Dienstende und Pausen liegen.");
            }
            else if (entity.TypeOfStamp == TypeOfStamp.Dienstende && allStampsOfDay.Any(i => entity.Time < i.Time))
            {
                throw new Exception("Die Uhrzeit von Dienstende muss nach der Uhrzeit von Dienstbeginn und Pausen liegen.");
            }
            //else if (entity.TypeOfStamp == TypeOfStamp.Pause)
            //{
            //    var nearestBreakEnd = allStampsOfDay.Where(i => i.TypeOfStamp == TypeOfStamp.PauseEnde).OrderBy(x => x.Time - prevTime).First();

            //    if (allStampsOfDay.Where(i => i.TypeOfStamp == TypeOfStamp.Dienstbeginn).Any(i => entity.Time < i.Time))
            //    {
            //        throw new Exception("Die Uhrzeit von der Pause muss nach der Uhrzeit von Dienstbeginn liegen.");
            //    }
            //    else if (allStampsOfDay.Where(i => i.TypeOfStamp != TypeOfStamp.Dienstende).Any(i => entity.Time > i.Time))
            //    {
            //        throw new Exception("Die Uhrzeit von der Pause muss vor der Uhrzeit von Dienstende und Pausenende liegen.");
            //    }

            //}
            //else if (entity.TypeOfStamp == TypeOfStamp.PauseEnde)
            //{
            //    if (allStampsOfDay.Where(i => i.TypeOfStamp == TypeOfStamp.Pause).Any(i => entity.Time < i.Time))
            //    {
            //        throw new Exception("Die Uhrzeit von der Pausenende muss nach der Uhrzeit von Pausenbeginn liegen.");
            //    }
            //    else if (allStampsOfDay.Where(i => i.TypeOfStamp != TypeOfStamp.Dienstbeginn).Any(i => entity.Time > i.Time))
            //    {
            //        throw new Exception("Die Uhrzeit von der Pause muss vor der Uhrzeit von Dienstende und Pausenende liegen.");
            //    }
            //}
            await base.Update(entity);
        }

        public async Task<Stamp[]> GetForDayForEmployee(Employee employee)
        {
            var emptyStamps = new List<Stamp>();

            var currentDate = DateTime.Now;
            var workMonths = await _context.WorkMonths.Where(i => i.EmployeeId.Equals(employee.Id)).Include(i => i.WorkDays).ToArrayAsync();
            if (workMonths == null)
            {
                return emptyStamps.ToArray();
            }

            var workMonth = workMonths.Where(i => i.Date.Month.Equals(currentDate.Month) && i.Date.Year.Equals(currentDate.Year)).FirstOrDefault();
            if (workMonth == null)
            {
                return emptyStamps.ToArray();
            }

            var workDay = workMonth.WorkDays.Where(i => i.Status == Status.OPEN).FirstOrDefault();
            if (workDay == null)
            {
                return emptyStamps.ToArray();
            }

            var stamps = await _context.Stamps.Where(i => i.WorkDayId == workDay.Id).ToArrayAsync();

            return stamps;
        }

        private Stamp CreateStamp(TypeOfStamp typeOfStamp, DateTime currentDate, Guid? workDayId)
        {
            Stamp newStamp = new Stamp()
            {
                TypeOfStamp = typeOfStamp,
                Time = currentDate
            };

            if (workDayId != null)
            {
                newStamp.WorkDayId = workDayId;
            }

            return newStamp;
        }

        private WorkDay CreateWorkDayWithStamp(DateTime currentDate, Guid? workMonthId)
        {
            WorkDay newWorkDay = new WorkDay()
            {
                StartDate = currentDate,
                Status = Status.OPEN,
                WorkedHours = 0,
                Stamps = new List<Stamp>()
                        {
                            CreateStamp(TypeOfStamp.Dienstbeginn, currentDate, null)
                        }
            };

            if (workMonthId != null)
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
            if (workMonth == null) throw new Exception("Kein Dienstbeginn vorhanden.");
            WorkDay workDay = workMonth.WorkDays.Where(i => i.Status == Status.OPEN).FirstOrDefault();
            workDay = await _context.WorkDays.Where(i => i.Id == workDay.Id).Include(i => i.Stamps).FirstOrDefaultAsync();
            if (workDay != null)
            {
                if(workDay.Stamps == null || workDay.Stamps.Count <= 0)
                {
                    if (workMonth == null) throw new Exception("Kein Dienstbeginn vorhanden");
                }
                
                if (workDay.Stamps.Any())
                {
                    var stamps = workDay.Stamps.OrderByDescending(i => i.Time);
                    var breakType = TypeOfStamp.Pause;
                    foreach (var item in stamps)
                    {
                        if (item.TypeOfStamp == TypeOfStamp.Pause)
                        {
                            breakType = TypeOfStamp.PauseEnde;
                            break;
                        }
                        else if (item.TypeOfStamp == TypeOfStamp.PauseEnde)
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
            throw new Exception("Kein Dienstbeginn vorhanden.");
        }

        public async Task<WorkDay> StampAsync(Employee employee)
        {
            DateTime currentDate = DateTime.Now;
            var workMonth = await _context.WorkMonths.Where(i => i.EmployeeId.Equals(employee.Id)).Where(i => i.Date.Year.Equals(currentDate.Year) && i.Date.Month.Equals(currentDate.Month)).Include(i => i.WorkDays).FirstOrDefaultAsync();
            var newReturnWorkDay = new WorkDay();

            var vacationDay = _context.Vacations.Where(i => i.EmployeeId == employee.Id).Where(i => i.Status == TypeOfVacation.Bestaetigt).Any(i => i.StartDate.Date <= currentDate.Date && i.EndDate.Date >= currentDate.Date);
            var illDay = _context.NotificationOfIllness.Where(i => i.EmployeeId == employee.Id).Where(i => i.IsConfirmed).Any(i => i.StartDate.Date <= currentDate.Date && i.EndDate.Date >= currentDate.Date);

            if (vacationDay)
            {
                throw new Exception("Sie haben momentan Urlaub und können daher nicht Stempeln.");
            }
            else if(illDay)
            {
                throw new Exception("Sie sind momentan Krank gemeldet und können daher nicht Stempeln.");
            }

            if (workMonth != null)
            {
                var workDay = workMonth.WorkDays.OrderByDescending(i => i.EndDate).FirstOrDefault();
                if (workDay != null && (currentDate - workDay.EndDate).TotalHours < 11)
                {
                    throw new Exception("Es muss zwischen zwei Arbeitstagen mindestens 11 Stunden liegen!");
                }

                workDay = workMonth.WorkDays.Where(i => i.Status == Status.OPEN).FirstOrDefault();
                if (workDay != null) //Dienstende
                {
                    workDay = await _context.WorkDays.Where(i => i.Id == workDay.Id).Include(i => i.Stamps).FirstAsync();
                    var breakCount = workDay.Stamps.Where(i => i.TypeOfStamp == TypeOfStamp.Pause).Count();
                    var breakEndCount = workDay.Stamps.Where(i => i.TypeOfStamp == TypeOfStamp.PauseEnde).Count();

                    if (breakCount != breakEndCount) throw new Exception("Dienst darf nicht vor Pausenende beendet werden!");

                    newReturnWorkDay = workDay;
                    Stamp newStamp = CreateStamp(TypeOfStamp.Dienstende, currentDate, workDay.Id);

                    workDay.Status = Status.CLOSED; 
                    workDay.EndDate = currentDate;
                    workDay.WorkedHours = 0;
                    workMonth.WorkedHours = 0;

                    await AddAsync(newStamp);
                    await Update(workMonth);
                    await Update(workDay);
                }
                else //Dienstbeginn
                {
                    newReturnWorkDay = CreateWorkDayWithStamp(currentDate, workMonth.Id);
                    await AddAsync(newReturnWorkDay);
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

