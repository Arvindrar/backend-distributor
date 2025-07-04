using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendDistributor.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedSalesEmployeeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "SalesEmployees",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SalesEmployees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "SalesEmployees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "SalesEmployees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "SalesEmployees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SalesEmployees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "SalesEmployees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "SalesEmployees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "SalesEmployees",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "SalesEmployees");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SalesEmployees");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "SalesEmployees");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "SalesEmployees");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "SalesEmployees");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SalesEmployees");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "SalesEmployees");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "SalesEmployees");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "SalesEmployees");
        }
    }
}
