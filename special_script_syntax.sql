

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



CREATE TABLE [dbo].[sim_actions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(255) NOT NULL,
    [description] NVARCHAR(2000),
    [action_key] NVARCHAR(50),
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0
);

CREATE TABLE [dbo].[practice_step_actions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [step_id] INT NOT NULL,
    [action_id] INT NOT NULL,
    [name] NVARCHAR(255) NOT NULL,
    [description] NVARCHAR(2000),
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([step_id]) REFERENCES [dbo].[practice_steps]([id]),
    FOREIGN KEY ([action_id]) REFERENCES [dbo].[sim_actions]([id])
);



DROP TABLE [dbo].[simulation_component_types];
DROP TABLE [dbo].[practice_step_types];