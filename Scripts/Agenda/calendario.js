let fechaSeleccionadaGlobal = null;
// Prevent duplicate initialization
(function () {

    if (window.__agendaFullCalendarInitialized) {
        console.warn("AgendaFullCalendar already initialized");
        return;
    }
    window.__agendaFullCalendarInitialized = true;

    document.addEventListener("DOMContentLoaded", function () {

        console.log("Inicializando FullCalendar (docente)");

        const calendarEl = document.getElementById("calendar");
        if (!calendarEl) {
            console.error("No se encontró el contenedor #calendar");
            return;
        }

        const calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: "dayGridMonth",
            locale: "es",
            dateClick: function (info) {
                console.log("Fecha clickeada:", info.dateStr);

                // Guardamos la fecha seleccionada
                fechaSeleccionadaGlobal = info.dateStr;
                // Mostrar la fecha en el título del modal
                document.getElementById("fechaSeleccionadaTexto").textContent = info.dateStr;
                abrirModalEventosDia(info.dateStr);

            },
            height: "auto",
            eventDisplay: "block",

            views: {
                dayGridMonth: {
                    dayMaxEvents: 3,
                    dayMaxEventRows: true
                }
            },

            // Cargar calendario con eventos
            events: async function (fetchInfo, successCallback, failureCallback) {
                try {
                    const res = await fetch("/EventosAgenda/ObtenerEventosDocente");
                    const data = await res.json();

                    if (!Array.isArray(data)) {
                        console.warn("Respuesta de eventos no es un arreglo");
                        successCallback([]);
                        return;
                    }

                    const eventos = data.map(e => {
                        const inicio = convertirFechaNetAInput(e.fechaInicio);
                        const final = ajustarFechaFin(
                            convertirFechaNetAInput(e.fechaFinal)
                        );

                        return {
                            id: e.eventoId,
                            title: e.titulo,
                            start: inicio,
                            end: final,
                            allDay: true,
                            color: e.color === "azul" ? "#007bff" : "#6c757d",
                            borderColor: "transparent"
                        };
                    });

                    successCallback(eventos);

                } catch (err) {
                    console.error("Error cargando eventos del docente:", err);
                    failureCallback(err);
                }
            }
        });

        calendar.render();

        /*Obtener el modal y la lista de eventos del DOM*/
        const modalEventoEl = document.getElementById("modalEvento");
        const modalEvento = bootstrap.Modal.getOrCreateInstance(modalEventoEl);

        const listaEventos = document.getElementById("listaEventos");
        const textoFecha = document.getElementById("fechaSeleccionadaTexto");

        modalEventoEl.addEventListener("hidden.bs.modal", () => {
            listaEventos.innerHTML = "";
        });

        function abrirModalEventosDia(fecha) {
            textoFecha.textContent = fecha;
            listaEventos.innerHTML = `<p class="text-muted">Cargando eventos...</p>`;

            modalEvento.show();
            cargarEventosDocentePorFecha(fecha);
        }

        async function cargarEventosDocentePorFecha(fecha) {
            try {
                const resp = await fetch(`/EventosAgenda/ObtenerEventosPorFecha?fecha=${fecha}`);
                const data = await resp.json();

                listaEventos.innerHTML = "";

                // Revisar si es un arreglo vacío o contiene mensaje de "no hay eventos"
                if (!Array.isArray(data) || data.length === 0 || data.mensaje) {
                    listaEventos.innerHTML = "<p>No hay eventos para esta fecha.</p>";
                    return;
                }

                data.forEach(ev => {
                    const div = document.createElement("div");
                    div.classList.add("evento-item");
                    div.innerHTML = `
                        <h3 class="evento-titulo" data-id="${ev.eventoId}">
                            ${ev.titulo}
                        </h3>
                        <!--<p>${ev.descripcion}</p>-->
                    `;
                    listaEventos.appendChild(div);
                });

                // Agregar evento click para abrir detalle
                listaEventos.querySelectorAll(".evento-titulo").forEach(titulo => {
                    titulo.addEventListener("click", function () {
                        const id = this.dataset.id;
                        if (id) {
                            abrirModalDetalle(id);
                        }
                    });
                });
            } catch (e) {
                console.error("Error cargando eventos:", e);
                listaEventos.innerHTML = `<p class="text-danger">Error al cargar eventos.</p>`;
            }
        }

        function formatearFecha(fechaStr) {
            const f = new Date(fechaStr.replace("/Date(", "").replace(")/", ""));
            return f.toLocaleString("es-MX", {
                dateStyle: "short",
                timeStyle: "short"
            });
        }

        function ajustarFechaFin(fecha) {
            const date = new Date(fecha);
            date.setDate(date.getDate());
            return date.toISOString().split("T")[0];
        }

        //Cuando se crea un evento, recargar eventos
        document.addEventListener('eventoCreado', () => {
            calendar.refetchEvents();
        });

        document.addEventListener('eventoEditado', () => {
            calendar.refetchEvents();
        });

        document.addEventListener('eventoEliminado', () => {
            calendar.refetchEvents();
            // Limpiar lista del modal de día
            const listaEventos = document.getElementById("listaEventos");
            if (listaEventos) {
                listaEventos.innerHTML = "<p class='text-muted'>Actualizando...</p>";
            }

        });
    });

})();

function convertirFechaNetAInput(fechaNet) {
    const timestamp = parseInt(fechaNet.replace("/Date(", "").replace(")/", ""));
    const fechaUTC = new Date(timestamp);

    // Convertir a hora local sin que el navegador lo cambie
    const fechaLocal = new Date(fechaUTC.getTime() - fechaUTC.getTimezoneOffset() * 60000);

    return fechaLocal.toISOString().slice(0, 16);
}