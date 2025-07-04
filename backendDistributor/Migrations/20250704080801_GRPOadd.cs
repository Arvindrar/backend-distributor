using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backendDistributor.Migrations
{
    /// <inheritdoc />
    public partial class GRPOadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GRPONumberTrackers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastUsedNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRPONumberTrackers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GRPOs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GRPONo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchaseOrderNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VendorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VendorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GRPODate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VendorRefNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShipToAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GRPORemarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRPOs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GRPOAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GRPOId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRPOAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GRPOAttachments_GRPOs_GRPOId",
                        column: x => x.GRPOId,
                        principalTable: "GRPOs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GRPOItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GRPOId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UOM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WarehouseLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRPOItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GRPOItems_GRPOs_GRPOId",
                        column: x => x.GRPOId,
                        principalTable: "GRPOs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GRPONumberTrackers",
                columns: new[] { "Id", "LastUsedNumber" },
                values: new object[] { 2, 1000000 });

            migrationBuilder.CreateIndex(
                name: "IX_GRPOAttachments_GRPOId",
                table: "GRPOAttachments",
                column: "GRPOId");

            migrationBuilder.CreateIndex(
                name: "IX_GRPOItems_GRPOId",
                table: "GRPOItems",
                column: "GRPOId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GRPOAttachments");

            migrationBuilder.DropTable(
                name: "GRPOItems");

            migrationBuilder.DropTable(
                name: "GRPONumberTrackers");

            migrationBuilder.DropTable(
                name: "GRPOs");
        }
    }
}
