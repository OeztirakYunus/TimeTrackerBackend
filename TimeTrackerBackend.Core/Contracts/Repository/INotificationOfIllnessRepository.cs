
namespace TimeTrackerBackend.Contracts.Repository
{
    using TimeTrackerBackend.Core.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TimeTrackerBackend.Core.Contracts.Repository;

    public interface INotificationOfIllnessRepository : IRepository<NotificationOfIllness>
    {
        Task<NotificationOfIllness[]> GetAllAsyncByEmployeeId(string id);
        Task<NotificationOfIllness[]> GetAllAsyncForCompany(Guid companyId);
        Task Confirm(NotificationOfIllness notificationOfIllness);
        Task Reject(NotificationOfIllness notificationOfIllness);
    }
}
