using Microsoft.EntityFrameworkCore;
using Raktarkezelo.Models.Stock;
using Raktarkezelo.Models.User;


namespace Raktarkezelo.Data
{
    public class RaktarDb : DbContext
    {
        public RaktarDb(DbContextOptions<RaktarDb> options) : base(options)
        {
        }

        public DbSet<Userinfo> User { get; set; }
        public DbSet<Products> Products { get; set; }
    }
}