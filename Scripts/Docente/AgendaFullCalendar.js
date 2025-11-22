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

// ---------- CERRAR MODAL EDITAR ----------
const modalEditar = document.getElementById("modalEditarEvento");
const btnCerrarEditar = document.querySelector(".close-editar");

btnCerrarEditar.addEventListener("click", () => {
    modalEditar.style.display = "none";
});

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








// EDITAR
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
        document.getElementById("modalEditarEvento").style.display = "block";

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

    if (!confirm("¿Deseas editar este evento?")) return;

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
            alert(data.mensaje || "Error al actualizar evento");
            return;
        }

        alert("Evento actualizado correctamente");

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

function convertirFechaNetAInput(fechaNet) {
    const timestamp = parseInt(fechaNet.replace("/Date(", "").replace(")/", ""));
    const fechaUTC = new Date(timestamp);

    // Convertir a hora local sin que el navegador lo cambie
    const fechaLocal = new Date(fechaUTC.getTime() - fechaUTC.getTimezoneOffset() * 60000);

    return fechaLocal.toISOString().slice(0, 16);
}


// ELIMINAR
if (btnEliminarEvento) {
    btnEliminarEvento.addEventListener("click", async function () {
        const id = modalDetalle.dataset.eventoId;

        if (!id) {
            alert("No se encontró el ID del evento.");
            return;
        }

        const confirmar = confirm("¿Seguro que deseas eliminar este evento?");
        if (!confirmar) return;

        try {
            const resp = await fetch(`/EventosAgenda/EliminarEvento/${id}`, {
                method: "DELETE"
            });

            const data = await resp.json();

            if (!resp.ok) {
                alert(data.mensaje || "Error al eliminar el evento");
                return;
            }

            alert("Evento eliminado correctamente");

            // Cerrar modal
            modalDetalle.style.display = "none";
            modalEvento.style.display = "none";
            //calendar.refetchEvents();

        } catch (err) {
            console.error("Error eliminando evento:", err);
            alert("Ocurrió un error al eliminar el evento");
        }
    });
}
