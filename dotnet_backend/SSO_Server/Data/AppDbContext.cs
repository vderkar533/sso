using Microsoft.EntityFrameworkCore;
using SSO.Server.Models;

namespace SSO.Server.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OneTimeToken> OneTimeTokens => Set<OneTimeToken>();
    public DbSet<ApplicationClient> Applications => Set<ApplicationClient>();
    public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
    public DbSet<ContractorProfile> ContractorProfiles => Set<ContractorProfile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.EmployeeId);
            entity.Property(x => x.EmployeeId)
                .HasMaxLength(50)
                .IsRequired()
                .ValueGeneratedNever();
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.Property(x => x.MobileNumber).HasMaxLength(20);
            entity.Property(x => x.AadharNumber).HasMaxLength(20);
            entity.Property(x => x.UserName).HasMaxLength(256);
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.HasIndex(x => x.Email).IsUnique();
        });

        builder.Entity<OneTimeToken>(entity =>
        {
            entity.ToTable("OneTimeTokens");
            entity.HasKey(x => x.OneTimeTokenId);
            entity.Property(x => x.UserId).HasMaxLength(50);
            entity.Property(x => x.TokenHash).HasMaxLength(64);
            entity.HasIndex(x => new { x.UserId, x.Purpose, x.TokenHash });
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Grade>(entity =>
        {
            entity.HasKey(x => x.GradeId);
            entity.Property(x => x.GradeId).ValueGeneratedOnAdd();
            entity.Property(x => x.GradeCode).HasMaxLength(10);
            entity.Property(x => x.Band).HasMaxLength(50);
            entity.Property(x => x.Title).HasMaxLength(200);
        });

        builder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(x => x.DepartmentId);
            entity.Property(x => x.DepartmentId).ValueGeneratedOnAdd();
            entity.Property(x => x.DepartmentName).HasMaxLength(100);
            entity.HasIndex(x => x.DepartmentName).IsUnique();
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.RefreshTokenId);
            entity.HasIndex(x => x.Token).IsUnique();
            entity.Property(x => x.Token).HasMaxLength(500);
            entity.Property(x => x.UserId).HasMaxLength(50);
        });

        builder.Entity<ApplicationClient>(entity =>
        {
            entity.HasKey(x => x.ApplicationClientId);
            entity.HasIndex(x => x.ClientId).IsUnique();
            entity.Property(x => x.ClientId).HasMaxLength(100);
            entity.Property(x => x.ClientName).HasMaxLength(200);
            entity.Property(x => x.RedirectUri).HasMaxLength(500);
        });

        builder.Entity<EmployeeProfile>(entity =>
        {
            entity.ToTable("EmployeeProfiles");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId).HasMaxLength(50);
            entity.Property(x => x.EmployeeId).HasMaxLength(50);
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.Property(x => x.MobileNumber).HasMaxLength(20);
            entity.Property(x => x.AadharNumber).HasMaxLength(20);
            entity.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<EmployeeProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ContractorProfile>(entity =>
        {
            entity.ToTable("ContractorProfiles");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId).HasMaxLength(50);
            entity.Property(x => x.EmployeeId).HasMaxLength(50);
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.Property(x => x.MobileNumber).HasMaxLength(20);
            entity.Property(x => x.AadharNumber).HasMaxLength(20);
            entity.Property(x => x.AgencyName).HasMaxLength(200);
            entity.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<ContractorProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Grade>().HasData(GradeSeedData.All);
        builder.Entity<Department>().HasData(DepartmentSeedData.All);
        builder.Entity<ApplicationClient>().HasData(ApplicationSeedData.All);

        builder.Entity<ApplicationUser>()
            .HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId);

        builder.Entity<ApplicationUser>()
            .HasOne(x => x.Grade)
            .WithMany()
            .HasForeignKey(x => x.GradeId);
    }
}
