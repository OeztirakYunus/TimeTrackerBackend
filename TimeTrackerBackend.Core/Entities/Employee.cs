
namespace TimeTrackerBackend.Core.Entities
{
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using TimeTrackerBackend.Core.Enums;

    public class Employee : IdentityUser
    {
        
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public EmployeeRole EmployeeRole { get; set; }
        [Required]
        public string SocialSecurityNumber { get; set; }
        [Required]
        public int NumberOfKids { get; set; }
        public ICollection<WorkMonth> WorkMonths { get; set; } = new List<WorkMonth>();
        public Guid? CompanyId { get; set; }
        public Company Company { get; set; }


    }
}
