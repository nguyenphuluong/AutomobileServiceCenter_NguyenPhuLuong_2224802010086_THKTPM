using System.Globalization;
using System.Security.Claims;
using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Web.Areas.ServiceRequests.Models;
using ASC.Web.Controllers;
using ASC.Web.Data;
using ASC.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static ASC.Model.BaseTypes.Constants;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    [Authorize(Roles = "User,Admin,Engineer")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMasterDataCacheOperations _masterData;
        private readonly IHubContext<ServiceMessagesHub> _hubContext;

        public ServiceRequestController(
            IServiceRequestOperations serviceRequestOperations,
            IMasterDataCacheOperations masterData,
            IHubContext<ServiceMessagesHub> hubContext)
        {
            _serviceRequestOperations = serviceRequestOperations;
            _masterData = masterData;
            _hubContext = hubContext;
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
        public async Task<IActionResult> ServiceRequest(
            NewServiceRequestViewModel request)
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
                UpdatedBy = userEmail,
                CreatedDate = now,
                UpdatedDate = now,
                IsDeleted = false
            };

            await _serviceRequestOperations
                .CreateServiceRequestAsync(serviceRequest);

            return RedirectToAction(
                "Dashboard",
                "Dashboard",
                new { Area = "ServiceRequests" });
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var requests =
                await _serviceRequestOperations
                    .GetAllServiceRequestsAsync();

            var serviceRequest =
                requests.FirstOrDefault(x => x.RowKey == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return BadRequest();
            }

            var messages =
                await _serviceRequestOperations
                    .GetMessagesByRequestIdAsync(requestId);

            return Json(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(
            [FromBody] SendMessageModel model)
        {
            if (model == null ||
                string.IsNullOrWhiteSpace(model.RequestId) ||
                string.IsNullOrWhiteSpace(model.Message))
            {
                return BadRequest();
            }

            var userName =
                User.Identity?.Name
                ?? User.FindFirstValue(ClaimTypes.Email)
                ?? "unknown";

            var now = DateTime.Now;

            var newMessage = new ServiceRequestMessage
            {
                PartitionKey = model.RequestId,
                RowKey = Guid.NewGuid().ToString(),

                ServiceRequestId = model.RequestId,
                UserName = userName,
                Message = model.Message,

                CreatedAt = now,
                CreatedBy = userName,
                UpdatedBy = userName,
                CreatedDate = now,
                UpdatedDate = now,
                IsDeleted = false
            };

            await _serviceRequestOperations
                .AddMessageAsync(newMessage);

            await _hubContext
                .Clients
                .Group(model.RequestId)
                .SendAsync(
                    "ReceiveMessage",
                    userName,
                    model.Message);

            return Ok();
        }

        private async Task LoadVehicleMasterDataAsync()
        {
            var masterData =
                await _masterData.GetMasterDataCacheAsync();

            ViewBag.VehicleTypes =
                masterData.Values
                    .Where(p =>
                        p.PartitionKey ==
                        MasterKeys.VehicleType.ToString())
                    .ToList();

            ViewBag.VehicleNames =
                masterData.Values
                    .Where(p =>
                        p.PartitionKey ==
                        MasterKeys.VehicleName.ToString())
                    .ToList();
        }
    }

    public class SendMessageModel
    {
        public string RequestId { get; set; }

        public string Message { get; set; }
    }
}