let modalEditar;
let modalEditarEl;

function convertirFechaNetAInput(fechaNet) {
    const timestamp = parseInt(fechaNet.replace("/Date(", "").replace(")/", ""));
    const fechaUTC = new Date(timestamp);

    // Convertir a hora local sin que el navegador lo cambie
    const fechaLocal = new Date(fechaUTC.getTime() - fechaUTC.getTimezoneOffset() * 60000);

    return fechaLocal.toISOString().slice(0, 16);
}

document.addEventListener("DOMContentLoaded", () => {
    modalEditarEl = document.getElementById("modalEditarEvento");
    if (modalEditarEl && typeof bootstrap !== 'undefined' && bootstrap.Modal && typeof bootstrap.Modal.getOrCreateInstance === 'function') {
        try {
            modalEditar = bootstrap.Modal.getOrCreateInstance(modalEditarEl);
        } catch (e) {
            console.warn('No se pudo inicializar modalEditar:', e);
            modalEditar = null;
        }
    }
});

//Escuchar evento (viene de calendario-detalles) para abrir el modal de editar
document.addEventListener("editarEvento", (e) => {
    const { eventoId } = e.detail;
    abrirModalEditarEvento(eventoId);
});

async function abrirModalEditarEvento(eventoId) {
    try {
        const response = await fetch(`/EventosAgenda/GetEvento?id=${eventoId}`);
        const data = await response.json();

        if (!response.ok) throw new Error();

        document.getElementById("editar-evento-id").value = data.eventoId;
        document.getElementById("editar-titulo").value = data.titulo;
        document.getElementById("editar-descripcion").value = data.descripcion;
        document.getElementById("editar-color").value = data.color;

        document.getElementById("editar-fecha-inicio").value =
            convertirFechaNetAInput(data.fechaInicio);
        document.getElementById("editar-fecha-final").value =
            convertirFechaNetAInput(data.fechaFinal);

        await cargarGruposMateriasEditar(data);

        modalEditar.show();

    } catch (err) {
        console.error("Error cargando evento:", err);
        Swal.fire("Error", "No se pudo cargar el evento", "error");
    }
}

// Cargar grupos y/o materias en el modal
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

//Submit del formulario de edición
var _formEditarEvento = document.getElementById("formEditarEvento");
if (_formEditarEvento) {
    _formEditarEvento.addEventListener("submit", async e => {
    e.preventDefault();

    const confirm = await Swal.fire({
        title: "¿Editar este evento?",
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "Sí, editar"
    });

    if (!confirm.isConfirmed) return;

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

    document.querySelectorAll(".editar-chk-grupo:checked")
        .forEach(chk => modelo.GruposSeleccionados.push(+chk.dataset.grupo));

    document.querySelectorAll(".editar-chk-materia:checked, .editar-chk-materia-suelta:checked")
        .forEach(chk => modelo.MateriasSeleccionadas.push(+chk.dataset.materia));

    try {
        const resp = await fetch("/EventosAgenda/EditarEvento", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(modelo)
        });

        if (!resp.ok) throw new Error();

        Swal.fire("Editado", "Evento actualizado correctamente", "success");

        modalEditar.hide();

        // Notificar al sistema
        document.dispatchEvent(new CustomEvent("eventoEditado"));

    } catch (err) {
        console.error(err);
        Swal.fire("Error", "No se pudo editar el evento", "error");
    }
});

