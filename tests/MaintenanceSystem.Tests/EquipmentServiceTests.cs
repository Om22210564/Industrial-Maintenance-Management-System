using MaintenanceSystem.Data;
using MaintenanceSystem.Models;
using MaintenanceSystem.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MaintenanceSystem.Tests;

public class EquipmentServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly MaintenanceDbContext _dbContext;
    private readonly EquipmentService _service;

    public EquipmentServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<MaintenanceDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new MaintenanceDbContext(options);
        _dbContext.Database.EnsureCreated();

        _service = new EquipmentService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void AddEquipment_ValidEquipment_SavesToDatabase()
    {
        // Arrange
        Equipment equipment = new Equipment(
            1, "CNC Machine", "CNC-001",
            "Factory Floor A", EquipmentStatus.Operational);

        // Act
        _service.AddEquipment(equipment);

        // Assert
        Equipment? saved = _dbContext.Equipment
            .AsNoTracking()
            .FirstOrDefault(item => item.Id == 1);

        Assert.NotNull(saved);
        Assert.Equal("CNC Machine", saved.Name);
        Assert.Equal("CNC-001", saved.EquipmentCode);
    }

    [Fact]
    public void AddEquipment_DuplicateId_ThrowsException()
    {
        // Arrange
        _service.AddEquipment(new Equipment(
            1, "CNC Machine", "CNC-001",
            "Factory Floor A", EquipmentStatus.Operational));

        Equipment duplicate = new Equipment(
            1, "Another Machine", "CNC-002",
            "Factory Floor B", EquipmentStatus.Operational);

        // Act and Assert
        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => _service.AddEquipment(duplicate));

        Assert.Contains("ID 1 already exists", exception.Message);
        Assert.Equal(1, _dbContext.Equipment.Count());
    }

    [Fact]
    public void AddEquipment_DuplicateCode_ThrowsException()
    {
        // Arrange
        _service.AddEquipment(new Equipment(
            1, "CNC Machine", "CNC-001",
            "Factory Floor A", EquipmentStatus.Operational));

        Equipment duplicate = new Equipment(
            2, "Another Machine", "CNC-001",
            "Factory Floor B", EquipmentStatus.Operational);

        // Act and Assert
        Assert.Throws<InvalidOperationException>(
            () => _service.AddEquipment(duplicate));

        Assert.Equal(1, _dbContext.Equipment.Count());
    }

    [Fact]
    public void GetEquipmentById_ExistingId_ReturnsEquipment()
    {
        // Arrange
        _service.AddEquipment(new Equipment(
            1, "CNC Machine", "CNC-001",
            "Factory Floor A", EquipmentStatus.Operational));

        // Act
        Equipment? result = _service.GetEquipmentById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CNC Machine", result.Name);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void GetEquipmentById_NonexistentId_ReturnsNull()
    {
        // Act
        Equipment? result = _service.GetEquipmentById(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAllEquipment_ReturnsAllSavedEquipment()
    {
        // Arrange
        _service.AddEquipment(new Equipment(
            1, "CNC Machine", "CNC-001",
            "Factory Floor A", EquipmentStatus.Operational));

        _service.AddEquipment(new Equipment(
            2, "Water Pump", "PUMP-001",
            "Factory Floor B", EquipmentStatus.UnderMaintenance));

        // Act
        IReadOnlyList<Equipment> equipment =
            _service.GetAllEquipment();

        // Assert
        Assert.Equal(2, equipment.Count);
    }

    [Fact]
    public void RemoveEquipment_ExistingId_RemovesFromDatabase()
    {
        // Arrange
        _service.AddEquipment(new Equipment(
            1, "CNC Machine", "CNC-001",
            "Factory Floor A", EquipmentStatus.Operational));

        // Act
        bool removed = _service.RemoveEquipment(1);

        // Assert
        Assert.True(removed);
        Assert.False(_dbContext.Equipment.Any());
    }

    [Fact]
    public void RemoveEquipment_NonexistentId_ReturnsFalse()
    {
        // Act
        bool removed = _service.RemoveEquipment(99);

        // Assert
        Assert.False(removed);
    }

    [Fact]
    public void AddEquipment_NewContext_CanRetrieveSavedEquipment()
    {
        // Arrange
        _service.AddEquipment(new Equipment(
            1, "CNC Machine", "CNC-001",
            "Factory Floor A", EquipmentStatus.Operational));

        // Act
        using var anotherContext =
            new MaintenanceDbContext(_dbContext.Database
                .GetDbConnection()
                .ConnectionString == ":memory:"
                    ? new DbContextOptionsBuilder<MaintenanceDbContext>()
                        .UseSqlite(_connection)
                        .Options
                    : new DbContextOptionsBuilder<MaintenanceDbContext>()
                        .UseSqlite(_connection)
                        .Options);

        EquipmentService anotherService =
            new EquipmentService(anotherContext);

        Equipment? result = anotherService.GetEquipmentById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CNC Machine", result.Name);
    }
}