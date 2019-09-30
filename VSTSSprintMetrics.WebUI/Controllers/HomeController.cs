using Microsoft.AspNetCore.Mvc;

namespace VSTSSprintMetrics.WebUI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
