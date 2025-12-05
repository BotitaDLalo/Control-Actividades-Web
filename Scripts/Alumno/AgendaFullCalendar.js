document.addEventListener("DOMContentLoaded", function () {

    console.log("FullCalendar inicializando vista alumno...");

    const calendarEl = document.getElementById("calendar");
    const modal = document.getElementById("modalEvento");
    const btnCerrar = document.querySelector(".close-modal12");
    const listaEventos = document.getElementById("listaEventos");
    const textoFecha = document.getElementById("fechaSeleccionadaTexto");

    // Inicializar FullCalendar
    const calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: "dayGridMonth",
        locale: "es",
        height: "auto",

        // Abrir modal al seleccionar día
        dateClick: function (info) {
            abrirModal(info.dateStr);
        },

        events: []
    });

    calendar.render();

    // ---- MODAL ----

    function abrirModal(fecha) {
        textoFecha.textContent = fecha;
        modal.style.display = "flex";

        listaEventos.innerHTML =
            `<p style="color:#777;">(Cargando eventos...)</p>`;
        cargarEventosAlumno(fecha);
    }

    btnCerrar.addEventListener("click", () => {
        modal.style.display = "none";
        listaEventos.innerHTML = "";
    });

    window.addEventListener("click", e => {
        if (e.target === modal) {
            modal.style.display = "none";
            listaEventos.innerHTML = "";
        }
    });
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
            div.classList.add("evento-item");
            div.innerHTML = `
                <h3 class="evento-titulo" data-id="${ev.EventoId}">${ev.Titulo}</h3>
                <p>${ev.Descripcion}</p>
            `;
            lista.appendChild(div);
        });

        lista.querySelectorAll(".evento-titulo").forEach(titulo => {
            titulo.addEventListener("click", function () {
                const id = this.dataset.id;
                if (id) {
                    console.log("Abriste detalle: " + id);
                    abrirModalDetalle(id, alumnoIdGlobal);
                }
            });
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
        document.getElementById("detalle-descripcion").textContent = ev.Descripcion || "Sin descripción";
        document.getElementById("detalle-color").textContent = ev.Color;
        document.getElementById("detalle-docente").textContent = ev.Docente;

        //Materias y Grupo
        const contenedorDescripcion = document.querySelector(".detalle-descripcion");

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

        document.getElementById("modalDetalleEvento").style.display = "block";

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
function convertirFechaNetAInput(fechaNet) {
    const timestamp = parseInt(fechaNet.replace("/Date(", "").replace(")/", ""));
    const fechaUTC = new Date(timestamp);

    // Convertir a hora local sin que el navegador lo cambie
    const fechaLocal = new Date(fechaUTC.getTime() - fechaUTC.getTimezoneOffset() * 60000);

    return fechaLocal.toISOString().slice(0, 16);
}

document.querySelector(".close-detalle").addEventListener("click", () => {
    document.getElementById("modalDetalleEvento").style.display = "none";
});

document.getElementById("btnCerrarDetalle").addEventListener("click", () => {
    document.getElementById("modalDetalleEvento").style.display = "none";
});
