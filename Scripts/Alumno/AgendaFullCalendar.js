document.addEventListener("DOMContentLoaded", function () {

    console.log("FullCalendar inicializando vista alumno...");

    const calendarEl = document.getElementById("calendar");

    const modal = document.getElementById("modalEvento");
    const modalEl = bootstrap.Modal.getOrCreateInstance(modal);
    const listaEventos = document.getElementById("listaEventos");
    const textoFecha = document.getElementById("fechaSeleccionadaTexto");
    modal.addEventListener("hidden.bs.modal", () => {
        listaEventos.innerHTML = "";
    });
    // Inicializar FullCalendar
    const calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: "dayGridMonth",
        locale: "es",
        height: "auto",

        // Abrir modal al seleccionar día
        dateClick: function (info) {
            abrirModal(info.dateStr);
        },

        events: async function (fetchInfo, successCallback, failureCallback) {
            try {
                const res = await fetch(
                    `/EventosAgenda/ObtenerEventosAlumnoCalendario` +
                    `?alumnoId=${alumnoIdGlobal}` +
                    `&start=${fetchInfo.startStr}` +
                    `&end=${fetchInfo.endStr}`
                );

                const data = await res.json();
                
                if (!Array.isArray(data)) {
                    successCallback([]);
                    return;
                }
                
                const eventos = data.map(e => {
                    const inicio = convertirFechaNetAInput(e.start);
                    const final = ajustarFechaFin(
                        convertirFechaNetAInput(e.end)
                    );

                    return {
                        id: e.id,
                        title: e.title,
                        start: inicio,
                        end: final,
                        allDay: true,
                        color: e.color === "azul" ? "#007bff" : "#6c757d",
                        borderColor: "transparent"
                    };
                });

                successCallback(eventos);

            } catch (err) {
                console.error("Error cargando eventos alumno:", err);
                failureCallback(err);
            }
        }

    });

    calendar.render();

    function convertirFechaNetAInput(fechaNet) {
        const timestamp = parseInt(fechaNet.replace("/Date(", "").replace(")/", ""));
        const fechaUTC = new Date(timestamp);

        // Convertir a hora local sin que el navegador lo cambie
        const fechaLocal = new Date(fechaUTC.getTime() - fechaUTC.getTimezoneOffset() * 60000);

        return fechaLocal.toISOString().slice(0, 16);
    }

    function ajustarFechaFin(fecha) {
        const f = new Date(fecha);
        f.setDate(f.getDate() + 1); // end exclusivo
        return f.toISOString().split("T")[0];
    }


    // ---- MODAL ----

    function abrirModal(fecha) {
        textoFecha.textContent = new Date(fecha).toLocaleDateString("es-MX", {
            weekday: "long",
            year: "numeric",
            month: "long",
            day: "numeric"
        });

        listaEventos.innerHTML =
            `<p style="color:#777;">(Cargando eventos...)</p>`;
        modalEl.show();
        cargarEventosAlumno(fecha);
    }

});


async function cargarEventosAlumno(fecha) {
    try {
        const response = await fetch(`/EventosAgenda/ObtenerEventosAlumnoFecha?alumnoId=${alumnoIdGlobal}&fecha=${fecha}`);
        const data = await response.json();

        if (!data.ok) {
            document.getElementById("listaEventos").innerHTML =
                `<p class="sin-eventos">No se pudieron cargar los eventos.</p>`;
            return;
        }
        
        const lista = document.getElementById("listaEventos");
        lista.innerHTML = "";

        if (data.eventos.length === 0) {
            lista.innerHTML = `<p class="sin-eventos">No hay eventos.</p>`;
            return;
        }

        data.eventos.forEach(ev => {
            const div = document.createElement("div");
            const h3 = document.createElement("h3");
            h3.classList.add("evento-titulo");
            h3.dataset.id = ev.EventoId;
            h3.textContent = ev.Titulo;
            div.appendChild(h3);

            div.classList.add("evento-item");
            
            lista.appendChild(div);
        });

        lista.addEventListener("click", e => {
            const titulo = e.target.closest(".evento-titulo");
            if (!titulo) return;

            const id = titulo.dataset.id;
            abrirModalDetalle(id, alumnoIdGlobal);
        });

    } catch (error) {
        console.error("Error al cargar eventos:", error);
    }
}


async function abrirModalDetalle(eventoId, alumnoId) {
    try {
        console.log("ALU ID: " + alumnoId);
        console.log("EVE ID: " + eventoId)
        const resp = await fetch(`/EventosAgenda/ObtenerEventoAlumnoId?eventoId=${eventoId}&alumnoId=${alumnoId}`);

        if (!resp.ok) {
            const txt = await resp.text();
            console.error("Error en consulta detalle evento: ", txt);
            alert("No se pudieron obtener los detalles del evento.");
            return;
        }

        const data = await resp.json();
        if (!data.ok) {
            alert(data.mensaje || "Error al obtener evento.");
            return;
        }

        const ev = data.evento;

        document.getElementById("detalle-titulo").textContent = ev.Titulo;
        document.getElementById("detalle-fecha-inicio").textContent = formatearFecha(ev.FechaInicio);
        document.getElementById("detalle-fecha-final").textContent = formatearFecha(ev.FechaFinal);
        document.getElementById("detalle-docente").textContent = ev.Docente;
        document.getElementById("detalle-descripcion").textContent = ev.Descripcion || "Sin descripción";

        //Materias y Grupo
        const contenedorDescripcion = document.querySelector("#detalle-descripcion");

        // Eliminar anteriores (si se abre varias veces)
        const viejoBloque = document.getElementById("detalle-extra");
        if (viejoBloque) viejoBloque.remove();

        const extra = document.createElement("div");
        extra.id = "detalle-extra";

        // Si el evento es por grupo
        if (data.esPorGrupo && data.grupo) {
            extra.innerHTML += `
                <h4>Grupo</h4>
                <p>${data.grupo}</p>
            `;
        }

        // Materias
        if (data.materias && data.materias.length > 0) {
            extra.innerHTML += `<h4>Materias</h4>`;
            extra.innerHTML += `<ul>` +
                data.materias.map(m => `<li>${m.NombreMateria}</li>`).join("") +
                `</ul>`;
        }

        contenedorDescripcion.appendChild(extra);

        const modalDetalle = bootstrap.Modal.getOrCreateInstance(
            document.getElementById("modalDetalleEvento")
        );
        modalDetalle.show();

    } catch (err) {
        console.error("Error JS detalle evento:", err);
        alert("Error inesperado al cargar el detalle.");
    }
}

function formatearFecha(fechaStr) {
    const f = new Date(fechaStr);
    return f.toLocaleString("es-MX", {
        dateStyle: "short",
        timeStyle: "short"
    });
}
