USE [lssctc-db]

BEGIN TRY
    BEGIN TRANSACTION;

    -- ========================================
    --  # Account area
    -- ========================================
    CREATE TABLE [dbo].[users] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [username] NVARCHAR(255) NOT NULL UNIQUE,
        [password] NVARCHAR(2000) NOT NULL,
        [email] NVARCHAR(255) NOT NULL,
        [fullname] NVARCHAR(2000),
        [role] INT DEFAULT 5,	-- Admin=1, ProgramManager=2, SimulationManager=3, Instructor=4, Trainee=5
        [phone_number] NVARCHAR(2000),
        [avatar_url] NVARCHAR(2000),
        [is_active] BIT NOT NULL DEFAULT 1,
        [is_deleted] BIT NOT NULL DEFAULT 0
    );

    CREATE TABLE [dbo].[admins] (
        [id] INT PRIMARY KEY,
        FOREIGN KEY ([id]) REFERENCES [dbo].[users]([id])
    );

    CREATE TABLE [dbo].[simulation_managers] (
        [id] INT PRIMARY KEY,
        FOREIGN KEY ([id]) REFERENCES [dbo].[users]([id])
    );

    CREATE TABLE [dbo].[instructors] (
        [id] INT PRIMARY KEY,
        [hire_date] DATETIME2(0),
        [instructor_code] NVARCHAR(2000),
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0,
        FOREIGN KEY ([id]) REFERENCES [dbo].[users]([id])
    );

    CREATE TABLE [dbo].[trainees] (
        [id] INT PRIMARY KEY,
        [trainee_code] NVARCHAR(2000) NOT NULL,
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0,
        FOREIGN KEY ([id]) REFERENCES [dbo].[users]([id])
    );

    CREATE TABLE [dbo].[instructor_profiles] (
        [id] INT PRIMARY KEY,
        [experience_years] INT,
        [biography] NVARCHAR(2000),
        [professional_profile_url] NVARCHAR(2000),
        [specialization] NVARCHAR(2000),
        FOREIGN KEY ([id]) REFERENCES [dbo].[instructors]([id])
    );

    CREATE TABLE [dbo].[trainee_profiles] (
        [id] INT PRIMARY KEY, 
        [driver_license_number] NVARCHAR(100),        
        [driver_license_level] NVARCHAR(50),          
        [driver_license_issued_date] DATETIME2(0),
        [driver_license_valid_start_date] DATETIME2(0),
        [driver_license_valid_end_date] DATETIME2(0),
        [driver_license_image_url] NVARCHAR(2000),
        [education_level] NVARCHAR(255),
        [education_image_url] NVARCHAR(2000),
        [citizen_card_id] NVARCHAR(20),         
        [citizen_card_issued_date] DATE,            
        [citizen_card_place_of_issue] NVARCHAR(255),
        [citizen_card_image_url] NVARCHAR(2000),
        FOREIGN KEY ([id]) REFERENCES [dbo].[trainees]([id])
    );

    -- ========================================
    --  # Program & Course area
    -- ========================================
    CREATE TABLE [dbo].[training_programs] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL UNIQUE,
        [description] NVARCHAR(2000),
        [is_deleted] BIT DEFAULT 0,
        [is_active] BIT DEFAULT 1,
        [duration_hours] INT,
        [total_courses] INT,
        [image_url] NVARCHAR(2000)
    );

    CREATE TABLE [dbo].[course_codes] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL UNIQUE
    );

    CREATE TABLE [dbo].[course_categories] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL UNIQUE,
        [description] NVARCHAR(2000)
    );

    CREATE TABLE [dbo].[course_levels] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL UNIQUE,
        [description] NVARCHAR(2000)
    );

    CREATE TABLE [dbo].[courses] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL,
        [description] NVARCHAR(2000),
        [category_id] INT,
        [level_id] INT,
        [price] DECIMAL(10, 2),
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0,
        [image_url] NVARCHAR(2000),
        [duration_hours] INT,
        [course_code_id] INT,
        FOREIGN KEY ([category_id]) REFERENCES [dbo].[course_categories]([id]),
        FOREIGN KEY ([level_id]) REFERENCES [dbo].[course_levels]([id]),
        FOREIGN KEY ([course_code_id]) REFERENCES [dbo].[course_codes]([id])
    );

    CREATE TABLE [dbo].[program_courses] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [program_id] INT NOT NULL, 
        [courses_id] INT NOT NULL,
        [course_order] INT NOT NULL,
        [name] NVARCHAR(100),
        [description] NVARCHAR(2000),
        FOREIGN KEY ([program_id]) REFERENCES [dbo].[training_programs]([id]),
        FOREIGN KEY ([courses_id]) REFERENCES [dbo].[courses]([id])
    );

    -- ========================================
    --  # Section area
    -- ========================================
    CREATE TABLE [dbo].[sections] (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        [section_title] NVARCHAR(200) NOT NULL,
        [section_description] NVARCHAR(2000),
        [estimated_duration_minutes] INT,
        [is_deleted] BIT DEFAULT 0
    );

    CREATE TABLE [dbo].[course_sections](
        Id INT IDENTITY(1,1) PRIMARY KEY,
        [course_id] INT NOT NULL,
        [section_id] INT NOT NULL,
        [course_section_order] INT NOT NULL,       
        FOREIGN KEY ([course_id]) REFERENCES [dbo].[courses]([id]),
        FOREIGN KEY ([section_id]) REFERENCES [dbo].[sections]([id])
    );

    -- ========================================
    --  # Activity area
    -- ========================================
    CREATE TABLE [dbo].[activities] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [activity_title] NVARCHAR(200) NOT NULL,
        [activity_description] NVARCHAR(2000),
        [activity_type] NVARCHAR(100),   -- e.g., Video, Quiz, Reading, Simulation
        [estimated_duration_minutes] INT,
        [is_deleted] BIT DEFAULT 0
    );

    CREATE TABLE [dbo].[section_activities] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [section_id] INT NOT NULL,
        [activity_id] INT NOT NULL,
        [activity_order] INT,
        FOREIGN KEY ([section_id]) REFERENCES [dbo].[sections]([id]),
        FOREIGN KEY ([activity_id]) REFERENCES [dbo].[activities]([id])
    );

    -- ========================================
    --  # Material area
    -- ========================================
    CREATE TABLE [dbo].[learning_materials] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [learning_material_type] NVARCHAR(100) NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        [description] NVARCHAR(2000) NOT NULL,
        [material_url] NVARCHAR(2000) NOT NULL
    );

    CREATE TABLE [dbo].[activity_materials] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [activity_id] INT NOT NULL,
        [learning_material_id] INT NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        [description] NVARCHAR(2000) NOT NULL,
        FOREIGN KEY ([activity_id]) REFERENCES [dbo].[activities]([id]),
        FOREIGN KEY ([learning_material_id]) REFERENCES [dbo].[learning_materials]([id])
    );

    -- ========================================
    --  # Quiz area
    -- ========================================
    CREATE TABLE [dbo].[quizzes] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100),
        [pass_score_criteria] DECIMAL(5,2),
        [created_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [updated_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [timelimit_minute] INT,
        [total_score] DECIMAL(5,2),
        [description] NVARCHAR(2000)
    );

    CREATE TABLE [dbo].[quiz_questions] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [quiz_id] INT NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        [question_score] DECIMAL(5,2),
        [description] NVARCHAR(2000),
        [is_multiple_answers] BIT NOT NULL DEFAULT 1,
        FOREIGN KEY ([quiz_id]) REFERENCES [dbo].[quizzes]([id])
    );

    CREATE TABLE [dbo].[quiz_question_options] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [quiz_question_id] INT NOT NULL,
        [description] NVARCHAR(2000),
        [explanation] NVARCHAR(2000) NULL,
        [is_correct] BIT NOT NULL DEFAULT 1,
        [display_order] INT UNIQUE,
        [option_score] DECIMAL(5,2),
        [name] NVARCHAR(100) NOT NULL,
        FOREIGN KEY ([quiz_question_id]) REFERENCES [dbo].[quiz_questions]([id])
    );

    CREATE TABLE [dbo].[activity_quizzes] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [quiz_id] INT NOT NULL,
        [activity_id] INT NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        [description] NVARCHAR(2000),
        FOREIGN KEY ([quiz_id]) REFERENCES [dbo].[quizzes]([id]),
        FOREIGN KEY ([activity_id]) REFERENCES [dbo].[activities]([id])
    );

    -- ========================================
    --  # Practice area
    -- ========================================
    CREATE TABLE [dbo].[practices] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [practice_name] NVARCHAR(2000) NOT NULL,
        [practice_description] NVARCHAR(2000),
        [estimated_duration_minutes] INT,
        [difficulty_level] NVARCHAR(2000),
        [max_attempts] INT,
        [created_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0
    );

    CREATE TABLE [dbo].[tasks] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [practice_id] INT NOT NULL,
        [task_name] NVARCHAR(2000) NOT NULL,
        [task_description] NVARCHAR(2000),
        [expected_result] NVARCHAR(2000),
        [is_deleted] BIT DEFAULT 0,
        FOREIGN KEY ([practice_id]) REFERENCES [dbo].[practices]([id])
    );

    CREATE TABLE [dbo].[activity_practices] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [activity_id] INT NOT NULL,
        [practice_id] INT NOT NULL,
        [custom_deadline] DATETIME2(0),
        [custom_description] NVARCHAR(2000),
        [status] INT DEFAULT 1,
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0,
        FOREIGN KEY ([activity_id]) REFERENCES [dbo].[activities]([id]),
        FOREIGN KEY ([practice_id]) REFERENCES [dbo].[practices]([id])
    );

    CREATE TABLE [dbo].[brand_models] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(200) NOT NULL,
        [description] NVARCHAR(2000),
        [manufacturer] NVARCHAR(200),
        [country_of_origin] NVARCHAR(100),
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0
    );

    CREATE TABLE [dbo].[simulation_components] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [brand_model_id] INT NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        [description] NVARCHAR(2000),
        [image_url] NVARCHAR(2000),
        [is_active] BIT DEFAULT 1,
        [created_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [is_deleted] BIT DEFAULT 0,
        FOREIGN KEY ([brand_model_id]) REFERENCES [dbo].[brand_models]([id])
    );

    -- ========================================
    --  # class area
    -- ========================================
    CREATE TABLE [dbo].[program_course_classes] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [program_course_id] INT NOT NULL,

        [name] NVARCHAR(255) NULL,          -- (tuỳ chọn) nhãn/đợt/học kỳ
        [description] NVARCHAR(1000) NULL,  -- (tuỳ chọn) mô tả
        [created_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [updated_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),

        FOREIGN KEY ([program_course_id]) REFERENCES [dbo].[program_courses]([id])
    );

    CREATE TABLE [dbo].[class_codes] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL UNIQUE
    );

    CREATE TABLE [dbo].[classes] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL,
        [start_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [end_date] DATETIME2(0),
        [capacity] INT,
        [program_course_classes_id] INT NOT NULL,
        [class_code_id] INT,
        [description] NVARCHAR(2000) NOT NULL,
        [status] INT NOT NULL DEFAULT 1,
        FOREIGN KEY ([program_course_classes_id]) REFERENCES [dbo].[program_course_classes]([id]),
        FOREIGN KEY ([class_code_id]) REFERENCES [dbo].[class_codes]([id])
    );

    CREATE TABLE [dbo].[class_instructors] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [class_id] INT NOT NULL,
        [instructor_id] INT NOT NULL,
        [position] NVARCHAR(2000) NOT NULL,
        FOREIGN KEY ([class_id]) REFERENCES [dbo].[classes]([id]),
        FOREIGN KEY ([instructor_id]) REFERENCES [dbo].[instructors]([id])
    );

    CREATE TABLE [dbo].[enrollments] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [class_id] INT NOT NULL,
        [trainee_id] INT NOT NULL,
        [enrolled_at] DATETIME2(0) NOT NULL,
        [status] NVARCHAR(100) NULL,         -- ví dụ: Pending, Approved, Cancelled, Completed
        [note] NVARCHAR(2000) NULL,
        [is_active] BIT NOT NULL DEFAULT 1,

        FOREIGN KEY ([class_id]) REFERENCES [dbo].[classes]([id]),
        FOREIGN KEY ([trainee_id]) REFERENCES [dbo].[trainees]([id])
    );

    -- ========================================
    --  # progress area
    -- ========================================
    CREATE TABLE [dbo].[learning_progresses] (
        [id]                INT IDENTITY(1,1) PRIMARY KEY,
        [enrollment_id]     INT NOT NULL,   -- FK -> enrollments.id (học viên thuộc lớp nào)
        [status]            NVARCHAR(50) NOT NULL DEFAULT N'NotStarted', 
        -- Gợi ý giá trị: NotStarted | InProgress | Completed | Locked

        [overall_percent]   DECIMAL(5,2) NULL,        -- % hoàn thành tổng (0..100)
        [last_activity_at]  DATETIME2 NULL,           -- hoạt động gần nhất
        [started_at]        DATETIME2 NULL,
        [completed_at]      DATETIME2 NULL,

        FOREIGN KEY ([enrollment_id]) REFERENCES [dbo].[enrollments]([id])
    );

    CREATE TABLE [dbo].[section_records] (
        [id]                    INT IDENTITY(1,1) PRIMARY KEY,
        [learning_progress_id]  INT NOT NULL,   -- FK -> learning_progresses.id

        [title]                 NVARCHAR(255) NOT NULL,  -- tên/chặng/module hiển thị
        [display_order]         INT NOT NULL DEFAULT 1,  -- thứ tự trong progress

        [status]                NVARCHAR(50) NOT NULL DEFAULT N'NotStarted',
        [progress_percent]      DECIMAL(5,2) NULL,       -- 0..100
        [started_at]            DATETIME2 NULL,
        [completed_at]          DATETIME2 NULL,
        [last_activity_at]      DATETIME2 NULL,

        FOREIGN KEY ([learning_progress_id]) REFERENCES [dbo].[learning_progresses]([id])
    );

    CREATE TABLE [dbo].[activity_records] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [section_record_id] INT NOT NULL,    -- FK -> section_records.id
        [activity_type] NVARCHAR(50) NOT NULL,  -- Ví dụ: 'Quiz' hoặc 'Practice'
        [title] NVARCHAR(255) NOT NULL,
        [display_order] INT NOT NULL DEFAULT 1,
        [status] NVARCHAR(50) NOT NULL DEFAULT N'NotStarted',
        [progress_percent] DECIMAL(5,2) NULL,
        [started_at] DATETIME2 NULL,
        [completed_at] DATETIME2 NULL,
        [last_activity_at] DATETIME2 NULL,

        FOREIGN KEY ([section_record_id]) REFERENCES [dbo].[section_records]([id])
    );

    -- ========================================
    --  # quiz attempt
    -- ========================================
    CREATE TABLE [dbo].[quizzes] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [activity_record_id] INT NOT NULL,           
        [name] NVARCHAR(255) NOT NULL,
        [description] NVARCHAR(2000) NULL,
        [pass_score_criteria] DECIMAL(5,2) NULL,      
        [total_score] DECIMAL(5,2) NULL,
        [timelimit_minute] INT NULL,
        [created_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [updated_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),

        FOREIGN KEY ([activity_record_id]) REFERENCES [dbo].[activity_records]([id])
    );

    CREATE TABLE [dbo].[quiz_questions] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [quiz_id] INT NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        [question_score] DECIMAL(5,2),
        [description] NVARCHAR(2000),
        [is_multiple_answers] BIT NOT NULL DEFAULT 1,
        FOREIGN KEY ([quiz_id]) REFERENCES [dbo].[quizzes]([id])
    );

    CREATE TABLE [dbo].[quiz_question_options] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [quiz_question_id] INT NOT NULL,
        [description] NVARCHAR(2000),
        [explanation] NVARCHAR(2000) NULL,
        [is_correct] BIT NOT NULL DEFAULT 1,
        [display_order] INT UNIQUE,
        [option_score] DECIMAL(5,2),
        [name] NVARCHAR(100) NOT NULL,
        FOREIGN KEY ([quiz_question_id]) REFERENCES [dbo].[quiz_questions]([id])
    );

    -- ========================================
    --  # practice attempt
    -- ========================================
    CREATE TABLE [dbo].[practice_attempts] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
		[practice_name] NVARCHAR(255) NOT NULL,
		[description] NVARCHAR(1000) NOT NULL,
        [activity_record_id] INT NOT NULL,        -- FK -> activity_records.id
        [started_at] DATETIME2(0) NOT NULL,
        [completed_at] DATETIME2 NULL,
		[completion_percentage] DECIMAL(10,2) NULL,
        [status] INT DEFAULT 1,
		[is_pass] BIT,
        -- Gợi ý: InProgress | Completed | Failed | Cancelled

        FOREIGN KEY ([activity_record_id]) REFERENCES [dbo].[activity_records]([id])
    );

    CREATE TABLE [dbo].[practice_attempt_tasks] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [practice_attempt_id] INT NOT NULL,        -- FK -> practice_attempts.id
        [task_name] NVARCHAR(255) NOT NULL,        -- tên nhiệm vụ trong buổi thực hành
        [description] NVARCHAR(1000) NULL,         -- mô tả chi tiết (nếu có)
        [status] INT DEFAULT 1,
		[is_pass] BIT,
        -- Gợi ý: NotStarted | InProgress | Completed | Failed
        [started_at] DATETIME2 NULL,
        [completed_at] DATETIME2 NULL,
        [last_activity_at] DATETIME2 NULL,

        FOREIGN KEY ([practice_attempt_id]) REFERENCES [dbo].[practice_attempts]([id])
    );

    -- ========================================
    --  # certificate area
    -- ========================================
    CREATE TABLE [dbo].[certificates] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL UNIQUE,
        [description] NVARCHAR(2000),
        [effective_time] INT,
        [requirement] NVARCHAR(2000),
        [certifying_authority] NVARCHAR(2000),
        [image_url] NVARCHAR(2000)
    );	

    CREATE TABLE [dbo].[course_certificates] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [course_id] INT,
        [certificate_id] INT,
        FOREIGN KEY ([course_id]) REFERENCES [dbo].[courses]([id]),
        FOREIGN KEY ([certificate_id]) REFERENCES [dbo].[certificates]([id])
    );

    CREATE TABLE [dbo].[trainee_certificates] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL, 
        [description] NVARCHAR(2000),
        [course_certificate_id] INT,
        [valid_date_end] DATETIME2(0),
        [trainee_id] INT,
        [issued_date_start] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [issued_date_end] DATETIME2(0),
        [status] INT NOT NULL DEFAULT 1,
        FOREIGN KEY ([course_certificate_id]) REFERENCES [dbo].[course_certificates]([id]),
        FOREIGN KEY ([trainee_id]) REFERENCES [dbo].[trainees]([id])
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;

    -- Optional: Raise error info for debugging/logging
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR (@ErrorMessage, 16, 1);
END CATCH