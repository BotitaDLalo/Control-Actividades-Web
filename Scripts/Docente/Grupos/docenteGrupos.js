var div = document.getElementById("docente-datos");
var docenteIdGlobal = 0;
if (div && div.dataset && div.dataset.docenteid) {
    docenteIdGlobal = div.dataset.docenteid;
} else if (localStorage.getItem('docenteId')) {
    docenteIdGlobal = localStorage.getItem('docenteId');
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
            materiasNuevas.push({ NombreMateria: nombreMateria, Descripcion: descripcionMateria });
        }
    });

    // Crear el grupo en la base de datos
    const response = await fetch('/Grupos/CrearGrupo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            NombreGrupo: nombre,
            Descripcion: descripcion,
            CodigoColor: color,
            DocenteId: docenteIdGlobal
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
        const form = document.getElementById("gruposForm"); if (form) form.reset();
        if (typeof cargarGrupos === 'function') cargarGrupos();
        if (typeof cargarMateriasSinGrupo === 'function') cargarMateriasSinGrupo();
        if (typeof cargarMaterias === 'function') cargarMaterias();
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

//Funcion para obtener los grupos de la base de datos y mostrarlos (render como grid cards)
async function cargarGrupos() {
    try {
        const response = await fetch(`/Grupos/ObtenerGrupos?docenteId=${docenteIdGlobal}`);
        if (!response.ok) throw new Error('Error al obtener grupos');
        const grupos = await response.json();
        const listaGrupos = document.getElementById("listaGrupos");
        if (!listaGrupos) return;
        listaGrupos.innerHTML = "";

        if (!grupos || grupos.length === 0) {
            const mensaje = document.createElement("p");
            mensaje.classList.add("text-center", "text-muted");
            mensaje.textContent = "No hay grupos registrados.";
            listaGrupos.appendChild(mensaje);
            return;
        }

        grupos.forEach((grupo, index) => {
            const card = document.createElement('div');
            card.className = 'rounded card-layout d-flex align-items-center';

            // left icon
            const ico = document.createElement('div');
            ico.className = 'me-3';
            ico.innerHTML = '<i class="fas fa-graduation-cap fa-2x" style="color:#0d6efd"></i>';

            // text
            const text = document.createElement('div');
            text.style.flex = '1';
            const title = document.createElement('div');
            title.className = 'card-title';
            title.textContent = grupo.NombreGrupo;
            const subtitle = document.createElement('div');
            subtitle.className = 'card-subtitle';
            subtitle.textContent = grupo.Descripcion || '';
            text.appendChild(title);
            if (grupo.Descripcion) text.appendChild(subtitle);

            // settings dropdown
            const cta = document.createElement('div');
            cta.className = 'ms-3';
            // make this container a dropdown so Bootstrap styles work
            cta.classList.add('dropdown');
            const settingsButton = document.createElement('button');
            settingsButton.className = 'btn btn-link p-0 text-dark';
            settingsButton.type = 'button';
            settingsButton.setAttribute('data-bs-toggle', 'dropdown');
            settingsButton.setAttribute('aria-expanded', 'false');
            // mark as toggle for accessibility/styling
            settingsButton.classList.add('dropdown-toggle');
            settingsButton.innerHTML = '<i class="fas fa-cog"></i>';

            const dropdownMenu = document.createElement('ul');
            dropdownMenu.className = 'dropdown-menu dropdown-menu-end';
            const items = [
                { text: 'Editar', action: () => editarGrupo(grupo.GrupoId) },
                { text: 'Eliminar', action: () => eliminarGrupo(grupo.GrupoId) },
                { text: 'Crear Aviso Grupal', action: () => crearAvisoGrupal(grupo.GrupoId) }
            ];
            items.forEach(it => {
                const li = document.createElement('li');
                const a = document.createElement('a');
                a.href = '#';
                a.className = 'dropdown-item';
                a.textContent = it.text;
                a.addEventListener('click', function (e) { e.preventDefault(); it.action(); });
                li.appendChild(a);
                dropdownMenu.appendChild(li);
            });

            cta.appendChild(settingsButton);
            cta.appendChild(dropdownMenu);

            card.appendChild(ico);
            card.appendChild(text);
            card.appendChild(cta);

            // When clicking the card (except on the settings button or the menu), toggle the dropdown
            card.addEventListener('click', function (e) {
                // if click inside dropdown menu or on the settings button, do nothing (Bootstrap handles it)
                if (e.target.closest('.dropdown-menu') || e.target === settingsButton || e.target.closest('.dropdown-toggle')) return;
                try {
                    // use Bootstrap's Dropdown API to toggle
                    var bsDropdown = bootstrap.Dropdown.getOrCreateInstance(settingsButton);
                    bsDropdown.toggle();
                } catch (err) {
                    // bootstrap not available or error - fallback: click the settings button
                    settingsButton.click();
                }
            });

            listaGrupos.appendChild(card);
        });
    } catch (error) {
        console.error(error);
        Swal.fire({
            position: "top-end",
            icon: "error",
            title: "Error al cargar los grupos.",
            showConfirmButton: false,
            timer: 2000
        });
    }
}

// keep handleCardClick available but not used on groups page
async function handleCardClick(grupoId) {
    // legacy: function preserved
}

//Funciones de contenedor de grupo
function editarGrupo(id) {
    // placeholder
    alert("Editar grupo " + id);
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
