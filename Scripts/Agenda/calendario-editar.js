let modalEditar;
let modalEditarEl;

document.addEventListener("DOMContentLoaded", () => {
    modalEditarEl = document.getElementById("modalEditarEvento");
    modalEditar = bootstrap.Modal.getOrCreateInstance(modalEditarEl);

    activarTabs();
});


//Escuchar evento (viene de calendario-detalles) para abrir el modal de editar
document.addEventListener("editarEvento", (e) => {
    const { eventoId } = e.detail;
    abrirModalEditarEvento(eventoId);
});
function convertirFechaNetAInput(fechaNet) {
    const timestamp = parseInt(fechaNet.replace("/Date(", "").replace(")/", ""));
    const fechaUTC = new Date(timestamp);

    // Convertir a hora local sin que el navegador lo cambie
    const fechaLocal = new Date(fechaUTC.getTime() - fechaUTC.getTimezoneOffset() * 60000);

    return fechaLocal.toISOString().slice(0, 16);
}
function activarTabs() {
    document.querySelectorAll("#modalEditarEvento .nav-link-custom").forEach(btn => {
        btn.addEventListener("click", function () {
            const tabId = this.dataset.tab;

            document.querySelectorAll("#modalEditarEvento .nav-link-custom").forEach(b => b.classList.remove("active"));
            this.classList.add("active");

            document.querySelectorAll("#modalEditarEvento .tab-pane").forEach(tab => tab.classList.remove("active", "show"));
            document.getElementById(tabId).classList.add("active", "show");
        });
    });
}

async function abrirModalEditarEvento(eventoId) {
    try {
        const response = await fetch(`/EventosAgenda/GetEvento?id=${eventoId}`);
        const data = await response.json();

        if (!response.ok) throw new Error();

        document.getElementById("editar-evento-id").value = data.eventoId;
        document.getElementById("editar-titulo").value = data.titulo;
        document.getElementById("editar-descripcion").value = data.descripcion;

        document.getElementById("editar-fecha-inicio").value =
            convertirFechaNetAInput(data.fechaInicio);
        document.getElementById("editar-fecha-final").value =
            convertirFechaNetAInput(data.fechaFinal);

        document.getElementById("editar-color").value = data.color;
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

        const contGrupos = document.getElementById("contenedorGruposEditar");
        const contSueltas = document.getElementById("contenedorMateriasSueltasEditar");

        contGrupos.innerHTML = ""; // limpiar
        contSueltas.innerHTML = ""; // limpiar

        // GRUPOS
        data.grupos.forEach(grupo => {
            const divGrupo = document.createElement("div");
            divGrupo.classList.add("grupo-item");

            divGrupo.innerHTML = `
            <div class="grupo-header">
                <label>
                    <input type="checkbox" class="editar-chk-grupo" data-grupo="${grupo.GrupoId}">
                    <strong>${grupo.NombreGrupo}</strong>
                </label>
                <button type="button" class="btn-expandir" data-grupo="${grupo.GrupoId}">▶</button>
            </div>
            <div class="editar-materias-del-grupo hidden"></div>
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

            contGrupos.appendChild(divGrupo);
        });

        // MATERIAS SIN GRUPO
        
            
        data.materiasSueltas.forEach(mat => {
            const divMat = document.createElement("div");
            divMat.classList.add("materia-suelta-item");
            divMat.innerHTML = `
                <label>
                    <input type="checkbox"
                            class="editar-chk-materia-suelta"
                            data-materia="${mat.MateriaId}">
                    ${mat.NombreMateria}
                </label>
            `;

            contSueltas.appendChild(divMat);
        });

        activarExpandibles();
        activarLogicaCheckBoxes();
        

        // Materias y grupos seleccionados
        marcarSeleccionadosEditar(evento);

        activarLogicaEditar();

    } catch (error) {
        console.error("Error al cargar grupos/materias para editar:", error);
    }
}

function activarExpandibles() {
    document.querySelectorAll(".btn-expandir").forEach(boton => {
        boton.addEventListener("click", function () {
            const contenedorMaterias = this.closest(".grupo-item").querySelector(".editar-materias-del-grupo");
            const estaOculto = contenedorMaterias.classList.contains("hidden");
            contenedorMaterias.classList.toggle("hidden", !estaOculto);
            this.textContent = estaOculto ? "▼" : "▶";
        });
    });
}

function activarLogicaCheckBoxes() {
    // Grupo -> materias
    document.querySelectorAll(".editar-chk-grupo").forEach(chkGrupo => {
        chkGrupo.addEventListener("change", function () {
            const grupoId = this.dataset.grupo;
            document.querySelectorAll(`.editar-chk-materia[data-grupo="${grupoId}"]`)
                .forEach(chk => chk.checked = this.checked);
        });
    });

    // Materias -> grupo
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
}
