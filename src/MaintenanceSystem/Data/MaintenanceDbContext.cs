using MaintenanceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceSystem.Data;

public class MaintenanceDbContext : DbContext
{
    public MaintenanceDbContext(
        DbContextOptions<MaintenanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<MaintenanceRequest> MaintenanceRequests =>
    Set<MaintenanceRequest>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.EquipmentCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.EquipmentCode)
                .IsUnique();

            entity.Property(e => e.Location)
                .HasMaxLength(200);

            // Your existing code assigns IDs manually.
            entity.Property(e => e.Id)
                .ValueGeneratedNever();
        });
        
        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.HasKey(request => request.Id);

            entity.Property(request => request.Id)
                .ValueGeneratedNever();

            entity.Property(request => request.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(request => request.CreatedAt)
                .IsRequired();

            entity.Property(request => request.Status)
                .IsRequired();

            entity.HasOne<Equipment>()
                .WithMany()
                .HasForeignKey(request => request.EquipmentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}