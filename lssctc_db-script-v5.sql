


USE [lssctc-db]


BEGIN TRY
    BEGIN TRANSACTION;

    -- Your statements here

-- =========================
-- USER SYSTEM
-- =========================
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

CREATE TABLE [dbo].[program_managers] (
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

-- =========================
-- USER PROFILE
-- =========================
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

-- =========================
-- Independent Tables
-- =========================
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

CREATE TABLE [dbo].[certificates] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL UNIQUE,
    [description] NVARCHAR(2000),
    [effective_time] INT,
    [requirement] NVARCHAR(2000),
    [certifying_authority] NVARCHAR(2000),
    [image_url] NVARCHAR(2000)
);	

CREATE TABLE [dbo].[course_categories] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL UNIQUE,
    [description] NVARCHAR(2000)
);	-- Mobile Crane, Safety & Regulations, Basic Operation, Advanced Operation, Equipment Maintenance, Certification Preparation 

CREATE TABLE [dbo].[course_levels] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL UNIQUE,
    [description] NVARCHAR(2000)
);	-- need seed data: Entry, Standard, Expert

CREATE TABLE [dbo].[course_codes] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL UNIQUE
);	-- need seed data, CO000001, CO000002, CO000003, CO000004, CO000005

CREATE TABLE [dbo].[class_codes] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL UNIQUE
); -- need seed data, CL000001, CL000002, CL000003, CL000004, CL000005

CREATE TABLE [dbo].[learning_material_types] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL
); -- need seed data: PDF, Video, Image, URL

CREATE TABLE [dbo].[learning_materials] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [learning_material_type_id] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(2000) NOT NULL,
	[material_url] NVARCHAR(2000) NOT NULL,
    FOREIGN KEY ([learning_material_type_id]) REFERENCES [dbo].[learning_material_types]([id])
); -- need seed data

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

CREATE TABLE [dbo].[simulation_settings] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0
); 

CREATE TABLE [dbo].[simulation_timeslots] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [start_time] DATETIME2(0) NOT NULL,
    [end_time] DATETIME2(0) NOT NULL,
    [note] NVARCHAR(2000),
    [status] INT DEFAULT 1,
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0,
); 

CREATE TABLE [dbo].[simulation_component_types] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [is_deleted] BIT DEFAULT 0
); 

CREATE TABLE [dbo].[simulation_components] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [image_url] NVARCHAR(2000),
    [is_active] BIT DEFAULT 1,
    [created_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [is_deleted] BIT DEFAULT 0
);

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

CREATE TABLE [dbo].[practice_step_types] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [is_deleted] BIT DEFAULT 0
);

CREATE TABLE [dbo].[practice_steps] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [practice_id] INT NOT NULL,
    [step_name] NVARCHAR(2000) NOT NULL,
    [step_description] NVARCHAR(2000),
    [expected_result] NVARCHAR(2000),
    [step_order] INT NOT NULL,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([practice_id]) REFERENCES [dbo].[practices]([id])
);

CREATE TABLE [dbo].[practice_step_warning_types] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [is_failed_criteria] BIT DEFAULT 0
);

CREATE TABLE [dbo].[practice_step_warnings] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [practice_step_id] INT NOT NULL,
    [warning_message] NVARCHAR(2000),
    [warning_type_id] INT,
    [instruction] NVARCHAR(2000),
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([practice_step_id]) REFERENCES [dbo].[practice_steps]([id]),
    FOREIGN KEY ([warning_type_id]) REFERENCES [dbo].[practice_step_warning_types]([id])
);

CREATE TABLE [dbo].[practice_step_components] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [step_id] INT NOT NULL,
    [component_id] INT NOT NULL,
    [component_order] INT NOT NULL,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([step_id]) REFERENCES [dbo].[practice_steps]([id]),
    FOREIGN KEY ([component_id]) REFERENCES [dbo].[simulation_components]([id])
);

CREATE TABLE [dbo].[syllabuses] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
	[course_name] NVARCHAR(100) NOT NULL,
	[course_code] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0
);

CREATE TABLE [dbo].[syllabus_sections] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [syllabus_id] INT NOT NULL,
    [section_title] NVARCHAR(200) NOT NULL,
    [section_description] NVARCHAR(2000),
    [section_order] INT NOT NULL,
    [estimated_duration_minutes] INT,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([syllabus_id]) REFERENCES [dbo].[syllabuses]([id])
);


