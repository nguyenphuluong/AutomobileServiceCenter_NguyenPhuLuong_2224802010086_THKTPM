using ASC.Model.Models;
using ASC.Utilities;
using System.Linq.Expressions;

namespace ASC.Model.Queries
{
    public static class Queries
    {
        public static Expression<Func<ServiceRequest, bool>> GetDashboardQuery(
            DateTime? requestedDate,
            List<string> status,
            string email = "",
            string serviceEngineerEmail = "")
        {
            var query = PredicateBuilder.True<ServiceRequest>();

            if (requestedDate.HasValue)
            {
                var fromDate = requestedDate.Value.Date;

                query = query.And(u =>
                    u.RequestedDate.HasValue &&
                    u.RequestedDate.Value.Date >= fromDate);
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.And(u => u.PartitionKey == email);
            }

            if (!string.IsNullOrWhiteSpace(serviceEngineerEmail))
            {
                query = query.And(u => u.ServiceEngineer == serviceEngineerEmail);
            }

            if (status != null && status.Any())
            {
                var statusQuery = PredicateBuilder.False<ServiceRequest>();

                foreach (var item in status)
                {
                    var currentStatus = item;
                    statusQuery = statusQuery.Or(u => u.Status == currentStatus);
                }

                query = query.And(statusQuery);
            }

            query = query.And(u => u.IsDeleted == false);

            return query;
        }
    }
}