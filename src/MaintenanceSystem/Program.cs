using MaintenanceSystem.Models;

Equipment equipment = new Equipment(
    id: 1,
    name: "CNC Machine",
    equipmentcode: "CNC-001",
    location: "Factory Floor A",
    status: EquipmentStatus.Operational
);

Console.WriteLine("Equipment Details");
Console.WriteLine("-----------------");
Console.WriteLine($"ID: {equipment.Id}");
Console.WriteLine($"Name: {equipment.Name}");
Console.WriteLine($"Equipment Code: {equipment.EquipmentCode}");
Console.WriteLine($"Location: {equipment.Location}");
Console.WriteLine($"Status: {equipment.Status}");