-- =========================
-- Specific Tables
-- =========================


CREATE TABLE [dbo].[program_entry_requirements] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [program_id] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
	[document_url] NVARCHAR(2000),
    FOREIGN KEY ([program_id]) REFERENCES [dbo].[training_programs]([id])
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

CREATE TABLE [dbo].[course_certificates] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [course_id] INT,
    [certificate_id] INT,
    FOREIGN KEY ([course_id]) REFERENCES [dbo].[courses]([id]),
    FOREIGN KEY ([certificate_id]) REFERENCES [dbo].[certificates]([id])
);

CREATE TABLE [dbo].[course_syllabuses] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [course_id] INT NOT NULL,
    [syllabus_id] INT NOT NULL,
    FOREIGN KEY ([course_id]) REFERENCES [dbo].[courses]([id]),
    FOREIGN KEY ([syllabus_id]) REFERENCES [dbo].[syllabuses]([id])
);

CREATE TABLE [dbo].[classes] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [start_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [end_date] DATETIME2(0),
    [capacity] INT,
    [program_course_id] INT NOT NULL,
    [class_code_id] INT,
    [description] NVARCHAR(2000) NOT NULL,
    [status] INT NOT NULL DEFAULT 1,
    FOREIGN KEY ([program_course_id]) REFERENCES [dbo].[program_courses]([id]),
    FOREIGN KEY ([class_code_id]) REFERENCES [dbo].[class_codes]([id])
);

CREATE TABLE [dbo].[class_registrations] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [created_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [approved_date] DATETIME2(0),
    [class_id] INT NOT NULL,
	[trainee_id] INT NOT NULL,
	[trainee_contact] NVARCHAR(100),
    [description] NVARCHAR(2000) NOT NULL,
    [status] INT NOT NULL DEFAULT 1,
    FOREIGN KEY ([class_id]) REFERENCES [dbo].[classes]([id]),
    FOREIGN KEY ([trainee_id]) REFERENCES [dbo].[trainees]([id])
);

CREATE TABLE [dbo].[class_instructors] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [class_id] INT NOT NULL,
    [instructor_id] INT NOT NULL,
    [position] NVARCHAR(2000) NOT NULL,
    FOREIGN KEY ([class_id]) REFERENCES [dbo].[classes]([id]),
    FOREIGN KEY ([instructor_id]) REFERENCES [dbo].[instructors]([id])
);

CREATE TABLE [dbo].[class_members] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [trainee_id] INT NOT NULL,
    [class_id] INT NOT NULL,
    [assigned_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [status] INT NOT NULL DEFAULT 1,
    FOREIGN KEY ([trainee_id]) REFERENCES [dbo].[trainees]([id]),
    FOREIGN KEY ([class_id]) REFERENCES [dbo].[classes]([id])
);

CREATE TABLE [dbo].[training_progresses] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [course_member_id] INT NOT NULL,
    [status] INT NOT NULL DEFAULT 1,
    [progress_percentage] FLOAT,
    [start_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [last_updated] DATETIME2(0),
    [name] NVARCHAR(100),
    [description] NVARCHAR(2000),
    FOREIGN KEY ([course_member_id]) REFERENCES [dbo].[class_members]([id]),
);

CREATE TABLE [dbo].[training_result_types] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000)
);

