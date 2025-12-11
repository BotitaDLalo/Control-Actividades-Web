// Prevent duplicate initialization when script is loaded multiple times
if (window.__agendaFullCalendarInitialized) {
    console.warn('AgendaFullCalendar already initialized, skipping duplicate load');
} else {
    window.__agendaFullCalendarInitialized = true;

    document.addEventListener("DOMContentLoaded", function () {

        console.log("FullCalendar inicializando...");
    //modal
    //modal-detalles
    //modal-crear
        //modal-editar

        //JERARQUÍA
        //CALENDARIO CON TODOS LOS EVENTO
        //MODAL CON EVENTOS DEL DÍA SELECCIONADO
        //MODAL CREAR EVENTO    ||  MODAL DETALLES EVENTO ESPECÍFICO
                                    //MODAL EDITAR EVENTO

    const calendarEl = document.getElementById("calendar");

    //Modal de creación
    const modalCrear = document.getElementById("modalCrearEvento");
    const btnCerrarCrear = document.querySelector(".close-crear");

    // Inicializar FullCalendar
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

        // Cuando se selecciona un día 
        dateClick: function (info) {
            abrirModal(info.dateStr);
        },

        // EVENTOS DEL DOCENTE
        events: function (fetchInfo, successCallback, failureCallback) {
            fetch('/EventosAgenda/ObtenerEventosDocente')
                .then(res => res.json())
                .then(data => {

                    console.log("Eventos obtenidos:", data);
                    if (!Array.isArray(data)) {
                        successCallback([]);
                        return;
                    }

                    const eventos = data.map(e => {

                        const final = convertirFechaNetAInput(e.fechaFinal);

                        return {
                            id: e.eventoId,
                            title: e.titulo,
                            start:
                                convertirFechaNetAInput(e.fechaInicio),
                            end:
                                ajustarFechaFin(final),
                            color: e.color === "azul" ? "#007bff" : "#6c757d",
                            borderColor: "transparent"
                            
                        };
                    });

                    successCallback(eventos);
                })
                .catch(err => {
                    console.error("Error al cargar eventos:", err);
                    failureCallback(err);
                });
        }


    });

    calendar.render();

    function ajustarFechaFin(fecha) {
        const date = new Date(fecha);
        date.setDate(date.getDate() + 1);
        return date.toISOString().split("T")[0];
    }

    // MODAL
    
    const modal = document.getElementById("modalEvento");
    const btnAgregar = document.getElementById("btnAgregarEvento");
    const formContainer = document.getElementById("formEventoContainer");
    const listaEventos = document.getElementById("listaEventos");
    const textoFecha = document.getElementById("fechaSeleccionadaTexto");
    const btnCerrar = document.querySelector(".close-modal12");

    //MODAL QUE MUESTRA LOS EVENTOS DEL DÍA SELECCIONADO
    function convertirFecha(fecha) {
        // Asegurar formato ISO (YYYY-MM-DD)
        const fechaISO = fecha.replace(/\//g, "-");

        const fechaObj = new Date(fechaISO + "T00:00");
        return fechaObj.toLocaleDateString("es-ES");
    }

    function abrirModal(fecha) {
        textoFecha.textContent = convertirFecha(fecha);
        modal.style.display = "flex";

        //Fecha en el formulario al momento de crear evento
        document.getElementById("FechaInicio").value = fecha + "T00:00";
        document.getElementById("fechaFinal").value = fecha + "T23:59";

        cargarEventosDia(fecha);
    }
    
    if (btnCerrar) {
        btnCerrar.addEventListener("click", () => {
            if (modal) modal.style.display = "none";
            if (listaEventos) listaEventos.innerHTML = "";
            const modalEventoEl = document.getElementById('modalEvento');
            if (modalEventoEl) modalEventoEl.style.display = "none";
        });
    }

    // Modal de creación. Agregar nuevo evento
    if (btnAgregar) {
        btnAgregar.addEventListener("click", () => {
            if (modalCrear) modalCrear.style.display = "flex";
            cargarGruposMaterias();
        });
    }

    if (btnCerrarCrear) {
        btnCerrarCrear.addEventListener("click", () => {
            if (modalCrear) modalCrear.style.display = "none";
            limpiarFormularioEvento();
        });
    }

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

                //Encabezado con botón expandir, checkbox y nombre

                divGrupo.innerHTML = `
                <div class="grupo-header">
                    <label>
                        <input type="checkbox" class="chk-grupo" data-grupo="${grupo.GrupoId}">
                        <strong>${grupo.NombreGrupo}</strong>
                    </label>
                    <button type="button" class="btn-expandir" data-grupo=${grupo.GrupoId}">▶</button>
                </div>

                <div class="materias-del-grupo hidden"></div>
            `;

                //Contenedor para meter las materias
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

            activarExpandibles();
            activarLogicaCheckBoxes();
        }
        catch (err) {
            console.error("Error cargando grupos y materias:", err);
        }
    }

        function activarExpandibles() {
            const botonesexpandir = document.querySelectorAll(".btn-expandir");
            botonesexpandir.forEach(boton => {
                boton.addEventListener("click", function () {
                    const grupoId = this.dataset.grupo;
                    const contenedorMaterias = this.closest(".grupo-item").querySelector(".materias-del-grupo");

                    // Si está oculto, mostrarlo; si está visible, ocultarlo
                    const estaOculto = contenedorMaterias.classList.contains("hidden");
                    if (estaOculto) {
                        contenedorMaterias.classList.remove("hidden");
                        this.textContent = "▼"; // Cambiar ícono a "colapsar"
                    } else {
                        contenedorMaterias.classList.add("hidden");
                        this.textContent = "▶"; // Cambiar ícono a "expandir"
                    }
                });
            });
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

                Swal.fire({
                    title: "Evento creado correctamente",
                    icon: "success"
                });
                limpiarFormularioEvento();

                document.getElementById("modalCrearEvento").style.display = "none"; //oculta el modal
                calendar.refetchEvents(); // Recargar eventos
                


            } else {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "Ocurrió un error al crear el evento"
                });
            }
        }
        catch (error) {
            console.error("Error:", error);
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Ocurrió un error al crear el evento"
            });
        }
    });
    // stray token removed and ensure limpiar function exists
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

    // end initialized block
}

