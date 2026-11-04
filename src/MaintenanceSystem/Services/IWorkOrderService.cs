using MaintenanceSystem.Models;

namespace MaintenanceSystem.Services;

public interface IWorkOrderService
{
    WorkOrder CreateWorkOrder(
        int maintenanceRequestId,
        string description);

    IReadOnlyList<WorkOrder> GetAllWorkOrders();

    WorkOrder? GetWorkOrderById(int id);

    IReadOnlyList<WorkOrder> GetWorkOrdersByStatus(
        WorkOrderStatus status);

    bool AssignTechnician(
        int workOrderId,
        string technicianName);

    bool UpdateWorkOrderStatus(
        int workOrderId,
        WorkOrderStatus newStatus);

    WorkOrderPart IssuePartToWorkOrder(
        int workOrderId,
        int sparePartId,
        int quantity);

    IReadOnlyList<WorkOrderPart> GetPartsUsedByWorkOrder(
        int workOrderId);
}