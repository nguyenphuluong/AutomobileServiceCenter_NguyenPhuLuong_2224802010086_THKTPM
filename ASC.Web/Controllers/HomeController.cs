using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;
using ASC.Utilities;

namespace ASC.Web.Controllers
{
    public class HomeController : AnonymousController
    {
        private readonly ApplicationSettings _appSettings;

        public HomeController(IOptions<ApplicationSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public IActionResult Index()
        {
            // Lưu settings vào Session
            HttpContext.Session.SetSession("Test", _appSettings);

            // Đọc lại từ Session
            var settings = HttpContext.Session.GetSession<ApplicationSettings>("Test");

            // Đưa dữ liệu ra ViewBag
            ViewBag.ApplicationTitle = settings.ApplicationTitle;

            return View();
        }

        public IActionResult Dashboard()
        {
            ViewBag.ApplicationTitle = _appSettings.ApplicationTitle;
            return View();
        }
    }
}