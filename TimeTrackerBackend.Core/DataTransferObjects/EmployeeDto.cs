using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TimeTrackerBackend.Core.Entities;
using TimeTrackerBackend.Core.Enums;

namespace TimeTrackerBackend.Core.DataTransferObjects
{
	public class EmployeeDto
	{
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public EmployeeRole EmployeeRole { get; set; }
        public string SocialSecurityNumber { get; set; }
        public int NumberOfKids { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public ICollection<WorkMonth> WorkMonths { get; set; } = new List<WorkMonth>();
        public Guid? CompanyId { get; set; }
        public Company Company { get; set; }
    }
}

