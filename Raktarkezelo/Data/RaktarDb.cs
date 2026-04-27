using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Models.Entities;
using Raktarkezelo.Models.Enums;

namespace Raktarkezelo.Data
{
    public class RaktarDb : DbContext
    {
        public RaktarDb(DbContextOptions<RaktarDb> options) : base(options) { }

        public DbSet<Userinfo> Userinfo => Set<Userinfo>();
        public DbSet<Products> Products => Set<Products>();
        public DbSet<StockTransactions> StockTransactions => Set<StockTransactions>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Userinfo>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Userinfo>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<StockTransactions>()
                .HasOne(st => st.User)
                .WithMany(u => u.StockTransactions)
                .HasForeignKey(st => st.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransactions>()
                .HasOne(st => st.Product)
                .WithMany(p => p.StockTransactions)
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransactions>()
                .ToTable(t => t.HasCheckConstraint("CK_StockTransactions_Type", "[Type] IN ('IN','OUT','ADJUST')"));

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Products>().HasData(
                new Products { ProductId = 1, Name = "Coca-Cola 0.5L", Category = "Üdítő", Stock = 50, MinStock = 10 },
                new Products { ProductId = 2, Name = "Fanta 0.5L", Category = "Üdítő", Stock = 40, MinStock = 10 },
                new Products { ProductId = 3, Name = "Ásványvíz 1.5L", Category = "Üdítő", Stock = 60, MinStock = 15 },

                new Products { ProductId = 4, Name = "Golyóstoll kék", Category = "Írószer", Stock = 100, MinStock = 20 },
                new Products { ProductId = 5, Name = "Ceruza HB", Category = "Írószer", Stock = 80, MinStock = 20 },
                new Products { ProductId = 6, Name = "A4 füzet", Category = "Írószer", Stock = 35, MinStock = 10 },

                new Products { ProductId = 7, Name = "Toalettpapír", Category = "Háztartás", Stock = 25, MinStock = 8 },
                new Products { ProductId = 8, Name = "Papírtörlő", Category = "Háztartás", Stock = 20, MinStock = 6 },
                new Products { ProductId = 9, Name = "Mosogatószer", Category = "Háztartás", Stock = 18, MinStock = 5 },

                new Products { ProductId = 10, Name = "Sebtapasz", Category = "Egészségügy", Stock = 30, MinStock = 10 },
                new Products { ProductId = 11, Name = "Kézfertőtlenítő", Category = "Egészségügy", Stock = 22, MinStock = 8 },
                new Products { ProductId = 12, Name = "Maszk", Category = "Egészségügy", Stock = 100, MinStock = 25 },

                new Products { ProductId = 13, Name = "Elem AA", Category = "Elektronika", Stock = 45, MinStock = 12 },
                new Products { ProductId = 14, Name = "USB kábel", Category = "Elektronika", Stock = 28, MinStock = 8 },
                new Products { ProductId = 15, Name = "Hosszabbító", Category = "Elektronika", Stock = 12, MinStock = 4 }
            );
        }

    }
}