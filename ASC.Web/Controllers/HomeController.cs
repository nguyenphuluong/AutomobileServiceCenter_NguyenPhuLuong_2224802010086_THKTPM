using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;

namespace ASC.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationSettings _appSettings;

        public HomeController(IOptions<ApplicationSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public IActionResult Index()
        {
            ViewBag.ApplicationTitle = _appSettings.ApplicationTitle;
            return View();
        }

        public IActionResult Dashboard()
        {
            ViewBag.ApplicationTitle = _appSettings.ApplicationTitle;
            return View();
        }
    }
}