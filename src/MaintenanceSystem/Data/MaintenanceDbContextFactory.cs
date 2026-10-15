using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MaintenanceSystem.Data;

public class MaintenanceDbContextFactory
    : IDesignTimeDbContextFactory<MaintenanceDbContext>
{
    public MaintenanceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MaintenanceDbContext>()
            .UseSqlite("Data Source=maintenance.db")
            .Options;

        return new MaintenanceDbContext(options);
    }
}