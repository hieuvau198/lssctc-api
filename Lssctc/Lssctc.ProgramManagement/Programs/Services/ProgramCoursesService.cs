
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public class ProgramCoursesService : IProgramCoursesService
    {
        private readonly IUnitOfWork _uow;
        public ProgramCoursesService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        
        public async Task AddCourseToProgramAsync(int programId, int courseId)
        {
            if(programId <= 0 || courseId <= 0)
            {
                throw new ArgumentException("Program ID and Course ID must be valid.");
            }
            var program = await _uow.ProgramRepository
                .GetAllAsQueryable()
                .Where(p => p.Id == programId && p.IsDeleted != true)
                .Include(p => p.ProgramCourses)
                .FirstOrDefaultAsync();
            if(program == null)
            {
                throw new KeyNotFoundException($"Program with ID {programId} not found.");
            }
            var course = await _uow.CourseRepository
                .GetByIdAsync(courseId);
            if(course == null)
            {
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");
            }
            var existingProgramCourse = program.ProgramCourses
                .FirstOrDefault(pc => pc.CourseId == courseId);
            if(existingProgramCourse != null)
            {
                throw new InvalidOperationException($"Course with ID {courseId} is already added to Program with ID {programId}.");
            }
            int nextOrder = (program.ProgramCourses.Any())
                ? program.ProgramCourses.Max(pc => pc.CourseOrder) + 1
                : 1;
            var programCourse = new ProgramCourse
            {
                ProgramId = programId,
                CourseId = courseId,
                CourseOrder = nextOrder,
                Name = course.Name,
                Description = course.Description
            };
            await _uow.ProgramCourseRepository.CreateAsync(programCourse);
            await _uow.SaveChangesAsync();

        }

        public async Task UpdateProgramCourseAsync(int programId, int courseId, int newOrder)
        {
            if (programId <= 0 || courseId <= 0 || newOrder <= 0)
            {
                throw new ArgumentException("Program ID, Course ID, and new order must be valid.");
            }

            var program = await _uow.ProgramRepository
                .GetAllAsQueryable()
                .Where(p => p.Id == programId && p.IsDeleted != true)
                .Include(p => p.ProgramCourses)
                .FirstOrDefaultAsync();

            if (program == null)
            {
                throw new KeyNotFoundException($"Program with ID {programId} not found.");
            }

            var programCourses = program.ProgramCourses
                .OrderBy(pc => pc.CourseOrder)
                .ToList();

            var targetCourse = programCourses.FirstOrDefault(pc => pc.CourseId == courseId);
            if (targetCourse == null)
            {
                throw new KeyNotFoundException(
                    $"Course with ID {courseId} not found in Program with ID {programId}."
                );
            }

            if (newOrder > programCourses.Count)
            {
                newOrder = programCourses.Count;
            }

            // Reorder
            programCourses.Remove(targetCourse);
            programCourses.Insert(newOrder - 1, targetCourse);

            // Reassign new sequential orders
            for (int i = 0; i < programCourses.Count; i++)
            {
                programCourses[i].CourseOrder = i + 1;
                await _uow.ProgramCourseRepository.UpdateAsync(programCourses[i]);
            }

            await _uow.SaveChangesAsync();
        }

        public async Task RemoveCourseFromProgramAsync(int programId, int courseId)
        {
            if (programId <= 0 || courseId <= 0)
            {
                throw new ArgumentException("Program ID and Course ID must be valid.");
            }

            var program = await _uow.ProgramRepository
                .GetAllAsQueryable()
                .Where(p => p.Id == programId && p.IsDeleted != true)
                .Include(p => p.ProgramCourses)
                    .ThenInclude(pc => pc.Classes)
                .FirstOrDefaultAsync();

            if (program == null)
            {
                throw new KeyNotFoundException($"Program with ID {programId} not found.");
            }

            var programCourse = program.ProgramCourses
                .FirstOrDefault(pc => pc.CourseId == courseId);

            if (programCourse == null)
            {
                throw new KeyNotFoundException(
                    $"Course with ID {courseId} not found in Program with ID {programId}."
                );
            }
            if(programCourse.Classes != null && programCourse.Classes.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot remove Course with ID {courseId} from Program with ID {programId} because it has associated classes."
                );
            }

            await _uow.ProgramCourseRepository.DeleteAsync(programCourse);
            await _uow.SaveChangesAsync();

            // Reorder remaining courses
            var remainingCourses = program.ProgramCourses
                .Where(pc => pc.CourseId != courseId)
                .OrderBy(pc => pc.CourseOrder)
                .ToList();

            int order = 1;
            foreach (var pc in remainingCourses)
            {
                pc.CourseOrder = order++;
                await _uow.ProgramCourseRepository.UpdateAsync(pc);
            }

            await _uow.SaveChangesAsync();
        }

    }
}
