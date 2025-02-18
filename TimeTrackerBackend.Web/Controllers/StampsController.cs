

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
using DocumentFormat.OpenXml.Vml.Office;

namespace TimeTrackerBackend.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StampsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<Employee> _userManager;

        public StampsController(IUnitOfWork uow, UserManager<Employee> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        [HttpGet("stamp")]
        [Authorize]
        public async Task<IActionResult> Stamp()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var workDay = await _uow.StampRepository.StampAsync(user);
                await _uow.SaveChangesAsync();
                return Ok(workDay);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("stamp/{employeeId}/{dateTime}")]
        [Authorize]
        public async Task<IActionResult> StampManually(string employeeId, DateTime dateTime)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user.EmployeeRole != Core.Enums.EmployeeRole.Admin)
                {
                    return BadRequest(new { Status = "Error", Message = "Sie sind kein Admin." });
                }

                var employee = await _userManager.FindByIdAsync(employeeId);
                if (employee == null)
                {
                    return BadRequest(new { Status = "Error", Message = "Mitarbeiter nicht gefunden." });
                }

                if (!employee.CompanyId.Equals(user.CompanyId))
                {
                    return Unauthorized(new { Status = "Error", Message = "Sie sind nicht berechtigt." });
                }

                DateTime currentDate = DateTime.Now.ToUniversalTime();
                DateTime updatedDateTime = new DateTime(
                    dateTime.Year,
                    dateTime.Month,
                    dateTime.Day,
                    currentDate.Hour + 1,
                    currentDate.Minute,
                    currentDate.Second
                ).ToUniversalTime();
                var workDay = await _uow.StampRepository.StampManuallyAsync(employee, updatedDateTime);
                await _uow.SaveChangesAsync();
                return Ok(workDay);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("break")]
        [Authorize]
        public async Task<IActionResult> TakeABreak()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var workDay = await _uow.StampRepository.TakeABreakAsync(user);
                await _uow.SaveChangesAsync();
                return Ok(workDay);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("break/{employeeId}/{dateTime}")]
        [Authorize]
        public async Task<IActionResult> TakeABreakManually(string employeeId, DateTime dateTime)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user.EmployeeRole != Core.Enums.EmployeeRole.Admin)
                {
                    return BadRequest(new { Status = "Error", Message = "Sie sind kein Admin." });
                }

                var employee = await _userManager.FindByIdAsync(employeeId);
                if (employee == null)
                {
                    return BadRequest(new { Status = "Error", Message = "Mitarbeiter nicht gefunden." });
                }

                if (!employee.CompanyId.Equals(user.CompanyId))
                {
                    return Unauthorized(new { Status = "Error", Message = "Sie sind nicht berechtigt." });
                }
                DateTime currentDate = DateTime.Now.ToUniversalTime();
                DateTime updatedDateTime = new DateTime(
                    dateTime.Year,
                    dateTime.Month,
                    dateTime.Day,
                    currentDate.Hour,
                    currentDate.Minute,
                    currentDate.Second
                ).ToUniversalTime();
                var workDay = await _uow.StampRepository.TakeABreakManuallyAsync(employee, updatedDateTime);
                await _uow.SaveChangesAsync();
                return Ok(workDay);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("forDay")]
        public async Task<ActionResult<IEnumerable<Stamp>>> GetForDayForEmployee(Employee employee)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var all = await _uow.StampRepository.GetForDayForEmployee(user);
                return Ok(all);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Stamp>>> GetAll()
        {
            try
            {
                var all = await _uow.StampRepository.GetAllAsync();
                return Ok(all);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Stamp>> Get(string id)
        {
            try
            {
                var guid = Guid.Parse(id);
                var entity = await _uow.StampRepository.GetByIdAsync(guid);
                return entity;
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(Stamp valueToUpdate)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var workDay = await _uow.WorkDayRepository.GetByIdAsync((Guid)valueToUpdate.WorkDayId);
                var workMonth = await _uow.WorkMonthRepository.GetByIdAsync((Guid)workDay.WorkMonthId);
                var employee = await _userManager.FindByIdAsync(workMonth.EmployeeId);

                if (user.CompanyId != employee.CompanyId || user.EmployeeRole != Core.Enums.EmployeeRole.Admin)
                {
                    return Unauthorized(new { Status = "Unauthorized", Message = "Sie sind nicht bereichtigt zu bearbeiten." });
                }

                valueToUpdate = DateToUTC(valueToUpdate);
                await _uow.StampRepository.Update(valueToUpdate);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Updated!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPut("updateMany")]
        public async Task<IActionResult> Put(Stamp[] valuesToUpdate)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                for (int i = 0; i < valuesToUpdate.Length; i++)
                {
                    var workDay = await _uow.WorkDayRepository.GetByIdAsync((Guid)valuesToUpdate[i].WorkDayId);
                    var workMonth = await _uow.WorkMonthRepository.GetByIdAsync((Guid)workDay.WorkMonthId);
                    var employee = await _userManager.FindByIdAsync(workMonth.EmployeeId);

                    if (user.CompanyId != employee.CompanyId || user.EmployeeRole != Core.Enums.EmployeeRole.Admin)
                    {
                        return Unauthorized(new { Status = "Unauthorized", Message = "Sie sind nicht bereichtigt zu bearbeiten." });
                    }

                    valuesToUpdate[i] = DateToUTC(valuesToUpdate[i]);
                }

                await _uow.StampRepository.UpdateStamps(valuesToUpdate.ToList());
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Updated!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(Stamp valueToAdd)
        {
            try
            {
                valueToAdd = DateToUTC(valueToAdd);
                await _uow.StampRepository.AddAsync(valueToAdd);
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
                await _uow.StampRepository.Remove(guid);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Deleted." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        private async Task<Employee> GetCurrentUserAsync() => await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

        private Stamp DateToUTC(Stamp stamp)
        {
            stamp.Time = stamp.Time.ToUniversalTime();
            return stamp;
        }
    }
}
