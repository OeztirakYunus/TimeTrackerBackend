
namespace TimeTrackerBackend.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using TimeTrackerBackend.Core.Enums;

    public class Stamp : EntityObject
    {
        [Required]
        public TypeOfStamp TypeOfStamp { get; set; }
        [Required]
        public DateTime Time { get; set; }
        public Guid? WorkDayId { get; set; }
        public WorkDay WorkDay { get; set; }
    }
}
