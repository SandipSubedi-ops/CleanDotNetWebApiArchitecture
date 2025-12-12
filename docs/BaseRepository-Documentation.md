# BaseRepository - Generic Repository for Stored Procedures

## Overview
The `BaseRepository` provides a comprehensive, generic repository pattern implementation for executing stored procedures with Dapper. It supports:
- Single and multiple result sets
- Transaction management
- Multi-database/multi-tenant support
- Output parameters
- Dynamic parameter binding

## Features

### 1. **Default Connection Methods**
Methods that use the default connection string from `appsettings.json`:

- `GetList<T>()` - Execute SP and return a list
- `GetSingleData<T>()` - Execute SP and return a single item
- `PostandGetSingleData<T>()` - Execute SP (INSERT/UPDATE) and return result
- `GetMultipleData()` - Execute SP and return multiple result sets

### 2. **Generic (Multi-Database) Methods**
Methods that use a specific connection string based on `propCode/clientCode`:

- `GetListGen<T>()` - Execute SP on specific database and return a list
- `GetSingleDataGen<T>()` - Execute SP on specific database and return a single item
- `PostandGetSingleDataGen<T>()` - Execute SP on specific database (INSERT/UPDATE) and return result
- `GetMultipleDataGen()` - Execute SP on specific database and return multiple result sets

### 3. **Transaction Management**
- `BeginTransaction()` - Begin transaction on default connection
- `BeginTransactionGen(propCode)` - Begin transaction on specific client database

## Setup

### 1. Configure Connection Strings
Update your `appsettings.json` with multiple connection strings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MainDb;Trusted_Connection=True",
    "Client001": "Server=(localdb)\\mssqllocaldb;Database=Client001Db;Trusted_Connection=True",
    "Client002": "Server=(localdb)\\mssqllocaldb;Database=Client002Db;Trusted_Connection=True"
  }
}
```

### 2. Register in DI Container
The `BaseRepository` is already registered in `ServiceExtensions.cs`:

```csharp
services.AddScoped<IBaseRepository, BaseRepository>();
```

### 3. Inject into Your Services
```csharp
public class YourService
{
    private readonly IBaseRepository _baseRepository;

    public YourService(IBaseRepository baseRepository)
    {
        _baseRepository = baseRepository;
    }
}
```

## Usage Examples

### Example 1: Get a List of Items
```csharp
public async Task<List<User>> GetAllUsers()
{
    var parameters = new DynamicParameters();
    parameters.Add("@Status", "Active");

    return await _baseRepository.GetList<User>(
        "sp_GetAllUsers",
        parameters);
}
```

### Example 2: Get a Single Item
```csharp
public async Task<User> GetUserById(int userId)
{
    var parameters = new DynamicParameters();
    parameters.Add("@UserId", userId);

    return await _baseRepository.GetSingleData<User>(
        "sp_GetUserById",
        parameters);
}
```

### Example 3: Insert with Output Parameter
```csharp
public async Task<int> CreateUser(string email, string userName)
{
    var parameters = new DynamicParameters();
    parameters.Add("@Email", email);
    parameters.Add("@UserName", userName);
    parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

    await _baseRepository.PostandGetSingleData<int>(
        "sp_CreateUser",
        parameters);

    // Get the output parameter value
    return parameters.Get<int>("@UserId");
}
```

### Example 4: Using Transactions
```csharp
public async Task<bool> CreateUserWithAudit(string email, string userName)
{
    IDbTransaction transaction = null;
    try
    {
        transaction = _baseRepository.BeginTransaction();

        // Insert user
        var userParams = new DynamicParameters();
        userParams.Add("@Email", email);
        userParams.Add("@UserName", userName);
        userParams.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _baseRepository.PostandGetSingleData<int>(
            "sp_CreateUser",
            userParams,
            transaction);

        var userId = userParams.Get<int>("@UserId");

        // Log audit
        var auditParams = new DynamicParameters();
        auditParams.Add("@UserId", userId);
        auditParams.Add("@Action", "UserCreated");

        await _baseRepository.PostandGetSingleData<int>(
            "sp_LogAudit",
            auditParams,
            transaction);

        transaction.Commit();
        return true;
    }
    catch
    {
        transaction?.Rollback();
        throw;
    }
    finally
    {
        transaction?.Dispose();
    }
}
```

### Example 5: Multi-Database Support
```csharp
public async Task<List<User>> GetUsersFromClientDatabase(string clientCode)
{
    var parameters = new DynamicParameters();
    parameters.Add("@Status", "Active");

    // This will use the connection string named "Client001" from appsettings.json
    return await _baseRepository.GetListGen<User>(
        "Client001", // clientCode/propCode
        "sp_GetAllUsers",
        parameters);
}
```

### Example 6: Multiple Result Sets
```csharp
public async Task<(List<User> users, List<Role> roles)> GetUsersAndRoles()
{
    var parameters = new DynamicParameters();

    var results = await _baseRepository.GetMultipleData(
        "sp_GetUsersAndRoles",
        parameters);

    // First result set - Users
    var users = ((IEnumerable<dynamic>)results[0])
        .Select(x => new User 
        { 
            Id = x.Id, 
            Email = x.Email,
            UserName = x.UserName 
        }).ToList();

    // Second result set - Roles
    var roles = ((IEnumerable<dynamic>)results[1])
        .Select(x => new Role 
        { 
            Id = x.Id, 
            Name = x.Name 
        }).ToList();

    return (users, roles);
}
```

### Example 7: Multi-Database with Transaction
```csharp
public async Task<bool> CreateUserInClientDatabase(string clientCode, string email, string userName)
{
    IDbTransaction transaction = null;
    try
    {
        transaction = _baseRepository.BeginTransactionGen(clientCode);

        var parameters = new DynamicParameters();
        parameters.Add("@Email", email);
        parameters.Add("@UserName", userName);

        var result = await _baseRepository.PostandGetSingleDataGen<int>(
            clientCode,
            "sp_CreateUser",
            parameters,
            transaction);

        transaction.Commit();
        return true;
    }
    catch
    {
        transaction?.Rollback();
        throw;
    }
    finally
    {
        transaction?.Dispose();
    }
}
```

## Sample Stored Procedures

### sp_GetAllUsers
```sql
CREATE PROCEDURE sp_GetAllUsers
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SELECT Id, Email, UserName, CreatedAt, UpdatedAt
    FROM Users
    WHERE (@Status IS NULL OR Status = @Status)
