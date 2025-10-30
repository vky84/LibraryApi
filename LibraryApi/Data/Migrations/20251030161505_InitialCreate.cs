using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                table: "BorrowingRecords",
                columns: new[] { "Id", "BookId", "BorrowedDate", "DueDate", "ReturnedDate", "UserId", "UserName" },
                values: new object[,]
                {
                    { 1, 3, new DateTime(2025, 10, 20, 17, 15, 4, 275, DateTimeKind.Utc).AddTicks(8744), new DateTime(2025, 10, 27, 17, 15, 4, 275, DateTimeKind.Utc).AddTicks(8905), null, "user1", "John Doe" },
                    { 2, 1, new DateTime(2025, 10, 25, 17, 15, 4, 275, DateTimeKind.Utc).AddTicks(8916), new DateTime(2025, 11, 8, 17, 15, 4, 275, DateTimeKind.Utc).AddTicks(8920), new DateTime(2025, 10, 29, 17, 15, 4, 275, DateTimeKind.Utc).AddTicks(8924), "user2", "Jane Smith" }
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowingRecords");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
