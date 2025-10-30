# Database Setup Instructions

## Prerequisites
- PostgreSQL installed and running on your system
- .NET 8 SDK installed

## Setup Steps

1. **Create Database User (if needed):**
   ```sql
   -- Connect to PostgreSQL as superuser and run:
   CREATE USER libraryapi WITH PASSWORD 'your_password_here';
   CREATE DATABASE "LibraryDb" OWNER libraryapi;
   CREATE DATABASE "LibraryDb_Dev" OWNER libraryapi;
   GRANT ALL PRIVILEGES ON DATABASE "LibraryDb" TO libraryapi;
   GRANT ALL PRIVILEGES ON DATABASE "LibraryDb_Dev" TO libraryapi;
   ```

2. **Update Connection String:**
   - Update the password in `appsettings.json` and `appsettings.Development.json`
   - Replace `your_password_here` with your actual PostgreSQL password

3. **Install Entity Framework Tools (if not already installed):**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. **Create Initial Migration:**
   ```bash
   dotnet ef migrations add InitialCreate
   ```

5. **Apply Migration to Database:**
   ```bash
   dotnet ef database update
   ```

6. **Run the Application:**
   ```bash
   dotnet run
   ```

## Alternative: Auto-migration on startup
The application is configured to automatically create the database and apply the schema when it starts. This is suitable for development but not recommended for production.

## Database Schema
The application will create the following tables:
- `Books` - Stores book information
- `BorrowingRecords` - Stores borrowing history

## Connection String Format
```
Host=localhost;Database=LibraryDb;Username=postgres;Password=your_password_here
```

You can also specify port if PostgreSQL is not running on default port:
```
Host=localhost;Port=5432;Database=LibraryDb;Username=postgres;Password=your_password_here
```