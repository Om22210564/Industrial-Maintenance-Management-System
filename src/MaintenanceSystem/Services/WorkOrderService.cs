
using MaintenanceSystem.Data;
using MaintenanceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceSystem.Services;

public class WorkOrderService
{
    private readonly MaintenanceDbContext _context;
    private readonly MaintenanceRequestService _requestService;
    private readonly SparePartService _sparePartService;

    // Part usage remains in memory for now.
    private readonly List<WorkOrderPart> _workOrderParts = new();

    public WorkOrderService(
        MaintenanceDbContext context,
        MaintenanceRequestService requestService,
        SparePartService sparePartService)
    {
        _context = context;
        _requestService = requestService;
        _sparePartService = sparePartService;
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

        int nextWorkOrderId = _context.WorkOrders.Any()
            ? _context.WorkOrders.Max(order => order.Id) + 1
            : 1;

        WorkOrder workOrder = new WorkOrder(
            nextWorkOrderId,
            maintenanceRequestId,
            description);

        _context.WorkOrders.Add(workOrder);
        _context.SaveChanges();

        return workOrder;
    }

    public IReadOnlyList<WorkOrder> GetAllWorkOrders()
    {
        return _context.WorkOrders
            .AsNoTracking()
            .OrderBy(order => order.Id)
            .ToList()
            .AsReadOnly();
    }

    public WorkOrder? GetWorkOrderById(int id)
    {
        return _context.WorkOrders
            .FirstOrDefault(order => order.Id == id);
    }

    public IReadOnlyList<WorkOrder> GetWorkOrdersByStatus(
        WorkOrderStatus status)
    {
        return _context.WorkOrders
            .AsNoTracking()
            .Where(order => order.Status == status)
            .OrderBy(order => order.Id)
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
        _context.SaveChanges();

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
        _context.SaveChanges();

        return true;
    }

    public WorkOrderPart IssuePartToWorkOrder(
        int workOrderId,
        int sparePartId,
        int quantity)
    {
        WorkOrder? workOrder = GetWorkOrderById(workOrderId);

        if (workOrder is null)
        {
            throw new InvalidOperationException(
                $"Work order with ID {workOrderId} does not exist.");
        }

        if (workOrder.Status != WorkOrderStatus.InProgress)
        {
            throw new InvalidOperationException(
                "Parts can only be issued to an in-progress work order.");
        }

        SparePart? sparePart =
            _sparePartService.GetSparePartById(sparePartId);

        if (sparePart is null)
        {
            throw new InvalidOperationException(
                $"Spare part with ID {sparePartId} does not exist.");
        }

        bool issued =
            _sparePartService.IssuePart(sparePartId, quantity);

        if (!issued)
        {
            throw new InvalidOperationException(
                $"Could not issue spare part {sparePartId}.");
        }

        WorkOrderPart usage = new WorkOrderPart(
            workOrderId,
            sparePartId,
            quantity);

        _workOrderParts.Add(usage);

        return usage;
    }

    public IReadOnlyList<WorkOrderPart> GetPartsUsedByWorkOrder(
        int workOrderId)
    {
        return _workOrderParts
            .Where(part => part.WorkOrderId == workOrderId)
            .ToList()
            .AsReadOnly();
    }
}
