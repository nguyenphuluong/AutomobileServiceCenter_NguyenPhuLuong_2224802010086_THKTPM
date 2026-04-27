using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;
using ASC.Model.Queries;

namespace ASC.Business
{
    public class ServiceRequestOperations : IServiceRequestOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceRequestOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateServiceRequestAsync(ServiceRequest request)
        {
            await _unitOfWork.Repository<ServiceRequest>().AddAsync(request);
            _unitOfWork.CommitTransaction();
        }

        public ServiceRequest UpdateServiceRequest(ServiceRequest request)
        {
            _unitOfWork.Repository<ServiceRequest>().Update(request);
            _unitOfWork.CommitTransaction();

            return request;
        }

        public async Task<ServiceRequest> UpdateServiceRequestStatusAsync(
            string rowKey,
            string partitionKey,
            string status)
        {
            var serviceRequest = await _unitOfWork
                .Repository<ServiceRequest>()
                .FindAsync(partitionKey, rowKey);

            if (serviceRequest == null)
            {
                throw new NullReferenceException("Service request not found.");
            }

            serviceRequest.Status = status;
            serviceRequest.UpdatedDate = DateTime.Now;

            _unitOfWork.Repository<ServiceRequest>().Update(serviceRequest);
            _unitOfWork.CommitTransaction();

            return serviceRequest;
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsByRequestedDateAndStatus(
            DateTime? requestedDate,
            List<string> status = null,
            string email = "",
            string serviceEngineerEmail = "")
        {
            status ??= new List<string>();

            var query = Queries.GetDashboardQuery(
                requestedDate,
                status,
                email,
                serviceEngineerEmail);

            var serviceRequests = await _unitOfWork
                .Repository<ServiceRequest>()
                .FindAllByQuery(query);

            return serviceRequests.ToList();
        }
    }
}