document.addEventListener("DOMContentLoaded", function () {
    const icono = document.getElementById("notificaciones-icono");
    const panel = document.getElementById("notificaciones-panel");

    if (!icono || !panel) return;

    icono.addEventListener("click", function (event) {
        event.preventDefault();
        panel.classList.toggle("mostrar");
        panel.setAttribute('aria-hidden', panel.classList.contains('mostrar') ? 'false' : 'true');
    });

    document.addEventListener("click", function (event) {
        if (!icono.contains(event.target) && !panel.contains(event.target)) {
            panel.classList.remove("mostrar");
            panel.setAttribute('aria-hidden', 'true');
        }
    });
});