BEGIN TRY
    BEGIN TRANSACTION;

    -- Your statements here
    -- Seed data for Crane Driver Training Center Management System

-- Course Categories (Crane-specific)
INSERT INTO [course_categories] ([name]) VALUES 
('Mobile Crane Operations'),
('Tower Crane Operations'),
('Overhead Crane Operations'),
('Safety and Regulations'),
('Equipment Maintenance'),
('Load Calculations'),
('Signal Communications'),
('Site Preparation');

-- Course Levels
INSERT INTO [course_levels] ([name]) VALUES 
('Foundation'),
('Basic Operator'),
('Advanced Operator'),
('Master Operator');

-- Material Types
INSERT INTO [material_types] ([name]) VALUES 
('Training Manual'),
('Safety Video'),
('Technical Diagram'),
('Simulation Exercise'),
('Assessment Form'),
('Certificate Template'),
('Maintenance Guide'),
('Regulation Document');

-- Session Types
INSERT INTO [session_types] ([name], [description]) VALUES 
('Theory Session', 'Classroom-based theoretical learning'),
('Practical Training', 'Hands-on crane operation training'),
('Simulation Session', 'Virtual reality crane operation practice'),
('Safety Briefing', 'Safety procedures and regulations training'),
('Assessment Session', 'Skills evaluation and testing'),
('Maintenance Training', 'Equipment maintenance and inspection'),
('Site Visit', 'Real construction site observation'),
('Certification Exam', 'Final certification examination');

-- Course Definitions (Crane Training Courses)
INSERT INTO [course_definitions] ([title], [course_code], [description], [category_id], [level_id], [duration]) VALUES 
('Mobile Crane Foundation Course', 'MC-F01', 'Basic principles and safety for mobile crane operations', 1, 1, 80),
('Tower Crane Basic Operations', 'TC-B01', 'Fundamental tower crane operating procedures and safety', 2, 2, 120),
('Load Calculation and Planning', 'LC-B01', 'Understanding load charts, weight calculations, and lift planning', 6, 2, 40),
('Advanced Mobile Crane Operations', 'MC-A01', 'Advanced techniques for complex mobile crane operations', 1, 3, 100),
('Crane Safety and Regulations', 'SR-F01', 'Comprehensive safety training and regulatory compliance', 4, 1, 60),
('Signal Communication Systems', 'SC-B01', 'Hand signals and radio communication for crane operations', 7, 2, 30),
('Overhead Bridge Crane Operations', 'OC-B01', 'Operating procedures for overhead and bridge cranes', 3, 2, 70);

-- Learning Materials (Crane-specific)
INSERT INTO [learning_materials] ([title], [content], [source_url], [material_type_id]) VALUES 
('Mobile Crane Safety Manual', 'Comprehensive safety guidelines for mobile crane operations', '/materials/mobile-crane-safety.pdf', 1),
('Tower Crane Setup Procedures', 'Step-by-step tower crane assembly and setup instructions', '/materials/tower-crane-setup-video.mp4', 2),
('Load Chart Reading Guide', 'How to interpret and use crane load capacity charts', '/materials/load-chart-guide.pdf', 3),
('Basic Hand Signals Reference', 'Standard hand signals for crane communication', '/materials/hand-signals-poster.pdf', 3),
('Pre-Operation Inspection Checklist', 'Daily inspection procedures for crane equipment', '/materials/inspection-checklist.pdf', 5),
('Crane Maintenance Handbook', 'Regular maintenance procedures and schedules', '/materials/maintenance-handbook.pdf', 7),
('OSHA Crane Regulations', 'Current OSHA standards for crane operations', '/materials/osha-regulations.pdf', 8),
('Simulation Exercise: Basic Lifting', 'Virtual practice for basic lifting operations', '/simulations/basic-lifting', 4);

-- Sample Active Courses
INSERT INTO [courses] ([course_definition_id], [title], [course_code], [description], [category], [level], [start_date], [end_date], [location], [status]) VALUES 
(1, 'Mobile Crane Foundation - September 2025', 'MC-F01-SEP25', 'Foundation course for new crane operators', 'Mobile Crane Operations', 'Foundation', '2025-09-15', '2025-10-15', 'Training Room A', 'planned'),
(2, 'Tower Crane Operations - October 2025', 'TC-B01-OCT25', 'Basic tower crane operation training', 'Tower Crane Operations', 'Basic Operator', '2025-10-01', '2025-11-15', 'Training Yard', 'planned'),
(5, 'Safety Training - September 2025', 'SR-F01-SEP25', 'Mandatory safety training for all operators', 'Safety and Regulations', 'Foundation', '2025-09-10', '2025-09-25', 'Safety Training Center', 'ongoing'),
(3, 'Load Calculations Workshop', 'LC-B01-SEP25', 'Practical load calculation training', 'Load Calculations', 'Basic Operator', '2025-09-20', '2025-10-05', 'Classroom B', 'planned');

