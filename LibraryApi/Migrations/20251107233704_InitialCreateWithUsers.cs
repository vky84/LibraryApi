using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ISBN = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Genre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    BookId = table.Column<int>(type: "integer", nullable: true),
                    BorrowingRecordId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSent = table.Column<bool>(type: "boolean", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MembershipType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BorrowingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BorrowedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowingRecords_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "Description", "Genre", "ISBN", "IsAvailable", "PublishedDate", "Title" },
                values: new object[,]
                {
                    { 1, "F. Scott Fitzgerald", "A classic American novel", "Fiction", "978-0-7432-7356-5", true, new DateTime(1925, 4, 10, 0, 0, 0, 0, DateTimeKind.Utc), "The Great Gatsby" },
                    { 2, "Harper Lee", "A gripping tale of racial injustice", "Fiction", "978-0-06-112008-4", true, new DateTime(1960, 7, 11, 0, 0, 0, 0, DateTimeKind.Utc), "To Kill a Mockingbird" },
                    { 3, "George Orwell", "A dystopian social science fiction novel", "Dystopian Fiction", "978-0-452-28423-4", false, new DateTime(1949, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc), "1984" },
                    { 4, "Jane Austen", "A romantic novel of manners", "Romance", "978-0-14-143951-8", true, new DateTime(1813, 1, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Pride and Prejudice" },
                    { 5, "J.D. Salinger", "A controversial coming-of-age story", "Fiction", "978-0-316-76948-0", true, new DateTime(1951, 7, 16, 0, 0, 0, 0, DateTimeKind.Utc), "The Catcher in the Rye" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "IsActive", "JoinedDate", "MembershipType", "UserId", "UserName" },
                values: new object[,]
                {
                    { 1, "waqas.siddiqui@me.com", "Waqas Siddiqui", true, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Standard", "user1", "vky84" },
                    { 2, "jane.smith@example.com", "Jane Elizabeth Smith", true, new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Premium", "user2", "Jane Smith" },
                    { 3, "bob.johnson@example.com", "Robert James Johnson", true, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Standard", "user3", "Bob Johnson" },
                    { 4, "john.doe@example.com", "John Michael Doe", true, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Standard", "user4", "John Doe" }
                });

            migrationBuilder.InsertData(
                table: "BorrowingRecords",
                columns: new[] { "Id", "BookId", "BorrowedDate", "DueDate", "ReturnedDate", "UserId", "UserName" },
                values: new object[,]
                {
                    { 1, 3, new DateTime(2025, 9, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, "user1", "John Doe" },
                    { 2, 1, new DateTime(2025, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "user2", "Jane Smith" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_ISBN",
                table: "Books",
                column: "ISBN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingRecords_BookId",
                table: "BorrowingRecords",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsSent_ScheduledFor",
                table: "Notifications",
                columns: new[] { "IsSent", "ScheduledFor" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsSent",
                table: "Notifications",
                columns: new[] { "UserId", "IsSent" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserId",
                table: "Users",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowingRecords");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
