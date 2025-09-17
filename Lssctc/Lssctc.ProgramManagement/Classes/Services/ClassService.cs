using AutoMapper;
using Lssctc.ProgramManagement.Classes.DTOs;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Classes.Services
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ClassDto> CreateClassAsync(ClassCreateDto dto)
        {
            // Validate ProgramCourse
            var programCourse = await _unitOfWork.ProgramCourseRepository.GetByIdAsync(dto.ProgramCourseId);
               

            if (programCourse == null)
                throw new Exception("Invalid ProgramCourseId");
            ClassCode? classCode = null;

            if (dto.ClassCode != null)
            {
                if (dto.ClassCode.Id.HasValue)
                {
                    // Use existing ClassCode
                    classCode = await _unitOfWork.ClassCodeRepository.GetByIdAsync(dto.ClassCode.Id.Value);

                    if (classCode == null)
                        throw new Exception("Invalid ClassCodeId");
                }
                else if (!string.IsNullOrWhiteSpace(dto.ClassCode.Name))
                {
                    // Create new ClassCode
                    classCode = new ClassCode { Name = dto.ClassCode.Name };
                    await _unitOfWork.ClassCodeRepository.CreateAsync(classCode);
                }
            }

            var entity = _mapper.Map<Class>(dto);

            if (classCode != null)
                entity.ClassCode = classCode;

            await _unitOfWork.ClassRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClassDto>(entity);
        }
        public async Task<ClassDto> AssignInstructorAsync(AssignInstructorDto dto)
        {
            // Validate Class
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(dto.ClassId);
            if (classEntity == null)
                throw new Exception("Invalid ClassId");

            // Validate Instructor
            var instructor = await _unitOfWork.InstructorRepository.GetByIdAsync(dto.InstructorId);
            if (instructor == null)
                throw new Exception("Invalid InstructorId");

            // Check if already assigned
            var existing = classEntity.ClassInstructors
                .FirstOrDefault(ci => ci.InstructorId == dto.InstructorId);

            if (existing != null)
                throw new Exception("Instructor already assigned to this class");

            // Create ClassInstructor
            var classInstructor = new ClassInstructor
            {
                ClassId = dto.ClassId,
                InstructorId = dto.InstructorId,
                Position = dto.Position
            };

            await _unitOfWork.ClassInstructorRepository.CreateAsync(classInstructor);
            await _unitOfWork.SaveChangesAsync();

            // Reload class with instructors
            var updatedClass =  _unitOfWork.ClassRepository.GetAllAsQueryable().Include(c => c.ClassInstructors).Where(c => c.Id == dto.ClassId);

            return _mapper.Map<ClassDto>(updatedClass);
        }

        public async Task<ClassDto?> AssignTraineeAsync(AssignTraineeDto dto)
        {
            var classEntity = await _unitOfWork.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ClassMembers)
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            if (classEntity == null)
                throw new Exception("Class not found");

            // Check if trainee already assigned
            if (classEntity.ClassMembers.Any(cm => cm.TraineeId == dto.TraineeId))
                throw new Exception("Trainee already assigned to this class");

            var newMember = new ClassMember
            {
                ClassId = dto.ClassId,
                TraineeId = dto.TraineeId,
                AssignedDate = DateTime.UtcNow,
                Status = 1 ,
            };

            classEntity.ClassMembers.Add(newMember);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClassDto>(classEntity);
        }

    }
}
