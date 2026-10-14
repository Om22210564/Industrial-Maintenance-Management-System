using MaintenanceSystem.Models;
using MaintenanceSystem.Services;

EquipmentService service = new EquipmentService();

Equipment cnc = new Equipment(
    1, "CNC Machine", "CNC-001",
    "Factory Floor A", EquipmentStatus.Operational);

Equipment pump = new Equipment(
    2, "Water Pump", "PUMP-001",
    "Factory Floor B", EquipmentStatus.UnderMaintenance);

Equipment conveyor = new Equipment(
    3, "Conveyor Belt", "CONV-001",
    "Factory Floor C", EquipmentStatus.Operational);

// Add equipment
service.AddEquipment(cnc);
service.AddEquipment(pump);
service.AddEquipment(conveyor);

Console.WriteLine("All Equipment");
Console.WriteLine("-------------");

foreach (Equipment item in service.GetAllEquipment())
{
    Console.WriteLine(
        $"{item.Id} | {item.Name} | {item.EquipmentCode} | {item.Status}");
}

// Search for existing equipment
Console.WriteLine("\nSearch for ID 2:");

Equipment? found = service.GetEquipmentById(2);

if (found is not null)
{
    Console.WriteLine($"Found: {found.Name}");
}

// Search for nonexistent equipment
Console.WriteLine("\nSearch for ID 99:");

Equipment? missing = service.GetEquipmentById(99);

if (missing is null)
{
    Console.WriteLine("No equipment found.");
}

// Try adding a duplicate ID
Console.WriteLine("\nTry adding duplicate ID:");

try
{
    Equipment duplicate = new Equipment(
        1, "Another Machine", "OTHER-001",
        "Factory Floor D", EquipmentStatus.Operational);

    service.AddEquipment(duplicate);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Rejected: {ex.Message}");
}

// Remove existing equipment
Console.WriteLine("\nRemove ID 2:");
Console.WriteLine(service.RemoveEquipment(2)
    ? "Equipment removed."
    : "Equipment not found.");

// Remove nonexistent equipment
Console.WriteLine("\nRemove ID 99:");
Console.WriteLine(service.RemoveEquipment(99)
    ? "Equipment removed."
    : "Equipment not found.");

Console.WriteLine($"\nRemaining equipment: {service.GetAllEquipment().Count}");