
USE [lssctc-db]

BEGIN TRY
    BEGIN TRANSACTION;

    -- Your statements here
    -- Seed data for Crane Driver Training Center Management System

INSERT INTO [dbo].[training_programs] (
    name,
    description,
    duration_hours,
    total_courses,
    image_url
)
VALUES
(
    N'Mobile Crane Operator Certificate Program',
    N'Comprehensive training program covering all core skills required to safely and efficiently operate mobile cranes in industrial and construction environments. Includes theory, hands-on, and simulator practice.',
    120,
    5,
    N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR0wLbWNRr8K3ijqNcQsjBDbeSzI4mW4RffOA&s'
),
(
    N'Advanced Mobile Crane Skills Program',
    N'For experienced operators seeking to master complex lifts, safety leadership, and advanced simulation scenarios.',
    60,
    3,
    N'https://adrkurs.pl/wp-content/uploads/2023/03/hvg.jpeg'
);

INSERT INTO [dbo].[certificates] (
    name,
    description,
    effective_time,
    requirement,
    certifying_authority,
    image_url
)
VALUES
(
    N'Mobile Crane Operator Certificate',
    N'Issued upon successful completion of the Mobile Crane Operator Certificate Program. Recognizes core operational skills and safety awareness.',
    60,
    N'Pass all required courses, hold valid driver license, complete practical assessment.',
    N'LSSCTC Training Center',
    N'https://cdn-01-artemis.media-brady.com/Assets/ImageRoot/WPSAmericasWeb_Name/47/19/certification-wallet-cards-cnwc45-lg_dam_3624719.jpg'
),
(
    N'Advanced Crane Operations Certificate',
    N'Granted to operators demonstrating mastery of advanced skills and safety leadership.',
    36,
    N'Completion of advanced program and final assessment.',
    N'LSSCTC Training Center',
    N'https://store.iti.com/cdn/shop/products/MCOE-FormwTab-Web-Page01.jpg?v=1572472615'
);

INSERT INTO [dbo].[course_categories] (name, description)
VALUES
(N'Safety & Regulations', N'Courses on mobile crane safety, legal regulations, and compliance best practices.'),
(N'Basic Operation', N'Fundamental skills for new crane operators, including controls and standard lifting procedures.'),
(N'Advanced Operation', N'Complex maneuvers, troubleshooting, and advanced load handling techniques for experienced operators.'),
(N'Equipment Maintenance', N'Crane inspection, daily checks, and preventive maintenance.'),
(N'Certification Preparation', N'Review and practice modules to prepare for certification exams.');

INSERT INTO [dbo].[course_levels] (name, description)
VALUES
(N'Entry', N'For new trainees with no prior experience or minimal crane operation background.'),
(N'Standard', N'For trainees who have mastered basic skills and are ready for core operations.'),
(N'Expert', N'For seasoned operators aiming to master advanced skills and safety leadership.');

INSERT INTO [dbo].[course_codes] (name)
VALUES
(N'CO000001'), (N'CO000002'), (N'CO000003'), (N'CO000004'), (N'CO000005');

INSERT INTO [dbo].[class_codes] (name)
VALUES
(N'CL000001'), (N'CL000002'), (N'CL000003'), (N'CL000004'), (N'CL000005');

INSERT INTO [dbo].[learning_material_types] (name)
VALUES
(N'PDF'), (N'Video'), (N'Image'), (N'URL');


INSERT INTO [dbo].[learning_materials] (
    learning_material_type_id,
    name,
    description,
    material_url
)
VALUES
(
    1,
    N'Safety Handbook',
    N'Official LSSCTC safety procedures and legal compliance requirements.',
    N'https://www.osha.gov/sites/default/files/publications/OSHA3682.pdf'
),
(
    2,
    N'Crane Controls Introduction',
    N'Video walkthrough of standard mobile crane controls and instrumentation.',
    N'https://www.youtube.com/watch?v=yNkLy68VFMQ'
);



    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;

    -- Optional: Raise error info for debugging/logging
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR (@ErrorMessage, 16, 1);
END CATCH
