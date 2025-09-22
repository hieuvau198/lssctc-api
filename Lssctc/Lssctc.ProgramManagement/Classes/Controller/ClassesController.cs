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

            try
            {
                var result = await _classService.CreateClassByProgramCourseId(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Assign an Instructor to a Class
        /// </summary>
        [HttpPost("assign-instructor")]
        public async Task<IActionResult> AssignInstructor([FromBody] AssignInstructorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _classService.AssignInstructorToClass(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Assign a Trainee to a Class
        /// </summary>
        [HttpPost("assign-trainee")]
        public async Task<IActionResult> AssignTrainee([FromBody] ClassEnrollmentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _classService.EnrollTrainee(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get enrollment by ClassId (returns first enrollment if multiple exist)
        /// </summary>
        [HttpGet("{classId}/enrollment")]
        public async Task<IActionResult> GetEnrollmentByClassId(int classId)
        {
            try
            {
                var result = await _classService.GetClassEnrollmentById(classId);
                if (result == null)
                    return NotFound("No enrollment found for this class.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
                var result = await _classService.ApproveEnrollment(dto);
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
            try
            {
                var result = await _classService.GetMembersByClassId(classId);
                if (result == null || !result.Any())
                    return NotFound("No members found for this class.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get Instructor of a class by ClassId
        /// </summary>
        [HttpGet("{classId}/instructor")]
        public async Task<IActionResult> GetInstructor(int classId)
        {
            try
            {
                var result = await _classService.GetInstructorByClassId(classId);
                if (result == null)
                    return NotFound("No instructor found for this class.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //
        // TRAINING PROGRESS
        //

        /// <summary>
        /// Get training progress list by ClassMemberId
        /// </summary>
        [HttpGet("members/{memberId}/progress")]
        public async Task<IActionResult> GetTrainingProgressByMember(int memberId)
        {
            try
            {
                var result = await _classService.GetProgressByMember(memberId);
                if (result == null || !result.Any())
                    return NotFound("No training progress found for this member.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Add new training progress for a ClassMember
        /// </summary>
        [HttpPost("progress")]
        public async Task<IActionResult> AddTrainingProgress([FromBody] CreateTrainingProgressDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _classService.CreateProgress(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update training progress
        /// </summary>
        [HttpPut("progress")]
        public async Task<IActionResult> UpdateTrainingProgress([FromBody] UpdateTrainingProgressDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _classService.UpdateProgress(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //
        // TRAINING RESULT
        //

        /// <summary>
        /// Get all training results by TrainingProgressId
        /// </summary>
        [HttpGet("progress/{progressId}/results")]
        public async Task<IActionResult> GetTrainingResults(int progressId)
        {
            try
            {
                var result = await _classService.GetResultsByProgress(progressId);
                if (result == null || !result.Any())
                    return NotFound("No results found for this training progress.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Add a new training result
        /// </summary>
        [HttpPost("results")]
        public async Task<IActionResult> AddTrainingResult([FromBody] CreateTrainingResultDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _classService.CreateResult(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update an existing training result
        /// </summary>
        [HttpPut("results")]
        public async Task<IActionResult> UpdateTrainingResult([FromBody] UpdateTrainingResultDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _classService.UpdateResult(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a training result
        /// </summary>
        //[HttpDelete("results/{id}")]
        //public async Task<IActionResult> DeleteTrainingResult(int id)
        //{
        //    var success = await _classService.DeleteTrainingResultAsync(id);
        //    if (!success)
        //        return NotFound("Training result not found.");
        //    return NoContent();
        //}

        /// <summary>
        /// Create a new Section for a Class
        /// </summary>
        [HttpPost("sections")]
        public async Task<IActionResult> CreateSection([FromBody] SectionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _classService.CreateSectionAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //
        // SYLLABUS SECTION
        //

        /// <summary>
        /// Create a new Syllabus Section
        /// </summary>
        [HttpPost("syllabus-sections")]
        public async Task<IActionResult> CreateSyllabusSection([FromBody] SyllabusSectionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _classService.CreateSyllabusSectionAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
