
namespace TimeTrackerBackend.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;

    public class NotificationOfIllness : EntityObject
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public byte[] ConfirmationFile { get; set; }
        public bool IsConfirmed { get; set; }
        public string EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
