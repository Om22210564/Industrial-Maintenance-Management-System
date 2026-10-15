using MaintenanceSystem.Data;
using MaintenanceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceSystem.Services;

public class MaintenanceRequestService
{
    private readonly MaintenanceDbContext _context;
    private readonly EquipmentService _equipmentService;

    public MaintenanceRequestService(
        MaintenanceDbContext context,
        EquipmentService equipmentService)
    {
        _context = context;
        _equipmentService = equipmentService;
    }

    public MaintenanceRequest CreateRequest(
        int equipmentId,
        string description)
    {
        Equipment? equipment =
            _equipmentService.GetEquipmentById(equipmentId);

        if (equipment is null)
        {
            throw new InvalidOperationException(
                $"Equipment with ID {equipmentId} does not exist.");
        }

        int nextRequestId = _context.MaintenanceRequests.Any()
            ? _context.MaintenanceRequests.Max(request => request.Id) + 1
            : 1;

        MaintenanceRequest request = new MaintenanceRequest(
            nextRequestId,
            equipmentId,
            description);

        _context.MaintenanceRequests.Add(request);
        _context.SaveChanges();

        return request;
    }

    public IReadOnlyList<MaintenanceRequest> GetAllRequests()
    {
        return _context.MaintenanceRequests
            .AsNoTracking()
            .OrderBy(request => request.Id)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<MaintenanceRequest>
        GetRequestsByEquipmentId(int equipmentId)
    {
        return _context.MaintenanceRequests
            .AsNoTracking()
            .Where(request => request.EquipmentId == equipmentId)
            .OrderBy(request => request.Id)
            .ToList()
            .AsReadOnly();
    }

    public MaintenanceRequest? GetRequestById(int requestId)
    {
        return _context.MaintenanceRequests
            .FirstOrDefault(request => request.Id == requestId);
    }

    public bool UpdateRequestStatus(
        int requestId,
        MaintenanceRequestStatus status)
    {
        MaintenanceRequest? request = GetRequestById(requestId);

        if (request is null)
        {
            return false;
        }

        request.Status = status;
        _context.SaveChanges();

        return true;
    }
}
