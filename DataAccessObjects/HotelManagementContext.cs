using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public sealed class HotelManagementContext : DbContext
{
    public HotelManagementContext()
    {
    }

    public HotelManagementContext(DbContextOptions<HotelManagementContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<User> Users => Set<User>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(DbContextFactory.GetConnectionString());
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(builder =>
        {
            builder.ToTable("roles");
            builder.HasKey(role => role.RoleId);

            builder.Property(role => role.RoleId)
                .HasColumnName("role_id");

            builder.Property(role => role.Name)
                .HasColumnName("role_name")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.HasMany(role => role.Users)
                .WithOne(user => user.Role)
                .HasForeignKey(user => user.RoleId);
        });

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(user => user.UserId);

            builder.Property(user => user.UserId)
                .HasColumnName("user_id");

            builder.Property(user => user.RoleId)
                .HasColumnName("role_id");

            builder.Property(user => user.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(user => user.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(user => user.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(user => user.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(user => user.PhoneNumber)
                .HasColumnName("phone_number")
                .HasMaxLength(20);

            builder.Property(user => user.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(user => user.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(user => user.UpdatedAt)
                .HasColumnName("updated_at");
        });
    }
}
