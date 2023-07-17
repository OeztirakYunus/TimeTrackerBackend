using System;
using System.Collections.Generic;
using TimeTrackerBackend.Core.Entities;

namespace TimeTrackerBackend.Core.DataTransferObjects
{
	public class WorkMonthDto
	{
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public double WorkedHours { get; set; }
        public double BreakHours { get; set; }
        public List<WorkDay>[] WorkDays { get; set; }
        public string EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}

