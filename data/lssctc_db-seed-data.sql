USE [lssctc-db]
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- ============================================================
    -- 1. Create Users (1 Admin, 1 Sim Manager, 1 Instructor, 1 Trainee)
    --    Passwords here are placeholders (e.g. hashed '123456')
    -- ============================================================
    PRINT 'Seeding Users...';
    
    -- Admin
    INSERT INTO [dbo].[users] (username, password, email, fullname, role, is_active)
    VALUES ('admin1', '123456', 'admin@example.com', 'System Administrator', 1, 1);
    DECLARE @AdminId INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[admins] (id) VALUES (@AdminId);

    -- Simulation Manager
    INSERT INTO [dbo].[users] (username, password, email, fullname, role, is_active)
    VALUES ('sim_manager', '123456', 'sim_manager@example.com', 'Sim Manager', 3, 1);
    DECLARE @SimMgrId INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[simulation_managers] (id) VALUES (@SimMgrId);

    -- Instructor
    INSERT INTO [dbo].[users] (username, password, email, fullname, role, is_active)
    VALUES ('thaygiao', '123456', 'thaygiao@example.com', 'John Instructor', 4, 1);
    DECLARE @InstId INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[instructors] (id, instructor_code, hire_date) 
    VALUES (@InstId, 'INST-001', SYSDATETIME());

    -- Trainee
    INSERT INTO [dbo].[users] (username, password, email, fullname, role, is_active)
    VALUES ('anhhocvien1', '123456', 'anhhocvien1@example.com', 'Jane Trainee', 5, 1);
    DECLARE @TraineeId INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[trainees] (id, trainee_code) 
    VALUES (@TraineeId, 'TR-2024-001');

    -- ============================================================
    -- 2. Master Data (Categories, Levels, Codes)
    -- ============================================================
    PRINT 'Seeding Master Data...';

    INSERT INTO [dbo].[course_categories] (name, description) VALUES 
    ('Safety Protocols', 'Safety first learning modules'),
    ('Machine Operation', 'How to operate cranes'),
    ('Maintenance', 'Equipment care and fix');
    DECLARE @CatId1 INT = SCOPE_IDENTITY(); -- Get last one (Maintenance) or pick arbitrarily

    INSERT INTO [dbo].[course_levels] (name, description) VALUES 
    ('Beginner', 'No experience required'),
    ('Intermediate', 'Some hours required'),
    ('Expert', 'Certification required');
    DECLARE @LvlId1 INT = SCOPE_IDENTITY();

    INSERT INTO [dbo].[course_codes] (name) VALUES ('C-SAFE-101'), ('C-OP-201');
    DECLARE @CodeId1 INT = SCOPE_IDENTITY(); -- C-OP-201
    DECLARE @CodeId2 INT = @CodeId1 - 1;     -- C-SAFE-101

    -- ============================================================
    -- 3. Courses (2 Courses)
    -- ============================================================
    PRINT 'Seeding Courses...';

    -- Course 1
    INSERT INTO [dbo].[courses] (name, description, category_id, level_id, course_code_id, price, duration_hours)
    VALUES ('Basic Crane Safety', 'Introduction to site safety', @CatId1-2, @LvlId1-2, @CodeId2, 100.00, 10);
    DECLARE @Course1Id INT = SCOPE_IDENTITY();

    -- Course 2
    INSERT INTO [dbo].[courses] (name, description, category_id, level_id, course_code_id, price, duration_hours)
    VALUES ('Advanced Hydraulic Lifting', 'Complex lifting techniques', @CatId1-1, @LvlId1, @CodeId1, 250.00, 20);
    DECLARE @Course2Id INT = SCOPE_IDENTITY();

    -- ============================================================
    -- 4. Training Programs (2 Programs)
    -- ============================================================
    PRINT 'Seeding Programs...';

    -- Program 1
    INSERT INTO [dbo].[training_programs] (name, description, total_courses)
    VALUES ('Certified Safety Officer', 'Complete safety track', 1);
    DECLARE @Prog1Id INT = SCOPE_IDENTITY();

    -- Program 2
    INSERT INTO [dbo].[training_programs] (name, description, total_courses)
    VALUES ('Master Crane Operator', 'Full operation track', 2);
    DECLARE @Prog2Id INT = SCOPE_IDENTITY();

    -- Link Courses to Programs
    INSERT INTO [dbo].[program_courses] (program_id, course_id, course_order, name) VALUES 
    (@Prog1Id, @Course1Id, 1, 'Safety Module'),
    (@Prog2Id, @Course1Id, 1, 'Safety Foundation'),
    (@Prog2Id, @Course2Id, 2, 'Advanced Operation');

    -- ============================================================
    -- 5. Practices & Tasks (2 Practices, 2 Tasks each)
    -- ============================================================
    PRINT 'Seeding Practices and Tasks...';

    -- Practice 1: Pre-op Check
    INSERT INTO [dbo].[practices] (practice_name, practice_description, practice_code, difficulty_level, estimated_duration_minutes)
    VALUES ('Pre-Operation Inspection', 'Daily checks before starting engine', 'PRAC-01', 'Easy', 15);
    DECLARE @Prac1Id INT = SCOPE_IDENTITY();

    -- Tasks for Practice 1
    INSERT INTO [dbo].[sim_tasks] (task_name, task_code, expected_result) VALUES 
    ('Check Oil Level', 'TASK-01-01', 'Oil level between min and max'),
    ('Inspect Tires', 'TASK-01-02', 'Tire pressure normal, no cuts');
    -- Link tasks to Practice 1
    INSERT INTO [dbo].[practice_tasks] (practice_id, task_id) 
    SELECT @Prac1Id, id FROM [dbo].[sim_tasks] WHERE task_code IN ('TASK-01-01', 'TASK-01-02');

    -- Practice 2: Load Lifting
    INSERT INTO [dbo].[practices] (practice_name, practice_description, practice_code, difficulty_level, estimated_duration_minutes)
    VALUES ('Container Lifting', 'Lifting standard 20ft container', 'PRAC-02', 'Hard', 45);
    DECLARE @Prac2Id INT = SCOPE_IDENTITY();

    -- Tasks for Practice 2
    INSERT INTO [dbo].[sim_tasks] (task_name, task_code, expected_result) VALUES 
    ('Secure Locks', 'TASK-02-01', 'All twist locks secured'),
    ('Lift to Safe Height', 'TASK-02-02', 'Load stable at 2m height');
    -- Link tasks to Practice 2
    INSERT INTO [dbo].[practice_tasks] (practice_id, task_id) 
    SELECT @Prac2Id, id FROM [dbo].[sim_tasks] WHERE task_code IN ('TASK-02-01', 'TASK-02-02');

    -- ============================================================
    -- 6. Linking Practices to Courses (Optional but recommended)
    --    This makes the practices appear inside the course structure
    -- ============================================================
    PRINT 'Linking Practices to Course Curriculum...';

    -- Create a Section for Course 2
    INSERT INTO [dbo].[sections] (section_title, section_description) 
    VALUES ('Simulation Modules', 'Practical simulation exercises');
    DECLARE @SectionId INT = SCOPE_IDENTITY();

    INSERT INTO [dbo].[course_sections] (course_id, section_id, section_order)
    VALUES (@Course2Id, @SectionId, 1);

    -- Create Activities for the Practices
    INSERT INTO [dbo].[activities] (activity_title, activity_type, estimated_duration_minutes) VALUES
    ('Daily Inspection Sim', 2, 15), -- Type 2 = Practice (assumed based on context)
    ('Container Lift Sim', 2, 45);
    DECLARE @Act2Id INT = SCOPE_IDENTITY();
    DECLARE @Act1Id INT = @Act2Id - 1;

    -- Link Activities to Section
    INSERT INTO [dbo].[section_activities] (section_id, activity_id, activity_order) VALUES
    (@SectionId, @Act1Id, 1),
    (@SectionId, @Act2Id, 2);

    -- Link Activities to Practices
    INSERT INTO [dbo].[activity_practices] (activity_id, practice_id, is_active) VALUES
    (@Act1Id, @Prac1Id, 1),
    (@Act2Id, @Prac2Id, 1);

    COMMIT TRANSACTION;
    PRINT 'Database seeded successfully.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR (@ErrorMessage, 16, 1);
END CATCH
GO