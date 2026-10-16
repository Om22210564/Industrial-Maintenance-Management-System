
using MaintenanceSystem.Data;
using MaintenanceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceSystem.Services;

public class WorkOrderService
{
    private readonly MaintenanceDbContext _context;
    private readonly IMaintenanceRequestService _maintenanceRequestService;
    private readonly SparePartService _sparePartService;

    // Part usage remains in memory for now.

    public WorkOrderService(
        MaintenanceDbContext context,
        IMaintenanceRequestService maintenanceRequestService,
        SparePartService sparePartService)
    {
        _context = context;
        _maintenanceRequestService = maintenanceRequestService;
        _sparePartService = sparePartService;
    }

    public WorkOrder CreateWorkOrder(
        int maintenanceRequestId,
        string description)
    {
        MaintenanceRequest? request =
            _maintenanceRequestService.GetRequestById(maintenanceRequestId);

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

    if (quantity <= 0)
    {
        throw new ArgumentException(
            "Issue quantity must be greater than zero.",
            nameof(quantity));
    }

    SparePart? sparePart =
        _sparePartService.GetSparePartById(sparePartId);

    if (sparePart is null)
    {
        throw new InvalidOperationException(
            $"Spare part with ID {sparePartId} does not exist.");
    }

    using var transaction = _context.Database.BeginTransaction();

    try
    {
        bool issued =
            _sparePartService.IssuePart(sparePartId, quantity);

        if (!issued)
        {
            throw new InvalidOperationException(
                $"Could not issue spare part {sparePartId}.");
        }

        WorkOrderPart? usage = _context.WorkOrderParts
            .FirstOrDefault(part =>
                part.WorkOrderId == workOrderId &&
                part.SparePartId == sparePartId);

        if (usage is null)
        {
            usage = new WorkOrderPart(
                workOrderId,
                sparePartId,
                quantity);

            _context.WorkOrderParts.Add(usage);
        }
        else
        {
            usage.AddQuantity(quantity);
        }

        _context.SaveChanges();
        transaction.Commit();

        return usage;
    }
    catch
    {
        transaction.Rollback();

        // Discard tracked changes that may no longer match the database.
        _context.ChangeTracker.Clear();

        throw;
    }
}
public IReadOnlyList<WorkOrderPart> GetPartsUsedByWorkOrder(
    int workOrderId)
{
    return _context.WorkOrderParts
        .AsNoTracking()
        .Where(part => part.WorkOrderId == workOrderId)
        .OrderBy(part => part.SparePartId)
        .ToList()
        .AsReadOnly();
}
}
