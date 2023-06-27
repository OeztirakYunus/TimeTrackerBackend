
namespace TimeTrackerBackend.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;

    public class Company : EntityObject
    {
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
