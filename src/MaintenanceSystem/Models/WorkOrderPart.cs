namespace MaintenanceSystem.Models;

public class WorkOrderPart
{
    public int WorkOrderId { get; private set; }

    public int SparePartId { get; private set; }

    public int QuantityUsed { get; private set; }

    public WorkOrderPart(
        int workOrderId,
        int sparePartId,
        int quantityUsed)
    {
        if (workOrderId <= 0)
        {
            throw new ArgumentException(
                "Work order ID must be greater than zero.",
                nameof(workOrderId));
        }

        if (sparePartId <= 0)
        {
            throw new ArgumentException(
                "Spare part ID must be greater than zero.",
                nameof(sparePartId));
        }

        if (quantityUsed <= 0)
        {
            throw new ArgumentException(
                "Quantity used must be greater than zero.",
                nameof(quantityUsed));
        }

        WorkOrderId = workOrderId;
        SparePartId = sparePartId;
        QuantityUsed = quantityUsed;
    }
}