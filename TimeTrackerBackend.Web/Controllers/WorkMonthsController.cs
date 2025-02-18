

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
    public class WorkMonthsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<Employee> _userManager;

        public WorkMonthsController(IUnitOfWork uow, UserManager<Employee> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        private async Task<Employee> GetCurrentUserAsync() => await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkMonth>>> GetAll()
        {
            try
            {
                var all = await _uow.WorkMonthRepository.GetAllAsync();
                return Ok(all);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("date/{date}")]
        [Authorize]
        public async Task<ActionResult<WorkMonthDto>> GetByDate(DateTime date)
        {
            try
            {
                date = date.ToUniversalTime();
                var user = await GetCurrentUserAsync();
                var workMonth = await _uow.WorkMonthRepository.GetByDate(date, user.Id);
              //  workMonth.Date = workMonth.Date.ToLocalTime();
                return Ok(workMonth);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("date/{employeeId}/{date}")]
        [Authorize]
        public async Task<ActionResult<WorkMonthDto>> GetByDateForEmployee(string employeeId, DateTime date)
        {
            try
            {
                date = date.ToUniversalTime();
                var user = await GetCurrentUserAsync();
                if(user.EmployeeRole != Core.Enums.EmployeeRole.Admin)
                {
                    return BadRequest(new { Status = "Error", Message = "Sie sind kein Admin." });
                }

                var employee = await _userManager.FindByIdAsync(employeeId);
                if(employee == null)
                {
                    return BadRequest(new { Status = "Error", Message = "Mitarbeiter nicht gefunden." });
                }

                if (!employee.CompanyId.Equals(user.CompanyId))
                {
                    return Unauthorized(new { Status = "Error", Message = "Sie sind nicht berechtigt." });
                }

                var workMonth = await _uow.WorkMonthRepository.GetByDate(date, employee.Id);
               // workMonth.Date = workMonth.Date.ToLocalTime();
                return Ok(workMonth);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkMonth>> Get(string id)
        {
            try
            {
                var guid = Guid.Parse(id);
                var entity = await _uow.WorkMonthRepository.GetByIdAsync(guid);
                return entity;
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(WorkMonth valueToUpdate)
        {
            try
            {
                var entity = await _uow.WorkMonthRepository.GetByIdAsync(valueToUpdate.Id);
                valueToUpdate.CopyProperties(entity);
                entity = DateToUTC(entity);
                await _uow.WorkMonthRepository.Update(entity);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Updated!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(WorkMonth valueToAdd)
        {
            try
            {
                valueToAdd = DateToUTC(valueToAdd);
                await _uow.WorkMonthRepository.AddAsync(valueToAdd);
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
                await _uow.WorkMonthRepository.Remove(guid);
                await _uow.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "Deleted." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }


        [HttpGet("createPdf/{employeeId}/{workMonthId}/{month}/{year}")]
        public async Task<IActionResult> GetPdf(string employeeId, string workMonthId, int month, int year)
        {
            try
            {
                var guid = Guid.Empty;
                if(!string.IsNullOrEmpty(workMonthId) && workMonthId != "null")
                {
                    guid = Guid.Parse(workMonthId);
                }
                var workMonth = await _uow.WorkMonthRepository.GetByIdAsync(guid);

                if(workMonth == null)
                {
                    workMonth = new WorkMonth()
                    {
                        Id = Guid.Empty,
                        EmployeeId = employeeId,
                        Date = new DateTime(year, month, 1)
                    };
                }

                var employee = await _userManager.FindByIdAsync(workMonth.EmployeeId);
                var user = await GetCurrentUserAsync();

                if (!employee.CompanyId.Equals(user.CompanyId) || (user.EmployeeRole != Core.Enums.EmployeeRole.Admin && employee.Id != user.Id))
                {
                    return Unauthorized(new { Status = "Error", Message = "Sie sind nicht berechtigt." });
                }

                var workMonthDto = await _uow.WorkMonthRepository.GetAsDto(workMonth);
                var (bytes, path) = PDFCreator.GeneratePdf(workMonthDto);
                return File(bytes, "application/pdf", path);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        //[HttpGet("createPdf/{employeeId}/{workMonthId}/{month}/{year}")]
        //public async Task<IActionResult> GetAllPdf(int month, int year)
        //{
        //    try
        //    {
        //        var user = await GetCurrentUserAsync();

        //        if ((user.EmployeeRole != Core.Enums.EmployeeRole.Admin))
        //        {
        //            return Unauthorized(new { Status = "Error", Message = "Sie sind nicht berechtigt." });
        //        }

        //        user.Company

        //        var guid = Guid.Empty;
        //        if (!string.IsNullOrEmpty(workMonthId) && workMonthId != "null")
        //        {
        //            guid = Guid.Parse(workMonthId);
        //        }
        //        var workMonth = await _uow.WorkMonthRepository.GetByIdAsync(guid);

        //        if (workMonth == null)
        //        {
        //            workMonth = new WorkMonth()
        //            {
        //                Id = Guid.Empty,
        //                EmployeeId = employeeId,
        //                Date = new DateTime(year, month, 1)
        //            };
        //        }

        //        var employee = await _userManager.FindByIdAsync(workMonth.EmployeeId);
               
        //        var workMonthDto = await _uow.WorkMonthRepository.GetAsDto(workMonth);
        //        var (bytes, path) = PDFCreator.GeneratePdf(workMonthDto);
        //        return File(bytes, "application/pdf", path);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { Status = "Error", Message = ex.Message });
        //    }
        //}

        private WorkMonth DateToUTC(WorkMonth workMonth)
        {
            workMonth.Date = workMonth.Date.ToUniversalTime();
            return workMonth;
        }
    }
}
