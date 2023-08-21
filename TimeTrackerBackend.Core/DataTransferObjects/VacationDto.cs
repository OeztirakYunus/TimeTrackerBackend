using System;
using System.ComponentModel.DataAnnotations;
using TimeTrackerBackend.Core.Entities;
using TimeTrackerBackend.Core.Enums;

namespace TimeTrackerBackend.Core.DataTransferObjects
{
	public class VacationDto
	{
        public Guid Id { get; set; }
        public DateTime DateOfRequest { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TypeOfVacation Status { get; set; }
        public string EmployeeId { get; set; }
        public EmployeeDto Employee { get; set; }
    }
}

