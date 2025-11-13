using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControlActividades.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        // Acción para mostrar la vista Calendario (se añadió porque existe Views/Home/Calendario.cshtml)
        public ActionResult Calendario()
        {
            return View();
        }

    }
}