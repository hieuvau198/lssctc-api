using System;
using System.Collections.Generic;
using Lssctc.Share.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.Share.Contexts;

public partial class LssctcDbContext : DbContext
{
    public LssctcDbContext()
    {
    }

    public LssctcDbContext(DbContextOptions<LssctcDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassCode> ClassCodes { get; set; }

    public virtual DbSet<ClassEnrollment> ClassEnrollments { get; set; }

    public virtual DbSet<ClassInstructor> ClassInstructors { get; set; }

    public virtual DbSet<ClassMember> ClassMembers { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseCategory> CourseCategories { get; set; }

    public virtual DbSet<CourseCertificate> CourseCertificates { get; set; }

    public virtual DbSet<CourseCode> CourseCodes { get; set; }

    public virtual DbSet<CourseLevel> CourseLevels { get; set; }

    public virtual DbSet<CourseSyllabuse> CourseSyllabuses { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<InstructorProfile> InstructorProfiles { get; set; }

    public virtual DbSet<LearningMaterial> LearningMaterials { get; set; }

    public virtual DbSet<LearningMaterialType> LearningMaterialTypes { get; set; }

    public virtual DbSet<LearningRecord> LearningRecords { get; set; }

    public virtual DbSet<LearningRecordPartition> LearningRecordPartitions { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<Practice> Practices { get; set; }

    public virtual DbSet<PracticeStep> PracticeSteps { get; set; }

    public virtual DbSet<PracticeStepComponent> PracticeStepComponents { get; set; }

    public virtual DbSet<PracticeStepType> PracticeStepTypes { get; set; }

    public virtual DbSet<PracticeStepWarning> PracticeStepWarnings { get; set; }

    public virtual DbSet<PracticeStepWarningType> PracticeStepWarningTypes { get; set; }

    public virtual DbSet<ProgramCourse> ProgramCourses { get; set; }

    public virtual DbSet<ProgramEntryRequirement> ProgramEntryRequirements { get; set; }

    public virtual DbSet<ProgramManager> ProgramManagers { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<QuizQuestionOption> QuizQuestionOptions { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<SectionMaterial> SectionMaterials { get; set; }

    public virtual DbSet<SectionPartition> SectionPartitions { get; set; }

    public virtual DbSet<SectionPartitionType> SectionPartitionTypes { get; set; }

    public virtual DbSet<SectionPractice> SectionPractices { get; set; }

    public virtual DbSet<SectionPracticeAttempt> SectionPracticeAttempts { get; set; }

    public virtual DbSet<SectionPracticeAttemptStep> SectionPracticeAttemptSteps { get; set; }

    public virtual DbSet<SectionPracticeTimeslot> SectionPracticeTimeslots { get; set; }

    public virtual DbSet<SectionQuiz> SectionQuizzes { get; set; }

    public virtual DbSet<SectionQuizAttempt> SectionQuizAttempts { get; set; }

    public virtual DbSet<SectionQuizAttemptAnswer> SectionQuizAttemptAnswers { get; set; }

    public virtual DbSet<SectionQuizAttemptQuestion> SectionQuizAttemptQuestions { get; set; }

    public virtual DbSet<SimulationComponent> SimulationComponents { get; set; }

    public virtual DbSet<SimulationComponentType> SimulationComponentTypes { get; set; }

    public virtual DbSet<SimulationManager> SimulationManagers { get; set; }

    public virtual DbSet<SimulationSetting> SimulationSettings { get; set; }

    public virtual DbSet<SimulationTimeslot> SimulationTimeslots { get; set; }

    public virtual DbSet<SyllabusSection> SyllabusSections { get; set; }

    public virtual DbSet<Syllabuse> Syllabuses { get; set; }

    public virtual DbSet<Trainee> Trainees { get; set; }

    public virtual DbSet<TraineeCertificate> TraineeCertificates { get; set; }

    public virtual DbSet<TraineeProfile> TraineeProfiles { get; set; }

    public virtual DbSet<TrainingProgram> TrainingPrograms { get; set; }

    public virtual DbSet<TrainingProgress> TrainingProgresses { get; set; }

    public virtual DbSet<TrainingResult> TrainingResults { get; set; }

    public virtual DbSet<TrainingResultType> TrainingResultTypes { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionProgram> TransactionPrograms { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=lssctc-db;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__admins__3213E83FB71AAE67");

            entity.ToTable("admins");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__admins__id__3D5E1FD2");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__certific__3213E83F2AFFA0C3");

            entity.ToTable("certificates");

            entity.HasIndex(e => e.Name, "UQ__certific__72E12F1B8FC8F2D8").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertifyingAuthority)
                .HasMaxLength(2000)
                .HasColumnName("certifying_authority");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.EffectiveTime).HasColumnName("effective_time");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(2000)
                .HasColumnName("image_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Requirement)
                .HasMaxLength(2000)
                .HasColumnName("requirement");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__classes__3213E83FF4C36904");

            entity.ToTable("classes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.ClassCodeId).HasColumnName("class_code_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasPrecision(0)
                .HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProgramCourseId).HasColumnName("program_course_id");
            entity.Property(e => e.StartDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.ClassCode).WithMany(p => p.Classes)
                .HasForeignKey(d => d.ClassCodeId)
                .HasConstraintName("FK__classes__class_c__41EDCAC5");

            entity.HasOne(d => d.ProgramCourse).WithMany(p => p.Classes)
                .HasForeignKey(d => d.ProgramCourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classes__program__40F9A68C");
        });

        modelBuilder.Entity<ClassCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_co__3213E83F2667010B");

            entity.ToTable("class_codes");

            entity.HasIndex(e => e.Name, "UQ__class_co__72E12F1B18C390C0").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ClassEnrollment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_en__3213E83F937E4FB2");

            entity.ToTable("class_enrollments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedDate)
                .HasPrecision(0)
                .HasColumnName("approved_date");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.CreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TraineeContact)
                .HasMaxLength(100)
                .HasColumnName("trainee_contact");
            entity.Property(e => e.TraineeId).HasColumnName("trainee_id");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassEnrollments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_enr__class__46B27FE2");

            entity.HasOne(d => d.Trainee).WithMany(p => p.ClassEnrollments)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_enr__train__47A6A41B");
        });

        modelBuilder.Entity<ClassInstructor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_in__3213E83F2DF1EFDB");

            entity.ToTable("class_instructors");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.Position)
                .HasMaxLength(2000)
                .HasColumnName("position");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassInstructors)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_ins__class__4A8310C6");

            entity.HasOne(d => d.Instructor).WithMany(p => p.ClassInstructors)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_ins__instr__4B7734FF");
        });

        modelBuilder.Entity<ClassMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_me__3213E83F96A2E8CC");

            entity.ToTable("class_members");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("assigned_date");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TraineeId).HasColumnName("trainee_id");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassMembers)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_mem__class__51300E55");

            entity.HasOne(d => d.Trainee).WithMany(p => p.ClassMembers)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_mem__train__503BEA1C");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__courses__3213E83FEEE3173D");

            entity.ToTable("courses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CourseCodeId).HasColumnName("course_code_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.DurationHours).HasColumnName("duration_hours");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(2000)
                .HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LevelId).HasColumnName("level_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");

            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__courses__categor__2EDAF651");

            entity.HasOne(d => d.CourseCode).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CourseCodeId)
                .HasConstraintName("FK__courses__course___30C33EC3");

            entity.HasOne(d => d.Level).WithMany(p => p.Courses)
                .HasForeignKey(d => d.LevelId)
                .HasConstraintName("FK__courses__level_i__2FCF1A8A");
        });

        modelBuilder.Entity<CourseCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83F2DE04F77");

            entity.ToTable("course_categories");

            entity.HasIndex(e => e.Name, "UQ__course_c__72E12F1B574196BF").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CourseCertificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83F429C3704");

            entity.ToTable("course_certificates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificateId).HasColumnName("certificate_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");

            entity.HasOne(d => d.Certificate).WithMany(p => p.CourseCertificates)
                .HasForeignKey(d => d.CertificateId)
                .HasConstraintName("FK__course_ce__certi__3864608B");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseCertificates)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__course_ce__cours__37703C52");
        });

        modelBuilder.Entity<CourseCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83FC530BBD1");

            entity.ToTable("course_codes");

            entity.HasIndex(e => e.Name, "UQ__course_c__72E12F1BDDF6A087").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CourseLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_l__3213E83F346BC416");

            entity.ToTable("course_levels");

            entity.HasIndex(e => e.Name, "UQ__course_l__72E12F1BD296FE3D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CourseSyllabuse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_s__3213E83F0D70CA80");

            entity.ToTable("course_syllabuses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.SyllabusId).HasColumnName("syllabus_id");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSyllabuses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_sy__cours__3B40CD36");

            entity.HasOne(d => d.Syllabus).WithMany(p => p.CourseSyllabuses)
                .HasForeignKey(d => d.SyllabusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_sy__sylla__3C34F16F");
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__instruct__3213E83F848C48BF");

            entity.ToTable("instructors");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.HireDate)
                .HasPrecision(0)
                .HasColumnName("hire_date");
            entity.Property(e => e.InstructorCode)
                .HasMaxLength(2000)
                .HasColumnName("instructor_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Instructor)
                .HasForeignKey<Instructor>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__instructors__id__47DBAE45");
        });

        modelBuilder.Entity<InstructorProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__instruct__3213E83FF5D70ABE");

            entity.ToTable("instructor_profiles");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Biography)
                .HasMaxLength(2000)
                .HasColumnName("biography");
            entity.Property(e => e.ExperienceYears).HasColumnName("experience_years");
            entity.Property(e => e.ProfessionalProfileUrl)
                .HasMaxLength(2000)
                .HasColumnName("professional_profile_url");
            entity.Property(e => e.Specialization)
                .HasMaxLength(2000)
                .HasColumnName("specialization");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.InstructorProfile)
                .HasForeignKey<InstructorProfile>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__instructor_p__id__4F7CD00D");
        });

        modelBuilder.Entity<LearningMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83FB6A2E516");

            entity.ToTable("learning_materials");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.LearningMaterialTypeId).HasColumnName("learning_material_type_id");
            entity.Property(e => e.MaterialUrl)
                .HasMaxLength(2000)
                .HasColumnName("material_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.LearningMaterialType).WithMany(p => p.LearningMaterials)
                .HasForeignKey(d => d.LearningMaterialTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___learn__6A30C649");
        });

        modelBuilder.Entity<LearningMaterialType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83FDCACD9E0");

            entity.ToTable("learning_material_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<LearningRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83FE27ADDAF");

            entity.ToTable("learning_records");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(true)
                .HasColumnName("is_completed");
            entity.Property(e => e.IsTraineeAttended)
                .HasDefaultValue(true)
                .HasColumnName("is_trainee_attended");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Progress)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("progress");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.SectionName)
                .HasMaxLength(255)
                .HasColumnName("section_name");
            entity.Property(e => e.TrainingProgressId).HasColumnName("training_progress_id");

            entity.HasOne(d => d.Section).WithMany(p => p.LearningRecords)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___secti__72910220");

            entity.HasOne(d => d.TrainingProgress).WithMany(p => p.LearningRecords)
                .HasForeignKey(d => d.TrainingProgressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___train__73852659");
        });

        modelBuilder.Entity<LearningRecordPartition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F30C9F96A");

            entity.ToTable("learning_record_partitions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompletedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("completed_at");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsComplete)
                .HasDefaultValue(true)
                .HasColumnName("is_complete");
            entity.Property(e => e.LearningRecordId).HasColumnName("learning_record_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.RecordPartitionOrder).HasColumnName("record_partition_order");
            entity.Property(e => e.SectionPartitionId).HasColumnName("section_partition_id");
            entity.Property(e => e.StartedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("started_at");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.LearningRecord).WithMany(p => p.LearningRecordPartitions)
                .HasForeignKey(d => d.LearningRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___learn__7B264821");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.LearningRecordPartitions)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___secti__7A3223E8");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payments__3213E83F098B7950");

            entity.ToTable("payments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("VND")
                .HasColumnName("currency");
            entity.Property(e => e.Note)
                .HasMaxLength(2000)
                .HasColumnName("note");
            entity.Property(e => e.PaymentDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TraineeId).HasColumnName("trainee_id");

            entity.HasOne(d => d.Trainee).WithMany(p => p.Payments)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payments__traine__2F9A1060");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payment___3213E83FD8796FA3");

            entity.ToTable("payment_transactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payment_t__payme__39237A9A");

            entity.HasOne(d => d.Transaction).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payment_t__trans__3A179ED3");
        });

        modelBuilder.Entity<Practice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83FF387D213");

            entity.ToTable("practices");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_date");
            entity.Property(e => e.DifficultyLevel)
                .HasMaxLength(2000)
                .HasColumnName("difficulty_level");
            entity.Property(e => e.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.MaxAttempts).HasColumnName("max_attempts");
            entity.Property(e => e.PracticeDescription)
                .HasMaxLength(2000)
                .HasColumnName("practice_description");
            entity.Property(e => e.PracticeName)
                .HasMaxLength(2000)
                .HasColumnName("practice_name");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
        });

        modelBuilder.Entity<PracticeStep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F50BA4ACB");

            entity.ToTable("practice_steps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExpectedResult)
                .HasMaxLength(2000)
                .HasColumnName("expected_result");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PracticeId).HasColumnName("practice_id");
            entity.Property(e => e.StepDescription)
                .HasMaxLength(2000)
                .HasColumnName("step_description");
            entity.Property(e => e.StepName)
                .HasMaxLength(2000)
                .HasColumnName("step_name");
            entity.Property(e => e.StepOrder).HasColumnName("step_order");

            entity.HasOne(d => d.Practice).WithMany(p => p.PracticeSteps)
                .HasForeignKey(d => d.PracticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___pract__1332DBDC");
        });

        modelBuilder.Entity<PracticeStepComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F8CDA7A2B");

            entity.ToTable("practice_step_components");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ComponentId).HasColumnName("component_id");
            entity.Property(e => e.ComponentOrder).HasColumnName("component_order");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.StepId).HasColumnName("step_id");

            entity.HasOne(d => d.Component).WithMany(p => p.PracticeStepComponents)
                .HasForeignKey(d => d.ComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___compo__1F98B2C1");

            entity.HasOne(d => d.Step).WithMany(p => p.PracticeStepComponents)
                .HasForeignKey(d => d.StepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___step___1EA48E88");
        });

        modelBuilder.Entity<PracticeStepType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F5D38A531");

            entity.ToTable("practice_step_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PracticeStepWarning>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F1CF393E5");

            entity.ToTable("practice_step_warnings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Instruction)
                .HasMaxLength(2000)
                .HasColumnName("instruction");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PracticeStepId).HasColumnName("practice_step_id");
            entity.Property(e => e.WarningMessage)
                .HasMaxLength(2000)
                .HasColumnName("warning_message");
            entity.Property(e => e.WarningTypeId).HasColumnName("warning_type_id");

            entity.HasOne(d => d.PracticeStep).WithMany(p => p.PracticeStepWarnings)
                .HasForeignKey(d => d.PracticeStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___pract__19DFD96B");

            entity.HasOne(d => d.WarningType).WithMany(p => p.PracticeStepWarnings)
                .HasForeignKey(d => d.WarningTypeId)
                .HasConstraintName("FK__practice___warni__1AD3FDA4");
        });

        modelBuilder.Entity<PracticeStepWarningType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83FACFD83FF");

            entity.ToTable("practice_step_warning_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsFailedCriteria)
                .HasDefaultValue(false)
                .HasColumnName("is_failed_criteria");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ProgramCourse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__program___3213E83F561C4F88");

            entity.ToTable("program_courses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseOrder).HasColumnName("course_order");
            entity.Property(e => e.CoursesId).HasColumnName("courses_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");

            entity.HasOne(d => d.Courses).WithMany(p => p.ProgramCourses)
                .HasForeignKey(d => d.CoursesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_c__cours__3493CFA7");

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramCourses)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_c__progr__339FAB6E");
        });

        modelBuilder.Entity<ProgramEntryRequirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__program___3213E83F0DA5E6AC");

            entity.ToTable("program_entry_requirements");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.DocumentUrl)
                .HasMaxLength(2000)
                .HasColumnName("document_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramEntryRequirements)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_e__progr__2A164134");
        });

        modelBuilder.Entity<ProgramManager>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__program___3213E83F63E7560B");

            entity.ToTable("program_managers");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.ProgramManager)
                .HasForeignKey<ProgramManager>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_mana__id__4316F928");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quizzes__3213E83F60E66E6E");

            entity.ToTable("quizzes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PassScoreCriteria)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("pass_score_criteria");
            entity.Property(e => e.TimelimitMinute).HasColumnName("timelimit_minute");
            entity.Property(e => e.TotalScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("total_score");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_que__3213E83F677D17E2");

            entity.ToTable("quiz_questions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsMultipleAnswers)
                .HasDefaultValue(true)
                .HasColumnName("is_multiple_answers");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.QuestionScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("question_score");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_ques__quiz___71D1E811");
        });

        modelBuilder.Entity<QuizQuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_que__3213E83FC0890C36");

            entity.ToTable("quiz_question_options");

            entity.HasIndex(e => e.DisplayOrder, "UQ__quiz_que__1B16645E76A837DC").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(true)
                .HasColumnName("is_correct");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.OptionScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("option_score");
            entity.Property(e => e.QuizQuestionId).HasColumnName("quiz_question_id");

            entity.HasOne(d => d.QuizQuestion).WithMany(p => p.QuizQuestionOptions)
                .HasForeignKey(d => d.QuizQuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_ques__quiz___76969D2E");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sections__3213E83F1618AC11");

            entity.ToTable("sections");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassesId).HasColumnName("classes_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.EndDate)
                .HasPrecision(0)
                .HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.StartDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.SyllabusSectionId).HasColumnName("syllabus_section_id");

            entity.HasOne(d => d.Classes).WithMany(p => p.Sections)
                .HasForeignKey(d => d.ClassesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sections__classe__671F4F74");

            entity.HasOne(d => d.SyllabusSection).WithMany(p => p.Sections)
                .HasForeignKey(d => d.SyllabusSectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sections__syllab__681373AD");
        });

        modelBuilder.Entity<SectionMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FA6C652A4");

            entity.ToTable("section_materials");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.LearningMaterialId).HasColumnName("learning_material_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.SectionPartitionId).HasColumnName("section_partition_id");

            entity.HasOne(d => d.LearningMaterial).WithMany(p => p.SectionMaterials)
                .HasForeignKey(d => d.LearningMaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_m__learn__7EF6D905");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.SectionMaterials)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_m__secti__7E02B4CC");
        });

        modelBuilder.Entity<SectionPartition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F5FB2F18A");

            entity.ToTable("section_partitions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PartitionTypeId).HasColumnName("partition_type_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");

            entity.HasOne(d => d.PartitionType).WithMany(p => p.SectionPartitions)
                .HasForeignKey(d => d.PartitionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__parti__6CD828CA");

            entity.HasOne(d => d.Section).WithMany(p => p.SectionPartitions)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__secti__6DCC4D03");
        });

        modelBuilder.Entity<SectionPartitionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FCD5E0CE1");

            entity.ToTable("section_partition_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsActionRequired).HasColumnName("is_action_required");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PassCriteria)
                .HasMaxLength(2000)
                .HasColumnName("pass_criteria");
        });

        modelBuilder.Entity<SectionPractice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F65A389A2");

            entity.ToTable("section_practices");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomDeadline)
                .HasPrecision(0)
                .HasColumnName("custom_deadline");
            entity.Property(e => e.CustomDescription)
                .HasMaxLength(2000)
                .HasColumnName("custom_description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PracticeId).HasColumnName("practice_id");
            entity.Property(e => e.SectionPartitionId).HasColumnName("section_partition_id");
            entity.Property(e => e.SimulationTimeslotId).HasColumnName("simulation_timeslot_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.Practice).WithMany(p => p.SectionPractices)
                .HasForeignKey(d => d.PracticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__pract__09746778");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.SectionPractices)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__secti__0880433F");

            entity.HasOne(d => d.SimulationTimeslot).WithMany(p => p.SectionPractices)
                .HasForeignKey(d => d.SimulationTimeslotId)
                .HasConstraintName("FK__section_p__simul__0A688BB1");
        });

        modelBuilder.Entity<SectionPracticeAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FAC1D2852");

            entity.ToTable("section_practice_attempts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("attempt_date");
            entity.Property(e => e.AttemptStatus)
                .HasDefaultValue(1)
                .HasColumnName("attempt_status");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsPass)
                .HasDefaultValue(false)
                .HasColumnName("is_pass");
            entity.Property(e => e.LearningRecordPartitionId).HasColumnName("learning_record_partition_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("score");
            entity.Property(e => e.SectionPracticeId).HasColumnName("section_practice_id");

            entity.HasOne(d => d.LearningRecordPartition).WithMany(p => p.SectionPracticeAttempts)
                .HasForeignKey(d => d.LearningRecordPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__learn__24285DB4");

            entity.HasOne(d => d.SectionPractice).WithMany(p => p.SectionPracticeAttempts)
                .HasForeignKey(d => d.SectionPracticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__secti__2334397B");
        });

        modelBuilder.Entity<SectionPracticeAttemptStep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F05844D58");

            entity.ToTable("section_practice_attempt_steps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptId).HasColumnName("attempt_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsPass)
                .HasDefaultValue(false)
                .HasColumnName("is_pass");
            entity.Property(e => e.PracticeStepId).HasColumnName("practice_step_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("score");

            entity.HasOne(d => d.Attempt).WithMany(p => p.SectionPracticeAttemptSteps)
                .HasForeignKey(d => d.AttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__attem__28ED12D1");

            entity.HasOne(d => d.PracticeStep).WithMany(p => p.SectionPracticeAttemptSteps)
                .HasForeignKey(d => d.PracticeStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__pract__29E1370A");
        });

        modelBuilder.Entity<SectionPracticeTimeslot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F0AC7CF56");

            entity.ToTable("section_practice_timeslots");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Note)
                .HasMaxLength(1000)
                .HasColumnName("note");
            entity.Property(e => e.SectionPracticeId).HasColumnName("section_practice_id");
            entity.Property(e => e.SimulationTimeslotId).HasColumnName("simulation_timeslot_id");

            entity.HasOne(d => d.SectionPractice).WithMany(p => p.SectionPracticeTimeslots)
                .HasForeignKey(d => d.SectionPracticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__secti__0D44F85C");

            entity.HasOne(d => d.SimulationTimeslot).WithMany(p => p.SectionPracticeTimeslots)
                .HasForeignKey(d => d.SimulationTimeslotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__simul__0E391C95");
        });

        modelBuilder.Entity<SectionQuiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FFD4F56B1");

            entity.ToTable("section_quizzes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");
            entity.Property(e => e.SectionPartitionId).HasColumnName("section_partition_id");

            entity.HasOne(d => d.Quiz).WithMany(p => p.SectionQuizzes)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__quiz___01D345B0");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.SectionQuizzes)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__secti__02C769E9");
        });

        modelBuilder.Entity<SectionQuizAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FF5FF9D55");

            entity.ToTable("section_quiz_attempts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptOrder).HasColumnName("attempt_order");
            entity.Property(e => e.AttemptScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("attempt_score");
            entity.Property(e => e.LearningRecordPartitionId).HasColumnName("learning_record_partition_id");
            entity.Property(e => e.MaxScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("max_score");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.QuizAttemptDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("quiz_attempt_date");
            entity.Property(e => e.SectionQuizId).HasColumnName("section_quiz_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.LearningRecordPartition).WithMany(p => p.SectionQuizAttempts)
                .HasForeignKey(d => d.LearningRecordPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__learn__13F1F5EB");

            entity.HasOne(d => d.SectionQuiz).WithMany(p => p.SectionQuizAttempts)
                .HasForeignKey(d => d.SectionQuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__secti__12FDD1B2");
        });

        modelBuilder.Entity<SectionQuizAttemptAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FBBBDD4A8");

            entity.ToTable("section_quiz_attempt_answers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("attempt_score");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(true)
                .HasColumnName("is_correct");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.SectionQuizAttemptQuestionId).HasColumnName("section_quiz_attempt_question_id");

            entity.HasOne(d => d.SectionQuizAttemptQuestion).WithMany(p => p.SectionQuizAttemptAnswers)
                .HasForeignKey(d => d.SectionQuizAttemptQuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__secti__1C873BEC");
        });

        modelBuilder.Entity<SectionQuizAttemptQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F6E24AE75");

            entity.ToTable("section_quiz_attempt_questions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("attempt_score");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(true)
                .HasColumnName("is_correct");
            entity.Property(e => e.IsMultipleAnswers)
                .HasDefaultValue(true)
                .HasColumnName("is_multiple_answers");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.QuestionScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("question_score");
            entity.Property(e => e.SectionQuizAttemptId).HasColumnName("section_quiz_attempt_id");

            entity.HasOne(d => d.SectionQuizAttempt).WithMany(p => p.SectionQuizAttemptQuestions)
                .HasForeignKey(d => d.SectionQuizAttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__secti__18B6AB08");
        });

        modelBuilder.Entity<SimulationComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83FD66238B5");

            entity.ToTable("simulation_components");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(2000)
                .HasColumnName("image_url");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
        });

        modelBuilder.Entity<SimulationComponentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83FF9B4F38A");

            entity.ToTable("simulation_component_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SimulationManager>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F556E6523");

            entity.ToTable("simulation_managers");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.SimulationManager)
                .HasForeignKey<SimulationManager>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__simulation_m__id__403A8C7D");
        });

        modelBuilder.Entity<SimulationSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F9B821752");

            entity.ToTable("simulation_settings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SimulationTimeslot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F6D5B796E");

            entity.ToTable("simulation_timeslots");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndTime)
                .HasPrecision(0)
                .HasColumnName("end_time");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasMaxLength(2000)
                .HasColumnName("note");
            entity.Property(e => e.StartTime)
                .HasPrecision(0)
                .HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
        });

        modelBuilder.Entity<SyllabusSection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__syllabus__3213E83F03DE7290");

            entity.ToTable("syllabus_sections");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.SectionDescription)
                .HasMaxLength(2000)
                .HasColumnName("section_description");
            entity.Property(e => e.SectionOrder).HasColumnName("section_order");
            entity.Property(e => e.SectionTitle)
                .HasMaxLength(200)
                .HasColumnName("section_title");
            entity.Property(e => e.SyllabusId).HasColumnName("syllabus_id");

            entity.HasOne(d => d.Syllabus).WithMany(p => p.SyllabusSections)
                .HasForeignKey(d => d.SyllabusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__syllabus___sylla__2739D489");
        });

        modelBuilder.Entity<Syllabuse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__syllabus__3213E83F8BFA1DEE");

            entity.ToTable("syllabuses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(100)
                .HasColumnName("course_code");
            entity.Property(e => e.CourseName)
                .HasMaxLength(100)
                .HasColumnName("course_name");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Trainee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trainees__3213E83F70C510DA");

            entity.ToTable("trainees");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.TraineeCode)
                .HasMaxLength(2000)
                .HasColumnName("trainee_code");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Trainee)
                .HasForeignKey<Trainee>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__trainees__id__4CA06362");
        });

        modelBuilder.Entity<TraineeCertificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trainee___3213E83F7F0CE9E1");

            entity.ToTable("trainee_certificates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseCertificateId).HasColumnName("course_certificate_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IssuedDateEnd)
                .HasPrecision(0)
                .HasColumnName("issued_date_end");
            entity.Property(e => e.IssuedDateStart)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("issued_date_start");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TraineeId).HasColumnName("trainee_id");
            entity.Property(e => e.ValidDateEnd)
                .HasPrecision(0)
                .HasColumnName("valid_date_end");

            entity.HasOne(d => d.CourseCertificate).WithMany(p => p.TraineeCertificates)
                .HasForeignKey(d => d.CourseCertificateId)
                .HasConstraintName("FK__trainee_c__cours__6166761E");

            entity.HasOne(d => d.Trainee).WithMany(p => p.TraineeCertificates)
                .HasForeignKey(d => d.TraineeId)
                .HasConstraintName("FK__trainee_c__train__625A9A57");
        });

        modelBuilder.Entity<TraineeProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trainee___3213E83F9BD75A5F");

            entity.ToTable("trainee_profiles");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CitizenCardId)
                .HasMaxLength(20)
                .HasColumnName("citizen_card_id");
            entity.Property(e => e.CitizenCardImageUrl)
                .HasMaxLength(2000)
                .HasColumnName("citizen_card_image_url");
            entity.Property(e => e.CitizenCardIssuedDate).HasColumnName("citizen_card_issued_date");
            entity.Property(e => e.CitizenCardPlaceOfIssue)
                .HasMaxLength(255)
                .HasColumnName("citizen_card_place_of_issue");
            entity.Property(e => e.DriverLicenseImageUrl)
                .HasMaxLength(2000)
                .HasColumnName("driver_license_image_url");
            entity.Property(e => e.DriverLicenseIssuedDate)
                .HasPrecision(0)
                .HasColumnName("driver_license_issued_date");
            entity.Property(e => e.DriverLicenseLevel)
                .HasMaxLength(50)
                .HasColumnName("driver_license_level");
            entity.Property(e => e.DriverLicenseNumber)
                .HasMaxLength(100)
                .HasColumnName("driver_license_number");
            entity.Property(e => e.DriverLicenseValidEndDate)
                .HasPrecision(0)
                .HasColumnName("driver_license_valid_end_date");
            entity.Property(e => e.DriverLicenseValidStartDate)
                .HasPrecision(0)
                .HasColumnName("driver_license_valid_start_date");
            entity.Property(e => e.EducationImageUrl)
                .HasMaxLength(2000)
                .HasColumnName("education_image_url");
            entity.Property(e => e.EducationLevel)
                .HasMaxLength(255)
                .HasColumnName("education_level");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.TraineeProfile)
                .HasForeignKey<TraineeProfile>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__trainee_prof__id__52593CB8");
        });

        modelBuilder.Entity<TrainingProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83F69C8212E");

            entity.ToTable("training_programs");

            entity.HasIndex(e => e.Name, "UQ__training__72E12F1B6C141BC7").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.DurationHours).HasColumnName("duration_hours");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(2000)
                .HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.TotalCourses).HasColumnName("total_courses");
        });

        modelBuilder.Entity<TrainingProgress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83F2076811E");

            entity.ToTable("training_progresses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseMemberId).HasColumnName("course_member_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.LastUpdated)
                .HasPrecision(0)
                .HasColumnName("last_updated");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProgressPercentage).HasColumnName("progress_percentage");
            entity.Property(e => e.StartDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.CourseMember).WithMany(p => p.TrainingProgresses)
                .HasForeignKey(d => d.CourseMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__training___cours__55F4C372");
        });

        modelBuilder.Entity<TrainingResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83FD0299780");

            entity.ToTable("training_results");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Notes)
                .HasMaxLength(2000)
                .HasColumnName("notes");
            entity.Property(e => e.ResultDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("result_date");
            entity.Property(e => e.ResultValue)
                .HasMaxLength(2000)
                .HasColumnName("result_value");
            entity.Property(e => e.TrainingProgressId).HasColumnName("training_progress_id");
            entity.Property(e => e.TrainingResultTypeId).HasColumnName("training_result_type_id");

            entity.HasOne(d => d.TrainingProgress).WithMany(p => p.TrainingResults)
                .HasForeignKey(d => d.TrainingProgressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__training___train__5CA1C101");

            entity.HasOne(d => d.TrainingResultType).WithMany(p => p.TrainingResults)
                .HasForeignKey(d => d.TrainingResultTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__training___train__5BAD9CC8");
        });

        modelBuilder.Entity<TrainingResultType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83F50AB9848");

            entity.ToTable("training_result_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83F2EA38936");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.TransactionCode, "UQ__transact__DD5740BE53168FBF").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("VND")
                .HasColumnName("currency");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IssuedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("issued_at");
            entity.Property(e => e.Message)
                .HasMaxLength(2000)
                .HasColumnName("message");
            entity.Property(e => e.Note)
                .HasMaxLength(2000)
                .HasColumnName("note");
            entity.Property(e => e.PaidAt)
                .HasPrecision(0)
                .HasColumnName("paid_at");
            entity.Property(e => e.PayerId).HasColumnName("payer_id");
            entity.Property(e => e.PayerName)
                .HasMaxLength(2000)
                .HasColumnName("payer_name");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.ReceiverName)
                .HasMaxLength(2000)
                .HasColumnName("receiver_name");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(255)
                .HasColumnName("transaction_code");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .HasColumnName("transaction_type");
        });

        modelBuilder.Entity<TransactionProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83F15D3D46F");

            entity.ToTable("transaction_programs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Program).WithMany(p => p.TransactionPrograms)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__transacti__progr__3DE82FB7");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionPrograms)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__transacti__trans__3CF40B7E");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F84665E54");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "UQ__users__F3DBC572041694C8").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(2000)
                .HasColumnName("avatar_url");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(2000)
                .HasColumnName("fullname");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Password)
                .HasMaxLength(2000)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(2000)
                .HasColumnName("phone_number");
            entity.Property(e => e.Role)
                .HasDefaultValue(5)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
