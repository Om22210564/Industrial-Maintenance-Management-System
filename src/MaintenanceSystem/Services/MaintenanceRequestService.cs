using MaintenanceSystem.Models;

namespace MaintenanceSystem.Services;

public class MaintenanceRequestService
{
    private readonly List<MaintenanceRequest> _requests = new();
    private readonly EquipmentService _equipmentService;
    //This field stores a reference to the existing equipment service. The constructor receives that service:


    private int _nextRequestId = 1;

    public MaintenanceRequestService(
        EquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }
    //This means the request service doesn't create another equipment list. Both services work with the same equipment service instance when we pass it in from Program.cs.
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

        MaintenanceRequest request = new MaintenanceRequest(
            _nextRequestId,
            equipmentId,
            description);

        _requests.Add(request);
        _nextRequestId++;

        return request;
    }

    public IReadOnlyList<MaintenanceRequest> GetAllRequests()
    {
        return _requests.AsReadOnly();
    }

    public IReadOnlyList<MaintenanceRequest>
        GetRequestsByEquipmentId(int equipmentId)
    {
        return _requests
            .Where(request => request.EquipmentId == equipmentId)
            .ToList()
            .AsReadOnly();
    }

    public MaintenanceRequest? GetRequestById(int requestId)
    {
        return _requests.FirstOrDefault(
            request => request.Id == requestId);
    }

    public bool UpdateRequestStatus(
        int requestId,
        MaintenanceRequestStatus status)
    {
        MaintenanceRequest? request =
            GetRequestById(requestId);

        if (request is null)
        {
            return false;
        }

        request.Status = status;
        return true;
    }
}