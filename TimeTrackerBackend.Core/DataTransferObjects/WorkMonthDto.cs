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
        public List<WorkDayDto>[] WorkDays { get; set; }
        public string EmployeeId { get; set; }
        public Employee Employee { get; set; }

        //public void GetEmptyWorkMonthDto(DateTime dateTime)
        //{
        //    Id = Guid.Empty.ToString();
        //    Date = dateTime;
        //    WorkedHours = 0;
        //    BreakHours = 0;
        //    var daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);


        //}
    }
}