END
```

### sp_GetUserById
```sql
CREATE PROCEDURE sp_GetUserById
    @UserId INT
AS
BEGIN
    SELECT Id, Email, UserName, CreatedAt, UpdatedAt
    FROM Users
    WHERE Id = @UserId
END
```

### sp_CreateUser
```sql
CREATE PROCEDURE sp_CreateUser
    @Email NVARCHAR(255),
    @UserName NVARCHAR(100),
    @UserId INT OUTPUT
AS
BEGIN
    INSERT INTO Users (Email, UserName, CreatedAt, UpdatedAt)
    VALUES (@Email, @UserName, GETUTCDATE(), GETUTCDATE())
    
    SET @UserId = SCOPE_IDENTITY()
    
    SELECT @UserId AS Id
END
```

### sp_GetUsersAndRoles (Multiple Result Sets)
```sql
CREATE PROCEDURE sp_GetUsersAndRoles
AS
BEGIN
    -- First result set - Users
    SELECT Id, Email, UserName, CreatedAt, UpdatedAt
    FROM Users
    
    -- Second result set - Roles
    SELECT Id, Name, Description
    FROM Roles
END
```

## Key Methods Reference

| Method | Description | Use Case |
|--------|-------------|----------|
| `GetList<T>()` | Returns list from SP | Get all records |
| `GetSingleData<T>()` | Returns single item from SP | Get by ID, Get single aggregate |
| `PostandGetSingleData<T>()` | Execute SP and return result | INSERT/UPDATE with return value |
| `GetMultipleData()` | Returns multiple result sets | Dashboard data, related entities |
| `GetListGen<T>()` | Multi-DB: Returns list | Multi-tenant list queries |
| `GetSingleDataGen<T>()` | Multi-DB: Returns single item | Multi-tenant single queries |
| `PostandGetSingleDataGen<T>()` | Multi-DB: Execute and return | Multi-tenant INSERT/UPDATE |
| `GetMultipleDataGen()` | Multi-DB: Multiple result sets | Multi-tenant complex queries |
| `BeginTransaction()` | Start transaction on default DB | Atomic operations |
| `BeginTransactionGen()` | Start transaction on specific DB | Multi-tenant atomic operations |

## Best Practices

1. **Always use parameters** - Prevents SQL injection
2. **Use transactions for multi-step operations** - Ensures data consistency
3. **Dispose transactions properly** - Use try-catch-finally or using statements
4. **Use output parameters** - For getting generated IDs or return values
5. **Handle exceptions** - Always rollback transactions on error
6. **Use meaningful stored procedure names** - Follow naming conventions (sp_VerbNoun)
7. **Document your SPs** - Add comments explaining parameters and return values

## Multi-Tenant Architecture

The "Gen" methods support multi-tenant architectures where each client has their own database:

```
┌─────────────────┐
│  Application    │
└────────┬────────┘
         │
         ├──────────────┬──────────────┬──────────────┐
         │              │              │              │
    ┌────▼────┐    ┌────▼────┐    ┌────▼────┐   ┌────▼────┐
    │ Default │    │Client001│    │Client002│   │  PropA  │
    │   DB    │    │   DB    │    │   DB    │   │   DB    │
    └─────────┘    └─────────┘    └─────────┘   └─────────┘
```

Each database can be on different servers, and you route to them using the `propCode` parameter.

## Performance Considerations

1. **Connection Pooling** - Automatically handled by ADO.NET
2. **Async/Await** - All methods are async for better scalability
3. **Dispose Connections** - Using statements ensure proper disposal
4. **Stored Procedures** - Pre-compiled, optimized execution plans

## Troubleshooting

### Connection String Not Found
```
Error: Connection string for client code 'XXX' not found.
```
**Solution**: Add the connection string to `appsettings.json` under `ConnectionStrings` section

### Transaction Already Committed
```
Error: This SqlTransaction has completed; it is no longer usable.
```
**Solution**: Don't reuse transaction objects. Create new transactions for each operation.

### Multiple Active Result Sets
If you get MARS errors, add `MultipleActiveResultSets=true` to your connection string.

## License
This is part of the CleanDapperApp project.
