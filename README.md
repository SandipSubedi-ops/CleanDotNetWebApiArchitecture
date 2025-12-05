# .NET Clean Architecture with Dapper

This is an enterprise-level backend application template built with .NET 9, using Dapper as the ORM and following Clean Architecture principles (Onion Architecture).

## 🚀 Features

- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and Presentation layers.
- **Dapper ORM**: High-performance micro-ORM for data access.
- **Repository Pattern & Unit of Work**: Abstraction over data access logic and transaction management.
- **Authentication & Authorization**: JWT (JSON Web Token) based authentication with Role-Based Access Control (RBAC).
- **Background Jobs**: Integrated **Hangfire** for background job processing (e.g., scheduled emails).
- **Validation**: Input validation using **FluentValidation**.
- **Logging**: Structured logging with **Serilog**.
- **Global Exception Handling**: Centralized error handling via middleware.
- **Swagger UI**: Interactive API documentation.

## 🏗 Architecture

The solution is organized into the following projects:

- **`CleanDapperApp.Api`**: The Presentation layer (Web API). Contains Controllers, Middleware, and DI configuration.
- **`CleanDapperApp.Models`**: The Core Domain layer. Contains Entities, Interfaces (`IGenericRepository`, `IUnitOfWork`, `IEmailService`), DTOs, and Exceptions. Dependencies are minimal.
- **`CleanDapperApp.Repository`**: The Infrastructure layer. Contains the implementation of Repositories, Dapper Context, Services (`EmailService`, `JwtProvider`), and 3rd party library integrations.

## 🛠 Tech Stack

- **Framework**: .NET 9
- **Database**: SQL Server
- **ORM**: Dapper
- **Background Jobs**: Hangfire (Memory Storage for demo)
- **Logging**: Serilog
- **Documentation**: Swashbuckle (Swagger)

## 🏃‍♂️ Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (LocalDB, Express, or Docker)

### Installation

1.  Clone the repository.
2.  Navigate to the solution folder.
3.  Update the connection string in `src/CleanDapperApp.Api/appsettings.json`:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CleanDapperDb;Trusted_Connection=True;..."
    }
    ```
4.  Restore dependencies:
    ```bash
    dotnet restore
    ```

### Running the Application

Run the API project:

```bash
dotnet run --project src/CleanDapperApp.Api/CleanDapperApp.Api.csproj
```

The API will be available at `https://localhost:7153` (or similar port).

- **Swagger UI**: `https://localhost:7153/swagger`
- **Hangfire Dashboard**: `https://localhost:7153/hangfire`

### Database Setup

Since Dapper is used (database-first or code-first with scripts), ensure you create the necessary tables. For this template, the `Users` table is required:

```sql
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100),
    Email NVARCHAR(100),
    PasswordHash NVARCHAR(255),
    Role NVARCHAR(50),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);
```

## 🧪 Testing

Use the Swagger UI to interact with the API:

1.  **Register**: Create a new user account.
2.  **Login**: Get a JWT token.
3.  **Authorize**: Click the "Authorize" button in Swagger and paste the token (`Bearer <token>`).
4.  **Jobs**: Use the `Jobs` endpoints to test Hangfire email queuing.
