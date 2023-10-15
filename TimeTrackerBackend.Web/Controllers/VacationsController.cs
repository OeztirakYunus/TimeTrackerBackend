

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeTrackerBackend.Core.Contracts;
using TimeTrackerBackend.Core.Entities;
using CommonBase.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TimeTrackerBackend.Core.DataTransferObjects;

namespace TimeTrackerBackend.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VacationsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<Employee> _userManager;


        public VacationsController(IUnitOfWork uow, UserManager<Employee> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VacationDto>>> GetAllForUser()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var all = await _uow.VacationRepository.GetAllAsyncByEmployeeId(user.Id);
                var allDto = new List<VacationDto>();
                foreach (var item in all)
                {
                    allDto.Add(VacationEntityToDto(item));
                }
                return Ok(allDto);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("forCompany")]
        public async Task<ActionResult<IEnumerable<VacationDto>>> GetAllForCompany()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if(user.EmployeeRole != Core.Enums.EmployeeRole.Admin)
                {
                    return Unauthorized(new { Status = "Unauthorized", Message = "Sie sind nicht bereichtigt zu bearbeiten." });
                }
                var all = await _uow.VacationRepository.GetAllAsyncForCompany((Guid)user.CompanyId);
                var allDto = new List<VacationDto>();
                foreach (var item in all)
                {
                    allDto.Add(VacationEntityToDto(item));
                }
                return Ok(allDto);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Vacation>> Get(string id)
        {
            try
            {
                var guid = Guid.Parse(id);
                var entity = await _uow.VacationRepository.GetByIdAsync(guid);
                return entity;
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("confirm/{id}")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var vac = await _uow.VacationRepository.GetByIdAsync(id);
                var vacEmployee = await _userManager.FindByIdAsync(vac.EmployeeId);

                if(vac == null)
                {
                    return BadRequest(new { Status = "Error", Message = "Urlaubsantrag nicht gefunden." });

                }

                if (user.EmployeeRole != Core.Enums.EmployeeRole.Admin || vacEmployee.CompanyId != user.CompanyId)
                {
                    return Unauthorized(new { Status = "Unauthorized", Message = "Sie sind nicht bereichtigt zu bearbeiten." });
                }

                await _uow.VacationRepository.ConfirmVacation(vac);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Updated!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }


        [HttpGet("reject/{id}")]
        public async Task<IActionResult> Reject(Guid id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var vac = await _uow.VacationRepository.GetByIdAsync(id);
                var vacEmployee = await _userManager.FindByIdAsync(vac.EmployeeId);

                if (vac == null)
                {
                    return BadRequest(new { Status = "Error", Message = "Urlaubsantrag nicht gefunden." });

                }

                if (user.EmployeeRole != Core.Enums.EmployeeRole.Admin || vacEmployee.CompanyId != user.CompanyId)
                {
                    return Unauthorized(new { Status = "Unauthorized", Message = "Sie sind nicht bereichtigt zu bearbeiten." });
                }

                await _uow.VacationRepository.RejectVacation(vac);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Updated!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(Vacation valueToUpdate)
        {
            try
            {
                var entity = await _uow.VacationRepository.GetByIdAsync(valueToUpdate.Id);
                valueToUpdate.CopyProperties(entity);
                await _uow.VacationRepository.Update(entity);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Updated!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(Vacation valueToAdd)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                valueToAdd.Status = Core.Enums.TypeOfVacation.InBearbeitung;
                valueToAdd.EmployeeId = user.Id;
                valueToAdd.DateOfRequest = DateTime.Now;

                await _uow.VacationRepository.AddAsync(valueToAdd);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Added!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var guid = Guid.Parse(id);

                var user = await GetCurrentUserAsync();
                var toDelete = await _uow.VacationRepository.GetByIdAsync(guid);
                var toDelUser = await _userManager.FindByIdAsync(toDelete.EmployeeId);

                if (user.EmployeeRole != Core.Enums.EmployeeRole.Admin || toDelUser.CompanyId != user.CompanyId)
                {
                    return Unauthorized(new { Status = "Unauthorized", Message = "Sie sind nicht bereichtigt zu bearbeiten." });
                }

                await _uow.VacationRepository.Remove(guid);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Deleted." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        private async Task<Employee> GetCurrentUserAsync() => await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

        private VacationDto VacationEntityToDto(Vacation vacation)
        {
            VacationDto vacationDto = new VacationDto()
            {
                Id = vacation.Id,
                EmployeeId = vacation.EmployeeId,
                StartDate = vacation.StartDate,
                EndDate = vacation.EndDate,
                DateOfRequest = vacation.DateOfRequest,
                Status = vacation.Status,
                Employee = EmployeeEntityToDto(vacation.Employee)
            };
            return vacationDto;
        }

        private EmployeeDto EmployeeEntityToDto(Employee employee)
        {
            EmployeeDto employeeDto = new EmployeeDto()
            {
                Company = employee.Company,
                CompanyId = employee.CompanyId,
                EmployeeRole = employee.EmployeeRole,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                NumberOfKids = employee.NumberOfKids,
                SocialSecurityNumber = employee.SocialSecurityNumber,
                WorkMonths = employee.WorkMonths,
                PhoneNumber = employee.PhoneNumber,
                Email = employee.Email,
                Id = employee.Id
            };

            return employeeDto;
        }
    }
}
