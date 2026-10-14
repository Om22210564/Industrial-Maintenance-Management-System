using MaintenanceSystem.Models;
using MaintenanceSystem.Services;
using Xunit;

namespace MaintenanceSystem.Tests;

public class WorkOrderServiceTests
{
    private static WorkOrderService CreateService(
        out MaintenanceRequestService requestService)
    {
        EquipmentService equipmentService = new EquipmentService();

        equipmentService.AddEquipment(new Equipment(
            1, "CNC Machine", "CNC-001",
            "Factory Floor A", EquipmentStatus.Operational));

        requestService = new MaintenanceRequestService(
            equipmentService);

        requestService.CreateRequest(
            1, "Unusual vibration detected");

        requestService.CreateRequest(
            1, "Abnormal noise detected");

        return new WorkOrderService(requestService);
    }

    [Fact]
    public void CreateWorkOrder_ValidRequest_CreatesWorkOrder()
    {
        // Arrange
        WorkOrderService service = CreateService(
            out MaintenanceRequestService requestService);

        // Act
        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Inspect spindle and check bearing wear");

        // Assert
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
        // Arrange
        WorkOrderService service = CreateService(
            out MaintenanceRequestService requestService);

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.CreateWorkOrder(99, "Investigate unknown issue"));
    }

    [Theory]
    [InlineData(MaintenanceRequestStatus.Resolved)]
    [InlineData(MaintenanceRequestStatus.Cancelled)]
    public void CreateWorkOrder_IneligibleRequest_ThrowsException(
        MaintenanceRequestStatus status)
    {
        // Arrange
        WorkOrderService service = CreateService(
            out MaintenanceRequestService requestService);

        MaintenanceRequest request =
            requestService.GetRequestById(1)!;

        request.Status = status;

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.CreateWorkOrder(
                request.Id, "Inspect equipment"));
    }

    [Fact]
    public void UpdateWorkOrderStatus_ValidTransition_UpdatesStatus()
    {
        // Arrange
        WorkOrderService service = CreateService(
            out MaintenanceRequestService requestService);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Inspect spindle");

        // Act
        bool updated = service.UpdateWorkOrderStatus(
            workOrder.Id, WorkOrderStatus.InProgress);

        // Assert
        Assert.True(updated);
        Assert.Equal(WorkOrderStatus.InProgress, workOrder.Status);
        Assert.Null(workOrder.CompletedAt);
    }

    [Fact]
    public void UpdateWorkOrderStatus_InvalidTransition_ThrowsException()
    {
        // Arrange
        WorkOrderService service = CreateService(
            out MaintenanceRequestService requestService);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Inspect spindle");

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.UpdateWorkOrderStatus(
                workOrder.Id, WorkOrderStatus.Completed));

        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);
        Assert.Null(workOrder.CompletedAt);
    }

    [Fact]
    public void UpdateWorkOrderStatus_Completed_SetsCompletedAt()
    {
        // Arrange
        WorkOrderService service = CreateService(
            out MaintenanceRequestService requestService);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Inspect spindle");

        service.UpdateWorkOrderStatus(
            workOrder.Id, WorkOrderStatus.InProgress);

        // Act
        bool updated = service.UpdateWorkOrderStatus(
            workOrder.Id, WorkOrderStatus.Completed);

        // Assert
        Assert.True(updated);
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
        Assert.NotNull(workOrder.CompletedAt);
    }

    [Fact]
    public void AssignTechnician_EmptyName_ThrowsException()
    {
        // Arrange
        WorkOrderService service = CreateService(
            out MaintenanceRequestService requestService);

        WorkOrder workOrder = service.CreateWorkOrder(
            1, "Inspect spindle");

        // Act and Assert
        Assert.Throws<ArgumentException>(() =>
            service.AssignTechnician(workOrder.Id, "   "));

        Assert.Null(workOrder.AssignedTo);
    }
}