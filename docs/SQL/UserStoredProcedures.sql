-- =============================================
-- User Management Stored Procedures
-- These are the SQL stored procedures needed for the UserController
-- =============================================

-- =============================================
-- Table Schema (Create this first)
-- =============================================
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- =============================================
-- 1. Get All Users
-- =============================================
CREATE PROCEDURE sp_GetAllUsers
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Username,
        Email,
        PhoneNumber,
        Address,
        CreatedAt,
        UpdatedAt
    FROM Users
    WHERE IsActive = 1
    ORDER BY CreatedAt DESC;
END
GO

-- =============================================
-- 2. Get User By ID (with additional details)
-- =============================================
CREATE PROCEDURE sp_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.Id,
        u.Username,
        u.Email,
        u.PhoneNumber,
        u.Address,
        u.CreatedAt,
        u.UpdatedAt,
        -- Dummy data for demonstration
        ISNULL((SELECT COUNT(*) FROM Orders WHERE UserId = u.Id), 0) AS TotalOrders,
        ISNULL((SELECT SUM(TotalAmount) FROM Orders WHERE UserId = u.Id), 0) AS TotalSpent,
        (SELECT MAX(LoginDate) FROM UserLogins WHERE UserId = u.Id) AS LastLoginDate
    FROM Users u
    WHERE u.Id = @UserId AND u.IsActive = 1;
END
GO

-- =============================================
-- 3. Search Users By Email
-- =============================================
CREATE PROCEDURE sp_SearchUsersByEmail
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Username,
        Email,
        PhoneNumber,
        Address,
        CreatedAt,
        UpdatedAt
    FROM Users
    WHERE Email LIKE '%' + @Email + '%' 
        AND IsActive = 1
    ORDER BY Email;
END
GO

-- =============================================
-- 4. Create User
-- =============================================
CREATE PROCEDURE sp_CreateUser
    @Username NVARCHAR(100),
    @Email NVARCHAR(255),
    @PhoneNumber NVARCHAR(20),
    @Address NVARCHAR(500),
    @UserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if email already exists
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
        BEGIN
            RAISERROR('Email already exists', 16, 1);
            RETURN;
        END
        
        INSERT INTO Users (Username, Email, PhoneNumber, Address, CreatedAt, IsActive)
        VALUES (@Username, @Email, @PhoneNumber, @Address, GETUTCDATE(), 1);
        
        SET @UserId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        
        -- Return the created user
        SELECT 
            Id,
            Username,
            Email,
            PhoneNumber,
            Address,
            CreatedAt,
            UpdatedAt
        FROM Users
        WHERE Id = @UserId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- 5. Update User
-- =============================================
CREATE PROCEDURE sp_UpdateUser
    @UserId INT,
    @Username NVARCHAR(100),
    @Email NVARCHAR(255),
    @PhoneNumber NVARCHAR(20),
    @Address NVARCHAR(500),
    @RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if email is being changed and if it already exists
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND Id != @UserId)
        BEGIN
            RAISERROR('Email already exists', 16, 1);
            RETURN;
        END
        
        UPDATE Users
        SET 
            Username = @Username,
            Email = @Email,
            PhoneNumber = @PhoneNumber,
            Address = @Address,
            UpdatedAt = GETUTCDATE()
        WHERE Id = @UserId AND IsActive = 1;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        -- Return the updated user
        SELECT 
            Id,
            Username,
            Email,
            PhoneNumber,
            Address,
            CreatedAt,
            UpdatedAt
        FROM Users
        WHERE Id = @UserId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- 6. Delete User (Soft Delete)
-- =============================================
CREATE PROCEDURE sp_DeleteUser
    @UserId INT,
    @RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET 
        IsActive = 0,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @UserId;
    
    SET @RowsAffected = @@ROWCOUNT;
END
GO

