using System.Linq;
using MaintenanceSystem.Models;

namespace MaintenanceSystem.Services;

public class EquipmentService
{
    private readonly List<Equipment> _equipment = new();

    public void AddEquipment(Equipment equipment)
    {
        if (_equipment.Any(item => item.Id == equipment.Id))
        {
            throw new InvalidOperationException(
                $"Equipment with ID {equipment.Id} already exists.");
        }

        if (_equipment.Any(item =>
            item.EquipmentCode == equipment.EquipmentCode))
        {
            throw new InvalidOperationException(
                $"Equipment code {equipment.EquipmentCode} already exists.");
        }

        _equipment.Add(equipment);
    }

    public IReadOnlyList<Equipment> GetAllEquipment()
    {
        return _equipment.AsReadOnly();
    }

    public Equipment? GetEquipmentById(int id)
    {
        return _equipment.FirstOrDefault(item => item.Id == id);
    }

    public bool RemoveEquipment(int id)
    {
        Equipment? equipment = GetEquipmentById(id);

        if (equipment is null)
        {
            return false;
        }

        _equipment.Remove(equipment);
        return true;
    }
}