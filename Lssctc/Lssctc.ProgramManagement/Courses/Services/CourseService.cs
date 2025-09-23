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

        public async Task<PagedResult<CourseDto>> GetCourses(CourseQueryParameters parameters)
        {
            var query = _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
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

        public async Task<CourseDto?> GetCourseById(int id)
        {
            var course = await _unitOfWork.CourseRepository
                .GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

            return course == null ? null : _mapper.Map<CourseDto>(course);
        }
        public async Task<PagedResult<CourseDto>> GetCoursesByCategoryId(int categoryId, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
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

        public async Task<PagedResult<CourseDto>> GetCoursesByLevelId(int levelId, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
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
        public async Task<PagedResult<CourseDto>> GetCoursesByFilter(int? categoryId, int? levelId, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.CourseRepository.GetAllAsQueryable()
                .Include(c => c.Category)
                .Include(c => c.Level)
                .Include(c => c.CourseCode)
                .Where(c => c.IsDeleted == false);

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId.Value);

            if (levelId.HasValue)
                query = query.Where(c => c.LevelId == levelId.Value);

            query = query.OrderBy(c => c.Name);

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);

            return new PagedResult<CourseDto>
            {
                Items = pagedResult.Items.Select(c => _mapper.Map<CourseDto>(c)),
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

            // ✅ create course code
            var newCourseCode = new CourseCode
            {
                Name = dto.CourseCodeName
            };
            await _unitOfWork.CourseCodeRepository.CreateAsync(newCourseCode);
            await _unitOfWork.SaveChangesAsync();

            // map dto → entity
            var newCourse = _mapper.Map<Course>(dto);
            newCourse.CourseCodeId = newCourseCode.Id;
            newCourse.IsActive = true;
            newCourse.IsDeleted = false;

            await _unitOfWork.CourseRepository.CreateAsync(newCourse);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseDto>(newCourse);
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
            _mapper.Map(dto, course);
            await _unitOfWork.CourseRepository.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseDto>(course);
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
            // for test: courseSyllabus.SyllabusId = another

            await _unitOfWork.SyllabuseRepository.UpdateAsync(courseSyllabus.Syllabus);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseSyllabusDto>(courseSyllabus);
        }
        public async Task<CourseSyllabusDto> GetSyllabusByCourseId(int courseId)
        {
            var courseSyllabuses = await _unitOfWork.CourseSyllabuseRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .Include(cs => cs.Syllabus)
                .Include(cs => cs.Course).FirstOrDefaultAsync();

            return _mapper.Map<CourseSyllabusDto>(courseSyllabuses);
        }
        public async Task<IEnumerable<CourseCategoryDto>> GetAllCourseCategoriesAsync()
        {
            var categories = await _unitOfWork.CourseCategoryRepository
                .GetAllAsQueryable()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourseCategoryDto>>(categories);
        }

        public async Task<IEnumerable<CourseLevelDto>> GetAllCourseLevelsAsync()
        {
            var levels = await _unitOfWork.CourseLevelRepository
                .GetAllAsQueryable()
                .OrderBy(l => l.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourseLevelDto>>(levels);
        }

    }
}
