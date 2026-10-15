using MaintenanceSystem.Models;
using MaintenanceSystem.Services;
using Xunit;
using MaintenanceSystem.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace MaintenanceSystem.Tests;

public class WorkOrderServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly MaintenanceDbContext _dbContext;

    public WorkOrderServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<MaintenanceDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new MaintenanceDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    private WorkOrderService CreateService(
        out MaintenanceRequestService requestService,
        out SparePartService sparePartService)
    {
        EquipmentService equipmentService =
            new EquipmentService(_dbContext);

        equipmentService.AddEquipment(new Equipment(
            1, "CNC Machine", "CNC-001",
            "Factory Floor A", EquipmentStatus.Operational));

        requestService = new MaintenanceRequestService(
            equipmentService);

        requestService.CreateRequest(
            1, "Unusual vibration detected");

        requestService.CreateRequest(
            1, "Abnormal noise detected");

        sparePartService = new SparePartService();

        sparePartService.AddSparePart(new SparePart(
            1, "BRG-001", "Ball Bearing",
            5, 2, 12.50m));

        return new WorkOrderService(
            requestService,
            sparePartService);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void CreateWorkOrder_ValidRequest_CreatesWorkOrder()
    {
        WorkOrderService service = CreateService(
            out _, out _);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Inspect spindle and check bearing wear");

        Assert.Equal(1, workOrder.Id);
        Assert.Equal(1, workOrder.MaintenanceRequestId);
        Assert.Equal(
            "Inspect spindle and check bearing wear",
            workOrder.Description);
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);
        Assert.Null(workOrder.AssignedTo);
        Assert.Null(workOrder.CompletedAt);
    }

    [Fact]
    public void CreateWorkOrder_UnknownRequest_ThrowsException()
    {
        WorkOrderService service = CreateService(
            out _, out _);

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateWorkOrder(99, "Investigate unknown issue"));
    }

    [Theory]
    [InlineData(MaintenanceRequestStatus.Resolved)]
    [InlineData(MaintenanceRequestStatus.Cancelled)]
    public void CreateWorkOrder_IneligibleRequest_ThrowsException(
        MaintenanceRequestStatus status)
    {
        WorkOrderService service = CreateService(
            out MaintenanceRequestService requestService,
            out _);

        MaintenanceRequest request =
            requestService.GetRequestById(1)!;

        request.Status = status;

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateWorkOrder(
                request.Id, "Inspect equipment"));
    }

    [Fact]
    public void UpdateWorkOrderStatus_ValidTransition_UpdatesStatus()
    {
        WorkOrderService service = CreateService(
            out _, out _);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Inspect spindle");

        bool updated = service.UpdateWorkOrderStatus(
            workOrder.Id, WorkOrderStatus.InProgress);

        Assert.True(updated);
        Assert.Equal(WorkOrderStatus.InProgress, workOrder.Status);
        Assert.Null(workOrder.CompletedAt);
    }

    [Fact]
    public void UpdateWorkOrderStatus_InvalidTransition_ThrowsException()
    {
        WorkOrderService service = CreateService(
            out _, out _);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Inspect spindle");

        Assert.Throws<InvalidOperationException>(() =>
            service.UpdateWorkOrderStatus(
                workOrder.Id, WorkOrderStatus.Completed));

        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);
        Assert.Null(workOrder.CompletedAt);
    }

    [Fact]
    public void UpdateWorkOrderStatus_Completed_SetsCompletedAt()
    {
        WorkOrderService service = CreateService(
            out _, out _);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Inspect spindle");

        service.UpdateWorkOrderStatus(
            workOrder.Id, WorkOrderStatus.InProgress);

        bool updated = service.UpdateWorkOrderStatus(
            workOrder.Id, WorkOrderStatus.Completed);

        Assert.True(updated);
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
        Assert.NotNull(workOrder.CompletedAt);
    }

    [Fact]
    public void AssignTechnician_EmptyName_ThrowsException()
    {
        WorkOrderService service = CreateService(
            out _, out _);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Inspect spindle");

        Assert.Throws<ArgumentException>(() =>
            service.AssignTechnician(workOrder.Id, "   "));

        Assert.Null(workOrder.AssignedTo);
    }

    [Fact]
    public void IssuePartToWorkOrder_InProgressOrder_ReducesStockAndRecordsUsage()
    {
        WorkOrderService service = CreateService(
            out _, out SparePartService sparePartService);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Repair spindle");

        service.UpdateWorkOrderStatus(
            workOrder.Id, WorkOrderStatus.InProgress);

        WorkOrderPart usage = service.IssuePartToWorkOrder(
            workOrder.Id, 1, 2);

        Assert.Equal(workOrder.Id, usage.WorkOrderId);
        Assert.Equal(1, usage.SparePartId);
        Assert.Equal(2, usage.QuantityUsed);

        Assert.Equal(
            3,
            sparePartService.GetSparePartById(1)!.QuantityInStock);

        IReadOnlyList<WorkOrderPart> usages =
            service.GetPartsUsedByWorkOrder(workOrder.Id);

        Assert.Single(usages);
        Assert.Equal(2, usages[0].QuantityUsed);
    }

    [Fact]
    public void IssuePartToWorkOrder_InsufficientStock_ThrowsAndPreservesStock()
    {
        WorkOrderService service = CreateService(
            out _, out SparePartService sparePartService);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Repair spindle");

        service.UpdateWorkOrderStatus(
            workOrder.Id, WorkOrderStatus.InProgress);

        Assert.Throws<InvalidOperationException>(() =>
            service.IssuePartToWorkOrder(workOrder.Id, 1, 6));

        Assert.Equal(
            5,
            sparePartService.GetSparePartById(1)!.QuantityInStock);

        Assert.Empty(service.GetPartsUsedByWorkOrder(workOrder.Id));
    }

    [Theory]
    [InlineData(WorkOrderStatus.Open)]
    [InlineData(WorkOrderStatus.Completed)]
    [InlineData(WorkOrderStatus.Cancelled)]
    public void IssuePartToWorkOrder_OrderNotInProgress_ThrowsAndPreservesStock(
        WorkOrderStatus status)
    {
        WorkOrderService service = CreateService(
            out _, out SparePartService sparePartService);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Repair spindle");

        if (status == WorkOrderStatus.Completed)
        {
            service.UpdateWorkOrderStatus(
                workOrder.Id, WorkOrderStatus.InProgress);

            service.UpdateWorkOrderStatus(
                workOrder.Id, WorkOrderStatus.Completed);
        }
        else if (status == WorkOrderStatus.Cancelled)
        {
            service.UpdateWorkOrderStatus(
                workOrder.Id, WorkOrderStatus.Cancelled);
        }

        Assert.Throws<InvalidOperationException>(() =>
            service.IssuePartToWorkOrder(workOrder.Id, 1, 1));

        Assert.Equal(
            5,
            sparePartService.GetSparePartById(1)!.QuantityInStock);

        Assert.Empty(service.GetPartsUsedByWorkOrder(workOrder.Id));
    }

    [Fact]
    public void IssuePartToWorkOrder_UnknownWorkOrder_ThrowsAndPreservesStock()
    {
        WorkOrderService service = CreateService(
            out _, out SparePartService sparePartService);

        Assert.Throws<InvalidOperationException>(() =>
            service.IssuePartToWorkOrder(99, 1, 1));

        Assert.Equal(
            5,
            sparePartService.GetSparePartById(1)!.QuantityInStock);
    }

    [Fact]
    public void IssuePartToWorkOrder_UnknownSparePart_ThrowsAndPreservesStock()
    {
        WorkOrderService service = CreateService(
            out _, out SparePartService sparePartService);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Repair spindle");

        service.UpdateWorkOrderStatus(
            workOrder.Id, WorkOrderStatus.InProgress);

        Assert.Throws<InvalidOperationException>(() =>
            service.IssuePartToWorkOrder(workOrder.Id, 99, 1));

        Assert.Equal(
            5,
            sparePartService.GetSparePartById(1)!.QuantityInStock);

        Assert.Empty(service.GetPartsUsedByWorkOrder(workOrder.Id));
    }
}