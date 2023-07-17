

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

                if(user.CompanyId != employee.CompanyId || user.EmployeeRole != Core.Enums.EmployeeRole.Admin)
                {
                    return Unauthorized(new { Status = "Unauthorized", Message = "Sie sind nicht bereichtigt zu bearbeiten." });
                }

                await _uow.StampRepository.Update(valueToUpdate);
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
    }
}
