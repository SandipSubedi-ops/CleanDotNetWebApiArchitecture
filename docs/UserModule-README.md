# User Module - BaseRepository Implementation

This module demonstrates a complete implementation of user management using the `BaseRepository` with stored procedures.

## 📁 Structure

```
CleanDapperApp.Models/
└── UserModel/
    ├── UserModel.cs        # Database entity
    └── UserDto.cs          # DTOs (Create, Update, Response, Details)

CleanDapperApp.Api/
└── Controllers/
    └── UserController.cs   # API endpoints

docs/SQL/
└── UserStoredProcedures.sql  # Database scripts
```

## 🎯 Features Implemented

### 1. **Complete CRUD Operations**
- ✅ Create User
- ✅ Get All Users
- ✅ Get User By ID
- ✅ Update User
- ✅ Delete User (Soft Delete)

### 2. **Advanced Features**
- ✅ Search Users by Email
- ✅ Get Active Users Only
- ✅ User Statistics (Multiple Result Sets)
- ✅ Transaction Example (Create User with Order)

### 3. **Best Practices**
- ✅ DTOs for clean API contracts
- ✅ Output parameters for getting generated IDs
- ✅ Proper error handling
- ✅ Authorization required
- ✅ Swagger documentation via attributes

## 📝 API Endpoints

### User Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/user` | Get all active users |
| GET | `/api/user/{id}` | Get user details by ID |
| GET | `/api/user/search?email={email}` | Search users by email |
| GET | `/api/user/active` | Get all active users |
| GET | `/api/user/{id}/statistics` | Get user statistics |
| POST | `/api/user` | Create a new user |
| POST | `/api/user/with-order` | Create user with welcome order (transaction) |
| PUT | `/api/user/{id}` | Update user |
| DELETE | `/api/user/{id}` | Soft delete user |

## 🚀 Quick Start

### 1. Database Setup

Execute the SQL script to create tables and stored procedures:

```sql
-- Run this file:
docs/SQL/UserStoredProcedures.sql
```

This will create:
- `Users` table
- `Orders` table (for demonstration)
- `UserActivities` table
- `UserLogins` table
- All necessary stored procedures
- Sample data for testing

### 2. Test with Swagger

1. Run the application:
   ```bash
   dotnet run --project src/CleanDapperApp.Api
   ```

2. Navigate to Swagger UI: `https://localhost:5001/swagger`

3. Authenticate first (use the `/api/auth/login` endpoint)

4. Test the User endpoints

### 3. Example Requests

#### Create User
```http
POST /api/user
Content-Type: application/json
Authorization: Bearer {your_token}

{
  "username": "John Doe",
  "email": "john@example.com",
  "phoneNumber": "+1234567890",
  "address": "123 Main Street"
}
```

**Response:**
```json
{
  "success": true,
  "message": "User created successfully",
  "data": {
    "id": 4,
    "username": "John Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "address": "123 Main Street",
    "createdAt": "2025-12-12T07:00:00Z",
    "updatedAt": null
  }
}
```

#### Get User Details
```http
GET /api/user/1
Authorization: Bearer {your_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "username": "John Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "+1234567890",
    "address": "123 Main St, City",
    "totalOrders": 2,
    "totalSpent": 225.50,
    "createdAt": "2025-12-12T06:00:00Z",
    "updatedAt": null,
    "lastLoginDate": "2025-12-12T07:00:00Z"
  }
}
```

#### Update User
```http
PUT /api/user/1
Content-Type: application/json
Authorization: Bearer {your_token}

{
  "id": 1,
  "username": "John Updated",
  "email": "john.updated@example.com",
  "phoneNumber": "+9876543210",
  "address": "456 New Street"
}
```

#### Search Users
```http
GET /api/user/search?email=john
Authorization: Bearer {your_token}
```

#### Get User Statistics
```http
GET /api/user/1/statistics
Authorization: Bearer {your_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "userInfo": {
      "id": 1,
      "username": "John Doe",
      "email": "john.doe@example.com",
      ...
    },
    "ordersSummary": {
      "totalOrders": 2,
      "totalSpent": 225.50,
      "averageOrderValue": 112.75,
      "lastOrderDate": "2025-12-12T06:30:00Z"
    },
    "recentActivities": [
      {
        "activityType": "Order",
        "activityDescription": "Placed order #1001",
        "activityDate": "2025-12-12T06:30:00Z"
      }
    ]
  }
}
```

## 🔧 BaseRepository Methods Used

