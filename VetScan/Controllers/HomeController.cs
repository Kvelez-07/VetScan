using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VetScan.Models;

namespace VetScan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) => _logger = logger;

        [HttpGet]
        public IActionResult Index()
        {
            // Verificar si existe la variable de sesión UserId
            if (HttpContext.Session.GetInt32("UserId") == null) return RedirectToAction("Login", "AppUsers");
            // Si está logueado, mostrar la vista normal
            return View();
        }

        [HttpGet]
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
