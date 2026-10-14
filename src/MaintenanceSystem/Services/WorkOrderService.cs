using MaintenanceSystem.Models;

namespace MaintenanceSystem.Services;

public class WorkOrderService
{
    private readonly List<WorkOrder> _workOrders = new();
    private readonly MaintenanceRequestService _requestService;

    private int _nextWorkOrderId = 1;

    public WorkOrderService(
        MaintenanceRequestService requestService)
    {
        _requestService = requestService;
    }

    public WorkOrder CreateWorkOrder(
        int maintenanceRequestId,
        string description)
    {
        MaintenanceRequest? request =
            _requestService.GetRequestById(maintenanceRequestId);

        if (request is null)
        {
            throw new InvalidOperationException(
                $"Maintenance request with ID {maintenanceRequestId} does not exist.");
        }

        if (request.Status == MaintenanceRequestStatus.Resolved ||
            request.Status == MaintenanceRequestStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Maintenance request {maintenanceRequestId} is not eligible for work.");
        }

        WorkOrder workOrder = new WorkOrder(
            _nextWorkOrderId,
            maintenanceRequestId,
            description);

        _workOrders.Add(workOrder);
        _nextWorkOrderId++;

        return workOrder;
    }

    public IReadOnlyList<WorkOrder> GetAllWorkOrders()
    {
        return _workOrders.AsReadOnly();
    }

    public WorkOrder? GetWorkOrderById(int id)
    {
        return _workOrders.FirstOrDefault(
            workOrder => workOrder.Id == id);
    }

    public IReadOnlyList<WorkOrder> GetWorkOrdersByStatus(
        WorkOrderStatus status)
    {
        return _workOrders
            .Where(workOrder => workOrder.Status == status)
            .ToList()
            .AsReadOnly();
    }

    public bool AssignTechnician(
        int workOrderId,
        string technicianName)
    {
        if (string.IsNullOrWhiteSpace(technicianName))
        {
            throw new ArgumentException(
                "Technician name cannot be empty.",
                nameof(technicianName));
        }

        WorkOrder? workOrder = GetWorkOrderById(workOrderId);

        if (workOrder is null)
        {
            return false;
        }

        if (workOrder.Status != WorkOrderStatus.Open &&
            workOrder.Status != WorkOrderStatus.InProgress)
        {
            throw new InvalidOperationException(
                "A technician can only be assigned to an open or in-progress work order.");
        }

        workOrder.SetTechnician(technicianName.Trim());
        return true;
    }

    public bool UpdateWorkOrderStatus(
        int workOrderId,
        WorkOrderStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus))
        {
            throw new ArgumentException(
                "Invalid work order status.",
                nameof(newStatus));
        }

        WorkOrder? workOrder = GetWorkOrderById(workOrderId);

        if (workOrder is null)
        {
            return false;
        }

        bool isAllowed =
            (workOrder.Status == WorkOrderStatus.Open &&
                (newStatus == WorkOrderStatus.InProgress ||
                 newStatus == WorkOrderStatus.Cancelled))
            ||
            (workOrder.Status == WorkOrderStatus.InProgress &&
                (newStatus == WorkOrderStatus.Completed ||
                 newStatus == WorkOrderStatus.Cancelled));

        if (!isAllowed)
        {
            throw new InvalidOperationException(
                $"Cannot change work order from {workOrder.Status} to {newStatus}.");
        }

        workOrder.SetStatus(newStatus);
        return true;
    }
}