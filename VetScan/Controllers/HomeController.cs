using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VetScan.Models;

namespace VetScan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Verificar si existe la variable de sesi�n UserId
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                // Si no est� logueado, redirigir al login
                return RedirectToAction("Login", "AppUsers");
            }
            // Si est� logueado, mostrar la vista normal
            return View();
        }

        // Esta acci�n ser�a accesible sin login
        public IActionResult PublicPage() => View();

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
