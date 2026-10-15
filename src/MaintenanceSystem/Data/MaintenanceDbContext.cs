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
    }
}