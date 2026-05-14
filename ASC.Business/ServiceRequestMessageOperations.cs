using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;

namespace ASC.Business
{
    public class ServiceRequestMessageOperations
        : IServiceRequestMessageOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceRequestMessageOperations(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateMessageAsync(
            ServiceRequestMessage message)
        {
            await _unitOfWork
                .Repository<ServiceRequestMessage>()
                .AddAsync(message);

            _unitOfWork.CommitTransaction();
        }

        public async Task<List<ServiceRequestMessage>> GetMessagesAsync(
            string serviceRequestId)
        {
            var data = await _unitOfWork
                .Repository<ServiceRequestMessage>()
                .FindAllAsync();

            return data
                .Where(x => x.ServiceRequestId == serviceRequestId)
                .OrderBy(x => x.CreatedAt)
                .ToList();
        }
    }
}