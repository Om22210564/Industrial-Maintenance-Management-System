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


// Create the maintenance request service
MaintenanceRequestService requestService =
    new MaintenanceRequestService(service);

SparePartService sparePartService = new SparePartService();

// Create a request for the CNC machine
Console.WriteLine("\nCreate maintenance requests:");

MaintenanceRequest cncRequest = requestService.CreateRequest(
    cnc.Id,
    "Unusual vibration detected");

Console.WriteLine(
    $"Created request {cncRequest.Id} for {cnc.Name}");

// Create a request for the water pump
MaintenanceRequest pumpRequest = requestService.CreateRequest(
    pump.Id,
    "Water leakage");

Console.WriteLine(
    $"Created request {pumpRequest.Id} for {pump.Name}");

// Try creating a request for nonexistent equipment
Console.WriteLine("\nTry creating request for equipment ID 99:");

try
{
    requestService.CreateRequest(
        99,
        "Unknown equipment issue");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Rejected: {ex.Message}");
}

// List all maintenance requests
Console.WriteLine("\nAll Maintenance Requests");
Console.WriteLine("------------------------");

foreach (MaintenanceRequest request in
    requestService.GetAllRequests())
{
    Console.WriteLine(
        $"{request.Id} | Equipment {request.EquipmentId} | " +
        $"{request.Description} | {request.Status} | " +
        $"{request.CreatedAt}");
}

// Filter requests by equipment ID
Console.WriteLine("\nRequests for equipment ID 1:");

foreach (MaintenanceRequest request in
    requestService.GetRequestsByEquipmentId(1))
{
    Console.WriteLine(
        $"{request.Id} | {request.Description}");
}

// Change request status
Console.WriteLine("\nUpdate request status:");

bool updated = requestService.UpdateRequestStatus(
    cncRequest.Id,
    MaintenanceRequestStatus.InProgress);

Console.WriteLine(updated
    ? $"Request {cncRequest.Id} updated to InProgress."
    : "Request not found.");

// Look up an unknown request
Console.WriteLine("\nSearch for request ID 99:");

MaintenanceRequest? missingRequest =
    requestService.GetRequestById(99);

Console.WriteLine(missingRequest is null
    ? "Request not found."
    : $"Found: {missingRequest.Description}");


// Create the work order service
WorkOrderService workOrderService =
    new WorkOrderService(requestService,sparePartService);

// Create work orders for existing maintenance requests
Console.WriteLine("\nCreate work orders:");

WorkOrder cncWorkOrder = workOrderService.CreateWorkOrder(
    cncRequest.Id,
    "Inspect spindle and check bearing wear");

WorkOrder pumpWorkOrder = workOrderService.CreateWorkOrder(
    pumpRequest.Id,
    "Inspect seals and repair water leakage");

Console.WriteLine(
    $"Created work order {cncWorkOrder.Id}: {cncWorkOrder.Description}");

Console.WriteLine(
    $"Created work order {pumpWorkOrder.Id}: {pumpWorkOrder.Description}");

// Try creating a work order for an unknown request
Console.WriteLine("\nTry creating work order for request ID 99:");

try
{
    workOrderService.CreateWorkOrder(
        99,
        "Investigate unknown request");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Rejected: {ex.Message}");
}

// Assign a technician
Console.WriteLine("\nAssign technician:");

bool assigned = workOrderService.AssignTechnician(
    cncWorkOrder.Id,
    "John Smith");

Console.WriteLine(assigned
    ? $"Assigned {cncWorkOrder.AssignedTo} to work order {cncWorkOrder.Id}."
    : "Work order not found.");

// Start the work
Console.WriteLine("\nStart work order:");

bool started = workOrderService.UpdateWorkOrderStatus(
    cncWorkOrder.Id,
    WorkOrderStatus.InProgress);

Console.WriteLine(started
    ? $"Work order {cncWorkOrder.Id} is now InProgress."
    : "Work order not found.");

// Register spare parts
sparePartService.AddSparePart(new SparePart(
    1, "BRG-001", "Ball Bearing",
    5, 2, 12.50m));

// Issue two bearings to the CNC work order
WorkOrderPart usage = workOrderService.IssuePartToWorkOrder(
    cncWorkOrder.Id,
    1,
    2);

Console.WriteLine(
    $"Issued {usage.QuantityUsed} bearings to work order " +
    $"{usage.WorkOrderId}.");

Console.WriteLine(
    $"Remaining stock: " +
    $"{sparePartService.GetSparePartById(1)!.QuantityInStock}");

    
// List all work orders
Console.WriteLine("\nAll Work Orders");
Console.WriteLine("---------------");

foreach (WorkOrder workOrder in workOrderService.GetAllWorkOrders())
{
    Console.WriteLine(
        $"{workOrder.Id} | Request {workOrder.MaintenanceRequestId} | " +
        $"{workOrder.Description} | {workOrder.Status} | " +
        $"{workOrder.AssignedTo ?? "Unassigned"}");
}

// Filter by status
Console.WriteLine("\nOpen Work Orders:");

foreach (WorkOrder workOrder in
    workOrderService.GetWorkOrdersByStatus(WorkOrderStatus.Open))
{
    Console.WriteLine(
        $"{workOrder.Id} | {workOrder.Description}");
}

// Complete the first work order
Console.WriteLine("\nComplete work order:");

bool completed = workOrderService.UpdateWorkOrderStatus(
    cncWorkOrder.Id,
    WorkOrderStatus.Completed);

Console.WriteLine(completed
    ? $"Completed at: {cncWorkOrder.CompletedAt}"
    : "Work order not found.");

// Try reopening the completed work order
Console.WriteLine("\nTry reopening completed work order:");

try
{
    workOrderService.UpdateWorkOrderStatus(
        cncWorkOrder.Id,
        WorkOrderStatus.Open);
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