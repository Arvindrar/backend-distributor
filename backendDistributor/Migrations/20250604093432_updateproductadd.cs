using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendDistributor.Migrations
{
    /// <inheritdoc />
    public partial class updateproductadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceMin",
                table: "Products",
                newName: "WholesalePrice");

            migrationBuilder.RenameColumn(
                name: "PriceMax",
                table: "Products",
                newName: "RetailPrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WholesalePrice",
                table: "Products",
                newName: "PriceMin");

            migrationBuilder.RenameColumn(
                name: "RetailPrice",
                table: "Products",
                newName: "PriceMax");
        }
    }
}