CREATE TABLE [dbo].[training_results] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [training_result_type_id] INT NOT NULL,
    [training_progress_id] INT NOT NULL,
    [result_value] NVARCHAR(2000),
    [result_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [notes] NVARCHAR(2000),
    FOREIGN KEY ([training_result_type_id]) REFERENCES [dbo].[training_result_types]([id]),
    FOREIGN KEY ([training_progress_id]) REFERENCES [dbo].[training_progresses]([id])
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


CREATE TABLE [dbo].[sections] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [classes_id] INT NOT NULL,
	[syllabus_section_id] INT NOT NULL,
    [duration_minutes] INT,
    [order] INT NOT NULL,
    [start_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [end_date] DATETIME2(0),
    [status] INT NOT NULL DEFAULT 1,
    FOREIGN KEY ([classes_id]) REFERENCES [dbo].[classes]([id]),
	FOREIGN KEY ([syllabus_section_id]) REFERENCES [dbo].[syllabus_sections]([id])
);

CREATE TABLE [dbo].[section_partition_types] (
  [id] INT IDENTITY(1,1) PRIMARY KEY,
  [description] NVARCHAR(2000) NULL,
  [name] NVARCHAR(100) NOT NULL,
  [pass_criteria] NVARCHAR(2000) NOT NULL,
  [is_action_required] BIT NULL
);

CREATE TABLE [dbo].[section_partitions] (
  [id] INT IDENTITY(1,1) PRIMARY KEY,
  [section_id] INT NOT NULL,      
  [name] NVARCHAR(100) NULL,
  [partition_type_id] INT NOT NULL,
  [display_order] INT NULL,
  [description] NVARCHAR(2000) NULL,
    FOREIGN KEY ([partition_type_id]) REFERENCES [dbo].[section_partition_types]([id]),
    FOREIGN KEY ([section_id]) REFERENCES [dbo].[sections]([id])
);

CREATE TABLE [dbo].[learning_records] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_id] INT NOT NULL,
    [name] NVARCHAR(100),
    [training_progress_id] INT NOT NULL,
    [section_name] NVARCHAR(255),
    [is_completed] BIT NOT NULL DEFAULT 1,
    [is_trainee_attended] BIT NOT NULL DEFAULT 1,
    [progress] DECIMAL(5,2),
    FOREIGN KEY ([section_id]) REFERENCES [dbo].[sections]([id]),
    FOREIGN KEY ([training_progress_id]) REFERENCES [dbo].[training_progresses]([id])
);

CREATE TABLE [dbo].[learning_record_partitions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_partition_id] INT NOT NULL,
    [name] NVARCHAR(100),
    [learning_record_id] INT NOT NULL,
    [description] NVARCHAR(2000),
    [updated_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [completed_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [started_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [is_complete] BIT NOT NULL DEFAULT 1,
	[record_partition_order] INT NOT NULL,
    FOREIGN KEY ([section_partition_id]) REFERENCES [dbo].[section_partitions]([id]),
    FOREIGN KEY ([learning_record_id]) REFERENCES [dbo].[learning_records]([id])
);

CREATE TABLE [dbo].[section_materials] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_partition_id] INT NOT NULL,
	[learning_material_id] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(2000) NOT NULL,
    FOREIGN KEY ([section_partition_id]) REFERENCES [dbo].[section_partitions]([id]),
	FOREIGN KEY ([learning_material_id]) REFERENCES [dbo].[learning_materials]([id])
);

CREATE TABLE [dbo].[section_quizzes] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [quiz_id] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
    [section_partition_id] INT NOT NULL,
    [description] NVARCHAR(2000),
    FOREIGN KEY ([quiz_id]) REFERENCES [dbo].[quizzes]([id]),
    FOREIGN KEY ([section_partition_id]) REFERENCES [dbo].[section_partitions]([id])
);

CREATE TABLE [dbo].[section_practices] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_partition_id] INT NOT NULL,
    [practice_id] INT NOT NULL,
    [custom_deadline] DATETIME2(0),
    [custom_description] NVARCHAR(2000),
    [status] INT DEFAULT 1,
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([section_partition_id]) REFERENCES [dbo].[section_partitions]([id]),
    FOREIGN KEY ([practice_id]) REFERENCES [dbo].[practices]([id])
);

CREATE TABLE [dbo].[section_practice_timeslots] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_practice_id] INT NOT NULL,
    [simulation_timeslot_id] INT NOT NULL,
    [note] NVARCHAR(1000),
    FOREIGN KEY ([section_practice_id]) REFERENCES [dbo].[section_practices]([id]),
    FOREIGN KEY ([simulation_timeslot_id]) REFERENCES [dbo].[simulation_timeslots]([id])
);

CREATE TABLE [dbo].[section_quiz_attempts] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_quiz_id] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
    [attempt_score] DECIMAL(5,2),
    [learning_record_partition_id] INT NOT NULL,
    [max_score] DECIMAL(5,2),
    [quiz_attempt_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [status] INT NOT NULL DEFAULT 1,
    [attempt_order] INT,
	[is_pass] BIT,
    FOREIGN KEY ([section_quiz_id]) REFERENCES [dbo].[section_quizzes]([id]),
	FOREIGN KEY ([learning_record_partition_id]) REFERENCES [dbo].[learning_record_partitions]([id])

);

