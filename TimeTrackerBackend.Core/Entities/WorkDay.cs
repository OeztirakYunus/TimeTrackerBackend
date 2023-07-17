
namespace TimeTrackerBackend.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using TimeTrackerBackend.Core.Enums;

    public class WorkDay : EntityObject
    {
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [NotMapped]
        public double WorkedHours { get; set; }
        [NotMapped]
        public double BreakHours { get; set; }
        public Status Status { get; set; }
        public ICollection<Stamp> Stamps { get; set; } = new List<Stamp>();
        public Guid? WorkMonthId { get; set; }
        public WorkMonth WorkMonth { get; set; }
    }
}
