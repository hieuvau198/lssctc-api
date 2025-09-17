using Lssctc.ProgramManagement.Classes.DTOs;
using Lssctc.ProgramManagement.Classes.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lssctc.ProgramManagement.Classes.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassesController(IClassService classService)
        {
            _classService = classService;
        }

        /// <summary>
        /// Create a new Class
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _classService.CreateClassAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Assign an Instructor to a Class
        /// </summary>
        [HttpPost("assign-instructor")]
        public async Task<IActionResult> AssignInstructor([FromBody] AssignInstructorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _classService.AssignInstructorAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Assign a Trainee to a Class
        /// </summary>
        [HttpPost("assign-trainee")]
        public async Task<IActionResult> AssignTrainee([FromBody] AssignTraineeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _classService.AssignTraineeAsync(dto);
            return Ok(result);
        }
    }
}
