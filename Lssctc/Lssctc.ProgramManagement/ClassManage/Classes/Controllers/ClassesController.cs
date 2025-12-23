using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.ProgramManagement.ClassManage.Classes.Services;
using Lssctc.ProgramManagement.ClassManage.Enrollments.Dtos;
using Lssctc.ProgramManagement.ClassManage.Enrollments.Services;
using Lssctc.ProgramManagement.HttpCustomResponse; // Added for BadRequestException
using Lssctc.Share.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : ControllerBase
    {
        private readonly IClassesService _service;
        private readonly IClassInstructorsService _instructorsService;
        private readonly IEnrollmentsService _enrollmentsService;

        public ClassesController(
            IClassesService service,
            IClassInstructorsService instructorsService,
            IEnrollmentsService enrollmentsService)
        {
            _service = service;
            _instructorsService = instructorsService;
            _enrollmentsService = enrollmentsService;
        }

        #region Class

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllClassesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var result = await _service.GetClassesAsync(pageNumber, pageSize, searchTerm, sortBy, sortDirection, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClassDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetClassByIdAsync(id);
                if (result == null) return NotFound(new { message = $"Class with ID {id} not found." });
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ClassDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
        {
            try
            {
                var result = await _service.CreateClassAsync(dto);
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(ClassDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClassDto dto)
        {
            try
            {
                var result = await _service.UpdateClassAsync(id, dto);
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPut("{id}/open")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Open(int id)
        {
            try
            {
                await _service.OpenClassAsync(id);
                return Ok(new { message = "Class opened successfully." });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPut("{id}/start")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> Start(int id)
        {
            try
            {
                await _service.StartClassAsync(id);
                return Ok(new { message = "Class started successfully." });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> CompleteClass(int id)
        {
            try
            {
                await _service.CompleteClassAsync(id);
                return Ok(new { message = "Class completed successfully." });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CancelClass(int id)
        {
            try
            {
                await _service.CancelClassAsync(id);
                return Ok(new { message = "Class cancelled successfully." });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        #endregion

        #region Class Instructor

        [HttpGet("{id}/instructor")]
        [ProducesResponseType(typeof(ClassInstructorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInstructor(int id)
        {
            try
            {
                var result = await _instructorsService.GetInstructorByClassIdAsync(id);
                if (result == null) return NotFound(new { message = "No instructor found for this class." });
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("available-instructors")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<ClassInstructorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailableInstructors(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _instructorsService.GetAvailableInstructorsAsync(startDate, endDate);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost("{id}/instructor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignInstructor(int id, [FromBody] AssignInstructorDto dto)
        {
            try
            {
                await _instructorsService.AssignInstructorAsync(id, dto.InstructorId);
                return Ok(new { message = "Instructor assigned successfully." });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}/instructor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveInstructor(int id)
        {
            try
            {
                await _instructorsService.RemoveInstructorAsync(id);
                return Ok(new { message = "Instructor removed successfully." });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion

        #region Class Trainees

        [HttpGet("{id}/trainees")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(PagedResult<EnrollmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTraineesByClass(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _enrollmentsService.GetEnrollmentsForClassAsync(id, pageNumber, pageSize);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost("{id}/import-trainees")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportTraineesToClass(int id, IFormFile file)
        {
            try
            {
                var result = await _service.ImportTraineesToClassAsync(id, file);
                return Ok(new { message = result });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion

        #region Classes By other Filters

        [HttpGet("program/{programId}/course/{courseId}")]
        [ProducesResponseType(typeof(IEnumerable<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByProgramAndCourse(int programId, int courseId)
        {
            try
            {
                var result = await _service.GetClassesByProgramAndCourseAsync(programId, courseId);
                if (result == null || !result.Any())
                    return NotFound(new { message = "No classes found for this program and course." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("program/{programId}/course/{courseId}/available")]
        [ProducesResponseType(typeof(IEnumerable<ClassWithEnrollmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableByProgramAndCourse(int programId, int courseId)
        {
            try
            {
                int? traineeId = GetUserIdFromClaimsOptional();
                var result = await _service.GetAvailableClassesByProgramCourseForTraineeAsync(programId, courseId, traineeId);
                return Ok(result ?? new List<ClassWithEnrollmentDto>());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("course/{courseId}")]
        [ProducesResponseType(typeof(IEnumerable<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            try
            {
                var result = await _service.GetClassesByCourseAsync(courseId);
                if (result == null || !result.Any())
                    return NotFound(new { message = "No classes found for this course." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("course/{courseId}/for-trainee")]
        [Authorize(Roles = "Admin, Trainee")]
        [ProducesResponseType(typeof(IEnumerable<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCourseForTrainee(int courseId)
        {
            try
            {
                var result = await _service.GetClassesByCourseIdForTrainee(courseId);
                if (result == null || !result.Any())
                    return NotFound(new { message = "No open or in-progress classes found for this course." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("instructor/{instructorId}")]
        [Authorize(Roles = "Admin, Instructor")]
        [ProducesResponseType(typeof(IEnumerable<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByInstructor(int instructorId)
        {
            try
            {
                var result = await _service.GetClassesByInstructorAsync(instructorId);
                if (result == null || !result.Any())
                    return NotFound(new { message = "No classes found for this instructor." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion

        #region Classes By Trainee

        [HttpGet("trainee/{traineeId}")]
        [Authorize(Roles = "Admin, Instructor, Trainee")]
        [ProducesResponseType(typeof(IEnumerable<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByTrainee(int traineeId)
        {
            try
            {
                var result = await _service.GetAllClassesByTraineeAsync(traineeId);
                if (result == null || !result.Any())
                    return NotFound(new { message = "No classes found for this trainee." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("trainee/{traineeId}/paged")]
        [Authorize(Roles = "Admin, Instructor, Trainee")]
        [ProducesResponseType(typeof(PagedResult<ClassDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedByTrainee(int traineeId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetPagedClassesByTraineeAsync(traineeId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("trainee/{traineeId}/class/{classId}")]
        [Authorize(Roles = "Admin, Instructor, Trainee")]
        [ProducesResponseType(typeof(ClassDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClassForTrainee(int traineeId, int classId)
        {
            try
            {
                var result = await _service.GetClassByIdAndTraineeAsync(classId, traineeId);
                if (result == null)
                    return NotFound(new { message = "Class not found for this trainee." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        [HttpGet("my-classes")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(IEnumerable<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyClasses()
        {
            try
            {
                var traineeId = GetUserIdFromClaims();
                var result = await _service.GetAllClassesByTraineeAsync(traineeId);
                if (result == null || !result.Any())
                    return NotFound(new { message = "No classes found for you." });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("my-classes/paged")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(PagedResult<ClassDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyPagedClasses([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var traineeId = GetUserIdFromClaims();
                var result = await _service.GetPagedClassesByTraineeAsync(traineeId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("my-classes/{classId}")]
        [Authorize(Roles = "Trainee")]
        [ProducesResponseType(typeof(ClassDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyClassById(int classId)
        {
            try
            {
                var traineeId = GetUserIdFromClaims();
                var result = await _service.GetClassByIdAndTraineeAsync(classId, traineeId);
                if (result == null)
                    return NotFound(new { message = "Class not found or you are not enrolled." });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        #endregion

        #region Helpers
        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
        }

        private int? GetUserIdFromClaimsOptional()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
        #endregion

        #region Hard Delete for Demo

        [HttpDelete("{id}/hard-delete")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteClassDataRecursive(int id)
        {
            try
            {
                await _service.DeleteClassDataRecursiveAsync(id);
                return Ok(new { message = $"Class with ID {id} and all associated data has been permanently deleted." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred while deleting class data: {ex.Message}" });
            }
        }

        #endregion
    }
}