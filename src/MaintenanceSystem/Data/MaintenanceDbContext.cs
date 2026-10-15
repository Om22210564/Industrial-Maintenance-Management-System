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

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();


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
        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(workOrder => workOrder.Id);

            entity.Property(workOrder => workOrder.Id)
                .ValueGeneratedNever();

            entity.Property(workOrder => workOrder.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(workOrder => workOrder.AssignedTo)
                .HasMaxLength(200);

            entity.Property(workOrder => workOrder.CreatedAt)
                .IsRequired();

            entity.Property(workOrder => workOrder.Status)
                .IsRequired();

            entity.HasOne<MaintenanceRequest>()
                .WithMany()
                .HasForeignKey(workOrder => workOrder.MaintenanceRequestId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}