
USE [lssctc-db]

BEGIN TRY
    BEGIN TRANSACTION;

    -- Your statements here
    -- Seed data for Crane Driver Training Center Management System



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




    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;

    -- Optional: Raise error info for debugging/logging
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR (@ErrorMessage, 16, 1);
END CATCH
