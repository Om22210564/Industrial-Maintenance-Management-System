using MaintenanceSystem.Models;

namespace MaintenanceSystem.Services;

public interface IEquipmentService
{
    void AddEquipment(Equipment equipment);

    IReadOnlyList<Equipment> GetAllEquipment();

    Equipment? GetEquipmentById(int id);

    bool RemoveEquipment(int id);
}