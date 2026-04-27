using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Raktarkezelo.Migrations
{
    /// <inheritdoc />
    public partial class adatbazis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    MinStock = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "Userinfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Passwordhash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Userinfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransactions", x => x.TransactionId);
                    table.CheckConstraint("CK_StockTransactions_Type", "[Type] IN ('IN','OUT','ADJUST')");
                    table.ForeignKey(
                        name: "FK_StockTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Userinfo_UserId",
                        column: x => x.UserId,
                        principalTable: "Userinfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Category", "MinStock", "Name", "Note", "Stock" },
                values: new object[,]
                {
                    { 1, "Üdítő", 10, "Coca-Cola 0.5L", null, 50 },
                    { 2, "Üdítő", 10, "Fanta 0.5L", null, 40 },
                    { 3, "Üdítő", 15, "Ásványvíz 1.5L", null, 60 },
                    { 4, "Írószer", 20, "Golyóstoll kék", null, 100 },
                    { 5, "Írószer", 20, "Ceruza HB", null, 80 },
                    { 6, "Írószer", 10, "A4 füzet", null, 35 },
                    { 7, "Háztartás", 8, "Toalettpapír", null, 25 },
                    { 8, "Háztartás", 6, "Papírtörlő", null, 20 },
                    { 9, "Háztartás", 5, "Mosogatószer", null, 18 },
                    { 10, "Egészségügy", 10, "Sebtapasz", null, 30 },
                    { 11, "Egészségügy", 8, "Kézfertőtlenítő", null, 22 },
                    { 12, "Egészségügy", 25, "Maszk", null, 100 },
                    { 13, "Elektronika", 12, "Elem AA", null, 45 },
                    { 14, "Elektronika", 8, "USB kábel", null, 28 },
                    { 15, "Elektronika", 4, "Hosszabbító", null, 12 }
                });



            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_ProductId",
                table: "StockTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_UserId",
                table: "StockTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Userinfo_Email",
                table: "Userinfo",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Userinfo_Username",
                table: "Userinfo",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockTransactions");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Userinfo");
        }
    }
}
