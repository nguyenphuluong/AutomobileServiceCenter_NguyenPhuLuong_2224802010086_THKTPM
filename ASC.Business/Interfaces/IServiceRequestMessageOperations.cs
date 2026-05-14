using ASC.Model.Models;

namespace ASC.Business.Interfaces
{
    public interface IServiceRequestMessageOperations
    {
        Task CreateMessageAsync(ServiceRequestMessage message);

        Task<List<ServiceRequestMessage>> GetMessagesAsync(
            string serviceRequestId);
    }
}