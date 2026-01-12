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
    });

})();

function ajustarFechaFin(fecha) {
    const date = new Date(fecha);
    date.setDate(date.getDate() );
    return date.toISOString().split("T")[0];
}

function convertirFechaNetAInput(fechaNet) {
    const timestamp = parseInt(fechaNet.replace("/Date(", "").replace(")/", ""));
    const fechaUTC = new Date(timestamp);

    // Convertir a hora local sin que el navegador lo cambie
    const fechaLocal = new Date(fechaUTC.getTime() - fechaUTC.getTimezoneOffset() * 60000);

    return fechaLocal.toISOString().slice(0, 16);
}