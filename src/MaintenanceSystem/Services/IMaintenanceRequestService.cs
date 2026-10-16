using MaintenanceSystem.Models;

namespace MaintenanceSystem.Services;

public interface IMaintenanceRequestService
{
    MaintenanceRequest CreateRequest(int equipmentId, string description);

    IReadOnlyList<MaintenanceRequest> GetAllRequests();

    IReadOnlyList<MaintenanceRequest> GetRequestsByEquipmentId(int equipmentId);

    MaintenanceRequest? GetRequestById(int requestId);

    bool UpdateRequestStatus(int requestId, MaintenanceRequestStatus status);
}