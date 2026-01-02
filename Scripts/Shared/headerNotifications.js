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
            icono.classList.remove("selected");
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
        icono.classList.add("selected");
        ocultarIndicadorNotificaciones();
    });

    //cerrar cuando se haga clic fuera
    document.addEventListener("click", function (event) {
        if (!icono.contains(event.target) && !panel.contains(event.target)) {
            panel.style.display = "none"; 
            panel.setAttribute('aria-hidden', 'true');
            icono.classList.remove("selected");
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
        const icono = obtenerIcono(n.TipoId);
        const encabezado = obtenerEncabezado(n.TipoId);
        html += `
            <div class="noti-item p-2 border-bottom" data-id="${n.NotificacionId}"
                                                     data-tipo="${n.TipoId}"
                                                     data-materia="${n.MateriaId ?? ''}"
                                                     data-grupo="${n.GrupoId ?? ''}">
                <div class="noti-left">
                    <div class="noti-icono">
                        <img src="${icono}" class="icono-svg" alt="noti-icono">
                    </div>
                </div>

                <div class="noti-contenido">
                    <div><strong>${encabezado}</strong></div>
                    <div class="small text-muted">${n.Title}</div>
                    <div class="small text-secondary">${new Date(n.FechaRecibido).toLocaleString()}</div>
                </div>    

                <div class="noti-opciones">
                    <button class="noti-menu btn-borrar-noti">x</button>
                </div>  
            </div>
        `;
    });

    lista.innerHTML = html;
}


// Asegurar que se ejecute cuando la página cargue
document.addEventListener("DOMContentLoaded", initSignalRNotifications);

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



// Punto rojo que indica nuevas notificaciones
function mostrarIndicadorNotificaciones() {
    const indicador = document.getElementById("notificaciones-indicador");
    if (indicador) indicador.style.display = "block";
}

function ocultarIndicadorNotificaciones() {
    const indicador = document.getElementById("notificaciones-indicador");
    if (indicador) indicador.style.display = "none";
}

function obtenerIcono(tipoId) {
    switch (tipoId) {
        case 1:
            return "/Content/Iconos/notiActividadCalificada.svg";

        case 2:
            return "/Content/Iconos/notiActividadCreada-Azul.svg";

        case 3:
            return "/Content/Iconos/notiActividadEntregada.svg";

        case 4:
            return "/Content/Iconos/notiAviso-Azul.svg";

        case 5:
            return "/Content/Iconos/notiEvento.svg";

        case 6:
            return "/Content/Iconos/notiGrupoAsinado.svg";

        case 7:
            return "/Content/Iconos/notiMateriaAsignada.svg";

        default:
            return "/Content/Iconos/NOTIFICACION-26.svg";
    }
}

function obtenerEncabezado(notificacion) {
    switch (notificacion) {
        case 1:
            return `Actividad calificada en ${notificacion.Materia || 'tu materia'}`;

        case 2:
            return `Nueva actividad en ${notificacion.Materia || 'tu materia'}`;

        case 3:
            return 'Alumno entregó tarea';

        case 4:
            return `Nuevo aviso en ${notificacion.Materia || 'tu materia'}`;

        case 5:
            return `Evento asignado en ${notificacion.Materia || 'tu materia'}`;

        case 6:
            return 'Te asignaron a un grupo';

        case 7:
            return 'Te asignaron a la materia ';

        default:
            return 'Nueva notificación';
    }
}


//Insertar notificación en el panel tiempo real
function insertarNotificacionEnPanel(notificacion) {
    const panel = document.getElementById("notificaciones-panel");
    if (!panel || panel.style.display === "none") return; //si el panel está cerrado insertamos la notificación

    const listaNoti = panel.querySelector(".lista-notificaciones");
    if (!listaNoti) return; 

    const icono = obtenerIcono(notificacion.Tipo);
    const encabezado = obtenerEncabezado(notificacion);

    //Insertamos
    const html = `
        <div class="noti-item p-2 border-bottom" data-id="${notificacion.NotificacionId}"
                                                 data-tipo="${notificacion.TipoId}"
                                                 data-materia="${notificacion.MateriaId ?? ''}"
                                                 data-grupo="${notificacion.GrupoId ?? ''}">
            <div class="noti-left">
                <div class="noti-icono">
                    <img src="${icono}" class="icono-svg" alt="icono-noti">
                </div>
            </div>
            <div class="noti-contenido">
                <div><strong>${encabezado}</strong></div>
                <div class="small text-muted">${notificacion.Title}</div>
                <div class="small text-secondary">${new Date(notificacion.FechaRecibido).toLocaleString()}</div>
            </div>

            <div class="noti-opciones">
                <button class="noti-menu btn-borrar-noti">x</button>
            </div>
        </div>
    `;

    listaNoti.insertAdjacentHTML('afterbegin', html);

    //Eliminar la primera en la cola
    const maxNotificaciones = 20; //Modificar también en NotificacionesService.cs
    const items = listaNoti.querySelectorAll(".noti-item");

    if (items.length > maxNotificaciones) {
        const eliminar = items[items.length - 1];
        if (eliminar) eliminar.remove();
    }

}

//Redirigir al hacer clic en la notificación
document.addEventListener("click", function (e) {
    const item = e.target.closest(".noti-item");

    if (!item) return;

    //Si se presiona el botón de borrar, no redirigir
    if (e.target.classList.contains("btn-borrar-noti")) return;

    try {
        //Obtener detalles de la notificación para redirigir
        const tipoId = parseInt(item.dataset.tipo);
        const materiaId = item.dataset.materia;
        const grupoId = item.dataset.grupo;
        redirigir(tipoId, materiaId, grupoId);
    }
    catch (err) {
        console.error("Error redirigiendo desde notificación", err);
    }
});

function redirigir(tipoId, materiaId, grupoId) {
    switch (tipoId) {

        //Actividad Calificada
        case 1:
            window.location.href = `/Alumno/Clase?tipo=materia&id=${materiaId}`;
            break;

        //Actividad creada
        case 2:
            window.location.href = `/Alumno/Clase?tipo=materia&id=${materiaId}`;
            break;
       
        //Actividad Entregada (NOTIFICACIÓN PARA DOCENTE)
        case 3:
            window.location.href = "/Actividades/MisCalificaciones";
            break;

        //Aviso
        case 4:
            window.location.href = `/Alumno/Clase?tipo=materia&id=${materiaId}`;
            break;

        //Evento
        case 5:
            window.location.href = "/EventosAgenda/CalendarioAlumnos";
            break;

        //Grupo asignado
        case 6:
            window.location.href = "/Actividades/MisCalificaciones";
            break;

        //Materia Asignada
        case 7:
            window.location.href = `/Alumno/Clase?tipo=materia&id=${materiaId}`;
            break;

        default:
            console.warn("Tipo de notificación desconocido para redirección");
            
    }
}

//Eliminar notificación
document.addEventListener("click", async function (e) {

    const btn = e.target.closest(".btn-borrar-noti");
    if (!btn) return;

    const notiItem = btn.closest(".noti-item");
    const notiId = notiItem?.dataset.id;

    if (!notiId) return;

    try {
        const resp = await fetch(`/api/Notificaciones/EliminarNotificacion/${notiId}`, {
            method: "DELETE"
        });

        if (resp.ok) {
            notiItem.remove();
        } else {
            console.error("No se pudo eliminar la notificación");
        }
    } catch (err) {
        console.error("Error eliminando notificación", err);
    }

});