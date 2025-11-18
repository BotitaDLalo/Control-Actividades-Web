document.addEventListener("DOMContentLoaded", function () {

    console.log("FullCalendar inicializando...");

    const calendarEl = document.getElementById("calendar");

    //Modal de creación
    const modalCrear = document.getElementById("modalCrearEvento");
    const btnCerrarCrear = document.querySelector(".close-crear");

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

    // Modal de creación. Agregar nuevo evento
    btnAgregar.addEventListener("click", () => {
        modalCrear.style.display = "flex";
        cargarGruposMaterias();
    });

    btnCerrarCrear.addEventListener("click", () => {
        modalCrear.style.display = "none";
        limpiarFormularioEvento();
    });

    window.addEventListener("click", function (e) {
        if (e.target === modalCrear) {
            modalCrear.style.display = "none";
            limpiarFormularioEvento();
        }

        if (e.target === modal) {
            modal.style.display = "none";
            listaEventos.innerHTML = "";
        }
    });

    //Obtener materias y grupos
    async function cargarGruposMaterias() {
        try {
            const resp = await fetch("/EventosAgenda/ObtenerGruposYMaterias");
            const data = await resp.json();

            const contenedor = document.getElementById("contenedorGruposMaterias");
            contenedor.innerHTML = ""; // limpiar

            // Grupos
            data.grupos.forEach(grupo => {

                const divGrupo = document.createElement("div");
                divGrupo.classList.add("grupo-item");

                divGrupo.innerHTML = `
                <label>
                    <input type="checkbox" class="chk-grupo" data-grupo="${grupo.GrupoId}">
                    <strong>${grupo.NombreGrupo}</strong>
                </label>
                <div class="materias-del-grupo" style="margin-left: 20px;"></div>
            `;

                const contMaterias = divGrupo.querySelector(".materias-del-grupo");

                grupo.Materias.forEach(mat => {
                    const divMat = document.createElement("div");
                    divMat.innerHTML = `
                    <label>
                        <input type="checkbox" class="chk-materia" data-grupo="${grupo.GrupoId}" data-materia="${mat.MateriaId}">
                        ${mat.NombreMateria}
                    </label>
                `;
                    contMaterias.appendChild(divMat);
                });

                contenedor.appendChild(divGrupo);
            });

            // Materias sin grupos
            if (data.materiasSueltas.length > 0) {
                const titulo = document.createElement("h4");
                titulo.textContent = "Materias sin grupo";
                contenedor.appendChild(titulo);

                data.materiasSueltas.forEach(mat => {
                    const divMat = document.createElement("div");
                    divMat.classList.add("materia-suelta-item");
                    divMat.innerHTML = `
                    <label>
                        <input type="checkbox" class="chk-materia-suelta" data-materia="${mat.MateriaId}">
                        ${mat.NombreMateria}
                    </label>
                `;
                    contenedor.appendChild(divMat);
                });
            }

            activarLogicaCheckBoxes();
        }
        catch (err) {
            console.error("Error cargando grupos y materias:", err);
        }
    }

    function activarLogicaCheckBoxes() {

        // Marcar un grupo marca todas sus materias
        document.querySelectorAll(".chk-grupo").forEach(chkGrupo => {
            chkGrupo.addEventListener("change", function () {
                const grupoId = this.dataset.grupo;

                document.querySelectorAll(`.chk-materia[data-grupo="${grupoId}"]`)
                    .forEach(chk => chk.checked = this.checked);
            });
        });

        // Al desmarcar todas las materias se desmarca el grupo
        document.querySelectorAll(".chk-materia").forEach(chk => {
            chk.addEventListener("change", function () {
                const grupoId = this.dataset.grupo;

                const todas = document.querySelectorAll(`.chk-materia[data-grupo="${grupoId}"]`);
                const marcadas = document.querySelectorAll(`.chk-materia[data-grupo="${grupoId}"]:checked`);

                const chkGrupo = document.querySelector(`.chk-grupo[data-grupo="${grupoId}"]`);

                // si todas las materias están marcadas se marca el grupo
                if (marcadas.length === todas.length) chkGrupo.checked = true;

                // si se desmarca alguna materia se desmarca el grupo
                if (marcadas.length < todas.length) chkGrupo.checked = false;
            });
        });
    }



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