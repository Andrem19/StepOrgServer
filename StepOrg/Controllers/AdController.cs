using Microsoft.AspNetCore.Mvc;

namespace StepOrg.Controllers
{
    public class AdController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
