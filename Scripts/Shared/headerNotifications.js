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

        if (panel.style.display === "flex") {
            panel.style.display = "none";
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
        panel.setAttribute('aria-hidden', 'false');
    });

    //cerrar cuando se haga clic fuera
    document.addEventListener("click", function (event) {
        if (!icono.contains(event.target) && !panel.contains(event.target)) {
            panel.style.display = "none"; 
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
    const lista = panel.querySelector(".lista-notificaciones");
    const mensajeVacio = panel.querySelector(".text-muted");

    if (!panel || !lista) return;

    // Si no hay notificaciones
    if (!notificaciones || notificaciones.length === 0) {
        lista.innerHTML = "";
        mensajeVacio.style.display = "block";
        return;
    }

    mensajeVacio.style.display = "none";

    let html = "";

    notificaciones.forEach(n => {
        html += `
            <div class="noti-item p-2 border-bottom">
                <div><strong>${n.Title}</strong></div>
                <div class="small text-muted">${n.Body}</div>
                <div class="small text-secondary">${new Date(n.FechaRecibido).toLocaleString()}</div>
            </div>
        `;
    });

    lista.innerHTML = html;
}
//Inicializar signalr 
function initSignalRNotifications() {
    
    const hub = $.connection.notificacionesHub;
    
    // Método que se ejecuta cuando el servidor envía una nueva notificación
    hub.client.nuevaNotificacion = function (notificacion) {
        // Manejar la nueva notificación recibida
        console.log("Nueva notificación recibida: ", notificacion);

        // actualizar el panel de notificaciones si está abierto
        mostrarIndicadorNotificaciones();
        insertarNotificacionEnPanel(notificacion); //insertar si está abierto
    };

    //Iniciamos la conexión
    $.connection.hub.start()
        .done(function () {
            //console.log("Conectado al hub de notificaciones");
            console.log("SignalR conectado. UserId: ", $.connection.hub.id);
        })
        .fail(function (error) {
            console.error("Error al conectar al hub de notificaciones: ", error);
        });
}

// Asegurar que se ejecute cuando la página cargue
document.addEventListener("DOMContentLoaded", initSignalRNotifications);


// Punto rojo que indica nuevas notificaciones
function mostrarIndicadorNotificaciones() {
    const indicador = document.getElementById("notificaciones-indicador");
    if (indicador) indicador.style.display = "block";
}

function ocultarIndicadorNotificaciones() {
    const indicador = document.getElementById("notificaciones-indicador");
    if (indicador) indicador.style.display = "none";
}

//Insertar notificación en el panel tiempo real
function insertarNotificacionEnPanel(notificacion) {
    const panel = document.getElementById("notificaciones-panel");
    if (!panel || panel.style.display === "none") return; //si el panel está cerrado insertamos la notificación

    const listaNoti = panel.querySelector(".lista-notificaciones");
    if (!listaNoti) return; 

    //Insertamos
    const html = `
        <div class="noti-item p-2 border-bottom">
            <div><strong>${notificacion.Title}</strong></div>
            <div class="small text-muted">${notificacion.Body}</div>
            <div class="small text-secondary">${new Date(notificacion.FechaRecibido).toLocaleString()}</div>
        </div>
    `;

    listaNoti.insertAdjacentHTML('afterbegin', html);

}


