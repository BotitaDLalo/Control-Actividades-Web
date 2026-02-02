using System.Web.Mvc;

namespace ControlActividades.Controllers
{
    // Shim controller to satisfy legacy requests to '/datatables'.
    // Returns a tiny JavaScript snippet that no-ops DataTables calls when the real library is not present.
    public class DatatablesController : Controller
    {
        // GET: /datatables
        public ActionResult Index()
        {
            const string js = "(function(){try{if(window.jQuery){if(!$.fn.dataTable){$.fn.dataTable=function(){return this;}}if(!window.DataTable){window.DataTable=function(){};} }}catch(e){} })();";
            return JavaScript(js);
        }
    }
}
