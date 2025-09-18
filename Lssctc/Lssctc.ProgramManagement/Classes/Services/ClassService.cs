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

            
            if (dto.ClassCodeId != null)
            {
                // Use existing ClassCode
                var classCode = await _unitOfWork.ClassCodeRepository.GetByIdAsync(dto.ClassCodeId);
                if (classCode == null)
                    throw new Exception("Invalid ClassCodeId, Class code doesnt exist");
            }

            var entity = _mapper.Map<Class>(dto);

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

        //public async Task<ClassDto?> AssignTraineeAsync(AssignTraineeDto dto)
        //{
        //    var classEntity = await _unitOfWork.ClassRepository
        //        .GetAllAsQueryable()
        //        .Include(c => c.ClassMembers)
        //        .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

        //    if (classEntity == null)
        //        throw new Exception("Class not found");

        //    // Check if trainee already assigned
        //    if (classEntity.ClassMembers.Any(cm => cm.TraineeId == dto.TraineeId))
        //        throw new Exception("Trainee already assigned to this class");

        //    var newMember = new ClassMember
        //    {
        //        ClassId = dto.ClassId,
        //        TraineeId = dto.TraineeId,
        //        AssignedDate = DateTime.UtcNow,
        //        Status = 1 ,
        //    };

        //    classEntity.ClassMembers.Add(newMember);
        //    await _unitOfWork.SaveChangesAsync();

        //    return _mapper.Map<ClassDto>(classEntity);
        //}

        //create class enrollment
        public async Task<ClassEnrollmentDto> EnrollTraineeAsync(ClassEnrollmentCreateDto dto)
        {
            // validate class
            var classEntity = await _unitOfWork.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ClassEnrollments)
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            if (classEntity == null)
                throw new Exception("Class not found");

            // validate trainee
            var trainee = await _unitOfWork.TraineeRepository.GetByIdAsync(dto.TraineeId);
            if (trainee == null)
                throw new Exception("Trainee not found");

            // check if already enrolled
            if (classEntity.ClassEnrollments.Any(e => e.TraineeId == dto.TraineeId))
                throw new Exception("Trainee already enrolled in this class");

            // map to entity
            var enrollment = _mapper.Map<ClassEnrollment>(dto);
            enrollment.Status = 0; // 0 = Pending

            await _unitOfWork.ClassEnrollmentRepository.CreateAsync(enrollment);
            await _unitOfWork.SaveChangesAsync();

            // reload with nav props
            enrollment = await _unitOfWork.ClassEnrollmentRepository
                .GetAllAsQueryable()
                .Include(e => e.Class)
                .Include(e => e.Trainee)
                .FirstOrDefaultAsync(e => e.Id == enrollment.Id);

            return _mapper.Map<ClassEnrollmentDto>(enrollment);
        }

        public async Task<ClassEnrollmentDto> GetClassEnrollmentById(int classid)
        {
            return _mapper.Map<ClassEnrollmentDto>(await _unitOfWork.ClassEnrollmentRepository.GetAllAsQueryable().FirstOrDefaultAsync(ce => ce.ClassId == classid));
        }

        public async Task<ClassMemberDto> ApproveEnrollmentAsync(ApproveEnrollmentDto dto)
        {
            // get enrollment
            var enrollment = await _unitOfWork.ClassEnrollmentRepository
                .GetAllAsQueryable()
                .Include(e => e.Class)
                .Include(e => e.Trainee)
                .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

            if (enrollment == null)
                throw new Exception("Enrollment not found");

            if (enrollment.Status == 1) // already approved
                throw new Exception("Enrollment already approved");

            // update enrollment
            enrollment.Status = 1; // 1 = Approved
            enrollment.ApprovedDate = DateTime.UtcNow;
            enrollment.Description = dto.Description ?? enrollment.Description;

            _unitOfWork.ClassEnrollmentRepository.UpdateAsync(enrollment);

            // create class member
            var member = new ClassMember
            {
                
                ClassId = enrollment.ClassId,
                TraineeId = enrollment.TraineeId,
                AssignedDate = DateTime.UtcNow,
                Status = 1 // Active
            };

            await _unitOfWork.ClassMemberRepository.CreateAsync(member);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClassMemberDto>(member);
        }

        public async Task<IEnumerable<ClassMemberDto>> GetClassMembersByClassIdAsync(int classId)
        {
            var members = await _unitOfWork.ClassMemberRepository
                .GetAllAsQueryable()
                .Include(cm => cm.Trainee)
                .Where(cm => cm.ClassId == classId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClassMemberDto>>(members);
        }
    }
}
