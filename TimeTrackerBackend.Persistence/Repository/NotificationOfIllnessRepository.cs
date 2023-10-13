
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

    public class NotificationOfIllnessRepository : Repository<NotificationOfIllness>, INotificationOfIllnessRepository
    {
        public NotificationOfIllnessRepository(ApplicationDbContext context) : base(context)
        {

        }
        override public Task<NotificationOfIllness[]> GetAllAsync()
        {
            return _context.NotificationOfIllness.ToArrayAsync();
        }

        public async Task<NotificationOfIllness[]> GetAllAsyncByEmployeeId(string id)
        {
            return await _context.NotificationOfIllness.Where(i => i.EmployeeId == id).Include(i => i.Employee).ToArrayAsync();
        }

        public async Task<NotificationOfIllness[]> GetAllAsyncForCompany(Guid companyId)
        {
            var notificationOfIllnesses = new List<NotificationOfIllness>();
            var employees = await _context.Users.Where(i => i.CompanyId == companyId).ToListAsync();
            foreach (var item in employees)
            {
                notificationOfIllnesses.AddRange(await _context.NotificationOfIllness.Where(i => i.EmployeeId == item.Id).Include(i => i.Employee).ToArrayAsync());
            }
            return notificationOfIllnesses.ToArray();
        }

        public async Task Confirm(NotificationOfIllness notificationOfIllness)
        {
            notificationOfIllness.IsConfirmed = true;
            await Update(notificationOfIllness);
        }

        public async Task Reject(NotificationOfIllness notificationOfIllness)
        {
            notificationOfIllness.IsConfirmed = false;
            await Update(notificationOfIllness);
        }
    }
}
