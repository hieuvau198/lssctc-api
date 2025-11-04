using Lssctc.ProgramManagement.Accounts.Managemetns.Dtos;
using Lssctc.ProgramManagement.Accounts.Managemetns.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Accounts.Managemetns.Controllers
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

        #region Users

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _usersService.GetAllUsersAsync();
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
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllTrainees()
        {
            try
            {
                var users = await _usersService.GetAllTraineesAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("instructors")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllInstructors()
        {
            try
            {
                var users = await _usersService.GetAllInstructorsAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("simulation-managers")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllSimulationManagers()
        {
            try
            {
                var users = await _usersService.GetAllSimulationManagersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
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

        [HttpPut("{id:int}")]
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