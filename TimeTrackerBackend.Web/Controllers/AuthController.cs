using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TimeTrackerBackend.Core.Contracts;
using TimeTrackerBackend.Core.DataTransferObjects;
using TimeTrackerBackend.Core.Entities;

namespace TimeTrackerBackend.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _uow;
        private readonly UserManager<Employee> _userManager;
        public AuthController(IConfiguration configuration, UserManager<Employee> userManager, IUnitOfWork uow)
        {
            _config = configuration;
            _userManager = userManager;
            _uow = uow;
        }

        [Route("login")]
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login()
        {
            try
            {
                var credentials = GetCredentials();
                var email = credentials[0];
                var password = credentials[1];

                var authenticatedUser = await _userManager.FindByEmailAsync(email);
                if (authenticatedUser == null)
                {
                    return Unauthorized(new { Status = "Error", Message = $"User with Email {email} does not exist!" });
                }

                if (!await _userManager.CheckPasswordAsync(authenticatedUser, password))
                {
                    return Unauthorized(new { Status = "Error", Message = $"User with Email {email} has sent a wrong password!" });
                }

                var (token, roles) = await GenerateJwtToken(authenticatedUser);
                return Ok(new
                {
                    auth_token = new JwtSecurityTokenHandler().WriteToken(token),
                    userMail = authenticatedUser.Email,
                    userRoles = roles,
                    expiration = token.ValidTo
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// JWT erzeugen. Minimale Claim-Infos: Email und Rolle
        /// </summary>
        /// <param name="authenticatedUser"></param>
        /// <returns>Token mit Claims</returns>
        private async Task<(JwtSecurityToken, string)> GenerateJwtToken(Employee authenticatedUser)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var userRoles = await _userManager.GetRolesAsync(authenticatedUser);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, authenticatedUser.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = new JwtSecurityToken(
              issuer: _config["Jwt:Issuer"],
              audience: _config["Jwt:Audience"],
              claims: authClaims,
              expires: DateTime.Now.AddMinutes(60),
              signingCredentials: credentials);

            return (token, string.Join(',', userRoles));
        }

        private string[] GetCredentials()
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            return credentials;
        }

        /// <summary>
        /// Neuen Benutzer registrieren.
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns></returns>
        [Route("register")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterDto newUser)
        {
            var existingUser = await _userManager.FindByEmailAsync(newUser.Email);
            // gibt es schon einen Benutzer mit der Mailadresse?
            if (existingUser != null)
            {
                return BadRequest(new { Status = "Error", Message = "User already exists!" });
            }

            Employee user = new Employee
            {
                Email = newUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = newUser.Email,
                EmployeeRole = Core.Enums.EmployeeRole.Admin,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                PhoneNumber = newUser.PhoneNumber,
                NumberOfKids = newUser.NumberOfKids,
                SocialSecurityNumber = newUser.SocialSecurityNumber
            };
            var resultUser = await _userManager.CreateAsync(user, newUser.Password);
            var resultRole = await _userManager.AddToRoleAsync(user, "Admin");

            if (!resultUser.Succeeded)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    Message = string.Join(" ", resultUser.Errors.Select(e => e.Description))
                });
            }
            else if (!resultRole.Succeeded)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    Message = string.Join(" ", resultRole.Errors.Select(e => e.Description))
                });
            }

            var employee = await _userManager.FindByEmailAsync(newUser.Email);

            Company company = new Company
            {
                CompanyName = newUser.CompanyName,
                Employees = new List<Employee> { employee }
            };
            try
            {
                await _uow.CompanyRepository.Update(company);
                await _uow.SaveChangesAsync();
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }

            return Ok(new { Status = "Ok", Message = $"User {user.Email} successfully added." });
        }

        /// <summary>
        /// Neuen Benutzer registrieren.
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns></returns>
        [Route("addUser")]
        [HttpPost]
        public async Task<ActionResult> AddUser(AddUserDto newUser)
        {
            var existingUser = await _userManager.FindByEmailAsync(newUser.Email);
            // gibt es schon einen Benutzer mit der Mailadresse?
            if (existingUser != null)
            {
                return BadRequest(new { Status = "Error", Message = "User already exists!" });
            }

            var company = await GetCurrentUserAsync();

            Employee user = new Employee
            {
                Email = newUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = newUser.Email,
                EmployeeRole = Core.Enums.EmployeeRole.User,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                PhoneNumber = newUser.PhoneNumber,
                NumberOfKids = newUser.NumberOfKids,
                SocialSecurityNumber = newUser.SocialSecurityNumber,
                CompanyId = company.CompanyId
            };
            var resultUser = await _userManager.CreateAsync(user, newUser.Password);
            var resultRole = await _userManager.AddToRoleAsync(user, "User");

            if (!resultUser.Succeeded)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    Message = string.Join(" ", resultUser.Errors.Select(e => e.Description))
                });
            }
            else if (!resultRole.Succeeded)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    Message = string.Join(" ", resultRole.Errors.Select(e => e.Description))
                });
            }

            return Ok(new { Status = "Ok", Message = $"User {user.Email} successfully added." });
        }

        [HttpGet]
        public async Task<ActionResult<ICollection<Employee>>> Get()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var guid = (Guid)user.CompanyId;
                var entity = _userManager.Users.Where(i => i.CompanyId.Equals(guid)).ToArray();
                return entity;
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("role")]
        public async Task<ActionResult<string>> GetRole()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                return Ok(new { Role = user.EmployeeRole.ToString() });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var userToDelete = await _userManager.FindByIdAsync(id);

                if (user.CompanyId.Equals(userToDelete.CompanyId) && user.EmployeeRole == Core.Enums.EmployeeRole.Admin)
                {
                    var workMonths = await _uow.WorkMonthRepository.GetByEmployeeId(userToDelete.Id);

                    foreach (var item in workMonths)
                    {
                        await _uow.WorkMonthRepository.Remove(item.Id);
                    }
                    await _uow.SaveChangesAsync();
                    await _userManager.DeleteAsync(userToDelete);
                }
                else
                {
                    return Unauthorized(new { Status = "Unauthorized", Message = $"No permission" });
                }
                
                return Ok(new { Status = "Ok", Message = $"User {userToDelete.Email} successfully deleted." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        private async Task<Employee> GetCurrentUserAsync() => await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
    }
}
