using MaintenanceSystem.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MaintenanceSystem.Services;
using MaintenanceSystem.Models;
using Xunit;

namespace MaintenanceSystem.Tests;

public class MaintenanceRequestServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly MaintenanceDbContext _dbContext;

    public MaintenanceRequestServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<MaintenanceDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new MaintenanceDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    private MaintenanceRequestService CreateService()
    {
        EquipmentService equipmentService =
            new EquipmentService(_dbContext);

        equipmentService.AddEquipment(new Equipment(
            1, "CNC Machine", "CNC-001",
            "Factory Floor A", EquipmentStatus.Operational));

        equipmentService.AddEquipment(new Equipment(
            2, "Water Pump", "PUMP-001",
            "Factory Floor B", EquipmentStatus.UnderMaintenance));

        return new MaintenanceRequestService(equipmentService);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }


    [Fact]
    public void CreateRequest_ExistingEquipment_CreatesOpenRequest()
    {
        // Arrange
        MaintenanceRequestService service = CreateService();

        // Act
        MaintenanceRequest request = service.CreateRequest(
            1, "Unusual vibration detected");

        // Assert
        Assert.Equal(1, request.Id);
        Assert.Equal(1, request.EquipmentId);
        Assert.Equal("Unusual vibration detected", request.Description);
        Assert.Equal(MaintenanceRequestStatus.Open, request.Status);
    }

    [Fact]
    public void CreateRequest_NonexistentEquipment_ThrowsException()
    {
        // Arrange
        MaintenanceRequestService service = CreateService();

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.CreateRequest(99, "Unknown equipment issue"));
    }

    [Fact]
    public void GetRequestsByEquipmentId_ReturnsOnlyMatchingRequests()
    {
        // Arrange
        MaintenanceRequestService service = CreateService();

        service.CreateRequest(1, "Unusual vibration detected");
        service.CreateRequest(2, "Water leakage");
        service.CreateRequest(1, "Abnormal noise");

        // Act
        IReadOnlyList<MaintenanceRequest> requests =
            service.GetRequestsByEquipmentId(1);

        // Assert
        Assert.Equal(2, requests.Count);
        Assert.All(requests, request =>
            Assert.Equal(1, request.EquipmentId));
    }

    [Fact]
    public void UpdateRequestStatus_ExistingRequest_UpdatesStatus()
    {
        // Arrange
        MaintenanceRequestService service = CreateService();

        MaintenanceRequest request = service.CreateRequest(
            1, "Unusual vibration detected");

        // Act
        bool updated = service.UpdateRequestStatus(
            request.Id, MaintenanceRequestStatus.InProgress);

        // Assert
        Assert.True(updated);
        Assert.Equal(MaintenanceRequestStatus.InProgress, request.Status);
    }

    [Fact]
    public void GetRequestById_NonexistentId_ReturnsNull()
    {
        // Arrange
        MaintenanceRequestService service = CreateService();

        // Act
        MaintenanceRequest? request = service.GetRequestById(99);

        // Assert
        Assert.Null(request);
    }

    [Fact]
    public void UpdateRequestStatus_NonexistentId_ReturnsFalse()
    {
        // Arrange
        MaintenanceRequestService service = CreateService();

        // Act
        bool updated = service.UpdateRequestStatus(
            99, MaintenanceRequestStatus.Resolved);

        // Assert
        Assert.False(updated);
    }
}