### Simple Queries
```csharp
// Get list of users
var users = await _baseRepository.GetList<UserResponseDto>(
    "sp_GetAllUsers",
    parameters);

// Get single user
var user = await _baseRepository.GetSingleData<UserDetailsDto>(
    "sp_GetUserById",
    parameters);
```

### With Output Parameters
```csharp
// Create and get generated ID
var parameters = new DynamicParameters();
parameters.Add("@Username", dto.Username);
parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

await _baseRepository.PostandGetSingleData<UserResponseDto>(
    "sp_CreateUser",
    parameters);

var newUserId = parameters.Get<int>("@UserId");
```

### Multiple Result Sets
```csharp
// Get multiple related data in one call
var results = await _baseRepository.GetMultipleData(
    "sp_GetUserStatistics",
    parameters);

var userInfo = results[0];
var ordersSummary = results[1];
var activities = results[2];
```

### Transactions
```csharp
IDbTransaction transaction = null;
try
{
    transaction = _baseRepository.BeginTransaction();
    
    // Multiple operations
    await _baseRepository.PostandGetSingleData<UserResponseDto>(
        "sp_CreateUser", userParams, transaction);
        
    await _baseRepository.PostandGetSingleData<int>(
        "sp_CreateWelcomeOrder", orderParams, transaction);
    
    transaction.Commit();
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
```

## 📊 DTOs Explained

### CreateUserDto
Used for creating new users. Contains only the fields needed for creation:
- Username
- Email
- PhoneNumber
- Address

### UpdateUserDto
Used for updating users. Includes the ID plus updatable fields:
- Id
- Username
- Email
- PhoneNumber
- Address

### UserResponseDto
Standard response for user data:
- Id
- Username
- Email
- PhoneNumber
- Address
- CreatedAt
- UpdatedAt

### UserDetailsDto
Extended response with additional information:
- All fields from UserResponseDto
- TotalOrders
- TotalSpent
- LastLoginDate

### UserModel
Database entity representing the Users table:
- All database fields including IsActive

## 🎨 Stored Procedures

All stored procedures follow a consistent pattern:

1. **Naming Convention**: `sp_{Action}{Entity}`
   - `sp_GetAllUsers`
   - `sp_CreateUser`
   - `sp_UpdateUser`

2. **Error Handling**: All mutations use `TRY-CATCH` blocks

3. **Soft Deletes**: Delete operations set `IsActive = 0` instead of removing records

4. **Output Parameters**: Used for getting generated IDs and affected row counts

5. **Return Values**: Most procedures return the affected entity

## 🔐 Security

- All endpoints require authentication (`[Authorize]` attribute)
- Email uniqueness is enforced at the database level
- Soft deletes preserve data integrity
- Transaction example shows atomic operations

## 📈 Performance Tips

1. **Indexing**: Add indexes on:
   - `Users.Email` (already unique)
   - `Users.IsActive`
   - `Orders.UserId`
   - `UserActivities.UserId`

2. **Pagination**: For production, add pagination parameters:
   ```csharp
   parameters.Add("@PageNumber", pageNumber);
   parameters.Add("@PageSize", pageSize);
   ```

3. **Caching**: Consider caching frequently accessed user data

## 🧪 Testing

### Unit Testing
```csharp
[Fact]
public async Task CreateUser_ShouldReturnNewUser()
{
    // Arrange
    var mockRepo = new Mock<IBaseRepository>();
    var controller = new UserController(mockRepo.Object);
    var dto = new CreateUserDto { ... };
    
    // Act
    var result = await controller.CreateUser(dto);
    
    // Assert
    Assert.IsType<CreatedAtActionResult>(result);
}
```

### Integration Testing
Run the SQL script to set up test data, then use Postman or Swagger to test endpoints.

## 🚨 Common Issues

### Issue: "User not found" after creation
**Solution**: Check that `IsActive` is set to `1` in the stored procedure

### Issue: Email already exists
**Solution**: The duplicate check is in the stored procedure. Ensure unique emails or handle the error gracefully

### Issue: Transaction timeout
**Solution**: Keep transactions short. Don't hold locks for extended periods

## 📚 Next Steps

1. **Add Validation**: Implement FluentValidation for DTOs
2. **Add Logging**: Log all user operations
3. **Add Caching**: Cache user lookups
4. **Add Pagination**: Implement paging for list endpoints
5. **Add Filtering**: Add more search/filter options
6. **Add Sorting**: Allow sorting by different fields

## 🔗 Related Documentation

- [BaseRepository Documentation](../BaseRepository-Documentation.md)
- [JWT Authentication Guide](../Authentication.md)
- [API Documentation](../API-Documentation.md)

