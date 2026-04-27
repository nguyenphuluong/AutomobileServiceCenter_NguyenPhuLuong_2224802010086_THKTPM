using ASC.Model.BaseTypes;

namespace ASC.Model.Models
{
    public class ServiceRequest : BaseEntity, IAuditTracker
    {
        public ServiceRequest()
        {
        }

        public ServiceRequest(string email)
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = email;
        }

        public string VehicleName { get; set; } = string.Empty;

        public string VehicleType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string RequestedServices { get; set; } = string.Empty;

        public DateTime? RequestedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public string? ServiceEngineer { get; set; }
    }
}