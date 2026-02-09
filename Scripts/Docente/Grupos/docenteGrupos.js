var div = document.getElementById("docente-datos");
var docenteIdGlobal = div.dataset.docenteid;
/*
if (div && div.dataset && div.dataset.docenteid) {
    docenteIdGlobal = div.dataset.docenteid;
} else if (localStorage.getItem('docenteId')) {
    docenteIdGlobal = localStorage.getItem('docenteId');
}
*/
function abrirImportarAlumnos(grupoId) {
    // reutiliza modal/handler de GrupoActionsModal: establecer currentGrupoId y disparar click en input
    window.currentGrupoId = grupoId;
    var input = document.getElementById('fileImportarAlumnos');
    if (!input) {
        // crear input temporal si no existe (GrupoActionsModal normalmente crea uno cuando se muestra)
        input = document.createElement('input');
        input.type = 'file';
        input.accept = '.xlsx,.xls';
        input.id = 'fileImportarAlumnos';
        input.style.display = 'none';
        document.body.appendChild(input);
        input.addEventListener('change', async function (e) {
            var f = e.target.files[0];
            if (!f) return;
            var fd = new FormData();
            fd.append('file', f);
            fd.append('GrupoId', window.currentGrupoId || grupoId);
            try {
                var resp = await fetch('/api/Alumnos/ImportarAlumnosExcel', { method: 'POST', body: fd });
                var json = await resp.json().catch(() => ({}));
                if (!resp.ok) {
                    Swal.fire('Error', json.mensaje || 'Error al importar', 'error');
                    return;
                }
                Swal.fire('Éxito', 'Importación completada', 'success');
            } catch (err) {
                console.error(err);
                Swal.fire('Error',
                    'No se pudo subir archivo',
                    'error');
            }
        });
    }
    // abrir selector
    input.click();
}

//Crea un nuevo grupo, con la posibilidad de agregar una materia sin grupo, y crear directamente varias materia para ese grupo
async function guardarGrupo() {
    const nombre = document.getElementById("nombreGrupo").value;
    const descripcion = document.getElementById("descripcionGrupo").value;
    const color = "#2196F3";
    const checkboxes = document.querySelectorAll(".materia-checkbox:checked");

    if (nombre.trim() === '') {
        Swal.fire({
            position: "top-end",
            icon: "question",
            title: "Ingrese nombre del grupo.",
            showConfirmButton: false,
            timer: 2500
        });
        return;
    }

    // Obtener IDs de materias seleccionadas en los checkboxes
    const materiasSeleccionadas = Array.from(checkboxes).map(cb => cb.value);

    // Obtener materias creadas en los inputs
    const materiasNuevas = [];
    document.querySelectorAll(".materia-item").forEach(materiaDiv => {
        const nombreMateria = materiaDiv.querySelector(".nombreMateria").value.trim();
        const descripcionMateria = materiaDiv.querySelector(".descripcionMateria").value.trim();
        if (nombreMateria) {
            materiasNuevas.push(
                {
                    NombreMateria: nombreMateria,
                    Descripcion: descripcionMateria
                });
        }
    });

    // Crear el grupo en la base de datos
    const response = await fetch('/Grupos/CrearGrupo', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            NombreGrupo: nombre,
            Descripcion: descripcion,
            CodigoColor: color,
        })
    });
    
    if (response.ok) {
        const grupoCreado = await response.json();
        const grupoId = grupoCreado.grupoId;

        // Guardar materias nuevas directamente asociadas al grupo
        for (const materia of materiasNuevas) {
            const responseMateria = await fetch('/Materias/CrearMateria', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    NombreMateria: materia.NombreMateria,
                    Descripcion: materia.Descripcion,
                    CodigoColor: color, // Enviamos el color de la materia
                    DocenteId: docenteIdGlobal
                })
            });

            if (responseMateria.ok) {
                const materiaCreada = await responseMateria.json();
                materiasSeleccionadas.push(materiaCreada.materiaId);
            }
        }

        // Asociar materias seleccionadas al grupo
        if (materiasSeleccionadas.length > 0) {
            checkboxes.forEach(cb => console.log(cb.value, cb.checked));
            await asociarMateriasAGrupo(grupoId, materiasSeleccionadas);
        }

        Swal.fire({
            position: "top-end",
            icon: "success",
            title: "Grupo registrado correctamente.",
            showConfirmButton: false,
            timer: 2000
        });
        const form = document.getElementById("gruposForm");
        if (form) form.reset();
        if (typeof cargarGrupos === 'function') cargarGrupos();
     
    } else {
        Swal.fire({
            position: "top-end",
            icon: "error",
            title: "Error al registrar grupo.",
            showConfirmButton: false,
            timer: 2000
        });
    }
}

