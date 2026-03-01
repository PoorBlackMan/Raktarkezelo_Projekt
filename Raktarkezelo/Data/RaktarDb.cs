using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Models.Entities;

namespace Raktarkezelo.Data
{
    public class RaktarDb : DbContext
    {
        public RaktarDb(DbContextOptions<RaktarDb> options) : base(options) { }

        public DbSet<Userinfo> Userinfos => Set<Userinfo>();
        public DbSet<Products> Products => Set<Products>();
        public DbSet<StockTransactions> StockTransactions => Set<StockTransactions>();

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
        }
    }
}