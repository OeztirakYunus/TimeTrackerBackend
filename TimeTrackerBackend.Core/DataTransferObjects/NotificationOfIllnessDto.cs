using System;
using System.ComponentModel.DataAnnotations;
using TimeTrackerBackend.Core.Entities;

namespace TimeTrackerBackend.Core.DataTransferObjects
{
	public class NotificationOfIllnessDto
	{
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public byte[] ConfirmationFile { get; set; }
        public bool IsConfirmed { get; set; }
        public string EmployeeId { get; set; }
        public EmployeeDto Employee { get; set; }
    }
}

