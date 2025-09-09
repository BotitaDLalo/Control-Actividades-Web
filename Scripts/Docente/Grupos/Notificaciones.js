document.addEventListener("DOMContentLoaded", function () {
    const icono = document.getElementById("notificaciones-icono");
    const panel = document.getElementById("notificaciones-panel");

    icono.addEventListener("click", function (event) {
        event.preventDefault();
        panel.classList.toggle("mostrar");
    });

    document.addEventListener("click", function (event) {
        if (!icono.contains(event.target) && !panel.contains(event.target)) {
            panel.classList.remove("mostrar");
        }
    });
});


