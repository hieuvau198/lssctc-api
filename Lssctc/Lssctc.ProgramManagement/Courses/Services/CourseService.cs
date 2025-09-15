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
            var entity = _mapper.Map<Course>(dto);
            entity.IsActive = true;
            entity.IsDeleted = false;

            await _unitOfWork.CourseRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseDto>(entity);
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
    }
}
