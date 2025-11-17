document.addEventListener("DOMContentLoaded", function () {

    console.log("FullCalendar inicializando...");

    const calendarEl = document.getElementById("calendar");

    const calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: "dayGridMonth",
        locale: "es",
        height: "auto",

        // Cuando se selecciona un día 
        dateClick: function (info) {
            console.log("Día seleccionado:", info.dateStr);
            abrirModal(info.dateStr);
        },

        // EVENTOS DEL DOCENTE
        events: function (fetchInfo, successCallback, failureCallback) {
            fetch('/EventosAgenda/ObtenerEventosDocente')
                .then(res => res.json())
                .then(data => {

                    
                    if (!Array.isArray(data)) {
                        successCallback([]);
                        return;
                    }

                    const eventos = data.map(e => ({
                        id: e.eventoId,
                        title: e.titulo,
                        start: e.fechaInicio,
                        end: e.fechaFinal,
                        backgroundColor: e.color === "azul" ? "#007bff" : "#6c757d",
                        borderColor: "transparent",
                    }));

                    successCallback(eventos);
                })
                .catch(err => {
                    console.error("Error al cargar eventos:", err);
                    failureCallback(err);
                });
        }
    });

    calendar.render();

    // MODAL
    
    const modal = document.getElementById("modalEvento");
    const btnCerrar = document.querySelector(".close-modal12");
    const btnAgregar = document.getElementById("btnAgregarEvento");
    const formContainer = document.getElementById("formEventoContainer");
    const listaEventos = document.getElementById("listaEventos");
    const textoFecha = document.getElementById("fechaSeleccionadaTexto");

    function abrirModal(fecha) {
        textoFecha.textContent = fecha;
        modal.style.display = "flex";

        //Fecha en el formulario al momento de crear evento
        document.getElementById("FechaInicio").value = fecha + "T08:00";
        document.getElementById("fechaFinal").value = fecha + "T09:00";

        cargarEventosDia(fecha);
    }

    btnCerrar.addEventListener("click", () => {
        modal.style.display = "none";
        listaEventos.innerHTML = "";
        formContainer.style.display = "none";
    });

    btnAgregar.addEventListener("click", () => {
        formContainer.style.display = "block";
    });


    // CREAR

    document.getElementById("formEvento").addEventListener("submit", async function (e) {
        e.preventDefault();

        const formData = new FormData(this);

        try {
            const response = await fetch("/EventosAgenda/CrearEvento", {
                method: "POST",
                body: formData
            });

            const data = await response.json();

            if (response.ok) {
                alert(data.mensaje);
                limpiarFormularioEvento();

                document.getElementById("formEventoContainer").style.display = "none"; //oculta el modal
                calendar.refetchEvents(); // Recargar eventos

            } else {
                alert(data.mensaje || "Error al crear evento");
            }
        }
        catch (error) {
            console.error("Error:", error);
            alert("Error al conectar con el servidor");
        }
    });

    function limpiarFormularioEvento() {
        $("#formEvento")[0].reset();
        $("#FechaInicio").val("");
        $("#FechaFinal").val("");
    }


    // Cargar eventos del día para el modal
    async function cargarEventosDia(fecha) {
        try {
            const resp = await fetch(`/EventosAgenda/ObtenerEventosPorFecha?fecha=${fecha}`);
            const data = await resp.json();

            listaEventos.innerHTML = "";

            if (data.mensaje) {
                listaEventos.innerHTML = "<p>No hay eventos para esta fecha.</p>";
                return;
            }

            data.forEach(ev => {
                const div = document.createElement("div");
                div.classList.add("evento-item");
                div.innerHTML = `
                    <h3 class="evento-titulo">${ev.titulo}</h3>
                    <p>${ev.descripcion}</p>
                `;
                listaEventos.appendChild(div);
            });

        } catch (e) {
            console.error("Error cargando eventos:", e);
        }
    }

});