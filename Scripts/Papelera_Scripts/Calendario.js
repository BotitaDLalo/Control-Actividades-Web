/*document.addEventListener("DOMContentLoaded", function () {
    var icono = document.getElementById("calendario-icono");
    var panel = document.getElementById("calendario-panel");
    var input = document.getElementById("calendario-input");

    // Cargar idioma español
    flatpickr.localize(flatpickr.l10ns.es);

    // Inicializar Flatpickr con formato MX y en español
    flatpickr(input, {
        inline: true,
        dateFormat: "d/m/Y", // Formato MX: día/mes/año
        locale: "es", // Idioma español
        defaultDate: new Date(),
    });

    // Toggle para mostrar/ocultar el calendario
    icono.addEventListener("click", function (event) {
        event.preventDefault();
        panel.classList.toggle("mostrar");
    });

    // Cierra el panel si se hace clic fuera de él o del icono
    document.addEventListener("click", function (event) {
        if (!icono.contains(event.target) && !panel.contains(event.target)) {
            panel.classList.remove("mostrar");
        }
    });
});
*/