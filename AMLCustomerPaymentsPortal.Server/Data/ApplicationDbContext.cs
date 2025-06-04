using AMLCustomerPaymentsPortal.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AMLCustomerPaymentsPortal.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.IdNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.AccountNumber) 
                .IsUnique();
        }
    }
}