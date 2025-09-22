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

    public virtual DbSet<ClassInstructor> ClassInstructors { get; set; }

    public virtual DbSet<ClassMember> ClassMembers { get; set; }

    public virtual DbSet<ClassRegistration> ClassRegistrations { get; set; }

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

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__admins__3213E83F685A1660");

            entity.ToTable("admins");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__admins__id__75A278F5");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__certific__3213E83F2639C3F3");

            entity.ToTable("certificates");

            entity.HasIndex(e => e.Name, "UQ__certific__72E12F1BE6CA2087").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__classes__3213E83FC3FB290E");

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
                .HasConstraintName("FK__classes__class_c__793DFFAF");

            entity.HasOne(d => d.ProgramCourse).WithMany(p => p.Classes)
                .HasForeignKey(d => d.ProgramCourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classes__program__7849DB76");
        });

        modelBuilder.Entity<ClassCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_co__3213E83FED6202B1");

            entity.ToTable("class_codes");

            entity.HasIndex(e => e.Name, "UQ__class_co__72E12F1BFB830AD2").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ClassInstructor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_in__3213E83F2C5079C5");

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
                .HasConstraintName("FK__class_ins__class__01D345B0");

            entity.HasOne(d => d.Instructor).WithMany(p => p.ClassInstructors)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_ins__instr__02C769E9");
        });

        modelBuilder.Entity<ClassMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_me__3213E83FFD05E911");

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
                .HasConstraintName("FK__class_mem__class__0880433F");

            entity.HasOne(d => d.Trainee).WithMany(p => p.ClassMembers)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_mem__train__078C1F06");
        });

        modelBuilder.Entity<ClassRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_re__3213E83FFA4B8DA1");

            entity.ToTable("class_registrations");

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

            entity.HasOne(d => d.Class).WithMany(p => p.ClassRegistrations)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_reg__class__7E02B4CC");

            entity.HasOne(d => d.Trainee).WithMany(p => p.ClassRegistrations)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_reg__train__7EF6D905");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__courses__3213E83F2F21085E");

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
                .HasConstraintName("FK__courses__categor__662B2B3B");

            entity.HasOne(d => d.CourseCode).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CourseCodeId)
                .HasConstraintName("FK__courses__course___681373AD");

            entity.HasOne(d => d.Level).WithMany(p => p.Courses)
                .HasForeignKey(d => d.LevelId)
                .HasConstraintName("FK__courses__level_i__671F4F74");
        });

        modelBuilder.Entity<CourseCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83FFF925C31");

            entity.ToTable("course_categories");

            entity.HasIndex(e => e.Name, "UQ__course_c__72E12F1BC6AA4AAF").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83F24A122E8");

            entity.ToTable("course_certificates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificateId).HasColumnName("certificate_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");

            entity.HasOne(d => d.Certificate).WithMany(p => p.CourseCertificates)
                .HasForeignKey(d => d.CertificateId)
                .HasConstraintName("FK__course_ce__certi__6FB49575");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseCertificates)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__course_ce__cours__6EC0713C");
        });

        modelBuilder.Entity<CourseCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83F13FE7767");

            entity.ToTable("course_codes");

            entity.HasIndex(e => e.Name, "UQ__course_c__72E12F1B4ED84DD8").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CourseLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_l__3213E83FCD6CAAB1");

            entity.ToTable("course_levels");

            entity.HasIndex(e => e.Name, "UQ__course_l__72E12F1B44793371").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__course_s__3213E83F557BF8C3");

            entity.ToTable("course_syllabuses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.SyllabusId).HasColumnName("syllabus_id");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSyllabuses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_sy__cours__72910220");

            entity.HasOne(d => d.Syllabus).WithMany(p => p.CourseSyllabuses)
                .HasForeignKey(d => d.SyllabusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_sy__sylla__73852659");
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__instruct__3213E83FFEBAF182");

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
                .HasConstraintName("FK__instructors__id__00200768");
        });

        modelBuilder.Entity<InstructorProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__instruct__3213E83FD9C38A04");

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
                .HasConstraintName("FK__instructor_p__id__07C12930");
        });

        modelBuilder.Entity<LearningMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83FBA95DBB9");

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
                .HasConstraintName("FK__learning___learn__22751F6C");
        });

        modelBuilder.Entity<LearningMaterialType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F6408A26F");

            entity.ToTable("learning_material_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<LearningRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F50162BFA");

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
                .HasConstraintName("FK__learning___secti__29E1370A");

            entity.HasOne(d => d.TrainingProgress).WithMany(p => p.LearningRecords)
                .HasForeignKey(d => d.TrainingProgressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___train__2AD55B43");
        });

        modelBuilder.Entity<LearningRecordPartition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F9CD0FD4A");

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
                .HasConstraintName("FK__learning___learn__32767D0B");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.LearningRecordPartitions)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___secti__318258D2");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payments__3213E83F88CFC6FD");

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
                .HasConstraintName("FK__payments__traine__66EA454A");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payment___3213E83FB07EDF64");

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
                .HasConstraintName("FK__payment_t__payme__7073AF84");

            entity.HasOne(d => d.Transaction).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payment_t__trans__7167D3BD");
        });

        modelBuilder.Entity<Practice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F441ACAEF");

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
        });

        modelBuilder.Entity<PracticeStep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F2C1A7655");

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
                .HasConstraintName("FK__practice___pract__4A8310C6");
        });

        modelBuilder.Entity<PracticeStepComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F3504BB21");

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
                .HasConstraintName("FK__practice___compo__56E8E7AB");

            entity.HasOne(d => d.Step).WithMany(p => p.PracticeStepComponents)
                .HasForeignKey(d => d.StepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___step___55F4C372");
        });

        modelBuilder.Entity<PracticeStepType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F9DDE0921");

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
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F28DA9958");

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
                .HasConstraintName("FK__practice___pract__51300E55");

            entity.HasOne(d => d.WarningType).WithMany(p => p.PracticeStepWarnings)
                .HasForeignKey(d => d.WarningTypeId)
                .HasConstraintName("FK__practice___warni__5224328E");
        });

        modelBuilder.Entity<PracticeStepWarningType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F8A2ADD9B");

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
            entity.HasKey(e => e.Id).HasName("PK__program___3213E83F2C257403");

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
                .HasConstraintName("FK__program_c__cours__6BE40491");

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramCourses)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_c__progr__6AEFE058");
        });

        modelBuilder.Entity<ProgramEntryRequirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__program___3213E83F99EBB7E9");

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
                .HasConstraintName("FK__program_e__progr__6166761E");
        });

        modelBuilder.Entity<ProgramManager>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__program___3213E83F656598C5");

            entity.ToTable("program_managers");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.ProgramManager)
                .HasForeignKey<ProgramManager>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_mana__id__7B5B524B");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quizzes__3213E83F122EB702");

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
            entity.HasKey(e => e.Id).HasName("PK__quiz_que__3213E83F7E70E710");

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
                .HasConstraintName("FK__quiz_ques__quiz___2A164134");
        });

        modelBuilder.Entity<QuizQuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_que__3213E83F2E1805B0");

            entity.ToTable("quiz_question_options");

            entity.HasIndex(e => e.DisplayOrder, "UQ__quiz_que__1B16645EAEFA11C0").IsUnique();

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
                .HasConstraintName("FK__quiz_ques__quiz___2EDAF651");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sections__3213E83FBAF2AF82");

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
                .HasConstraintName("FK__sections__classe__1E6F845E");

            entity.HasOne(d => d.SyllabusSection).WithMany(p => p.Sections)
                .HasForeignKey(d => d.SyllabusSectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sections__syllab__1F63A897");
        });

        modelBuilder.Entity<SectionMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F9B12E0AC");

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
                .HasConstraintName("FK__section_m__learn__36470DEF");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.SectionMaterials)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_m__secti__3552E9B6");
        });

        modelBuilder.Entity<SectionPartition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FF5BA8E14");

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
                .HasConstraintName("FK__section_p__parti__24285DB4");

            entity.HasOne(d => d.Section).WithMany(p => p.SectionPartitions)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__secti__251C81ED");
        });

        modelBuilder.Entity<SectionPartitionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F8698591E");

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
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FA084F73E");

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
                .HasConstraintName("FK__section_p__pract__40C49C62");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.SectionPractices)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__secti__3FD07829");

            entity.HasOne(d => d.SimulationTimeslot).WithMany(p => p.SectionPractices)
                .HasForeignKey(d => d.SimulationTimeslotId)
                .HasConstraintName("FK__section_p__simul__41B8C09B");
        });

        modelBuilder.Entity<SectionPracticeAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F5A5D4F21");

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
                .HasConstraintName("FK__section_p__learn__5B78929E");

            entity.HasOne(d => d.SectionPractice).WithMany(p => p.SectionPracticeAttempts)
                .HasForeignKey(d => d.SectionPracticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__secti__5A846E65");
        });

        modelBuilder.Entity<SectionPracticeAttemptStep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F5FE7ECBC");

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
                .HasConstraintName("FK__section_p__attem__603D47BB");

            entity.HasOne(d => d.PracticeStep).WithMany(p => p.SectionPracticeAttemptSteps)
                .HasForeignKey(d => d.PracticeStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__pract__61316BF4");
        });

        modelBuilder.Entity<SectionPracticeTimeslot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FC11D1B9A");

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
                .HasConstraintName("FK__section_p__secti__44952D46");

            entity.HasOne(d => d.SimulationTimeslot).WithMany(p => p.SectionPracticeTimeslots)
                .HasForeignKey(d => d.SimulationTimeslotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__simul__4589517F");
        });

        modelBuilder.Entity<SectionQuiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F4A0F09FC");

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
                .HasConstraintName("FK__section_q__quiz___39237A9A");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.SectionQuizzes)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__secti__3A179ED3");
        });

        modelBuilder.Entity<SectionQuizAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FBB7E93A7");

            entity.ToTable("section_quiz_attempts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptOrder).HasColumnName("attempt_order");
            entity.Property(e => e.AttemptScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("attempt_score");
            entity.Property(e => e.IsPass).HasColumnName("is_pass");
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
                .HasConstraintName("FK__section_q__learn__4B422AD5");

            entity.HasOne(d => d.SectionQuiz).WithMany(p => p.SectionQuizAttempts)
                .HasForeignKey(d => d.SectionQuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__secti__4A4E069C");
        });

        modelBuilder.Entity<SectionQuizAttemptAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FBF4FE601");

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
                .HasConstraintName("FK__section_q__secti__53D770D6");
        });

        modelBuilder.Entity<SectionQuizAttemptQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F257B3D8D");

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
                .HasConstraintName("FK__section_q__secti__5006DFF2");
        });

        modelBuilder.Entity<SimulationComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F7EB341C3");

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

        modelBuilder.Entity<SimulationComponentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F55517392");

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
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F7078F101");

            entity.ToTable("simulation_managers");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.SimulationManager)
                .HasForeignKey<SimulationManager>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__simulation_m__id__787EE5A0");
        });

        modelBuilder.Entity<SimulationSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83FD5A82625");

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
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F2E8773CF");

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
            entity.HasKey(e => e.Id).HasName("PK__syllabus__3213E83F6F9E1B3D");

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
                .HasConstraintName("FK__syllabus___sylla__5E8A0973");
        });

        modelBuilder.Entity<Syllabuse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__syllabus__3213E83F71580203");

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
            entity.HasKey(e => e.Id).HasName("PK__trainees__3213E83FCFD32AB9");

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
                .HasConstraintName("FK__trainees__id__04E4BC85");
        });

        modelBuilder.Entity<TraineeCertificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trainee___3213E83F23BFE478");

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
                .HasConstraintName("FK__trainee_c__cours__18B6AB08");

            entity.HasOne(d => d.Trainee).WithMany(p => p.TraineeCertificates)
                .HasForeignKey(d => d.TraineeId)
                .HasConstraintName("FK__trainee_c__train__19AACF41");
        });

        modelBuilder.Entity<TraineeProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trainee___3213E83F4A521A3D");

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
                .HasConstraintName("FK__trainee_prof__id__0A9D95DB");
        });

        modelBuilder.Entity<TrainingProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83F24CE6846");

            entity.ToTable("training_programs");

            entity.HasIndex(e => e.Name, "UQ__training__72E12F1BF78FF02A").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83FDBCFE2CD");

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
                .HasConstraintName("FK__training___cours__0D44F85C");
        });

        modelBuilder.Entity<TrainingResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83FA4D1381E");

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
                .HasConstraintName("FK__training___train__13F1F5EB");

            entity.HasOne(d => d.TrainingResultType).WithMany(p => p.TrainingResults)
                .HasForeignKey(d => d.TrainingResultTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__training___train__12FDD1B2");
        });

        modelBuilder.Entity<TrainingResultType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83F2A40A514");

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
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83F5FD2C591");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.TransactionCode, "UQ__transact__DD5740BE1DCEEB5E").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83F5147D233");

            entity.ToTable("transaction_programs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Program).WithMany(p => p.TransactionPrograms)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__transacti__progr__753864A1");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionPrograms)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__transacti__trans__74444068");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83FBAA658AC");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "UQ__users__F3DBC5727C621851").IsUnique();

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
