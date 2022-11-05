using Microsoft.AspNetCore.Mvc;

namespace StepOrg.Controllers
{
    public class UserSettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
