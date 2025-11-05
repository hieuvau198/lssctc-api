using Lssctc.ProgramManagement.Classes.Dtos;
using Lssctc.ProgramManagement.Classes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Classes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClassesController : ControllerBase
    {
        private readonly IClassesService _service;
        private readonly IClassInstructorsService _instructorsService;
        public ClassesController(IClassesService service, IClassInstructorsService instructorsService)
        {
            _service = service;
            _instructorsService = instructorsService;
        }

        #region Class

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllClassesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetClassesAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetClassByIdAsync(id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
        {
            try
            {
                var result = await _service.CreateClassAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClassDto dto)
        {
            try
            {
                var result = await _service.UpdateClassAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/open")]
        public async Task<IActionResult> Open(int id)
        {
            try
            {
                await _service.OpenClassAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/start")]
        public async Task<IActionResult> Start(int id)
        {
            try
            {
                await _service.StartClassAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteClass(int id)
        {
            try
            {
                await _service.CompleteClassAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelClass(int id)
        {
            try
            {
                await _service.CancelClassAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Class Instructor

        [HttpGet("{id}/instructor")]
        public async Task<IActionResult> GetInstructor(int id)
        {
            try
            {
                var result = await _instructorsService.GetInstructorByClassIdAsync(id);
                if (result == null) return NotFound("No instructor found for this class.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/instructor")]
        public async Task<IActionResult> AssignInstructor(int id, [FromBody] AssignInstructorDto dto)
        {
            try
            {
                await _instructorsService.AssignInstructorAsync(id, dto.InstructorId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}/instructor")]
        public async Task<IActionResult> RemoveInstructor(int id)
        {
            try
            {
                await _instructorsService.RemoveInstructorAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
