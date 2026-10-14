namespace MaintenanceSystem.Models;

public class SparePart
{
    public int Id { get; private set; }

    public string PartNumber { get; private set; }

    public string Name { get; private set; }

    public int QuantityInStock { get; private set; }

    public int MinimumStockLevel { get; private set; }

    public decimal UnitCost { get; private set; }

    public SparePart(
        int id,
        string partNumber,
        string name,
        int quantityInStock,
        int minimumStockLevel,
        decimal unitCost)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "Spare part ID must be greater than zero.",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(partNumber))
        {
            throw new ArgumentException(
                "Part number cannot be empty.",
                nameof(partNumber));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Part name cannot be empty.",
                nameof(name));
        }

        if (quantityInStock < 0)
        {
            throw new ArgumentException(
                "Quantity in stock cannot be negative.",
                nameof(quantityInStock));
        }

        if (minimumStockLevel < 0)
        {
            throw new ArgumentException(
                "Minimum stock level cannot be negative.",
                nameof(minimumStockLevel));
        }

        if (unitCost < 0)
        {
            throw new ArgumentException(
                "Unit cost cannot be negative.",
                nameof(unitCost));
        }

        Id = id;
        PartNumber = partNumber.Trim();
        Name = name.Trim();
        QuantityInStock = quantityInStock;
        MinimumStockLevel = minimumStockLevel;
        UnitCost = unitCost;
    }

    internal void ReduceStock(int quantity)
    {
        QuantityInStock -= quantity;
    }

    internal void IncreaseStock(int quantity)
    {
        QuantityInStock += quantity;
    }
}