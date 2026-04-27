using System.Globalization;
using System.Security.Claims;
using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Web.Areas.Configuration.Models;
using ASC.Web.Areas.ServiceRequests.Models;
using ASC.Web.Controllers;
using ASC.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static ASC.Model.BaseTypes.Constants;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    [Authorize(Roles = "User")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMasterDataCacheOperations _masterData;

        public ServiceRequestController(
            IServiceRequestOperations serviceRequestOperations,
            IMasterDataCacheOperations masterData)
        {
            _serviceRequestOperations = serviceRequestOperations;
            _masterData = masterData;
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            await LoadVehicleMasterDataAsync();

            return View(new NewServiceRequestViewModel
            {
                RequestedDate = DateTime.Now.ToString("dd/MM/yyyy")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel request)
        {
            if (!DateTime.TryParseExact(
                    request.RequestedDate,
                    new[] { "dd/MM/yyyy", "yyyy-MM-dd" },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime requestedDate))
            {
                ModelState.AddModelError(
                    nameof(request.RequestedDate),
                    "The Requested Date field is required.");
            }

            if (!ModelState.IsValid)
            {
                await LoadVehicleMasterDataAsync();
                return View(request);
            }

            var userEmail =
                User.FindFirstValue(ClaimTypes.Email)
                ?? User.Identity?.Name
                ?? "unknown";

            var now = DateTime.Now;

            var serviceRequest = new ServiceRequest
            {
                PartitionKey = userEmail,
                RowKey = Guid.NewGuid().ToString(),

                VehicleName = request.VehicleName ?? string.Empty,
                VehicleType = request.VehicleType ?? string.Empty,
                RequestedServices = request.RequestedServices ?? string.Empty,
                RequestedDate = requestedDate,

                Status = "New",
                ServiceEngineer = null,
                CompletedDate = null,

                CreatedBy = userEmail,
                CreatedDate = now,
                UpdatedBy = userEmail,
                UpdatedDate = now,
                IsDeleted = false
            };

            await _serviceRequestOperations.CreateServiceRequestAsync(serviceRequest);

            return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });
        }

        private async Task LoadVehicleMasterDataAsync()
        {
            var masterData = await _masterData.GetMasterDataCacheAsync();

            ViewBag.VehicleTypes = masterData.Values
                .Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString())
                .ToList();

            ViewBag.VehicleNames = masterData.Values
                .Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString())
                .ToList();
        }
    }
}