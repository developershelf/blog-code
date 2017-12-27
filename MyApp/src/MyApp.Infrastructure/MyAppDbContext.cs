using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Infrastructure
{
    public class MyAppDbContext : DbContext
    {
        public DbSet<MyModel> MyModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyModel>().HasKey(x => x.Id);
            modelBuilder.Entity<MyModel>().Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
}
