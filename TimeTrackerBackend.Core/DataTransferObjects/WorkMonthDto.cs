using System;
using System.Collections.Generic;
using TimeTrackerBackend.Core.Entities;

namespace TimeTrackerBackend.Core.DataTransferObjects
{
	public class WorkMonthDto
	{
        public DateTime Date { get; set; }
        public double WorkedHours { get; set; }
        public List<WorkDay>[] WorkDays { get; set; }
        public string EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}

