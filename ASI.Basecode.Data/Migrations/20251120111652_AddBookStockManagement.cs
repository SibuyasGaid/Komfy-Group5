using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookStockManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Quantity column with default value of 1
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Add AvailableQuantity column with default value of 1
            migrationBuilder.AddColumn<int>(
                name: "AvailableQuantity",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Update existing books: Set AvailableQuantity based on current Status
            // If Status is 'Available', set AvailableQuantity = 1
            // If Status is 'Borrowed' or other, set AvailableQuantity = 0
            migrationBuilder.Sql(@"
                UPDATE Books
                SET AvailableQuantity = CASE
                    WHEN Status = 'Available' THEN 1
                    ELSE 0
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableQuantity",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Books");
        }
    }
}
