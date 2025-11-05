using Biblioteca.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data
{
    public class LibraryContext : IdentityDbContext<ApplicationUser>
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<Reader> Readers => Set<Reader>();
        public DbSet<Reservation> Reservations => Set<Reservation>();

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

            builder.Entity<Reservation>(entity =>
            {
                entity.Property(r => r.Status)
                    .HasConversion<string>()
                    .HasMaxLength(32);

                entity.Property(r => r.ReservedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(r => r.Book)
                    .WithMany(b => b.Reservations)
                    .HasForeignKey(r => r.BookId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Reader)
                    .WithMany(re => re.Reservations)
                    .HasForeignKey(r => r.ReaderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}