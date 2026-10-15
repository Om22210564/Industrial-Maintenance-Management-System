using MaintenanceSystem.Data;
using MaintenanceSystem.Models;
using MaintenanceSystem.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MaintenanceSystem.Tests;

public class SparePartServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly MaintenanceDbContext _dbContext;

    public SparePartServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        DbContextOptions<MaintenanceDbContext> options =
            new DbContextOptionsBuilder<MaintenanceDbContext>()
                .UseSqlite(_connection)
                .Options;

        _dbContext = new MaintenanceDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    private SparePartService CreateService()
    {
        SparePartService service = new SparePartService(_dbContext);

        service.AddSparePart(new SparePart(
            1, "BRG-001", "Ball Bearing",
            5, 2, 12.50m));

        return service;
    }

    [Fact]
    public void AddSparePart_DuplicateId_ThrowsException()
    {
        SparePartService service = CreateService();

        SparePart duplicate = new SparePart(
            1, "SEAL-001", "Pump Seal",
            10, 3, 8.00m);

        Assert.Throws<InvalidOperationException>(() =>
            service.AddSparePart(duplicate));
    }

    [Fact]
    public void AddSparePart_DuplicatePartNumber_ThrowsException()
    {
        SparePartService service = CreateService();

        SparePart duplicate = new SparePart(
            2, "brg-001", "Another Bearing",
            10, 3, 15.00m);

        Assert.Throws<InvalidOperationException>(() =>
            service.AddSparePart(duplicate));
    }

    [Fact]
    public void IssuePart_SufficientStock_ReducesQuantity()
    {
        SparePartService service = CreateService();

        bool issued = service.IssuePart(1, 2);

        Assert.True(issued);
        Assert.Equal(3, service.GetSparePartById(1)!.QuantityInStock);
    }

    [Fact]
    public void IssuePart_InsufficientStock_ThrowsAndPreservesQuantity()
    {
        SparePartService service = CreateService();

        Assert.Throws<InvalidOperationException>(() =>
            service.IssuePart(1, 6));

        Assert.Equal(5, service.GetSparePartById(1)!.QuantityInStock);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IssuePart_NonPositiveQuantity_ThrowsException(
        int quantity)
    {
        SparePartService service = CreateService();

        Assert.Throws<ArgumentException>(() =>
            service.IssuePart(1, quantity));

        Assert.Equal(5, service.GetSparePartById(1)!.QuantityInStock);
    }

    [Fact]
    public void RestockPart_ValidQuantity_IncreasesStock()
    {
        SparePartService service = CreateService();

        bool restocked = service.RestockPart(1, 3);

        Assert.True(restocked);
        Assert.Equal(8, service.GetSparePartById(1)!.QuantityInStock);
    }

    [Fact]
    public void GetLowStockParts_ReturnsPartsAtOrBelowMinimum()
    {
        SparePartService service = CreateService();

        service.IssuePart(1, 3);

        IReadOnlyList<SparePart> lowStockParts =
            service.GetLowStockParts();

        Assert.Single(lowStockParts);
        Assert.Equal("BRG-001", lowStockParts[0].PartNumber);
        Assert.Equal(2, lowStockParts[0].QuantityInStock);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}