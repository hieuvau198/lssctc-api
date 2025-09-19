using AutoMapper;
using Lssctc.ProgramManagement.Courses.DTOs;
using Lssctc.ProgramManagement.HttpCustomResponse;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Courses.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<CourseDto>> GetAllCoursesAsync(CourseQueryParameters parameters)
        {
            var query = _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Where(c => c.IsDeleted == false);

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
                Items = pagedResult.Items.Select(c => _mapper.Map<CourseDto>(c)),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            var course = await _unitOfWork.CourseRepository
                .GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            return course == null ? null : _mapper.Map<CourseDto>(course);
        }
        public async Task<PagedResult<CourseDto>> GetCoursesByCategoryAsync(int categoryId, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Where(c => c.IsDeleted == false && c.CategoryId == categoryId)
                .OrderBy(c => c.Name);

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);

            return new PagedResult<CourseDto>
            {
                Items = pagedResult.Items.Select(c => _mapper.Map<CourseDto>(c)),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<PagedResult<CourseDto>> GetCoursesByLevelAsync(int levelId, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Where(c => c.IsDeleted == false && c.LevelId == levelId)
                .OrderBy(c => c.Name);

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);

            return new PagedResult<CourseDto>
            {
                Items = pagedResult.Items.Select(c => _mapper.Map<CourseDto>(c)),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto)
        {
            //validate course category
            if (!dto.CategoryId.HasValue)
                throw new BadRequestException("CategoryId is required.");
            else
            {
                var categoryExists = await _unitOfWork.CourseCategoryRepository.GetByIdAsync(dto.CategoryId.Value);
                if (categoryExists == null)
                    throw new BadRequestException($"Category with Id {dto.CategoryId.Value} does not exist.");
            }
            //validate course level
            if (!dto.LevelId.HasValue)
                throw new BadRequestException("LevelId is required.");
            else
            {
                var levelExists = await _unitOfWork.CourseLevelRepository.GetByIdAsync(dto.LevelId.Value);
                if (levelExists == null)
                    throw new BadRequestException($"Level with Id {dto.LevelId.Value} does not exist.");
            }
            //validate course code
            if (!dto.CourseCodeId.HasValue)
                throw new BadRequestException("CourseCodeId is required.");
            else
            {
                var codeExists = await _unitOfWork.CourseCodeRepository.GetByIdAsync(dto.CourseCodeId.Value);
                if (codeExists == null)
                    throw new BadRequestException($"CourseCode with Id {dto.CourseCodeId.Value} does not exist.");
            }
            //validate course price
            if (!dto.Price.HasValue)
                throw new BadRequestException("Price is required.");
            else if (dto.Price < 0)
                throw new BadRequestException("Price cannot be negative.");
            var newCourse = _mapper.Map<Course>(dto);
            newCourse.IsActive = true;
            newCourse.IsDeleted = false;

            await _unitOfWork.CourseRepository.CreateAsync(newCourse);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseDto>(newCourse);
        }

        public async Task<CourseDto?> UpdateCourseAsync(int id, UpdateCourseDto dto)
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

            //  Validate CourseCodeId
            if (dto.CourseCodeId.HasValue)
            {
                var codeExists = await _unitOfWork.CourseCodeRepository.GetByIdAsync(dto.CourseCodeId.Value);
                if (codeExists == null)
                    throw new BadRequestException($"CourseCode with Id {dto.CourseCodeId.Value} does not exist.");
            }
            //validate course price
            if (!dto.Price.HasValue)
                throw new BadRequestException("Price is required.");
            else if (dto.Price < 0)
                throw new BadRequestException("Price cannot be negative.");
            _mapper.Map(dto, course);
            await _unitOfWork.CourseRepository.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseDto>(course);
        }

        public async Task<bool> DeleteCourseAsync(int id)
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

        public async Task<CourseSyllabusDto> CreateCourseSyllabusAsync(CourseSyllabusCreateDto dto)
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

            var newCourseSyllabus = _mapper.Map<CourseSyllabuse>(dto);
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
                

            return _mapper.Map<CourseSyllabusDto>(newCourseSyllabus);
        }
        public async Task<CourseSyllabusDto?> UpdateCourseSyllabusAsync(int courseSyllabusId, UpdateCourseSyllabusDto dto)
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

            return _mapper.Map<CourseSyllabusDto>(courseSyllabus);
        }
        public async Task<IEnumerable<CourseSyllabusDto>> GetCourseSyllabusesByCourseIdAsync(int courseId)
        {
            var courseSyllabuses = await _unitOfWork.CourseSyllabuseRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .Include(cs => cs.Syllabus)
                .Include(cs => cs.Course)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourseSyllabusDto>>(courseSyllabuses);
        }

    }
}
