using Microsoft.AspNetCore.Mvc;

namespace Lyvads.API.Presentation.Controllers
{
    public class APostController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
