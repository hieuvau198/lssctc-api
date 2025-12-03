USE [lssctc-db]
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- ========================================
    --  # Account area
    -- ========================================

    CREATE TABLE [dbo].[users] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [username] NVARCHAR(255) NOT NULL UNIQUE,
        [password] NVARCHAR(1000) NOT NULL,
        [email] NVARCHAR(255) NOT NULL,
        [fullname] NVARCHAR(1000),
        [role] INT DEFAULT 5,	-- Admin=1, ProgramManager=2, SimulationManager=3, Instructor=4, Trainee=5
        [phone_number] NVARCHAR(1000),
        [avatar_url] NVARCHAR(1000),
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
        [instructor_code] NVARCHAR(1000),
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0,
        FOREIGN KEY ([id]) REFERENCES [dbo].[users]([id])
    );

    CREATE TABLE [dbo].[trainees] (
        [id] INT PRIMARY KEY,
        [trainee_code] NVARCHAR(1000) NOT NULL,
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0,
        FOREIGN KEY ([id]) REFERENCES [dbo].[users]([id])
    );

    CREATE TABLE [dbo].[instructor_profiles] (
        [id] INT PRIMARY KEY,
        [experience_years] INT,
        [biography] NVARCHAR(1000),
        [professional_profile_url] NVARCHAR(1000),
        [specialization] NVARCHAR(1000),
        FOREIGN KEY ([id]) REFERENCES [dbo].[instructors]([id])
    );

    CREATE TABLE [dbo].[trainee_profiles] (
        [id] INT PRIMARY KEY, 
        [driver_license_number] NVARCHAR(100),        
        [driver_license_level] NVARCHAR(50),          
        [driver_license_issued_date] DATETIME2(0),
        [driver_license_valid_start_date] DATETIME2(0),
        [driver_license_valid_end_date] DATETIME2(0),
        [driver_license_image_url] NVARCHAR(1000),
        [education_level] NVARCHAR(255),
        [education_image_url] NVARCHAR(1000),
        [citizen_card_id] NVARCHAR(20),         
        [citizen_card_issued_date] DATE,            
        [citizen_card_place_of_issue] NVARCHAR(255),
        [citizen_card_image_url] NVARCHAR(1000),
        FOREIGN KEY ([id]) REFERENCES [dbo].[trainees]([id])
    );


    -- ========================================
    --  Base lookup tables
    -- ========================================

    CREATE TABLE [dbo].[course_categories] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL UNIQUE,
        [description] NVARCHAR(1000)
    );	

    CREATE TABLE [dbo].[course_levels] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL UNIQUE,
        [description] NVARCHAR(1000)
    );	

    CREATE TABLE [dbo].[course_codes] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL UNIQUE
    );	


    -- ========================================
    --  Core course and program structure
    -- ========================================

    CREATE TABLE [dbo].[courses] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL,
        [description] NVARCHAR(1000),
        [category_id] INT,
        [level_id] INT,
        [price] DECIMAL(10, 2),
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0,
        [image_url] NVARCHAR(1000),
        [duration_hours] INT,
        [course_code_id] INT,
        FOREIGN KEY ([category_id]) REFERENCES [dbo].[course_categories]([id]),
        FOREIGN KEY ([level_id]) REFERENCES [dbo].[course_levels]([id]),
        FOREIGN KEY ([course_code_id]) REFERENCES [dbo].[course_codes]([id])
    );

    CREATE TABLE [dbo].[training_programs] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL UNIQUE,
        [description] NVARCHAR(1000),
        [is_deleted] BIT DEFAULT 0,
        [is_active] BIT DEFAULT 1,
        [duration_hours] INT,
        [total_courses] INT,
        [image_url] NVARCHAR(1000)
    );	

    CREATE TABLE [dbo].[program_courses] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [program_id] INT NOT NULL, 
        [course_id] INT NOT NULL,
        [course_order] INT NOT NULL,
        [name] NVARCHAR(100),
        [description] NVARCHAR(1000),
        FOREIGN KEY ([program_id]) REFERENCES [dbo].[training_programs]([id]),
        FOREIGN KEY ([course_id]) REFERENCES [dbo].[courses]([id])
    );


    -- ========================================
    --  Sections and course mapping
    -- ========================================

    CREATE TABLE [dbo].[sections] (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        [section_title] NVARCHAR(200) NOT NULL,
        [section_description] NVARCHAR(1000),
        [estimated_duration_minutes] INT,
        [is_deleted] BIT DEFAULT 0
    );

    CREATE TABLE [dbo].[course_sections](
        Id INT IDENTITY(1,1) PRIMARY KEY,
        [course_id] INT NOT NULL,
        [section_id] INT NOT NULL,
        [section_order] INT NOT NULL DEFAULT 0,
        FOREIGN KEY ([course_id]) REFERENCES [dbo].[courses]([id]),
        FOREIGN KEY ([section_id]) REFERENCES [dbo].[sections]([id])
    );


    -- ========================================
    --  Activities, materials, quizzes, practices
    -- ========================================

    CREATE TABLE [dbo].[activities] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [activity_title] NVARCHAR(200) NOT NULL,
        [activity_description] NVARCHAR(1000),
        [activity_type] INT DEFAULT 1,
        [estimated_duration_minutes] INT,
        [is_deleted] BIT DEFAULT 0
    );

    CREATE TABLE [dbo].[learning_materials] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [learning_material_type] INT DEFAULT 1,
        [name] NVARCHAR(100) NOT NULL,
        [description] NVARCHAR(1000) NOT NULL,
        [material_url] NVARCHAR(1000) NOT NULL
    );

    CREATE TABLE [dbo].[material_authors] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [instructor_id] INT NOT NULL,
        [material_id] INT NOT NULL,

        FOREIGN KEY ([instructor_id]) REFERENCES [dbo].[instructors]([id]),
        FOREIGN KEY ([material_id]) REFERENCES [dbo].[learning_materials]([id])
    );

    CREATE TABLE [dbo].[section_activities] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [section_id] INT NOT NULL,
        [activity_id] INT NOT NULL,
        [activity_order] INT,
        FOREIGN KEY ([section_id]) REFERENCES [dbo].[sections]([id]),
        FOREIGN KEY ([activity_id]) REFERENCES [dbo].[activities]([id])
    );

    CREATE TABLE [dbo].[activity_materials] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [activity_id] INT NOT NULL,
        [learning_material_id] INT NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        [description] NVARCHAR(1000) NOT NULL,
        FOREIGN KEY ([activity_id]) REFERENCES [dbo].[activities]([id]),
        FOREIGN KEY ([learning_material_id]) REFERENCES [dbo].[learning_materials]([id])
    );


    -- ========================================
    --  Quizzes and questions
    -- ========================================

    CREATE TABLE [dbo].[quizzes] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100),
        [pass_score_criteria] DECIMAL(5,2),
        [created_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [updated_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [timelimit_minute] INT,
        [total_score] DECIMAL(5,2),
        [description] NVARCHAR(1000)
    ); 

    CREATE TABLE [dbo].[quiz_authors] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [instructor_id] INT NOT NULL,
        [quiz_id] INT NOT NULL,

        FOREIGN KEY ([instructor_id]) REFERENCES [dbo].[instructors]([id]),
        FOREIGN KEY ([quiz_id]) REFERENCES [dbo].[quizzes]([id])
    );

    CREATE TABLE [dbo].[quiz_questions] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [quiz_id] INT NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        [question_score] DECIMAL(5,2),
        [description] NVARCHAR(1000),
        [is_multiple_answers] BIT NOT NULL DEFAULT 1,
        [image_url] NVARCHAR(255), -- Added to match EF entity QuizQuestion.ImageUrl
        FOREIGN KEY ([quiz_id]) REFERENCES [dbo].[quizzes]([id])
    ); 

    CREATE TABLE [dbo].[quiz_question_options] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [quiz_question_id] INT NOT NULL,
        [description] NVARCHAR(1000),
        [explanation] NVARCHAR(1000),
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
        [description] NVARCHAR(1000),
        FOREIGN KEY ([quiz_id]) REFERENCES [dbo].[quizzes]([id]),
        FOREIGN KEY ([activity_id]) REFERENCES [dbo].[activities]([id])
    );


    -- ========================================
    --  Practices and tasks
    -- ========================================

    CREATE TABLE [dbo].[practices] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [practice_name] NVARCHAR(1000) NOT NULL,
        [practice_description] NVARCHAR(1000),
        [estimated_duration_minutes] INT,
        [difficulty_level] NVARCHAR(1000),
        [max_attempts] INT,
        [created_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0,
        [practice_code] VARCHAR(50) DEFAULT 'NO PRACTICE CODE ASSIGNED' -- Added to match EF Entity
    );

    CREATE TABLE [dbo].[sim_tasks] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [task_name] NVARCHAR(1000) NOT NULL,
        [task_description] NVARCHAR(1000),
        [expected_result] NVARCHAR(1000),
        [is_deleted] BIT DEFAULT 0,
        [task_code] VARCHAR(50) DEFAULT 'NO PRACTICE CODE ASSIGNED' -- Added to match EF Entity (note: default string copied from EF config provided)
    );

    CREATE TABLE [dbo].[practice_tasks] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [practice_id] INT NOT NULL,
        [task_id] INT NOT NULL,
        [status] INT DEFAULT 1,
        FOREIGN KEY ([practice_id]) REFERENCES [dbo].[practices]([id]),
        FOREIGN KEY ([task_id]) REFERENCES [dbo].[sim_tasks]([id])
    );

    CREATE TABLE [dbo].[activity_practices] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [activity_id] INT NOT NULL,
        [practice_id] INT NOT NULL,
        [custom_deadline] DATETIME2(0),
        [custom_description] NVARCHAR(1000),
        [status] INT DEFAULT 1,
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0,
        FOREIGN KEY ([activity_id]) REFERENCES [dbo].[activities]([id]),
        FOREIGN KEY ([practice_id]) REFERENCES [dbo].[practices]([id])
    );


    -- ========================================
    --  Simulation components and brand models
    -- ========================================

    CREATE TABLE [dbo].[brand_models] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(200) NOT NULL,
        [description] NVARCHAR(1000),
        [manufacturer] NVARCHAR(200),
        [country_of_origin] NVARCHAR(100),
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0
    );

    CREATE TABLE [dbo].[simulation_components] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [brand_model_id] INT NOT NULL,
        [name] NVARCHAR(100) NOT NULL,
        [description] NVARCHAR(1000),
        [image_url] NVARCHAR(1000),
        [is_active] BIT DEFAULT 1,
        [created_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [is_deleted] BIT DEFAULT 0,
        [component_code] VARCHAR(50) DEFAULT 'COMPONENT_UNKNOWN', -- Added to match EF Entity
        FOREIGN KEY ([brand_model_id]) REFERENCES [dbo].[brand_models]([id])
    );


    -- ========================================
    --  Classes and enrollments
    -- ========================================

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
        [program_course_id] INT NOT NULL,
        [class_code_id] INT,
        [description] NVARCHAR(1000) NOT NULL,
        [status] INT DEFAULT 1,
        FOREIGN KEY ([program_course_id]) REFERENCES [dbo].[program_courses]([id]),
        FOREIGN KEY ([class_code_id]) REFERENCES [dbo].[class_codes]([id])
    );

    CREATE TABLE [dbo].[class_instructors] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [class_id] INT NOT NULL,
        [instructor_id] INT NOT NULL,
        [position] NVARCHAR(1000) NOT NULL,
        FOREIGN KEY ([class_id]) REFERENCES [dbo].[classes]([id]),
        FOREIGN KEY ([instructor_id]) REFERENCES [dbo].[instructors]([id])
    );

    CREATE TABLE [dbo].[enrollments] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [class_id] INT NOT NULL,
        [trainee_id] INT NOT NULL, 
        [enroll_date] DATETIME2(0) DEFAULT SYSDATETIME(),
        [status] INT DEFAULT 1,
        [is_active] BIT DEFAULT 1,
        [is_deleted] BIT DEFAULT 0,
        [note] NVARCHAR(500) NULL,                    
        FOREIGN KEY ([class_id]) REFERENCES [dbo].[classes]([id]),
        FOREIGN KEY ([trainee_id]) REFERENCES [dbo].[trainees]([id])
    );


    -- ========================================
    --  Certificates and progress tracking
    -- ========================================

    CREATE TABLE [dbo].[certificates] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(200) NOT NULL,
        [description] NVARCHAR(1000),
        [template_url] NVARCHAR(1000),
        [template_html] NVARCHAR(MAX),
        [is_active] BIT DEFAULT 1
    );

    CREATE TABLE [dbo].[course_certificates] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [course_id] INT NOT NULL,
        [certificate_id] INT NOT NULL,
        [passing_score] DECIMAL(5,2),
        [is_active] BIT DEFAULT 1,
        FOREIGN KEY ([course_id]) REFERENCES [dbo].[courses]([id]),
        FOREIGN KEY ([certificate_id]) REFERENCES [dbo].[certificates]([id])
    );

    CREATE TABLE [dbo].[trainee_certificates] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [enrollment_id] INT NOT NULL,
        [course_certificate_id] INT NOT NULL,
        [issued_date] DATETIME2(0) DEFAULT SYSDATETIME(),
        [certificate_code] NVARCHAR(100) UNIQUE,
        [pdf_url] NVARCHAR(1000), 
        FOREIGN KEY ([enrollment_id]) REFERENCES [dbo].[enrollments]([id]),
        FOREIGN KEY ([course_certificate_id]) REFERENCES [dbo].[course_certificates]([id])
    );

    CREATE TABLE [dbo].[learning_progresses] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [enrollment_id] INT NOT NULL,
        [course_id] INT NOT NULL,                     
        [status] INT DEFAULT 1,
        [progress_percentage] DECIMAL(5, 2) DEFAULT 0,
        [theory_score] DECIMAL(5, 2) DEFAULT 0,
        [practical_score] DECIMAL(5, 2) DEFAULT 0,
        [final_score] DECIMAL(5, 2) DEFAULT 0,
        [start_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [last_updated] DATETIME2(0),
        [name] NVARCHAR(100),
        [description] NVARCHAR(1000),
        FOREIGN KEY ([enrollment_id]) REFERENCES [dbo].[enrollments]([id])
    );

    CREATE TABLE [dbo].[section_records] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [name] NVARCHAR(100),
        [learning_progress_id] INT NOT NULL,
        [section_id] INT DEFAULT -1,
        [section_name] NVARCHAR(255),
        [is_completed] BIT NOT NULL DEFAULT 1,
        [is_trainee_attended] BIT NOT NULL DEFAULT 1,
        [progress] DECIMAL(5,2),
        FOREIGN KEY ([learning_progress_id]) REFERENCES [dbo].[learning_progresses]([id])
    );

    CREATE TABLE [dbo].[activity_records] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [section_record_id] INT NOT NULL,
        [activity_id] INT DEFAULT -1,
        [activity_type] INT NULL,                    
        [status] INT DEFAULT 1,
        [score] DECIMAL(5,2),
        [is_completed] BIT DEFAULT 0,
        [completed_date] DATETIME2(0),
        FOREIGN KEY ([section_record_id]) REFERENCES [dbo].[section_records]([id])
    );


    -- ========================================
    --  Practice and quiz attempts
    -- ========================================

    CREATE TABLE [dbo].[practice_attempts] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [activity_record_id] INT NOT NULL,
        [practice_id] INT DEFAULT -1,
        [score] DECIMAL(5,2),
        [attempt_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [attempt_status] INT DEFAULT 1,
        [description] NVARCHAR(1000),
        [is_pass] BIT DEFAULT 0,
        [is_deleted] BIT DEFAULT 0,
        [is_current] BIT DEFAULT 0,
        FOREIGN KEY ([activity_record_id]) REFERENCES [dbo].[activity_records]([id])
    );

    CREATE TABLE [dbo].[practice_attempt_tasks] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [practice_attempt_id] INT NOT NULL,
        [task_id] INT DEFAULT -1,
        [score] DECIMAL(5,2),
        [description] NVARCHAR(1000),
        [is_pass] BIT DEFAULT 0,
        [is_deleted] BIT DEFAULT 0,
        FOREIGN KEY ([practice_attempt_id]) REFERENCES [dbo].[practice_attempts]([id])
    );

    CREATE TABLE [dbo].[quiz_attempts] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [activity_record_id] INT NOT NULL,
        [quiz_id] INT DEFAULT -1,
        [name] NVARCHAR(100) NOT NULL,
        [attempt_score] DECIMAL(5,2),
        [max_score] DECIMAL(5,2),
        [quiz_attempt_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        [status] INT NOT NULL DEFAULT 1,
        [attempt_order] INT,
        [is_pass] BIT,
        FOREIGN KEY ([activity_record_id]) REFERENCES [dbo].[activity_records]([id])
    );

    CREATE TABLE [dbo].[quiz_attempt_questions] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [quiz_attempt_id] INT NOT NULL,
        [question_id] INT DEFAULT -1,
        [attempt_score] DECIMAL(5,2),
        [question_score] DECIMAL(5,2),
        [is_correct] BIT NOT NULL DEFAULT 1,
        [is_multiple_answers] BIT NOT NULL DEFAULT 1,
        [name] NVARCHAR(100) NOT NULL,
        [description] NVARCHAR(1000),
        FOREIGN KEY ([quiz_attempt_id]) REFERENCES [dbo].[quiz_attempts]([id])
    );

    CREATE TABLE [dbo].[quiz_attempt_answers] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [quiz_attempt_question_id] INT NOT NULL,
        [quiz_option_id] INT DEFAULT -1, 
        [attempt_score] DECIMAL(5,2),
        [is_correct] BIT NOT NULL DEFAULT 1,
        [description] NVARCHAR(1000),
        [name] NVARCHAR(100) NOT NULL,
        FOREIGN KEY ([quiz_attempt_question_id]) REFERENCES [dbo].[quiz_attempt_questions]([id])
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR (@ErrorMessage, 16, 1);
END CATCH