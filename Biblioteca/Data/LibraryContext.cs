using Biblioteca.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace Biblioteca.Data
{
    public class LibraryContext : IdentityDbContext<ApplicationUser>
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<Reader> Readers => Set<Reader>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Reader>(entity =>
            {
                entity.HasIndex(r => r.ReaderCode).IsUnique();
                entity.HasIndex(r => r.ApplicationUserId).IsUnique();

                entity.HasOne(r => r.ApplicationUser)
                    .WithOne(u => u.ReaderProfile)
                    .HasForeignKey<Reader>(r => r.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}