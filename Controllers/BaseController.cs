using ControlActividades.Filters;
using System;
using System.Web;
using System.Web.Mvc;

namespace ControlActividades.Controllers
{
    [Authorize]
    [NoCache]
    public abstract class BaseController : Controller
    {
    }
}