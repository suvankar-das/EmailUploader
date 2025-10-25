-- =============================================
--  Create Database
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'EmailUploaderDB')
BEGIN
    CREATE DATABASE EmailUploaderDB;
END
GO

USE EmailUploaderDB;
GO

-- =============================================
--  Create Emails Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Emails')
BEGIN
    CREATE TABLE dbo.Emails
    (
        EmailId INT IDENTITY(1,1) PRIMARY KEY,
        EmailAddress NVARCHAR(255) NOT NULL UNIQUE,
        UploadedDate DATETIME NOT NULL DEFAULT GETDATE(),
        IsActive BIT NOT NULL DEFAULT 1
    );

    CREATE NONCLUSTERED INDEX IX_Emails_EmailAddress 
    ON dbo.Emails(EmailAddress);
    
    CREATE NONCLUSTERED INDEX IX_Emails_UploadedDate 
    ON dbo.Emails(UploadedDate DESC);
END
GO

-- =============================================
--  Create User-Defined Table Type
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'EmailListType' AND is_table_type = 1)
BEGIN
    CREATE TYPE dbo.EmailListType AS TABLE
    (
        Email NVARCHAR(255) NOT NULL
    );
END
GO

-- =============================================
--  Create Stored Procedure - Insert Unique Emails
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'usp_InsertUniqueEmails')
BEGIN
    DROP PROCEDURE dbo.usp_InsertUniqueEmails;
END
GO

CREATE PROCEDURE dbo.usp_InsertUniqueEmails
    @EmailList dbo.EmailListType READONLY,
    @InsertedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Insert only emails that don't already exist (case-insensitive)
        INSERT INTO dbo.Emails (EmailAddress, UploadedDate, IsActive)
        SELECT DISTINCT LOWER(LTRIM(RTRIM(el.Email))), GETDATE(), 1
        FROM @EmailList el
        WHERE NOT EXISTS (
            SELECT 1 
            FROM dbo.Emails e 
            WHERE LOWER(e.EmailAddress) = LOWER(LTRIM(RTRIM(el.Email)))
        )
        AND el.Email IS NOT NULL
        AND LTRIM(RTRIM(el.Email)) <> '';
        
        -- Get the count of inserted rows
        SET @InsertedCount = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
        RETURN -1;
    END CATCH
END
GO

-- =============================================
--  Create Stored Procedure - Get All Emails
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'usp_GetAllEmails')
BEGIN
    DROP PROCEDURE dbo.usp_GetAllEmails;
END
GO

CREATE PROCEDURE dbo.usp_GetAllEmails
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        EmailId,
        EmailAddress,
        UploadedDate,
        IsActive
    FROM dbo.Emails
    WHERE IsActive = 1
    ORDER BY UploadedDate DESC;
END
GO

-- =============================================
--  Create Stored Procedure - Get Email Count
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'usp_GetEmailCount')
BEGIN
    DROP PROCEDURE dbo.usp_GetEmailCount;
END
GO

CREATE PROCEDURE dbo.usp_GetEmailCount
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT @TotalCount = COUNT(*)
    FROM dbo.Emails
    WHERE IsActive = 1;
    
    RETURN 0;
END
GO

-- =============================================
-- Verification Queries
-- =============================================
PRINT 'Database setup completed successfully!';
PRINT '';
PRINT 'Verification:';
SELECT 'Table Created' AS Status, COUNT(*) AS EmailCount FROM dbo.Emails;
GO

-- ===============================================
-- Reset
-- ===============================================
TRUNCATE TABLE Emails