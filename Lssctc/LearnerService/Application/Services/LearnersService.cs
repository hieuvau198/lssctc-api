using AutoMapper;
using LearnerService.Application.Common;
using LearnerService.Application.Dtos;
using LearnerService.Application.Interfaces;
using LearnerService.Domain.Entities;
using LearnerService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LearnerService.Application.Services;

public class LearnersService : ILearnersService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LearnersService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<LearnerDto>> GetLearnersAsync(LearnerQueryParameters parameters)
    {
        var query = _unitOfWork.LearnerRepository.GetAllAsQueryable()
            .Include(l => l.User)
            .Where(l => l.IsDeleted == false);

        if (!string.IsNullOrEmpty(parameters.EnrollmentStatus))
        {
            query = query.Where(l => l.EnrollmentStatus == parameters.EnrollmentStatus);
        }

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            query = query.Where(l => l.User.Fullname.Contains(parameters.SearchTerm));
        }

        query = query.OrderBy(l => l.User.Fullname);

        var pagedResult = await query.ToPagedResultAsync(parameters.PageNumber, parameters.PageSize);

        return new PagedResult<LearnerDto>
        {
            Items = pagedResult.Items.Select(l => _mapper.Map<LearnerDto>(l)),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<LearnerDto?> GetLearnerByIdAsync(int userId)
    {
        var learner = await _unitOfWork.LearnerRepository.GetAllAsQueryable()
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.UserId == userId && l.IsDeleted == false);

        return learner == null ? null : _mapper.Map<LearnerDto>(learner);
    }

    public async Task<LearnerDto> CreateLearnerAsync(CreateLearnerDto createLearnerDto)
    {
        var learner = _mapper.Map<Learner>(createLearnerDto);
        learner.EnrollmentStatus = "active";
        await _unitOfWork.LearnerRepository.CreateAsync(learner);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LearnerDto>(learner);
    }

    public async Task<LearnerDto?> UpdateLearnerAsync(int userId, UpdateLearnerDto updateLearnerDto)
    {
        var learner = await _unitOfWork.LearnerRepository.GetAllAsQueryable()
            .FirstOrDefaultAsync(l => l.UserId == userId && l.IsDeleted == false);

        if (learner == null)
            return null;

        _mapper.Map(updateLearnerDto, learner);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LearnerDto>(learner);
    }

    public async Task<bool> DeleteLearnerAsync(int userId)
    {
        var learner = await _unitOfWork.LearnerRepository.GetAllAsQueryable()
            .FirstOrDefaultAsync(l => l.UserId == userId && l.IsDeleted == false);

        if (learner == null)
            return false;

        learner.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}