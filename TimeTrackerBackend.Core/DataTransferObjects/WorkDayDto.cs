using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeTrackerBackend.Core.Entities;
using TimeTrackerBackend.Core.Enums;

namespace TimeTrackerBackend.Core.DataTransferObjects
{
	public class WorkDayDto
	{
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double WorkedHours { get; set; }
        public double BreakHours { get; set; }
        public Status Status { get; set; }
        public ICollection<Stamp> Stamps { get; set; } = new List<Stamp>();
        public Guid? WorkMonthId { get; set; }
        public WorkMonth WorkMonth { get; set; }
        public bool VacationDay { get; set; }
        public bool IllDay { get; set; }
    }
}

