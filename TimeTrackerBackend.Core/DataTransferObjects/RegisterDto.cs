using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerBackend.Core.DataTransferObjects
{
    public class RegisterDto : AddUserDto
    {
        public string CompanyName { get; set; }
    }
}
