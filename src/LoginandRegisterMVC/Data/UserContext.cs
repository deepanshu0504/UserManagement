using Microsoft.EntityFrameworkCore;
using LoginandRegisterMVC.Models;

namespace LoginandRegisterMVC.Data;

public class UserContext : DbContext
{
    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

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
    }
}
