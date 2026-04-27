using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Models.Entities;
using Raktarkezelo.Models.Enums;

namespace Raktarkezelo.Data
{
    /// <summary>
    /// Seeds default admin and manager accounts at application startup.
    /// Only inserts rows that do not already exist — safe to call on every run.
    /// BCrypt hashes are computed at runtime so the code is identical for every
    /// developer sharing this repository; each person's local database is seeded
    /// correctly the first time they run the application.
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(RaktarDb context)
        {
            // Ensure the database and all migrations are applied first
            await context.Database.MigrateAsync();

            // ── Default Manager ───────────────────────────────────────────────
            if (!await context.Userinfo.AnyAsync(u => u.Username == "manager"))
            {
                context.Userinfo.Add(new Userinfo
                {
                    Email = "manager@raktar.hu",
                    Username = "manager",
                    Passwordhash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    Role = UserRole.Manager,
                    IsActive = true
                });
            }

            // ── Default Admin ─────────────────────────────────────────────────
            if (!await context.Userinfo.AnyAsync(u => u.Username == "admin"))
            {
                context.Userinfo.Add(new Userinfo
                {
                    Email = "admin@raktar.hu",
                    Username = "admin",
                    Passwordhash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = UserRole.Admin,
                    IsActive = true
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