async function asociarMateriasAGrupo(grupoId, materiasSeleccionadas) {
    try {
        const response = await fetch('/Materias/AsociarMateriasAGrupo', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                GrupoId: grupoId,
                MateriaIds: materiasSeleccionadas
            })
        });

        if (response.ok) {
            const data = await response.json();
            console.log(data.mensaje || "Materias asociadas correctamente");
        } else {
            console.error("Error al asociar materias al grupo");
        }
    } catch (error) {
        console.error("Error en la solicitud:", error);
    }
}

//funcion que ayuda a agregar materias nuevas para el grupo
function agregarMateria() {
    const materiasContainer = document.getElementById("listaMaterias");
    if (!materiasContainer) return;

    const materiaDiv = document.createElement("div");
    materiaDiv.classList.add("materia-item");

    materiaDiv.innerHTML = `
        <input type="text" placeholder="Nombre de la Materia" class="nombreMateria">
        <input type="text" placeholder="Descripción" class="descripcionMateria">
        <button type="button" onclick="removerDeLista(this)">❌</button>
    `;

    materiasContainer.appendChild(materiaDiv);
}

// Remover materia del formulario antes de enviarla
function removerDeLista(button) {
    if (button && button.parentElement) button.parentElement.remove();
}

// keep handleCardClick available but not used on groups page
async function handleCardClick(grupoId) {
    // legacy: function preserved
}

//Funciones de contenedor de grupo
function editarGrupo(id) {
    // fallback: open simple edit prompt
    showEditarGrupoPrompt({ GrupoId: id });
}

function showEditarGrupoPrompt(grupo) {
    const nombre = prompt('Nombre del grupo', grupo.NombreGrupo || '');
    if (nombre === null) return; // cancel
    const descripcion = prompt('Descripción', grupo.Descripcion || '');

    // send update
    fetch('/api/Grupos/ActualizarGrupo', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ GrupoId: grupo.GrupoId, NombreGrupo: nombre, Descripcion: descripcion })
    }).then(r => {
        if (r.ok) {
            Swal.fire({ position: 'top-end', icon: 'success', title: 'Grupo actualizado', showConfirmButton: false, timer: 1500 });
            if (typeof cargarGrupos === 'function') cargarGrupos();
        } else {
            Swal.fire({ position: 'top-end', icon: 'error', title: 'Error al actualizar grupo', showConfirmButton: false, timer: 2000 });
        }
    }).catch(err => { console.error(err); Swal.fire({ position: 'top-end', icon: 'error', title: 'Error', showConfirmButton: false, timer: 2000 }); });
}

async function eliminarGrupo(grupoId) {
    Swal.fire({
        title: "¿Qué deseas eliminar?",
        text: "Elige si deseas eliminar solo el grupo o también las materias que contiene.",
        icon: "warning",
        showCancelButton: true,
        showDenyButton: true,
        confirmButtonText: "Eliminar solo grupo",
        denyButtonText: "Eliminar grupo y materias",
        cancelButtonText: "Cancelar"
    }).then(async (result) => {
        if (result.isConfirmed) {
            const response = await fetch(`/Grupos/EliminarGrupo?grupoId=${grupoId}`, { method: "DELETE" });
            if (response.ok) {
                Swal.fire({ position: "top-end", icon: "success", title: "El grupo ha sido eliminado.", showConfirmButton: false, timer: 2000 });
                if (typeof cargarGrupos === 'function') cargarGrupos();
            } else {
                Swal.fire({ position: "top-end", icon: "error", title: "No se pudo eliminar el grupo.", showConfirmButton: false, timer: 2000 });
            }
        } else if (result.isDenied) {
            const response = await fetch(`/Grupos/EliminarGrupoConMaterias?grupoId=${grupoId}`, { method: "DELETE" });
            if (response.ok) {
                Swal.fire({ position: "top-end", icon: "success", title: "El grupo y sus materias han sido eliminados.", showConfirmButton: false, timer: 2000 });
                if (typeof cargarGrupos === 'function') cargarGrupos();
            } else {
                Swal.fire({ position: "top-end", icon: "error", title: "No se pudo eliminar el grupo y sus materias.", showConfirmButton: false, timer: 2000 });
            }
        }
    });
}