function convertirFechaNetAInput(fechaNet) {
    const timestamp = parseInt(fechaNet.replace("/Date(", "").replace(")/", ""));
    const fechaUTC = new Date(timestamp);

    // Convertir a hora local sin que el navegador lo cambie
    const fechaLocal = new Date(fechaUTC.getTime() - fechaUTC.getTimezoneOffset() * 60000);

    return fechaLocal.toISOString().slice(0, 16);
}

// ---------- MODAL DE DETALLES DEL EVENTO ----------
const modalDetalle = document.getElementById("modalDetalleEvento");
const btnCerrarDetalle = document.querySelector(".close-detalle");
const btnCerrarDetalle2 = document.getElementById("btnCerrarDetalle");
const btnEditarEvento = document.getElementById("btnEditarEvento");
const btnEliminarEvento = document.getElementById("btnEliminarEvento");

// Cerrar modal detalles
if (btnCerrarDetalle) btnCerrarDetalle.addEventListener("click", () => { modalDetalle.style.display = "none"; });
if (btnCerrarDetalle2) btnCerrarDetalle2.addEventListener("click", () => { modalDetalle.style.display = "none"; });

async function abrirModalDetalle(eventoId) {
    try {

        const resp = await fetch(`/EventosAgenda/ObtenerEventoPorId?id=${eventoId}`);
        if (!resp.ok) {
            const txt = await resp.text();
            console.error("Error fetching detalle:", txt);
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "No se encontró este evento"
            });
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
        const opciones = { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' };
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



// EDITAR

// ---------- CERRAR MODAL EDITAR ----------
const modalEditar = document.getElementById("modalEditarEvento");
const btnCerrarEditar = document.querySelector(".close-editar");

btnCerrarEditar.addEventListener("click", () => {
    modalEditar.style.display = "none";
});


// Cerrar click fuera del contenido
window.addEventListener("click", function (e) {
    if (e.target === modalDetalle) {
        modalDetalle.style.display = "none";
    }
});


if (btnEditarEvento) btnEditarEvento.addEventListener("click", function () {
    const id = modalDetalle.dataset.eventoId;
    abrirModalEditarEvento(id);
});

//PRECARGAR MODAL DE EDICIÓN
async function abrirModalEditarEvento(eventoId) {
    try {
        // Llamar endpoint GET
        const response = await fetch(`/EventosAgenda/GetEvento?id=${eventoId}`);
        const data = await response.json();

        if (!response.ok) {
            alert("Error al obtener tu evento");
            return;
        }
        console.log(data);
        // Guardar temporalmente el id del evento para enviarlo al editar
        document.getElementById("editar-evento-id").value = data.eventoId;

        // Cargar datos en inputs
        document.getElementById("editar-titulo").value = data.titulo;
        document.getElementById("editar-descripcion").value = data.descripcion;

        document.getElementById("editar-fecha-inicio").value = convertirFechaNetAInput(data.fechaInicio);
        document.getElementById("editar-fecha-final").value = convertirFechaNetAInput(data.fechaFinal);

        document.getElementById("editar-color").value = data.color;

        // Precargar grupos y materias
        await cargarGruposMateriasEditar(data);

        // Abrir modal
        document.getElementById("modalEditarEvento").style.display = "flex";

    } catch (error) {
        console.error("Error al cargar evento:", error);
    }
}

// Carga de grupos y materias para edición
async function cargarGruposMateriasEditar(evento) {
    try {
        const resp = await fetch("/EventosAgenda/ObtenerGruposYMaterias");
        const data = await resp.json();

        const contenedor = document.getElementById("editar-contenedorGruposMaterias");
        contenedor.innerHTML = ""; // limpiar

        // GRUPOS
        data.grupos.forEach(grupo => {
            const divGrupo = document.createElement("div");
            divGrupo.classList.add("grupo-item");

            divGrupo.innerHTML = `
                <label>
                    <input type="checkbox" class="editar-chk-grupo" data-grupo="${grupo.GrupoId}">
                    <strong>${grupo.NombreGrupo}</strong>
                </label>
                <div class="editar-materias-del-grupo" style="margin-left: 20px;"></div>
            `;

            const contMat = divGrupo.querySelector(".editar-materias-del-grupo");

            grupo.Materias.forEach(mat => {
                const divMat = document.createElement("div");

                divMat.innerHTML = `
                    <label>
                        <input type="checkbox"
                               class="editar-chk-materia"
                               data-grupo="${grupo.GrupoId}"
                               data-materia="${mat.MateriaId}">
                        ${mat.NombreMateria}
                    </label>
                `;

                contMat.appendChild(divMat);
            });

            contenedor.appendChild(divGrupo);
        });

        // MATERIAS SIN GRUPO
        if (data.materiasSueltas.length > 0) {
            const titulo = document.createElement("h4");
            titulo.textContent = "Materias sin grupo";
            contenedor.appendChild(titulo);

            data.materiasSueltas.forEach(mat => {
                const divMat = document.createElement("div");

                divMat.innerHTML = `
                    <label>
                        <input type="checkbox"
                               class="editar-chk-materia-suelta"
                               data-materia="${mat.MateriaId}">
                        ${mat.NombreMateria}
                    </label>
                `;

                contenedor.appendChild(divMat);
            });
        }

        // Materias y grupos seleccionados
        marcarSeleccionadosEditar(evento);

        activarLogicaEditar();

    } catch (error) {
        console.error("Error al cargar grupos/materias para editar:", error);
    }
}

function marcarSeleccionadosEditar(evento) {

    // Marcar grupos
    evento.gruposSeleccionados.forEach(idGrupo => {
        const chkGrupo = document.querySelector(`.editar-chk-grupo[data-grupo="${idGrupo}"]`);
        if (chkGrupo) chkGrupo.checked = true;
    });

    // Marcar materias de grupo y que no pertenecen a ningún grupo
    evento.materiasSeleccionadas.forEach(idMat => {
        const chkMat = document.querySelector(`input[data-materia="${idMat}"]`);
        if (chkMat) chkMat.checked = true;
    });
}

function activarLogicaEditar() {

    // Grupo, marcar todas las materias
    document.querySelectorAll(".editar-chk-grupo").forEach(chkGrupo => {
        chkGrupo.addEventListener("change", function () {
            const grupoId = this.dataset.grupo;

            document.querySelectorAll(`.editar-chk-materia[data-grupo="${grupoId}"]`)
                .forEach(chk => chk.checked = this.checked);
        });
    });

    // Materias, actualizan estado del grupo. Todas marcadas = grupo marcado
    document.querySelectorAll(".editar-chk-materia").forEach(chk => {
        chk.addEventListener("change", function () {
            const grupoId = this.dataset.grupo;

            const todas = document.querySelectorAll(`.editar-chk-materia[data-grupo="${grupoId}"]`);
            const marcadas = document.querySelectorAll(`.editar-chk-materia[data-grupo="${grupoId}"]:checked`);

            const chkGrupo = document.querySelector(`.editar-chk-grupo[data-grupo="${grupoId}"]`);

            chkGrupo.checked = (marcadas.length === todas.length);
        });
    });
}

//MANDAR PETICIÓN DE EDICIÓN
document.getElementById("formEditarEvento").addEventListener("submit", async function (e) {
        e.preventDefault();

    const result = await Swal.fire({
        title: "¿Editar este evento?",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Sí, editarlo"
    });

    if (!result.isConfirmed) return;

    // Construir modelo para enviarlo al backend
    const modelo = {
        EventoId: parseInt(document.getElementById("editar-evento-id").value),
        Titulo: document.getElementById("editar-titulo").value,
        Descripcion: document.getElementById("editar-descripcion").value,
        Color: document.getElementById("editar-color").value,
        FechaInicio: document.getElementById("editar-fecha-inicio").value,
        FechaFinal: document.getElementById("editar-fecha-final").value,

        GruposSeleccionados: [],
        MateriasSeleccionadas: []
    };

    // Obtener seleccionados
    document.querySelectorAll(".editar-chk-grupo:checked").forEach(chk => {
        modelo.GruposSeleccionados.push(parseInt(chk.dataset.grupo));
    });

    document.querySelectorAll(".editar-chk-materia:checked, .editar-chk-materia-suelta:checked").forEach(chk => {
        modelo.MateriasSeleccionadas.push(parseInt(chk.dataset.materia));
    });

    try {
        const resp = await fetch("/EventosAgenda/EditarEvento", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(modelo)
        });

        const data = await resp.json();

        if (!resp.ok) {
            Swal.fire({
                title: "Error",
                text: "Ocurrió un error al editar el evento",
                icon: "error"
            });
            return;
        }

        Swal.fire({
            title: "Evento editado",
            text: "Se editó correctamente la información",
            icon: "success"
        });

        // Cerrar modal
        document.getElementById("modalEditarEvento").style.display = "none";
        modalDetalle.style.display = "none";

        // Refrescar calendario 
        //calendar.refetchEvents(); // Recargar eventos

    } catch (err) {
        console.error("Error al editar evento:", err);
        alert("Error inesperado");
    }
});

// ELIMINAR
if (btnEliminarEvento) {
    btnEliminarEvento.addEventListener("click", async function () {
        const id = modalDetalle.dataset.eventoId;
        //modalDetalle.style.display = "none";
        
        if (!id) {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Evento no encontrado"
            });
            return;
        }

        Swal.fire({
            title: "Eliminar evento",
            text: "Este evento no se podrá recuperar",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Sí, eliminar"
        }).then(async(result) => {
            if (result.isConfirmed) {

                try {
                    const resp = await fetch(`/EventosAgenda/EliminarEvento/${id}`, {
                        method: "DELETE"
                    });

                    const data = await resp.json();

                    if (!resp.ok) {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: "Error al eliminar el evento"
                        });
                        return;
                    }

                    Swal.fire({
                        title: "El evento ha sido eliminado",
                        text: "Evento eliminado correctamente",
                        icon: "success"
                    });

                    //calendar.refetchEvents();

                    // Cerrar modal
                    modalDetalle.style.display = "none";
                    //modalEvento.style.display = "none";

                } catch (err) {
                    console.error("Error eliminando evento:", err);
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: "Error al eliminar el evento"
                    });
                }

            }
        });
        
    });
}
