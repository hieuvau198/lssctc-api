using Lssctc.ProgramManagement.Accounts.Users.Dtos;
using Lssctc.ProgramManagement.Accounts.Users.Services;
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Accounts.Users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;
        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        #region Get Users

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetAllUsers([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            try
            {
                var users = await _usersService.GetUsersAsync(pageNumber, pageSize);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Instructor,SimulationManager")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            try
            {
                var user = await _usersService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("trainees")]
        [Authorize(Roles = "Admin,Instructor,SimulationManager")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetAllTrainees(
            [FromQuery] int pageNumber, 
            [FromQuery] int pageSize,
            [FromQuery] string? searchTerm,
            [FromQuery] bool? isActive)
        {
            try
            {
                var users = await _usersService.GetAllTraineesAsync(pageNumber, pageSize, searchTerm, isActive);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("instructors")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetAllInstructors(
            [FromQuery] int pageNumber, 
            [FromQuery] int pageSize,
            [FromQuery] string? searchTerm,
            [FromQuery] bool? isActive)
        {
            try
            {
                var users = await _usersService.GetAllInstructorsAsync(pageNumber, pageSize, searchTerm, isActive);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("simulation-managers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetAllSimulationManagers(
            [FromQuery] int pageNumber, 
            [FromQuery] int pageSize,
            [FromQuery] string? searchTerm,
            [FromQuery] bool? isActive)
        {
            try
            {
                var users = await _usersService.GetAllSimulationManagersAsync(pageNumber, pageSize, searchTerm, isActive);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Manage Users

        [HttpPost("trainees")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var newUser = await _usersService.CreateTraineeAccountAsync(dto);
                return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Username already exists.")
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("instructors")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> CreateInstructor([FromBody] CreateUserDto dto)
        {
            try
            {
                var newUser = await _usersService.CreateInstructorAccountAsync(dto);
                return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Username already exists.")
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("simulation-managers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> CreateSimulationManager([FromBody] CreateUserDto dto)
        {
            try
            {
                var newUser = await _usersService.CreateSimulationManagerAccountAsync(dto);
                return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Username or Email already exists.")
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("import-trainees")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ImportTrainees(IFormFile file)
        {
            try
            {
                var result = await _usersService.ImportTraineesAsync(file);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost("import-instructors")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ImportInstructors(IFormFile file)
        {
            try
            {
                var result = await _usersService.ImportInstructorsAsync(file);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost("import-simulation-managers")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ImportSimulationManagers(IFormFile file)
        {
            try
            {
                var result = await _usersService.ImportSimulationManagersAsync(file);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                await _usersService.UpdateUserAsync(id, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.Message == "User not found.")
                {
                    return NotFound(ex.Message);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _usersService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound("User not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Profiles

        [HttpPost("{id:int}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] UserChangePasswordDto dto)
        {
            try
            {
                await _usersService.ChangePasswordAsync(id, dto);
                return Ok("Password changed successfully.");
            }
            catch (Exception ex)
            {
                if (ex.Message == "User not found.")
                {
                    return NotFound(ex.Message);
                }
                if (ex.Message == "Current password is incorrect.")
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        #endregion
    }
}