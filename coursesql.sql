-- ================================
-- Insert sample data for CourseCategory
-- ================================
INSERT INTO course_categories(Name, Description)
VALUES 
(N'Crane Operation', N'Practical and theoretical training for crane operators'),
(N'Safety & Regulations', N'Workplace safety, crane regulations, and compliance'),
(N'Maintenance', N'Crane inspection, servicing, and troubleshooting');

-- ================================
-- Insert sample data for CourseLevel
-- ================================
INSERT INTO course_levels(Name, Description)
VALUES
(N'Beginner', N'Entry-level crane courses for new learners'),
(N'Intermediate', N'Courses for operators with some prior experience'),
(N'Advanced', N'Specialized courses for professional crane operators and supervisors');

-- ================================
-- Insert sample data for Course
-- ================================
INSERT INTO courses(Name, Description, category_id, level_id, Price, is_active, is_deleted, image_url, duration_hours, course_code_id)
VALUES
-- Crane Operation
(N'Crane Basics 101', 
 N'Introduction to different types of cranes, controls, and simulation environment.', 
 1, 1, 120.00, 1, 0, N'https://www.google.com.vn/imgres?q=crane&imgurl=https%3A%2F%2Fwww.wppecrane.com%2Fwp-content%2Fuploads%2F2021%2F12%2FGHC110-Square.jpg&imgrefurl=https%3A%2F%2Fwww.wppecrane.com%2Fequipment%2Ftelescoping-crawler-cranes%2Fgrove-ghc110-telescopic-crawler-crane%2F&docid=4V5B4nDMyZgY6M&tbnid=NomKjcAyz3-ySM&vet=12ahUKEwiw6dTFn9qPAxUemq8BHUdYBAAQM3oECCwQAA..i&w=2500&h=2500&hcb=2&ved=2ahUKEwiw6dTFn9qPAxUemq8BHUdYBAAQM3oECCwQAA', 20, NULL),

(N'Tower Crane Simulation', 
 N'Operate tower cranes in a safe, controlled simulation with real-world scenarios.', 
 1, 2, 250.00, 1, 0, N'https://www.google.com.vn/imgres?q=crane&imgurl=https%3A%2F%2Fwww.wppecrane.com%2Fwp-content%2Fuploads%2F2021%2F12%2FGHC110-Square.jpg&imgrefurl=https%3A%2F%2Fwww.wppecrane.com%2Fequipment%2Ftelescoping-crawler-cranes%2Fgrove-ghc110-telescopic-crawler-crane%2F&docid=4V5B4nDMyZgY6M&tbnid=NomKjcAyz3-ySM&vet=12ahUKEwiw6dTFn9qPAxUemq8BHUdYBAAQM3oECCwQAA..i&w=2500&h=2500&hcb=2&ved=2ahUKEwiw6dTFn9qPAxUemq8BHUdYBAAQM3oECCwQAA', 40, NULL),

(N'Mobile Crane Mastery', 
 N'Advanced simulation for mobile cranes including load management and complex lifts.', 
 1, 3, 400.00, 1, 0, N'https://www.google.com.vn/imgres?q=crane&imgurl=https%3A%2F%2Fwww.wppecrane.com%2Fwp-content%2Fuploads%2F2021%2F12%2FGHC110-Square.jpg&imgrefurl=https%3A%2F%2Fwww.wppecrane.com%2Fequipment%2Ftelescoping-crawler-cranes%2Fgrove-ghc110-telescopic-crawler-crane%2F&docid=4V5B4nDMyZgY6M&tbnid=NomKjcAyz3-ySM&vet=12ahUKEwiw6dTFn9qPAxUemq8BHUdYBAAQM3oECCwQAA..i&w=2500&h=2500&hcb=2&ved=2ahUKEwiw6dTFn9qPAxUemq8BHUdYBAAQM3oECCwQAA', 50, NULL),

-- Safety & Regulations
(N'Crane Safety Essentials', 
 N'OSHA standards, site safety, and emergency protocols for crane operation.', 
 2, 1, 100.00, 1, 0, N'https://www.google.com.vn/imgres?q=crane&imgurl=https%3A%2F%2Fwww.wppecrane.com%2Fwp-content%2Fuploads%2F2021%2F12%2FGHC110-Square.jpg&imgrefurl=https%3A%2F%2Fwww.wppecrane.com%2Fequipment%2Ftelescoping-crawler-cranes%2Fgrove-ghc110-telescopic-crawler-crane%2F&docid=4V5B4nDMyZgY6M&tbnid=NomKjcAyz3-ySM&vet=12ahUKEwiw6dTFn9qPAxUemq8BHUdYBAAQM3oECCwQAA..i&w=2500&h=2500&hcb=2&ved=2ahUKEwiw6dTFn9qPAxUemq8BHUdYBAAQM3oECCwQAA',50,NULL)
