const PanelMateria = Object.freeze({
    Avisos: "AvisosPartialView",
    Actividades: "ActividadesPartialView",
    Entregables: "EntregablesPartialView",
    Alumnos: "AlumnosPartialView",
    Configuracion: "ConfiguracionPartialView"
});
function cambiarPanel(btn) {
    // quitar active a todos
    document.querySelectorAll(".tab-button")
        .forEach(b => b.classList.remove("active"));


    // activar el actual
    btn.classList.add("active");

    // obtener panel desde enum
    const panelKey = btn.dataset.panel;
    const partial = PanelMateria[panelKey];


    // cargar partial view
    $("#contenedor-dinamico").load(
        `/Materias/${partial}`,
        { materiaId: window.materiaIdGlobal },
        function () {
            if (panelKey === "Avisos") {
                cargarAvisosDeMateria(); 
            }
        }
    );
}
