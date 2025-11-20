

-- Drop Database

ALTER DATABASE [lssctc-db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [lssctc-db];

-- 1. Drop all Foreign Key Constraints
DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql += N'
ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) 
    + ' DROP CONSTRAINT ' + QUOTENAME(fk.name) + ';'
FROM sys.foreign_keys AS fk
INNER JOIN sys.tables AS t ON fk.parent_object_id = t.object_id
INNER JOIN sys.schemas AS s ON t.schema_id = s.schema_id;

EXEC sp_executesql @sql;

-- 2. Drop all Tables
SET @sql = N'';

SELECT @sql += N'
DROP TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';'
FROM sys.tables AS t
INNER JOIN sys.schemas AS s ON t.schema_id = s.schema_id;

EXEC sp_executesql @sql;





DROP TABLE [dbo].[simulation_component_types];
DROP TABLE [dbo].[practice_step_types];




ALTER TABLE [dbo].[course_sections]
ADD [section_order] INT NOT NULL DEFAULT 0;


ALTER TABLE [dbo].[section_records]
ADD [duration_minutes] INT NULL 
    CONSTRAINT DF_section_records_duration_minutes DEFAULT 20;


alter table practices
add [practice_code] nvarchar null default 'NO PRACTICE CODE ASSIGNED'

ALTER TABLE practices
ALTER COLUMN practice_code VARCHAR(50);

SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'practices'
  AND COLUMN_NAME = 'practice_code';

  ALTER TABLE practices
DROP CONSTRAINT DF__practices__pract__03FB8544;

ALTER TABLE practices
ADD CONSTRAINT DF_practices_practice_code
DEFAULT('NO PRACTICE CODE ASSIGNED') FOR practice_code;

ALTER TABLE sim_tasks
ADD task_code NVARCHAR(50) default 'NO PRACTICE CODE ASSIGNED'

update sim_tasks
set task_code = 'TASK_UNKNOWN'

alter table simulation_components
add component_code nvarchar(50) NULL default 'COMPONENT_UNKNOWN'