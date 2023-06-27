
namespace TimeTrackerBackend.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;

    public class WorkDay : EntityObject
    {
        [Required]
        public DateTime Date { get; set; }
        public double WorkedHours { get; set; }
        public ICollection<Stamp> Stamps { get; set; } = new List<Stamp>();
        public Guid? WorkMonthId { get; set; }
        public WorkMonth WorkMonth { get; set; }
    }
}
