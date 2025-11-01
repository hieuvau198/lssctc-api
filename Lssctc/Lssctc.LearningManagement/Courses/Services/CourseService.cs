using Lssctc.LearningManagement.Courses.DTOs;
using Lssctc.ProgramManagement.HttpCustomResponse;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.LearningManagement.Courses.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Manual mapping methods
        private CourseDto MapCourseToDto(Course course)
        {
            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                CategoryId = course.CategoryId,
                CategoryName = course.Category?.Name,
                LevelId = course.LevelId,
                LevelName = course.Level?.Name,
                Price = course.Price,
                IsActive = course.IsActive,
                ImageUrl = course.ImageUrl,
                DurationHours = course.DurationHours,
                CourseCodeName = course.CourseCode?.Name
            };
        }

        private Course MapCreateDtoToCourse(CreateCourseDto dto)
        {
            return new Course
            {
                Name = dto.Name,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                LevelId = dto.LevelId,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                DurationHours = dto.DurationHours
            };
        }

        private void MapUpdateDtoToCourse(UpdateCourseDto dto, Course course)
        {
            course.Name = dto.Name;
            course.Description = dto.Description;
            course.CategoryId = dto.CategoryId;
            course.LevelId = dto.LevelId;
            course.Price = dto.Price;
            course.IsActive = dto.IsActive;
            course.ImageUrl = dto.ImageUrl;
            course.DurationHours = dto.DurationHours;
        }

        private CourseSyllabusDto MapCourseSyllabusToDto(CourseSyllabuse courseSyllabus)
        {
            return new CourseSyllabusDto
            {
                Id = courseSyllabus.Id,
                CourseId = courseSyllabus.CourseId,
                SyllabusId = courseSyllabus.SyllabusId,
                CourseName = courseSyllabus.Course?.Name,
                SyllabusName = courseSyllabus.Syllabus?.Name,
                SyllabusDescription = courseSyllabus.Syllabus?.Description
            };
        }

        private CourseSyllabuse MapCreateSyllabusDtoToEntity(CourseSyllabusCreateDto dto)
        {
            return new CourseSyllabuse
            {
                CourseId = dto.CourseId
            };
        }

        private CourseCategoryDto MapCourseCategoryToDto(CourseCategory category)
        {
            return new CourseCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        private CourseLevelDto MapCourseLevelToDto(CourseLevel level)
        {
            return new CourseLevelDto
            {
                Id = level.Id,
                Name = level.Name,
                Description = level.Description
            };
        }

        public async Task<List<CourseDto>> GetAllCourses()
        {
            var courses = await _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
                .Where(c => c.IsDeleted == false)
                .OrderBy(c => c.Id)
                .ToListAsync();
            
            return courses.Select(MapCourseToDto).ToList();
        }

        public async Task<PagedResult<CourseDto>> GetCourses(CourseQueryParameters parameters)
        {
            IQueryable<Course> query = _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
                .Where(predicate: c => c.IsDeleted == false)
                ;

            // 🔍 Filtering
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                query = query.Where(c => c.Name.Contains(parameters.SearchTerm));

            }

            if (parameters.CategoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == parameters.CategoryId.Value);
            }

            if (parameters.LevelId.HasValue)
            {
                query = query.Where(c => c.LevelId == parameters.LevelId.Value);
            }

            if (parameters.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == parameters.IsActive.Value);
            }

            // Default sort
            query = query.OrderBy(c => c.Name);

            var pagedResult = await query.ToPagedResultAsync(parameters.PageNumber, parameters.PageSize);

            return new PagedResult<CourseDto>
            {
                Items = pagedResult.Items.Select(MapCourseToDto),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<CourseDto?> GetCourseById(int id)
        {
            var course = await _unitOfWork.CourseRepository
                .GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
                .FirstOrDefaultAsync(c => c.Id == id);

            return course == null ? null : MapCourseToDto(course);
        }

        public async Task<PagedResult<CourseDto>> GetCoursesByCategoryId(int categoryId, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
                .Where(c =>  c.CategoryId == categoryId)
                .OrderBy(c => c.Name);

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);

            return new PagedResult<CourseDto>
            {
                Items = pagedResult.Items.Select(MapCourseToDto),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<PagedResult<CourseDto>> GetCoursesByLevelId(int levelId, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
                .Where(c => c.LevelId == levelId)
                .OrderBy(c => c.Name);

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);

            return new PagedResult<CourseDto>
            {
                Items = pagedResult.Items.Select(MapCourseToDto),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<PagedResult<CourseDto>> GetCoursesByFilter(int? categoryId, int? levelId, int pageNumber, int pageSize)
        {
            IQueryable<Course> query = _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode);

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId.Value);

            if (levelId.HasValue)
                query = query.Where(c => c.LevelId == levelId.Value);

            query = query.OrderBy(c => c.Name);

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);

            return new PagedResult<CourseDto>
            {
                Items = pagedResult.Items.Select(MapCourseToDto),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<CourseDto> CreateCourse(CreateCourseDto dto)
        {
            // validate category
            if (!dto.CategoryId.HasValue)
                throw new BadRequestException("CategoryId is required.");
            var categoryExists = await _unitOfWork.CourseCategoryRepository.GetByIdAsync(dto.CategoryId.Value);
            if (categoryExists == null)
                throw new BadRequestException($"Category with Id {dto.CategoryId.Value} does not exist.");

            // validate level
            if (!dto.LevelId.HasValue)
                throw new BadRequestException("LevelId is required.");
            var levelExists = await _unitOfWork.CourseLevelRepository.GetByIdAsync(dto.LevelId.Value);
            if (levelExists == null)
                throw new BadRequestException($"Level with Id {dto.LevelId.Value} does not exist.");

            // validate price
            if (!dto.Price.HasValue)
                throw new BadRequestException("Price is required.");
            if (dto.Price < 0)
                throw new BadRequestException("Price cannot be negative.");

            // validate course code uniqueness when provided
            if (!string.IsNullOrWhiteSpace(dto.CourseCodeName))
            {
                var existingCode = await _unitOfWork.CourseCodeRepository
                    .GetAllAsQueryable()
                    .FirstOrDefaultAsync(cc => cc.Name == dto.CourseCodeName);
                if (existingCode != null)
                    throw new BadRequestException($"Course code '{dto.CourseCodeName}' already exists.");
            }

            // ✅ create course code
            var newCourseCode = new CourseCode
            {
                Name = dto.CourseCodeName
            };
            await _unitOfWork.CourseCodeRepository.CreateAsync(newCourseCode);
            await _unitOfWork.SaveChangesAsync();

            // map dto → entity
            var newCourse = MapCreateDtoToCourse(dto);
            newCourse.CourseCodeId = newCourseCode.Id;
            newCourse.IsActive = true;
            newCourse.IsDeleted = false;

            await _unitOfWork.CourseRepository.CreateAsync(newCourse);
            await _unitOfWork.SaveChangesAsync();

            return MapCourseToDto(newCourse);
        }

        public async Task<CourseDto?> UpdateCourseById(int id, UpdateCourseDto dto)
        {
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
            if (course == null || course.IsDeleted == true) return null;
            
            //  Validate CategoryId
            if (dto.CategoryId.HasValue)
            {
                var categoryExists = await _unitOfWork.CourseCategoryRepository.GetByIdAsync(dto.CategoryId.Value);
                if (categoryExists == null)
                    throw new BadRequestException($"Category with Id {dto.CategoryId.Value} does not exist.");
            }

            //  Validate LevelId
            if (dto.LevelId.HasValue)
            {
                var levelExists = await _unitOfWork.CourseLevelRepository.GetByIdAsync(dto.LevelId.Value);
                if (levelExists == null)
                    throw new BadRequestException($"Level with Id {dto.LevelId.Value} does not exist.");
            }

            //validate course price
            if (!dto.Price.HasValue)
                throw new BadRequestException("Price is required.");
            else if (dto.Price < 0)
                throw new BadRequestException("Price cannot be negative.");
            
            MapUpdateDtoToCourse(dto, course);
            await _unitOfWork.CourseRepository.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return MapCourseToDto(course);
        }

        public async Task<bool> DeleteCourseById(int id)
        {
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);

            if (course == null || course.IsDeleted == true) return false;

            // Soft delete
            course.IsDeleted = true;
            course.IsActive = false;

            await _unitOfWork.CourseRepository.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<CourseSyllabusDto> CreateSyllabus(CourseSyllabusCreateDto dto)
        {
            // check for existence of coursesyllabus
            var exists = await _unitOfWork.CourseSyllabuseRepository
                .GetAllAsQueryable().FirstOrDefaultAsync(x => x.CourseId == dto.CourseId);

            if (exists != null)
                throw new InvalidOperationException("This course already contains the syllabus.");

            //get course 
            var course = await _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.CourseCode)
                .FirstOrDefaultAsync(c => c.Id == dto.CourseId);

            var newCourseSyllabus = MapCreateSyllabusDtoToEntity(dto);
            
            //create syllabus
            var sysllbus = new Syllabuse
            {
                Description = dto.SyllabusDescription,
                Name = dto.SyllabusName,
                CourseCode = course.CourseCode.Name,
                CourseName = course.Name,
                IsActive = true,
                IsDeleted = false
            };
            await _unitOfWork.SyllabuseRepository.CreateAsync(sysllbus);

            await _unitOfWork.SaveChangesAsync();
            
            //create coursesyllabus
            newCourseSyllabus.SyllabusId = sysllbus.Id;
            await _unitOfWork.CourseSyllabuseRepository.CreateAsync(newCourseSyllabus);
            await _unitOfWork.SaveChangesAsync();

            // reload with navigation props
            newCourseSyllabus = await _unitOfWork.CourseSyllabuseRepository
                .GetAllAsQueryable()
                .Include(cl => cl.Course)
                .Include(cl => cl.Syllabus)
                .FirstOrDefaultAsync(cl => cl.Id == newCourseSyllabus.Id);
                
            return MapCourseSyllabusToDto(newCourseSyllabus);
        }

        public async Task<CourseSyllabusDto?> UpdateSyllabusById(int courseSyllabusId, UpdateCourseSyllabusDto dto)
        {
            var courseSyllabus = await _unitOfWork.CourseSyllabuseRepository
                .GetAllAsQueryable()
                .Include(cs => cs.Syllabus)
                .Include(cs => cs.Course)
                .FirstOrDefaultAsync(cs => cs.Id == courseSyllabusId);

            if (courseSyllabus == null)
                return null;

            // Update syllabus details
            courseSyllabus.Syllabus.Name = dto.Name;
            courseSyllabus.Syllabus.Description = dto.Description;

            await _unitOfWork.SyllabuseRepository.UpdateAsync(courseSyllabus.Syllabus);
            await _unitOfWork.SaveChangesAsync();

            return MapCourseSyllabusToDto(courseSyllabus);
        }

        public async Task<CourseSyllabusDto> GetSyllabusByCourseId(int courseId)
        {
            var courseSyllabuses = await _unitOfWork.CourseSyllabuseRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .Include(cs => cs.Syllabus)
                .Include(cs => cs.Course).FirstOrDefaultAsync();

            return MapCourseSyllabusToDto(courseSyllabuses);
        }

        public async Task<IEnumerable<CourseCategoryDto>> GetAllCourseCategoriesAsync()
        {
            var categories = await _unitOfWork.CourseCategoryRepository
                .GetAllAsQueryable()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(MapCourseCategoryToDto);
        }

        public async Task<IEnumerable<CourseLevelDto>> GetAllCourseLevelsAsync()
        {
            var levels = await _unitOfWork.CourseLevelRepository
                .GetAllAsQueryable()
                .OrderBy(l => l.Name)
                .ToListAsync();

            return levels.Select(MapCourseLevelToDto);
        }
    }
}
