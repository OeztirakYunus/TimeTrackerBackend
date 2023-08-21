
namespace TimeTrackerBackend.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using TimeTrackerBackend.Core.DataTransferObjects;
    using TimeTrackerBackend.Core.Enums;

    public class Vacation : EntityObject
    {
        [Required]
        public DateTime DateOfRequest { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public TypeOfVacation Status { get; set; }
        public string EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
