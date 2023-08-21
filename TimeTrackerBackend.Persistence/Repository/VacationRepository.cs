
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
    using TimeTrackerBackend.Contracts.Repository;
    using DocumentFormat.OpenXml.Office2010.Excel;

    public class VacationRepository : Repository<Vacation>, IVacationRepository
    {
        public VacationRepository(ApplicationDbContext context) : base(context)
        {

        }
        override async public Task<Vacation[]> GetAllAsync()
        {
            return await _context.Vacations.ToArrayAsync();
        }
        public async Task<Vacation[]> GetAllAsyncByEmployeeId(string id)
        {
            return await _context.Vacations.Where(i => i.EmployeeId == id).Include(i => i.Employee).ToArrayAsync();
        }

        public async Task<Vacation[]> GetAllAsyncForCompany(Guid companyId)
        {
            var vacations = new List<Vacation>();
            var employees = await _context.Users.Where(i => i.CompanyId == companyId).ToListAsync();
            foreach (var item in employees)
            {
                vacations.AddRange(await _context.Vacations.Where(i => i.EmployeeId == item.Id).Include(i => i.Employee).ToArrayAsync());
            }
            return vacations.ToArray();
        }

        public async Task ConfirmVacation(Vacation vacation)
        {
            vacation.Status = Core.Enums.TypeOfVacation.Bestaetigt;
            await Update(vacation);
        }

        public async Task RejectVacation(Vacation vacation)
        {
            vacation.Status = Core.Enums.TypeOfVacation.Abgelehnt;
            await Update(vacation);
        }
    }
}
