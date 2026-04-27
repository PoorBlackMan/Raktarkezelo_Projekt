using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raktarkezelo.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove any previously seeded admin/manager rows that had broken BCrypt hashes.
            // The DbSeeder running at application startup will re-insert them correctly.
            migrationBuilder.Sql(
                "DELETE FROM Userinfo WHERE Username IN ('admin', 'manager')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down intentionally left empty — the seeder handles re-insertion on startup.
        }
    }
}
