namespace MaintenanceSystem.Models;

public class MaintenanceRequest
{
    public int Id { get; private set; }

    public int EquipmentId { get; private set; }

    public string Description { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public MaintenanceRequestStatus Status { get; set; }

    public MaintenanceRequest(
        int id,
        int equipmentId,
        string description)
    {
        if (id <= 0)
        {
            throw new ArgumentException(
                "Request ID must be greater than zero.",
                nameof(id));
        }

        if (equipmentId <= 0)
        {
            throw new ArgumentException(
                "Equipment ID must be greater than zero.",
                nameof(equipmentId));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Description cannot be empty.",
                nameof(description));
        }

        Id = id;
        EquipmentId = equipmentId;
        Description = description;
        CreatedAt = DateTime.Now;
        Status = MaintenanceRequestStatus.Open;
    }
}