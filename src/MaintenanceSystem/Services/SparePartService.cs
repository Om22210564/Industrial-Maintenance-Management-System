using MaintenanceSystem.Data;
using MaintenanceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceSystem.Services;

public class SparePartService
{
    private readonly MaintenanceDbContext _context;

    public SparePartService(MaintenanceDbContext context)
    {
        _context = context;
    }

    public void AddSparePart(SparePart sparePart)
    {
        if (_context.SpareParts.Any(part => part.Id == sparePart.Id))
        {
            throw new InvalidOperationException(
                $"Spare part with ID {sparePart.Id} already exists.");
        }

        if (_context.SpareParts.Any(part =>
            part.PartNumber.ToLower() == sparePart.PartNumber.ToLower()))
        {
            throw new InvalidOperationException(
                $"Part number {sparePart.PartNumber} already exists.");
        }

        _context.SpareParts.Add(sparePart);
        _context.SaveChanges();
    }

    public IReadOnlyList<SparePart> GetAllSpareParts()
    {
        return _context.SpareParts
            .AsNoTracking()
            .OrderBy(part => part.Id)
            .ToList()
            .AsReadOnly();
    }

    public SparePart? GetSparePartById(int id)
    {
        return _context.SpareParts
            .FirstOrDefault(part => part.Id == id);
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
        _context.SaveChanges();

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
        _context.SaveChanges();

        return true;
    }

    public IReadOnlyList<SparePart> GetLowStockParts()
    {
        return _context.SpareParts
            .AsNoTracking()
            .Where(part => part.QuantityInStock <= part.MinimumStockLevel)
            .OrderBy(part => part.Id)
            .ToList()
            .AsReadOnly();
    }
}