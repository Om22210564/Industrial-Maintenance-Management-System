using MaintenanceSystem.Models;

namespace MaintenanceSystem.Services;

public interface ISparePartService
{
    void AddSparePart(SparePart sparePart);

    IReadOnlyList<SparePart> GetAllSpareParts();

    SparePart? GetSparePartById(int id);

    bool IssuePart(int partId, int quantity);

    bool RestockPart(int partId, int quantity);

    IReadOnlyList<SparePart> GetLowStockParts();
}