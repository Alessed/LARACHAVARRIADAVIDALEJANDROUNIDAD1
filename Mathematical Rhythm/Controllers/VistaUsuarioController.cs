using Microsoft.AspNetCore.Mvc;

namespace Mathematical_Rhythm.Controllers
{
    public class VistaUsuarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Contacto()
        {
            return View();
        }
        public IActionResult Ayuda()
        {
            return View();
        }
        public IActionResult Buzon()
        {
            return View();
        }
    }
}
