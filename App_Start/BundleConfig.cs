using System;
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
                      "~/Scripts/bootstrap.bundle.min.js"));


            // Keep only global site CSS in the main bundle. Dashboard-specific CSS should not be bundled globally to avoid layout conflicts (e.g. ".layout" rule).
            /*************** - STYLES - ***************************/
            //ESTILOS GENERALES - USAR EN TODOS LOS LAYOUTS

            bundles.Add(new StyleBundle("~/Content/Variables").Include(
                    "~/Content/Variables/colors.css",
                    "~/Content/Variables/fuentes.css",
                    "~/Content/Variables/modo-claro.css"
            ));

            bundles.Add(new StyleBundle("~/Content/Moleculas").Include(
                    "~/Content/Moleculas/botones.css",
                    "~/Content/Moleculas/inputs.css"
            ));

            bundles.Add(new StyleBundle("~/Content/Componentes").Include(
                    "~/Content/Componentes/cards.css",
                    "~/Content/Componentes/modal.css",
                    "~/Content/Componentes/sweetAlertCustom.css",
                    "~/Content/Componentes/carga.css"
            ));
            //Content/site
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"
            ));

            bundles.Add(new StyleBundle("~/Content/Dashboard/css").Include(
                    "~/Content/Dashboard/cards.css",
                    "~/Content/Dashboard/content.css",
                    "~/Content/Dashboard/header.css",
                    "~/Content/Dashboard/sidebar.css"
            ));


            /*************** - SCRIPTS - ***************************/
            //SCRIPTS GENERALES - USAR EN TODOS LOS LAYOUTS
            bundles.Add(new ScriptBundle("~/bundles/carga")
                .Include("~/Scripts/Componentes/PantallaCarga.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/color")
                .Include("~/Scripts/Shared/modoColor.js")
            );

            bundles.Add(new Bundle("~/bundles/header")
                .Include("~/Scripts/Shared/headerNotifications.js",
                         "~/Scripts/sidebar.js")
            );


            /********************************************************/
            /*************** - DOCENTE - ***************************/
            /******************************************************/

            /*************** - STYLES DOCENTE - ***************************/
            bundles.Add(new StyleBundle("~/Content/Docente/css").Include(
                "~/Content/Docente/*.css"));


            /*************** - SCRIPTS DOCENTE - ***************************/
            //Calendario docente - Usar solo en la vista de calendario
            bundles.Add(new Bundle("~/bundles/calendario").Include(
                        "~/Scripts/Agenda/calendario.js",
                        "~/Scripts/Agenda/calendario-crear.js",
                        "~/Scripts/Agenda/calendario-detalles.js",
                        "~/Scripts/Agenda/calendario-editar.js"
            ));
            
            bundles.Add(new Bundle("~/bundles/iachat").Include(
                "~/Scripts/Docente/IA.js",
                "~/Scripts/Docente/IAChat.js"
                )
            );

            bundles.Add(new Bundle("~/bundles/iaactividades").Include(
                "~/Scripts/Docente/Grupos/ActividadIA.js",
                "~/Scripts/Docente/IA.js"
                )
            );

            //Usar en vista de materias (listado de materias y creación)
            bundles.Add(new Bundle("~/bundles/materiasCreacion").Include(
                "~/Scripts/Docente/Grupos/docente.js",
                "~/Scripts/Docente/Grupos/docenteMaterias.js",
                "~/Scripts/Docente/Grupos/docenteGrupos.js"
                )
            );

            //Usar solo en la vista de detalles de materia (avisos, actividades, etc).
            bundles.Add(new Bundle("~/bundles/materiasDetalles").Include(
                "~/Scripts/Materias/paneles.js",
                "~/Scripts/Docente/Grupos/scriptsAvisos.js",
                "~/Scripts/Docente/Grupos/DetalleActividad.js",
                "~/Scripts/Docente/Grupos/DetalleMateria.js",
                "~/Scripts/Docente/Grupos/DetalleMateriaIconos.js",
                "~/Scripts/Docente/Grupos/scriptsActividades.js",
                "~/Scripts/Docente/Grupos/scriptsAlumnos.js",
                "~/Scripts/Materias/configuracion.js",
                "~/Scripts/Materias/materia-detalles.js"
                )
            );


            bundles.Add(new Bundle("~/bundles/docenteGrupos").Include(
                "~/Scripts/Docente/Grupos/docenteGrupos.js",
                "~/Scripts/Docente/Grupos/docente.js",
                "~/Scripts/Grupos/grupos.js"
                )
            );


            // Use a plain Bundle here to avoid the default Microsoft Ajax minifier parsing ES6+ syntax which can throw NullReferenceException
            var docenteGruposBundle = new Bundle("~/bundles/docentegrupos").Include(
                "~/Scripts/Docente/Grupos/docente.js",
                "~/Scripts/Docente/Grupos/docenteGrupos.js",
                "~/Scripts/Docente/Grupos/docenteErrores.js",
                "~/Scripts/Docente/Grupos/PrincipalMG.js"

            );

            bundles.Add(docenteGruposBundle);


            /******************************************************/
            /*************** - ALUMNO - **************************/
            /****************************************************/

            /*************** - STYLES ALUMNO - **************************/
            bundles.Add(new StyleBundle("~/Content/Alumno").Include(
                "~/Content/Alumno/avisosSeccion.css",
                "~/Content/Alumno/Cards.css",
                "~/Content/Alumno/Clases.css",
                "~/Content/Alumno/Layout.css",
                "~/Content/Alumno/materias.css",
                "~/Content/Alumno/PaginasDi.css",
                "~/Content/Alumno/Vistamaterias.css",
                "~/Content/Alumno/DetalleActividadesEstilos.css",
                "~/Content/Materias/actividadesSeccion.css")
            );

            /*************** - SCRIPTS ALUMNO - **************************/
            bundles.Add(new Bundle("~/bundles/alumno").Include(
                 "~/Scripts/Alumno/alumno.js",
                 "~/Scripts/Alumno/Avisos.js",
                 "~/Scripts/Alumno/Clases.js",
                 "~/Scripts/Alumno/layout.js",
                 "~/Scripts/Alumno/materias.js",
                 "~/Scripts/Alumno/UnirseClase.js",
                 "~/Scripts/Alumno/VentanasDi.js",
                 "~/Scripts/Alumno/Vistamaterias.js",
                 "~/Scripts/Componentes/componenteAvisos.js")
            );

            bundles.Add(new Bundle("~/bundles/alumno").Include(
                "~/Scripts/Alumno/materias.js"

                )
            );

            
            bundles.Add(new StyleBundle("~/Content/Materia-Detalles/css").Include(
                "~/Content/Materias/*.css"));

            // Disable optimizations in development to load files directly
            BundleTable.EnableOptimizations = false;
        }

        // Public version token for cache-busting in views (changes on app restart)
        public static readonly string Version = DateTime.UtcNow.Ticks.ToString();
    }
}
