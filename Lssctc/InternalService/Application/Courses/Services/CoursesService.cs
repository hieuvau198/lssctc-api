using AutoMapper;
using InternalService.Application.Courses.Dtos;
using InternalService.Application.Courses.Interfaces;
using InternalService.Common;
using InternalService.Domain.Entities;
using InternalService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternalService.Application.Courses.Services;

public class CoursesService : ICoursesService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CoursesService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<CourseDto>> GetCoursesAsync(CourseQueryParameters parameters)
    {
        var query = _unitOfWork.CourseRepository.GetAllAsQueryable()
            .Include(c => c.CourseDefinition)
            .Where(c => c.IsDeleted == false);

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            query = query.Where(c => c.Title.Contains(parameters.SearchTerm) ||
                                   c.CourseCode.Contains(parameters.SearchTerm) ||
                                   c.Description!.Contains(parameters.SearchTerm));
        }

        if (!string.IsNullOrEmpty(parameters.Category))
        {
            query = query.Where(c => c.Category == parameters.Category);
        }

        if (!string.IsNullOrEmpty(parameters.Level))
        {
            query = query.Where(c => c.Level == parameters.Level);
        }

        if (!string.IsNullOrEmpty(parameters.Status))
        {
            query = query.Where(c => c.Status == parameters.Status);
        }

        query = query.OrderBy(c => c.Title);

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
        var course = await _unitOfWork.CourseRepository.GetAllAsQueryable()
            .Include(c => c.CourseDefinition)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

        return course == null ? null : _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto)
    {
        var course = _mapper.Map<Course>(createCourseDto);
        
        await _unitOfWork.CourseRepository.CreateAsync(course);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto?> UpdateCourseAsync(int id, UpdateCourseDto updateCourseDto)
    {
        var course = await _unitOfWork.CourseRepository.GetAllAsQueryable()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

        if (course == null)
            return null;

        _mapper.Map(updateCourseDto, course);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CourseDto>(course);
    }

    public async Task<bool> DeleteCourseAsync(int id)
    {
        var course = await _unitOfWork.CourseRepository.GetAllAsQueryable()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted == false);

        if (course == null)
            return false;

        course.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
