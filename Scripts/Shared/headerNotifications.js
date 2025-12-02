//Abrir panel de notificaciones en el header y obtenerlas del endpoint
function initHeaderNotifications() {
    const icono = document.getElementById("notificaciones-icono");
    const panel = document.getElementById("notificaciones-panel");

    if (!icono || !panel) return;

    // avoid attaching handlers multiple times
    if (icono.dataset.notificationsInited === '1') return;
    icono.dataset.notificationsInited = '1';

    icono.addEventListener("click", async function (event) {
        event.preventDefault();

        if (panel.classList.contains("display: flex")) {
            panel.setAttribute('aria-hidden', 'true');
            return;
        }

        try {
            const resp = await $.ajax({
                url: '/api/Notificaciones/ObtenerNotificaciones',
                method: 'GET',
                headers: {
                    "Accept": "application/json"
                }
            });

            renderizarNotificaciones(resp);

        } catch (e) {
            console.error("Error al cargar notificaciones: ", e);
        }
        panel.style.display = "flex";
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


//renderiza las notificaciones en el mini panel
function renderizarNotificaciones(notificaciones) {
    const panel = document.getElementById("notificaciones-panel");

    if (!panel) return;

    if (!notificaciones || notificaciones.length === 0) {
        panel.innerHTML = '<p>No hay notificaciones</p>';
        return;
    }

    let htmlNoti = `
        <div class="p-2"><strong>Notificaciones</strong></div>
        <div class="lista-notificaciones">
    `;

    notificaciones.forEach(n => {
        htmlNoti += `
            <div class="noti-item p-2 border-bottom">
                <div><strong>${n.Title}</strong></div>
                <div class="small text-muted">${n.Body}</div>
                <div class="small text-secondary">${new Date(n.FechaRecibido).toLocaleString()}</div>
            </div>
        `;
    });

    htmlNoti += `</div>`;

    panel.innerHTML = htmlNoti;
   
}