using Microsoft.AspNetCore.Mvc;

namespace EOS.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult About()
        {
            return View();
        }
    }
}
