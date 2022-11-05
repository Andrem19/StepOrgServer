using Microsoft.AspNetCore.Mvc;

namespace StepOrg.Controllers
{
    public class GroupController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
