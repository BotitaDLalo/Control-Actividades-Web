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
        document.getElementById("FechaInicio").value = fecha + "T00:00";
        document.getElementById("fechaFinal").value = fecha + "T23:59";

        cargarEventosDia(fecha);
    }

    btnCerrar.addEventListener("click", () => {
        modal.style.display = "none";
        listaEventos.innerHTML = "";
        modalEvento.style.display = "none";
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

        // Obtener grupos seleccionados
        const grupos = [...document.querySelectorAll('.chk-grupo:checked')]
            .map(x => x.dataset.grupo)
            .join(',');

        // Obtener materias seleccionadas
        const materias = [...document.querySelectorAll('.chk-materia:checked')]
            .map(x => x.dataset.materia);

        // Obtener materias sin grupo
        const materiasSueltas = [...document.querySelectorAll('.chk-materia-suelta:checked')]
            .map(x => x.dataset.materia);

        const todasLasMaterias = [...materias, ...materiasSueltas].join(',');

        // Agregar al FormData
        formData.append("GruposSeleccionados", grupos);
        formData.append("MateriasSeleccionadas", todasLasMaterias);

        try {

            const response = await fetch("/EventosAgenda/CrearEvento", {
                method: "POST",
                body: formData
            });

            const data = await response.json();

            if (response.ok) {
                alert(data.mensaje);
                limpiarFormularioEvento();

                document.getElementById("modalCrearEvento").style.display = "none"; //oculta el modal
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

    //VER EN CONSOLA LOS DATOS DE LOS CHECKBOXES AL HACER CLIC
    document.addEventListener("change", e => {
        if (e.target.matches(".chk-grupo, .chk-materia, .chk-materia-suelta")) {
            console.log("Click:", e.target);
            console.log("dataset:", e.target.dataset);
        }
    });


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
                    <h3 class="evento-titulo" data-id="${ev.eventoId}">${ev.titulo}</h3>
                    <p>${ev.descripcion}</p>
                `;
                listaEventos.appendChild(div);
            });

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
        }
    }

});

// ---------- MODAL DE DETALLES DEL EVENTO ----------
const modalDetalle = document.getElementById("modalDetalleEvento");
const btnCerrarDetalle = document.querySelector(".close-detalle");
const btnCerrarDetalle2 = document.getElementById("btnCerrarDetalle");
const btnEditarEvento = document.getElementById("btnEditarEvento");
const btnEliminarEvento = document.getElementById("btnEliminarEvento");

async function abrirModalDetalle(eventoId) {
    try {
        
        const resp = await fetch(`/EventosAgenda/ObtenerEventoPorId?id=${eventoId}`);
        if (!resp.ok) {
            const txt = await resp.text();
            console.error("Error fetching detalle:", txt);
            alert("No se pudo obtener los detalles del evento.");
            return;
        }
        const payload = await resp.json();
        console.log("DEBUG ObtenerEventoPorId payload:", payload);
        if (payload.mensaje) {
            alert(payload.mensaje || "Evento no encontrado");
            return;
        }

        const evento = payload.evento;
        const gruposConMaterias = payload.gruposConMaterias || [];
        const materiasSueltas = payload.materiasSueltas || [];
        const esPersonal = payload.esPersonal;

        // Rellenar campos
        document.getElementById("detalle-titulo").textContent = evento.titulo;
        document.getElementById("detalle-descripcion").textContent = evento.descripcion || "";

        // Fechas
        const opciones = { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute:'2-digit' };
        document.getElementById("detalle-fecha-inicio").textContent = new Date(evento.fechaInicio).toLocaleString('es-MX', opciones);
        document.getElementById("detalle-fecha-final").textContent = new Date(evento.fechaFinal).toLocaleString('es-MX', opciones);

        document.getElementById("detalle-color").textContent = evento.color;

        // Contenedor
        const contDest = document.getElementById("detalle-destinatarios");
        const contGrupos = document.getElementById("detalle-grupos");
        const contMaterias = document.getElementById("detalle-materias");
        const ulGrupos = document.getElementById("lista-grupos");
        const ulMaterias = document.getElementById("lista-materias");

        ulGrupos.innerHTML = "";
        ulMaterias.innerHTML = "";

        if (esPersonal || (gruposConMaterias.length === 0 && materiasSueltas.length === 0)) {
            contDest.style.display = "none";
        } else {
            contDest.style.display = "block";

            // Grupos con sus materias (todas las materias del grupo)
            if (gruposConMaterias.length > 0) {
                contGrupos.style.display = "block";
                gruposConMaterias.forEach(g => {
                    const liGrupo = document.createElement("li");
                    liGrupo.classList.add("grupo-item-detalle");
                    // nombre del grupo
                    const grpTitle = document.createElement("div");
                    grpTitle.classList.add("grupo-titulo-detalle");
                    grpTitle.textContent = g.nombre;
                    liGrupo.appendChild(grpTitle);

                    // lista de materias del grupo
                    const ulMat = document.createElement("ul");
                    ulMat.classList.add("materias-grupo-detalle");

                    (g.materias || []).forEach(m => {
                        const liMat = document.createElement("li");
                        liMat.classList.add("materia-item-detalle");
                        if (m.isSelected) {
                            liMat.textContent = m.nombre;
                            liMat.classList.add("materia-selected");
                        } else {
                            liMat.textContent = m.nombre;
                            liMat.classList.add("materia-not-selected"); // gris/tachado
                        }
                        ulMat.appendChild(liMat);
                    });

                    liGrupo.appendChild(ulMat);
                    ulGrupos.appendChild(liGrupo);
                });
            } else {
                contGrupos.style.display = "none";
            }

            // Materias sin grupo que tienen el evento
            if (materiasSueltas.length > 0) {
                contMaterias.style.display = "block";

                materiasSueltas.forEach(ms => {
                    const li = document.createElement("li");
                    li.textContent = ms.nombre;
                    li.classList.add("materia-suelta-detalle");
                    ulMaterias.appendChild(li);
                });

            } else {
                contMaterias.style.display = "none";
            }
        }


        // Abrir modal detalle
        modalDetalle.style.display = "flex";

        modalDetalle.dataset.eventoId = evento.eventoId;

    } catch (err) {
        console.error("Error abrirModalDetalle:", err);
        alert("Ocurrió un error al cargar los detalles del evento.");
    }
}

// Cerrar modal detalles
if (btnCerrarDetalle) btnCerrarDetalle.addEventListener("click", () => { modalDetalle.style.display = "none"; });
if (btnCerrarDetalle2) btnCerrarDetalle2.addEventListener("click", () => { modalDetalle.style.display = "none"; });

// Cerrar click fuera del contenido
window.addEventListener("click", function (e) {
    if (e.target === modalDetalle) {
        modalDetalle.style.display = "none";
    }
});

// Editar / Eliminar (SOLO OBTIENE ID DE EVENTO)
if (btnEditarEvento) btnEditarEvento.addEventListener("click", function () {
    const id = modalDetalle.dataset.eventoId;
    console.log("Editar evento:", id);
     
    
});

if (btnEliminarEvento) btnEliminarEvento.addEventListener("click", function () {
    const id = modalDetalle.dataset.eventoId;
    console.log("Eliminar evento:", id);
    //llamada a endpoint para eliminar
});
