

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
    public class WorkDaysController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<Employee> _userManager;

        public WorkDaysController(IUnitOfWork uow, UserManager<Employee> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        private async Task<Employee> GetCurrentUserAsync() => await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);


        [HttpGet("for-employee")]
        [Authorize]
        public async Task<ActionResult<WorkDayDto>> GetDayForEmployee()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var workDay = await _uow.WorkDayRepository.GetDayForEmployee(user);
                //workDay.StartDate = workDay.StartDate.ToLocalTime();
                //workDay.EndDate = workDay.EndDate.ToLocalTime();
                return Ok(workDay);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkDay>>> GetAll()
        {
            try
            {
                var all = await _uow.WorkDayRepository.GetAllAsync();
                return Ok(all);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkDay>> Get(string id)
        {
            try
            {
                var guid = Guid.Parse(id);
                var entity = await _uow.WorkDayRepository.GetByIdAsync(guid);
                return entity;
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(WorkDay valueToUpdate)
        {
            try
            {
                var entity = await _uow.WorkDayRepository.GetByIdAsync(valueToUpdate.Id);
                valueToUpdate.CopyProperties(entity);
                entity = DateToUTC(entity);
                await _uow.WorkDayRepository.Update(entity);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Updated!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(WorkDay valueToAdd)
        {
            try
            {
                valueToAdd = DateToUTC(valueToAdd);
                await _uow.WorkDayRepository.AddAsync(valueToAdd);
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
                await _uow.WorkDayRepository.Remove(guid);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Deleted." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        private WorkDay DateToUTC(WorkDay workDay)
        {
            workDay.EndDate = workDay.EndDate.ToUniversalTime();
            workDay.StartDate = workDay.StartDate.ToUniversalTime();
            return workDay;
        }
    }
}
