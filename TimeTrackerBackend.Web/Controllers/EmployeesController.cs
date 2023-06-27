

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using TimeTrackerBackend.Core.Contracts;
//using TimeTrackerBackend.Core.Entities;
//using CommonBase.Extensions;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace TimeTrackerBackend.Web.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class EmployeesController : ControllerBase
//    {
//        private readonly IUnitOfWork _uow;

//        public EmployeesController(IUnitOfWork uow)
//        {
//            _uow = uow;
//        }

//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<Employee>>> GetAll()
//        {
//            try
//            {
//                var all = await _uow.EmployeeRepository.GetAllAsync();
//                return Ok(all);
//            }
//            catch (System.Exception ex)
//            {
//                return BadRequest(new { Status = "Error", Message = ex.Message });
//            }
//        }

//        [HttpGet("{id}")]
//        public async Task<ActionResult<Employee>> Get(string id)
//        {
//            try
//            {
//                var guid = Guid.Parse(id);
//                var entity = await _uow.EmployeeRepository.GetByIdAsync(guid);
//                return entity;
//            }
//            catch (System.Exception ex)
//            {
//                return BadRequest(new { Status = "Error", Message = ex.Message });
//            }
//        }

//        [HttpPut]
//        public async Task<IActionResult> Put(Employee valueToUpdate)
//        {
//            try
//            {
//                var entity = await _uow.EmployeeRepository.GetByIdAsync(valueToUpdate.Id);
//                valueToUpdate.CopyProperties(entity);
//                await _uow.EmployeeRepository.Update(entity);
//                await _uow.SaveChangesAsync();
//                return Ok(new { Status = "Success", Message = "Updated!" });
//            }
//            catch (System.Exception ex)
//            {
//                return BadRequest(new { Status = "Error", Message = ex.Message });
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> Post(Employee valueToAdd)
//        {
//            try
//            {              
//                await _uow.EmployeeRepository.AddAsync(valueToAdd);
//                await _uow.SaveChangesAsync();
//                return Ok(new { Status = "Success", Message = "Added!" });
//            }
//            catch (System.Exception ex)
//            {
//                return BadRequest(new { Status = "Error", Message = ex.Message });
//            }
//        }

//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(string id)
//        {
//            try
//            {
//                var guid = Guid.Parse(id);
//                await _uow.EmployeeRepository.Remove(guid);
//                await _uow.SaveChangesAsync();
//                return Ok(new { Status = "Success", Message = "Deleted." });
//            }
//            catch (System.Exception ex)
//            {
//                return BadRequest(new { Status = "Error", Message = ex.Message });
//            }
//        }
//    }
//}
