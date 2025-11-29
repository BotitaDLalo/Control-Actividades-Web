function initHeaderNotifications() {
    const icono = document.getElementById("notificaciones-icono");
    const panel = document.getElementById("notificaciones-panel");

    if (!icono || !panel) return;

    // avoid attaching handlers multiple times
    if (icono.dataset.notificationsInited === '1') return;
    icono.dataset.notificationsInited = '1';

    icono.addEventListener("click", function (event) {
        try {
            event.preventDefault();
        } catch (e) { }
        panel.classList.toggle("mostrar");
        panel.setAttribute('aria-hidden', panel.classList.contains('mostrar') ? 'false' : 'true');
    });

    document.addEventListener("click", function (event) {
        if (!icono.contains(event.target) && !panel.contains(event.target)) {
            panel.classList.remove("mostrar");
            panel.setAttribute('aria-hidden', 'true');
        }
    });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initHeaderNotifications);
} else {
    // DOM already ready
    initHeaderNotifications();
}
