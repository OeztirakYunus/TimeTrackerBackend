
namespace TimeTrackerBackend.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    public class WorkMonth : EntityObject
    {
        [Required]
        public DateTime Date { get; set; }
        [NotMapped]
        public double WorkedHours { get; set; }
        public ICollection<WorkDay> WorkDays { get; set; } = new List<WorkDay>();
        public string EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
