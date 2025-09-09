using System.Web;
using System.Web.Optimization;

namespace ControlActividades
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información sobre los formularios.  De esta manera estará
            // para la producción, use la herramienta de compilación disponible en https://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            //bundles.Add(new ScriptBundle("~/bundles/docente")
            //        .IncludeDirectory("~/Scripts/Docente", "*.js")
            //        .IncludeDirectory("~/Scripts/Docente/Grupos", "*.js"));

            bundles.Add(new ScriptBundle("~/bundles/docente")
                .Include("~/Scripts/Docente/*.js")
                );

            bundles.Add(new ScriptBundle("~/bundles/docentegrupos").Include(
                "~/Scripts/Docente/Grupos/ActividadIA.js",
                "~/Scripts/Docente/Grupos/Calendario.js",
                "~/Scripts/Docente/Grupos/DetalleActividad.js",
                "~/Scripts/Docente/Grupos/DetalleMateria.js",
                "~/Scripts/Docente/Grupos/DetalleMaterialIconos.js",
                "~/Scripts/Docente/Grupos/docente.js",
                "~/Scripts/Docente/Grupos/docenteErrores.js",
                "~/Scripts/Docente/Grupos/docenteGrupos.js",
                "~/Scripts/Docente/Grupos/docenteMaterias.js",
                "~/Scripts/Docente/Grupos/Notificaciones.js",
                "~/Scripts/Docente/Grupos/PrincipalMG.js",
                "~/Scripts/Docente/Grupos/scriptsActividades.js",
                "~/Scripts/Docente/Grupos/scriptsAlumnos.js",
                "~/Scripts/Docente/Grupos/scriptsAvisos.js",
                "~/Scripts/Docente/Grupos/VistaMateriasD.js"
            ));


            bundles.Add(new StyleBundle("~/Content/Docente/css").Include(
                "~/Content/Docente/*.css"));




            //bundles.Add(new ScriptBundle("~/bundles/alumno").Include(
            //    "~/Scripts/Alumno/*.js"));


            bundles.Add(new ScriptBundle("~/bundles/alumno").Include(
                 "~/Scripts/Alumno/alumno.js",
                 "~/Scripts/Alumno/Avisos.js",
                 "~/Scripts/Alumno/Calendario.js",
                 "~/Scripts/Alumno/Clases.js",
                 "~/Scripts/Alumno/layout.js",
                 "~/Scripts/Alumno/materias.js",
                 "~/Scripts/Alumno/Notificaciones.js",
                 "~/Scripts/Alumno/UnirseClase.js",
                 "~/Scripts/Alumno/VentanasDi.js",
                 "~/Scripts/Alumno/Vistamaterias.js"
            ));

            bundles.Add(new StyleBundle("~/Content/Alumno/css").Include(
                "~/Content/Alumno/*.css"));

        }
    }
}
