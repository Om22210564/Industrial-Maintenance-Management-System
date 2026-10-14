namespace MaintenanceSystem.Models;

//constructor
public class Equipment
{
    public int Id {get;set;}// allows a property to be read and modified.
    public string? Name {get;private set;}// allows the property to be read from outside the class but modified only inside the class.
    public string? EquipmentCode {get;private set;}
    public string? Location {get;set;}
    public EquipmentStatus Status{get;set;}

    public Equipment(int id, string name, string equipmentcode,string location ,EquipmentStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Equipment name can't be empty", nameof(name));
        }
        if (string.IsNullOrWhiteSpace(equipmentcode))
        {
            throw new ArgumentException("Equipment code can't be empty", nameof(equipmentcode));
        }
        Id = id;
        Name = name;
        EquipmentCode = equipmentcode;
        Location = location;
        Status = status;
        
    }

}

