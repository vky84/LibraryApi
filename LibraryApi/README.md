# Library API

A simple Library Management API built with ASP.NET Core Web API that allows you to manage books and borrowing records.

## Features

### Books Service
- Add new books
- List all books
- Get details of a book by ID
- Update book records
- Delete books
- Get available books only

### Borrowing Service
- Borrow a book by ID
- Return a book
- Get all books borrowed by a specific user
- Get overdue books

## API Endpoints

### Books Controller (`/api/books`)

- `GET /api/books` - Get all books
- `GET /api/books/available` - Get only available books
- `GET /api/books/{id}` - Get book by ID
- `POST /api/books` - Add a new book
- `PUT /api/books/{id}` - Update an existing book
- `DELETE /api/books/{id}` - Delete a book

### Borrowing Controller (`/api/borrowing`)

- `POST /api/borrowing/borrow` - Borrow a book
- `POST /api/borrowing/return/{borrowingId}` - Return a borrowed book
- `GET /api/borrowing/user/{userId}` - Get all books borrowed by a user
- `GET /api/borrowing/overdue` - Get all overdue books

## Running the Application

1. Run the application: `dotnet run`
2. Open your browser and navigate to `https://localhost:7xxx/swagger` (port may vary)
3. Use the Swagger UI to test all the endpoints

## Sample Data

The application comes with sample books and borrowing records for testing purposes. All data is stored in memory and will be reset when the application restarts.

## Models

### Book
- Id (int)
- Title (string)
- Author (string)
- ISBN (string)
- PublishedDate (DateTime)
- Genre (string)
- IsAvailable (bool)
- Description (string)

### BorrowingRecord
- Id (int)
- BookId (int)
- UserId (string)
- UserName (string)
- BorrowedDate (DateTime)
- DueDate (DateTime)
- ReturnedDate (DateTime?)
- IsReturned (bool, computed)
- IsOverdue (bool, computed)

### BorrowBookRequest
- BookId (int)
- UserId (string)
- UserName (string)