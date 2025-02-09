

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
    public class NotificationOfIllnessesController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<Employee> _userManager;

        public NotificationOfIllnessesController(IUnitOfWork uow, UserManager<Employee> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }
 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationOfIllnessDto>>> GetAllForUser()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var all = await _uow.NotificationOfIllnessRepository.GetAllAsyncByEmployeeId(user.Id);
                var allDto = new List<NotificationOfIllnessDto>();
                for (int i = 0; i < all.Length; i++)
                {
                    all[i] = DateToUTC(all[i]);
                    allDto.Add(NotificationOfIllnessEntityToDto(all[i]));
                }
                return Ok(allDto);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("forCompany")]
        public async Task<ActionResult<IEnumerable<NotificationOfIllnessDto>>> GetAllForCompany()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user.EmployeeRole != Core.Enums.EmployeeRole.Admin)
                {
                    return Unauthorized(new { Status = "Unauthorized", Message = "Sie sind nicht bereichtigt zu bearbeiten." });
                }
                var all = await _uow.NotificationOfIllnessRepository.GetAllAsyncForCompany((Guid)user.CompanyId);
                var allDto = new List<NotificationOfIllnessDto>();
                for (int i = 0; i < all.Length; i++)
                {
                    all[i] = DateToUTC(all[i]);
                    allDto.Add(NotificationOfIllnessEntityToDto(all[i]));
                }
                return Ok(allDto);
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
                var noi = await _uow.NotificationOfIllnessRepository.GetByIdAsync(id);
                var noiEmployee = await _userManager.FindByIdAsync(noi.EmployeeId);

                if (noi == null)
                {
                    return BadRequest(new { Status = "Error", Message = "Krankmeldung nicht gefunden." });

                }

                if (user.EmployeeRole != Core.Enums.EmployeeRole.Admin || noiEmployee.CompanyId != user.CompanyId)
                {
                    return Unauthorized(new { Status = "Unauthorized", Message = "Sie sind nicht bereichtigt zu bearbeiten." });
                }

                await _uow.NotificationOfIllnessRepository.Confirm(noi);
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
                var noi = await _uow.NotificationOfIllnessRepository.GetByIdAsync(id);
                var noiEmployee = await _userManager.FindByIdAsync(noi.EmployeeId);

                if (noi == null)
                {
                    return BadRequest(new { Status = "Error", Message = "Krankmeldung nicht gefunden." });

                }

                if (user.EmployeeRole != Core.Enums.EmployeeRole.Admin || noiEmployee.CompanyId != user.CompanyId)
                {
                    return Unauthorized(new { Status = "Unauthorized", Message = "Sie sind nicht bereichtigt zu bearbeiten." });
                }

                await _uow.NotificationOfIllnessRepository.Reject(noi);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Updated!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationOfIllness>> Get(string id)
        {
            try
            {
                var guid = Guid.Parse(id);
                var entity = await _uow.NotificationOfIllnessRepository.GetByIdAsync(guid);
                return entity;
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(NotificationOfIllness valueToUpdate)
        {
            try
            {
                var entity = await _uow.NotificationOfIllnessRepository.GetByIdAsync(valueToUpdate.Id);
                valueToUpdate.CopyProperties(entity);
                entity = DateToUTC(entity);
                await _uow.NotificationOfIllnessRepository.Update(entity);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Updated!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(NotificationOfIllness valueToAdd)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                valueToAdd.IsConfirmed = false;
                valueToAdd.EmployeeId = user.Id;
                valueToAdd = DateToUTC(valueToAdd);
                await _uow.NotificationOfIllnessRepository.AddAsync(valueToAdd);
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
                var toDelete = await _uow.NotificationOfIllnessRepository.GetByIdAsync(guid);
                var toDelUser = await _userManager.FindByIdAsync(toDelete.EmployeeId);

                if (user.EmployeeRole != Core.Enums.EmployeeRole.Admin || toDelUser.CompanyId != user.CompanyId)
                {
                    return Unauthorized(new { Status = "Unauthorized", Message = "Sie sind nicht bereichtigt zu bearbeiten." });
                }

                await _uow.NotificationOfIllnessRepository.Remove(guid);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Deleted." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        private async Task<Employee> GetCurrentUserAsync() => await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

        private NotificationOfIllnessDto NotificationOfIllnessEntityToDto(NotificationOfIllness notificationOfIllness)
        {
            NotificationOfIllnessDto notificationOfIllnessDto = new NotificationOfIllnessDto()
            {
                Id = notificationOfIllness.Id,
                StartDate = notificationOfIllness.StartDate,
                EndDate = notificationOfIllness.EndDate,
                ConfirmationFile = notificationOfIllness.ConfirmationFile,
                IsConfirmed = notificationOfIllness.IsConfirmed,
                EmployeeId = notificationOfIllness.EmployeeId,
                Employee = EmployeeEntityToDto(notificationOfIllness.Employee)
            };

            return notificationOfIllnessDto;
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

        private NotificationOfIllness DateToUTC(NotificationOfIllness notificationOfIllness)
        {
            notificationOfIllness.StartDate = notificationOfIllness.StartDate.ToUniversalTime();
            notificationOfIllness.EndDate = notificationOfIllness.EndDate.ToUniversalTime();
            return notificationOfIllness;
        }
    }
}
