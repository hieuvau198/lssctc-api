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
        /// Create a new empty Class
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
        public async Task<IActionResult> AssignTrainee([FromBody] ClassEnrollmentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _classService.EnrollTraineeAsync(dto);
            return Ok(result);
        }
        /// <summary>
        /// Get enrollment by ClassId (returns first enrollment if multiple exist)
        /// </summary>
        [HttpGet("{classId}/enrollment")]
        public async Task<IActionResult> GetEnrollmentByClassId(int classId)
        {
            var result = await _classService.GetClassEnrollmentById(classId);
            if (result == null)
                return NotFound("No enrollment found for this class.");

            return Ok(result);
        }

        /// <summary>
        /// Approve an enrollment and add to ClassMembers
        /// </summary>
        [HttpPost("approve-enrollment")]
        public async Task<IActionResult> ApproveEnrollment([FromBody] ApproveEnrollmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _classService.ApproveEnrollmentAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all members of a class by ClassId
        /// </summary>
        [HttpGet("{classId}/members")]
        public async Task<IActionResult> GetClassMembers(int classId)
        {
            var result = await _classService.GetClassMembersByClassIdAsync(classId);

            if (result == null || !result.Any())
                return NotFound("No members found for this class.");

            return Ok(result);
        }
    }
}