CREATE TABLE [dbo].[section_quiz_attempt_questions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_quiz_attempt_id] INT NOT NULL,
    [attempt_score] DECIMAL(5,2),
    [question_score] DECIMAL(5,2),
    [is_correct] BIT NOT NULL DEFAULT 1,
    [is_multiple_answers] BIT NOT NULL DEFAULT 1,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    FOREIGN KEY ([section_quiz_attempt_id]) REFERENCES [dbo].[section_quiz_attempts]([id])
);

CREATE TABLE [dbo].[section_quiz_attempt_answers] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_quiz_attempt_question_id] INT NOT NULL,
    [attempt_score] DECIMAL(5,2),
    [is_correct] BIT NOT NULL DEFAULT 1,
    [description] NVARCHAR(2000),
    [name] NVARCHAR(100) NOT NULL,
    FOREIGN KEY ([section_quiz_attempt_question_id]) REFERENCES [dbo].[section_quiz_attempt_questions]([id])
);

CREATE TABLE [dbo].[section_practice_attempts] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_practice_id] INT NOT NULL,
    [learning_record_partition_id] INT NOT NULL,
    [score] DECIMAL(5,2),
    [attempt_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [attempt_status] INT DEFAULT 1,
    [description] NVARCHAR(2000),
    [is_pass] BIT DEFAULT 0,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([section_practice_id]) REFERENCES [dbo].[section_practices]([id]),
    FOREIGN KEY ([learning_record_partition_id]) REFERENCES [dbo].[learning_record_partitions]([id])
);

CREATE TABLE [dbo].[section_practice_attempt_steps] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [attempt_id] INT NOT NULL,
    [practice_step_id] INT NOT NULL,
    [score] DECIMAL(5,2),
    [description] NVARCHAR(2000),
    [is_pass] BIT DEFAULT 0,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([attempt_id]) REFERENCES [dbo].[section_practice_attempts]([id]),
    FOREIGN KEY ([practice_step_id]) REFERENCES [dbo].[practice_steps]([id])
);





CREATE TABLE [dbo].[payments] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [trainee_id] INT NOT NULL,
    [amount] DECIMAL(10, 2) NOT NULL,
    [currency] NVARCHAR(10) DEFAULT 'VND',
    [payment_method] NVARCHAR(50),
    [payment_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [status] INT NOT NULL DEFAULT 1,
    [note] NVARCHAR(2000),
    FOREIGN KEY ([trainee_id]) REFERENCES [dbo].[trainees]([id])
);

CREATE TABLE [dbo].[transactions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [transaction_code] NVARCHAR(255) NOT NULL UNIQUE,  
    [message] NVARCHAR(2000) NOT NULL,
    [payer_name] NVARCHAR(2000) NOT NULL,
    [receiver_name] NVARCHAR(2000) NOT NULL,
    [amount] DECIMAL(10,2) NOT NULL,
    [currency] NVARCHAR(10) DEFAULT 'VND',            
    [transaction_type] NVARCHAR(50),                  
    [description] NVARCHAR(2000),
    [status] INT NOT NULL DEFAULT 1,                  
    [issued_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [paid_at] DATETIME2(0),
    [payer_id] INT,                                   
    [receiver_id] INT,                                
    [note] NVARCHAR(2000)
);

CREATE TABLE [dbo].[payment_transactions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [payment_id] INT NOT NULL,
    [transaction_id] INT NOT NULL,
    [amount] DECIMAL(10,2) NOT NULL,
    [created_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY ([payment_id]) REFERENCES [dbo].[payments]([id]),
    FOREIGN KEY ([transaction_id]) REFERENCES [dbo].[transactions]([id])
);

CREATE TABLE [dbo].[transaction_programs] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [transaction_id] INT NOT NULL,
    [program_id] INT NOT NULL,
    FOREIGN KEY ([transaction_id]) REFERENCES [dbo].[transactions]([id]),
    FOREIGN KEY ([program_id]) REFERENCES [dbo].[training_programs]([id])
);




    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;

    -- Optional: Raise error info for debugging/logging
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR (@ErrorMessage, 16, 1);
END CATCH
