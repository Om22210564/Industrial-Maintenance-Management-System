
using MaintenanceSystem.Data;
using MaintenanceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceSystem.Services;

public class EquipmentService : IEquipmentService
{
    private readonly MaintenanceDbContext _context;

    public EquipmentService(MaintenanceDbContext context)
    {
        _context = context;
    }

    public void AddEquipment(Equipment equipment)
    {
        if (_context.Equipment.Any(item => item.Id == equipment.Id))
        {
            throw new InvalidOperationException(
                $"Equipment with ID {equipment.Id} already exists.");
        }

        if (_context.Equipment.Any(item =>
            item.EquipmentCode == equipment.EquipmentCode))
        {
            throw new InvalidOperationException(
                $"Equipment code {equipment.EquipmentCode} already exists.");
        }

        _context.Equipment.Add(equipment);
        _context.SaveChanges();
    }

    public IReadOnlyList<Equipment> GetAllEquipment()
    {
        return _context.Equipment
            .AsNoTracking()
            .ToList()
            .AsReadOnly();
    }

    public Equipment? GetEquipmentById(int id)
    {
        return _context.Equipment
            .AsNoTracking()
            .FirstOrDefault(item => item.Id == id);
    }

    // public bool RemoveEquipment(int id)
    // {
    //     Equipment? equipment = GetEquipmentById(id);

    //     if (equipment is null)
    //     {
    //         return false;
    //     }

    //     _context.Equipment.Remove(equipment);
    //     _context.SaveChanges();

    //     return true;
    // }
    
    public bool RemoveEquipment(int id)
{
    Equipment? equipment = _context.Equipment
        .FirstOrDefault(item => item.Id == id);

    if (equipment is null)
    {
        return false;
    }

    // Prevent deletion if maintenance requests reference this equipment
    bool hasMaintenanceRequests = _context.MaintenanceRequests
        .Any(request => request.EquipmentId == id);

    if (hasMaintenanceRequests)
    {
        throw new InvalidOperationException(
            $"Equipment with ID {id} cannot be removed because it has related maintenance requests.");
    }

    _context.Equipment.Remove(equipment);
    _context.SaveChanges();

    return true;
}

}