-- Tests (Crane-specific assessments)
INSERT INTO [tests] ([name], [description], [duration_minutes], [total_points]) VALUES 
('Mobile Crane Safety Test', 'Assessment of safety knowledge for mobile crane operations', 45, 100.00),
('Load Calculation Practical Exam', 'Hands-on test of load calculation and planning skills', 90, 150.00),
('Hand Signals Proficiency Test', 'Evaluation of signal communication skills', 30, 50.00),
('Tower Crane Theory Exam', 'Comprehensive theoretical knowledge test for tower cranes', 60, 120.00),
('Final Certification Assessment', 'Complete practical and theoretical evaluation', 180, 200.00);

-- Test Questions (Sample crane-related questions)
INSERT INTO [test_questions] ([test_id], [is_multiple_answers], [option_quantity], [answer_quantity], [points], [explanation]) VALUES 
(1, 0, 4, 1, 10.00, 'Basic safety protocol knowledge'),
(1, 1, 5, 2, 15.00, 'Multiple safety considerations in crane operations'),
(2, 0, 4, 1, 25.00, 'Load chart interpretation and calculation'),
(3, 0, 3, 1, 10.00, 'Standard hand signal recognition'),
(4, 0, 4, 1, 20.00, 'Tower crane assembly procedures'),
(5, 0, 4, 1, 40.00, 'Complex operational scenario');

-- Question Options (Sample answers)
INSERT INTO [question_options] ([question_id], [is_correct], [points], [order_index]) VALUES 
-- Question 1: Safety protocol
(1, 0, 0, 1), (1, 1, 10, 2), (1, 0, 0, 3), (1, 0, 0, 4),
-- Question 2: Multiple safety considerations
(2, 1, 7.5, 1), (2, 0, 0, 2), (2, 1, 7.5, 3), (2, 0, 0, 4), (2, 0, 0, 5),
-- Question 3: Load calculation
(3, 0, 0, 1), (3, 1, 25, 2), (3, 0, 0, 3), (3, 0, 0, 4),
-- Question 4: Hand signals
(4, 1, 10, 1), (4, 0, 0, 2), (4, 0, 0, 3),
-- Question 5: Tower crane procedures
(5, 0, 0, 1), (5, 0, 0, 2), (5, 1, 20, 3), (5, 0, 0, 4),
-- Question 6: Complex scenario
(6, 0, 0, 1), (6, 0, 0, 2), (6, 0, 0, 3), (6, 1, 40, 4);

-- Simulation Tasks (Crane operation scenarios)
INSERT INTO [simulation_tasks] ([course_id], [name], [description], [passing_criteria]) VALUES 
(1, 'Basic Load Lifting Simulation', 'Practice basic lifting and placing operations in virtual environment', 'Complete lift without safety violations, accuracy within 5cm'),
(2, 'Tower Crane Assembly Simulation', 'Virtual practice of tower crane setup and dismantling procedures', 'Complete assembly following all safety protocols, no critical errors'),
(4, 'Complex Load Planning Exercise', 'Plan and execute a multi-crane lift operation', 'Demonstrate proper load distribution and coordination, complete within time limit'),
(3, 'Emergency Response Simulation', 'Handle emergency situations during crane operations', 'Respond correctly to all emergency scenarios, follow evacuation procedures');

-- Link courses with their materials
INSERT INTO [course_materials] ([course_id], [material_id]) VALUES 
(1, 1), -- Mobile crane course with safety manual
(1, 4), -- Mobile crane course with hand signals
(1, 5), -- Mobile crane course with inspection checklist
(2, 2), -- Tower crane course with setup video
(2, 7), -- Tower crane course with OSHA regulations
(3, 3), -- Load calculation course with load chart guide
(4, 3), -- Load calculation course with load chart guide
(4, 6); -- Advanced course with maintenance handbook

-- Sample Training Sessions
INSERT INTO [training_sessions] ([name], [description], [session_type_id], [instructor_id], [start_date], [end_date], [location]) VALUES 
('Mobile Crane Safety Briefing', 'Mandatory safety orientation for new trainees', 4, 3, '2025-09-15 09:00:00', '2025-09-15 12:00:00', 'Safety Training Room'),
('Basic Lifting Simulation Session', 'Hands-on practice with crane simulator', 3, 3, '2025-09-16 14:00:00', '2025-09-16 17:00:00', 'Simulation Lab 1'),
('Load Calculation Workshop', 'Practical load planning and calculation training', 1, 3, '2025-09-20 10:00:00', '2025-09-20 16:00:00', 'Classroom A'),
('Tower Crane Practical Assessment', 'Skills evaluation on tower crane operations', 5, 3, '2025-10-15 09:00:00', '2025-10-15 17:00:00', 'Training Yard');

-- Link courses with training sessions
INSERT INTO [course_sessions] ([course_id], [training_session_id]) VALUES 
(1, 1), -- Mobile crane course with safety briefing
(1, 2), -- Mobile crane course with simulation session
(4, 3), -- Load calculation course with workshop
(2, 4); -- Tower crane course with practical assessment

-- Link training sessions with simulation tasks
INSERT INTO [training_session_simulation_tasks] ([training_session_id], [simulation_task_id]) VALUES 
(2, 1), -- Basic simulation session with basic lifting task
(4, 2); -- Tower crane session with assembly simulation

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;

    -- Optional: Raise error info for debugging/logging
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR (@ErrorMessage, 16, 1);
END CATCH