-- =============================================
-- 7. Get Active Users
-- =============================================
CREATE PROCEDURE sp_GetActiveUsers
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Username,
        Email,
        PhoneNumber,
        Address,
        CreatedAt,
        UpdatedAt
    FROM Users
    WHERE IsActive = @IsActive
    ORDER BY CreatedAt DESC;
END
GO

-- =============================================
-- 8. Get User Statistics (Multiple Result Sets)
-- =============================================
CREATE PROCEDURE sp_GetUserStatistics
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Result Set 1: User basic info
    SELECT 
        Id,
        Username,
        Email,
        PhoneNumber,
        Address,
        CreatedAt,
        UpdatedAt
    FROM Users
    WHERE Id = @UserId;
    
    -- Result Set 2: Orders summary (dummy data)
    SELECT 
        COUNT(*) AS TotalOrders,
        ISNULL(SUM(TotalAmount), 0) AS TotalSpent,
        ISNULL(AVG(TotalAmount), 0) AS AverageOrderValue,
        MAX(OrderDate) AS LastOrderDate
    FROM Orders
    WHERE UserId = @UserId;
    
    -- Result Set 3: Recent activities (dummy data)
    SELECT TOP 10
        ActivityType,
        ActivityDescription,
        ActivityDate
    FROM UserActivities
    WHERE UserId = @UserId
    ORDER BY ActivityDate DESC;
END
GO

-- =============================================
-- 9. Create Welcome Order (for transaction example)
-- =============================================
CREATE PROCEDURE sp_CreateWelcomeOrder
    @UserId INT,
    @OrderType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Orders (UserId, OrderType, OrderDate, TotalAmount, Status)
    VALUES (@UserId, @OrderType, GETUTCDATE(), 0.00, 'Pending');
    
    RETURN SCOPE_IDENTITY();
END
GO

-- =============================================
-- Supporting Tables (for demonstration)
-- =============================================

-- Orders table
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    OrderType NVARCHAR(50),
    OrderDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
);
GO

-- User Activities table
CREATE TABLE UserActivities (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    ActivityType NVARCHAR(50) NOT NULL,
    ActivityDescription NVARCHAR(500),
    ActivityDate DATETIME NOT NULL DEFAULT GETUTCDATE()
);
GO

-- User Logins table
CREATE TABLE UserLogins (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    LoginDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
    IpAddress NVARCHAR(50)
);
GO

-- =============================================
-- Sample Data For Testing
-- =============================================

-- Insert sample users
INSERT INTO Users (Username, Email, PhoneNumber, Address, CreatedAt, IsActive)
VALUES 
    ('John Doe', 'john.doe@example.com', '+1234567890', '123 Main St, City', GETUTCDATE(), 1),
    ('Jane Smith', 'jane.smith@example.com', '+0987654321', '456 Oak Ave, Town', GETUTCDATE(), 1),
    ('Bob Johnson', 'bob.johnson@example.com', '+1122334455', '789 Pine Rd, Village', GETUTCDATE(), 1);
GO

-- Insert sample orders
INSERT INTO Orders (UserId, OrderType, OrderDate, TotalAmount, Status)
VALUES 
    (1, 'Purchase', GETUTCDATE(), 150.00, 'Completed'),
    (1, 'Purchase', GETUTCDATE(), 75.50, 'Completed'),
    (2, 'Purchase', GETUTCDATE(), 200.00, 'Pending');
GO

-- Insert sample activities
INSERT INTO UserActivities (UserId, ActivityType, ActivityDescription, ActivityDate)
VALUES 
    (1, 'Login', 'User logged in', GETUTCDATE()),
    (1, 'Order', 'Placed order #1001', GETUTCDATE()),
    (2, 'Login', 'User logged in', GETUTCDATE());
GO

-- Insert sample logins
INSERT INTO UserLogins (UserId, LoginDate, IpAddress)
VALUES 
    (1, GETUTCDATE(), '192.168.1.1'),
    (2, GETUTCDATE(), '192.168.1.2');
GO
