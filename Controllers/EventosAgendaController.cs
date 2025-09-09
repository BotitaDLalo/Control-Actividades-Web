using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ControlActividades.Controllers
{
    public class EventosAgendaController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        public EventosAgendaController()
        {
        }

        public EventosAgendaController(ApplicationUserManager userManager, 
            ApplicationSignInManager signInManager, 
            RoleManager<IdentityRole> roleManager, 
            ApplicationDbContext DbContext, 
            FuncionalidadesGenerales fg)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Fg = fg;
        }


        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<RoleManager<IdentityRole>>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        public ApplicationDbContext Db
        {
            get
            {
                return _db ?? (_db = new ApplicationDbContext());
            }
            private set
            {
                _db = value;
            }
        }
        public FuncionalidadesGenerales Fg
        {
            get
            {
                return _fg ?? (_fg = new FuncionalidadesGenerales());
            }
            set
            {
                _fg = value;
            }
        }


        [HttpPost]
        public async Task<ActionResult> CrearEvento(tbEventosAgenda evento)
        {
            if (evento == null)
            {
                return new HttpStatusCodeResult(400, "Datos inválidos.");
            }

            if (evento.Color != "azul" && evento.Color != "gris")
            {
                return new HttpStatusCodeResult(400, "Solo se permiten los colores azul y gris.");
            }

            Db.tbEventosAgenda.Add(evento);
            await Db.SaveChangesAsync();

            return Json(new { mensaje = "Evento guardado exitosamente" }, JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult ObtenerEventosPorFecha(string fecha)
        {
            if (!DateTime.TryParse(fecha, out DateTime fechaSeleccionada))
            {
                return new HttpStatusCodeResult(400, "Fecha inválida.");
            }

            var eventos = Db.tbEventosAgenda
                .Where(e => DbFunctions.TruncateTime(e.FechaInicio) == fechaSeleccionada.Date)
                .Select(e => new
                {
                    eventoId = e.EventoId,
                    titulo = e.Titulo,
                    descripcion = e.Descripcion,
                    fechaInicio = e.FechaInicio,
                    fechaFinal = e.FechaFinal,
                    color = e.Color
                })
                .ToList();

            if (!eventos.Any())
            {
                return Json(new { mensaje = "No hay eventos para esta fecha." }, JsonRequestBehavior.AllowGet);
            }

            return Json(eventos, JsonRequestBehavior.AllowGet);
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }

                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }

                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
