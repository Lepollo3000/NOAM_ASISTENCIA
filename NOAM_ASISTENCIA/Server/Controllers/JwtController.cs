using Microsoft.AspNetCore.Mvc;

namespace NOAM_ASISTENCIA.Server.Controllers
{
    public class JwtController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