function agregarMateriaAlGrupo(id) {
    alert("Agregar Materia Al Grupo " + id);
}

function crearAvisoGrupal(id) {
    Swal.fire({
        title: "Crear Aviso",
        html: '<input id="tituloAviso" class="swal2-input" placeholder="Título del aviso">' + '<textarea id="descripcionAviso" class="swal2-textarea" placeholder="Descripción del aviso"></textarea>',
        showCancelButton: true,
        confirmButtonText: "Crear",
        cancelButtonText: "Cancelar",
        preConfirm: () => {
            const titulo = document.getElementById("tituloAviso").value.trim();
            const descripcion = document.getElementById("descripcionAviso").value.trim();
            if (!titulo || !descripcion) { Swal.showValidationMessage("Debes completar todos los campos"); return false; }
            return { titulo, descripcion };
        }
    }).then((result) => {
        if (result.isConfirmed) {
            const datos = { GrupoId: id, Titulo: result.value.titulo, Descripcion: result.value.descripcion, DocenteId: docenteIdGlobal };
            fetch("/Materias/CrearAvisoPorGrupo", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(datos) })
                .then(response => response.json())
                .then(data => { if (data.mensaje) Swal.fire("Éxito", data.mensaje, "success"); else Swal.fire("Error", "No se pudo crear el aviso", "error"); })
                .catch(error => { console.error("Error al enviar el aviso:", error); Swal.fire("Error", "Ocurrió un error al crear el aviso", "error"); });
        }
    });
}

async function subirExcelAlumnos(grupoId, materiaId) {
    const input = document.getElementById('excelFileInput');
    if (!input || input.files.length === 0) { Swal.fire({ icon: 'warning', title: 'Seleccione un archivo', text: 'Adjunte un .xlsx o .xls', position: 'top-end' }); return; }

    const file = input.files[0];
    const formData = new FormData();
    formData.append('file', file);
    if (grupoId) formData.append('GrupoId', grupoId);
    if (materiaId) formData.append('MateriaId', materiaId);

    try {
        const resp = await fetch('/api/CargaMasiva/ImportarAlumnosExcel', { method: 'POST', body: formData });
        const data = await resp.json();
        if (resp.ok) {
            const mensaje = `Leídos: ${data.TotalLeidos}\nAgregados: ${data.Agregados.length}\nOmitidos: ${data.Omitidos.length}\nNo encontrados: ${data.NoEncontrados.length}`;
            Swal.fire({ icon: 'success', title: 'Importación completada', text: mensaje, position: 'top-end' });
        } else {
            Swal.fire({ icon: 'error', title: 'Error', text: data.mensaje || 'Error al importar', position: 'top-end' });
        }
    } catch (err) { console.error(err); Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo subir el archivo', position: 'top-end' }); }
}


async function crearMateriaGrupo() {
    const nombre = document.querySelector("#nombreMateria").value.trim();
    const descripcion = document.querySelector("#descripcionMateria").value.trim();

    if (!nombre) {
        Swal.fire({
            icon: 'warning',
            title: 'Campo requerido',
            text: 'El nombre de la materia es obligatorio',
            confirmButtonText: 'Entendido'
        });
        return;
    }

    const params = new URLSearchParams(window.location.search);
    const grupoId = params.get('grupoId');

    const response = await fetch('/Materias/CrearMateriaConGrupo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            NombreMateria: nombre,
            Descripcion: descripcion,
            GrupoId: grupoId
        })
    });

    const data = await response.json();

    if (response.ok) {
        await Swal.fire({
            icon: 'success',
            title: 'Éxito',
            text: data.mensaje
        });

        const modalElement = document.getElementById('materiasGrupoModal');
        const modal = bootstrap.Modal.getInstance(modalElement);
        modal.hide();

        location.reload(); //Refresca la página para mostrar la nueva materia en el grupo
    } else {
        Swal.fire({ 
            icon: 'error',
            title: 'Error',
            text: data.mensaje || 'Ocurrió un error.'
        });
    }
}