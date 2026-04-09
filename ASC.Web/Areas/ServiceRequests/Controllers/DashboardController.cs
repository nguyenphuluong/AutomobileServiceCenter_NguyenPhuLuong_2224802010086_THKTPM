using ASC.Web.Configuration;
using ASC.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class DashboardController : BaseController
    {
        private readonly IOptions<ApplicationSettings> _settings;

        public DashboardController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public IActionResult Dashboard()
        {
            ViewBag.ApplicationTitle = _settings.Value.ApplicationTitle;
            return View();
        }
    }
}