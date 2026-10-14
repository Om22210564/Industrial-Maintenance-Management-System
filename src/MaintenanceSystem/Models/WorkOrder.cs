namespace MaintenanceSystem.Models;

public class WorkOrder
{
    public int Id { get; private set; }

    public int MaintenanceRequestId { get; private set; }

    public string Description { get; private set; }

    public string? AssignedTo { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public WorkOrderStatus Status { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public WorkOrder(
        int id,
        int maintenanceRequestId,
        string description)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "Work order ID must be greater than zero.",
                nameof(id));
        }

        if (maintenanceRequestId <= 0)
        {
            throw new ArgumentException(
                "Maintenance request ID must be greater than zero.",
                nameof(maintenanceRequestId));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Work order description cannot be empty.",
                nameof(description));
        }

        Id = id;
        MaintenanceRequestId = maintenanceRequestId;
        Description = description;
        AssignedTo = null;
        CreatedAt = DateTime.Now;
        Status = WorkOrderStatus.Open;
        CompletedAt = null;
    }

    internal void SetTechnician(string technicianName)
    {
        AssignedTo = technicianName;
    }

    internal void SetStatus(WorkOrderStatus status)
    {
        Status = status;

        if (status == WorkOrderStatus.Completed)
        {
            CompletedAt = DateTime.Now;
        }
    }
}
