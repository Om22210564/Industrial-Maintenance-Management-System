
using MaintenanceSystem.Models;

namespace MaintenanceSystem.Services;

public class SparePartService
{
    private readonly List<SparePart> _spareParts = new();

    public void AddSparePart(SparePart sparePart)
    {
        if (_spareParts.Any(part => part.Id == sparePart.Id))
        {
            throw new InvalidOperationException(
                $"Spare part with ID {sparePart.Id} already exists.");
        }

        if (_spareParts.Any(part =>
            string.Equals(
                part.PartNumber,
                sparePart.PartNumber,
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Part number {sparePart.PartNumber} already exists.");
        }

        _spareParts.Add(sparePart);
    }

    public IReadOnlyList<SparePart> GetAllSpareParts()
    {
        return _spareParts.AsReadOnly();
    }

    public SparePart? GetSparePartById(int id)
    {
        return _spareParts.FirstOrDefault(part => part.Id == id);
    }

    public bool IssuePart(int partId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Issue quantity must be greater than zero.",
                nameof(quantity));
        }

        SparePart? part = GetSparePartById(partId);

        if (part is null)
        {
            return false;
        }

        if (part.QuantityInStock < quantity)
        {
            throw new InvalidOperationException(
                $"Insufficient stock for {part.PartNumber}. " +
                $"Available: {part.QuantityInStock}, requested: {quantity}.");
        }

        part.ReduceStock(quantity);
        return true;
    }

    public bool RestockPart(int partId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Restock quantity must be greater than zero.",
                nameof(quantity));
        }

        SparePart? part = GetSparePartById(partId);

        if (part is null)
        {
            return false;
        }

        if (quantity > int.MaxValue - part.QuantityInStock)
        {
            throw new InvalidOperationException(
                "Restocking would exceed the maximum supported stock quantity.");
        }

        part.IncreaseStock(quantity);
        return true;
    }

    public IReadOnlyList<SparePart> GetLowStockParts()
    {
        return _spareParts
            .Where(part =>
                part.QuantityInStock <= part.MinimumStockLevel)
            .ToList()
            .AsReadOnly();
    }
}