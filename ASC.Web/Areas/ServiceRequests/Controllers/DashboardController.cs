using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Web.Areas.ServiceRequests.Models;
using ASC.Web.Controllers;
using ASC.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    [Authorize(Roles = "Admin,Engineer,User")]
    public class DashboardController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMasterDataCacheOperations _masterData;

        public DashboardController(
            IServiceRequestOperations serviceRequestOperations,
            IMasterDataCacheOperations masterData)
        {
            _serviceRequestOperations = serviceRequestOperations;
            _masterData = masterData;
        }

        public async Task<IActionResult> Dashboard()
        {
            var status = new List<string>
            {
                "New",
                "InProgress",
                "Initiated",
                "RequestForInformation"
            };

            var userEmail =
                User.FindFirstValue(ClaimTypes.Email)
                ?? User.Identity?.Name
                ?? "";

            List<ServiceRequest> serviceRequests;

            if (User.IsInRole("Admin"))
            {
                serviceRequests = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(
                        DateTime.UtcNow.AddDays(-7),
                        status);
            }
            else if (User.IsInRole("Engineer"))
            {
                serviceRequests = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(
                        DateTime.UtcNow.AddDays(-7),
                        status,
                        serviceEngineerEmail: userEmail);
            }
            else
            {
                serviceRequests = await _serviceRequestOperations
                    .GetServiceRequestsByRequestedDateAndStatus(
                        DateTime.UtcNow.AddYears(-1),
                        status,
                        email: userEmail);
            }

            var model = new DashboardViewModel
            {
                ServiceRequests = serviceRequests
                    .OrderByDescending(p => p.RequestedDate)
                    .ToList()
            };

            return View(model);
        }
    }
}