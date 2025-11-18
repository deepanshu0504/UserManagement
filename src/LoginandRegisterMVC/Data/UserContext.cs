using Microsoft.EntityFrameworkCore;
using LoginandRegisterMVC.Models;

namespace LoginandRegisterMVC.Data;

public class UserContext : DbContext
{
    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Blog> Blogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(128);
            entity.Property(e => e.Username).IsRequired();
            entity.Property(e => e.Password).IsRequired();
            entity.Property(e => e.Role).IsRequired();
        });

        // Blog configuration
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ShortDescription).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.FeaturedImage).IsRequired();
            entity.Property(e => e.MetaDescription).HasMaxLength(160);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.AuthorId).IsRequired();
            
            // Foreign key relationship
            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes for performance
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PublishedDate);
            entity.HasIndex(e => e.AuthorId);
        });
    }
}
