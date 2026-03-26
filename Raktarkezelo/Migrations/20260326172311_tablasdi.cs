using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Raktarkezelo.Migrations
{
    /// <inheritdoc />
    public partial class tablasdi : Migration
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    MinStock = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    { 1, "Üdítő", 10, "Coca-Cola 0.5L", "", 50 },
                    { 2, "Üdítő", 10, "Fanta 0.5L", "", 40 },
                    { 3, "Üdítő", 15, "Ásványvíz 1.5L", "", 60 },
                    { 4, "Írószer", 20, "Golyóstoll kék", "", 100 },
                    { 5, "Írószer", 20, "Ceruza HB", "", 80 },
                    { 6, "Írószer", 10, "A4 füzet", "", 35 },
                    { 7, "Háztartás", 8, "Toalettpapír", "", 25 },
                    { 8, "Háztartás", 6, "Papírtörlő", "", 20 },
                    { 9, "Háztartás", 5, "Mosogatószer", "", 18 },
                    { 10, "Egészségügy", 10, "Sebtapasz", "", 30 },
                    { 11, "Egészségügy", 8, "Kézfertőtlenítő", "", 22 },
                    { 12, "Egészségügy", 25, "Maszk", "", 100 },
                    { 13, "Elektronika", 12, "Elem AA", "", 45 },
                    { 14, "Elektronika", 8, "USB kábel", "", 28 },
                    { 15, "Elektronika", 4, "Hosszabbító", "", 12 }
                });

            migrationBuilder.InsertData(
                table: "Userinfo",
                columns: new[] { "Id", "Email", "IsActive", "Passwordhash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "manager@raktar.hu", true, "hmSFeWz6jXwM9xEWQCBbgwdkM1R1d1EdgfgDCumezqU=", 2, "manager" },
                    { 2, "admin@raktar.hu", true, "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", 1, "admin" }
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
