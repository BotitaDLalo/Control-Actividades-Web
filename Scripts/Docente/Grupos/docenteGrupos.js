var div = document.getElementById("docente-datos");
var docenteIdGlobal = 0;
if (div && div.dataset && div.dataset.docenteid) {
    docenteIdGlobal = div.dataset.docenteid;
} else if (localStorage.getItem('docenteId')) {
    docenteIdGlobal = localStorage.getItem('docenteId');
}

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
            } catch (err) { console.error(err); Swal.fire('Error', 'No se pudo subir archivo', 'error'); }
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
            card.className = 'rounded card-layout';
            card.style.position = 'relative';

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

            // create a row for icon + text
            const row = document.createElement('div');
            row.style.display = 'flex';
            row.style.width = '100%';
            row.appendChild(ico);
            row.appendChild(text);

            // settings dropdown container positioned top-right
            const cta = document.createElement('div');
            cta.className = 'dropdown';
            cta.style.position = 'absolute';
            cta.style.top = '8px';
            cta.style.right = '12px';

            const settingsButton = document.createElement('button');
            // visible icon button with dropdown (use inline SVG to avoid external icon libs)
            settingsButton.className = 'group-settings-btn dropdown-toggle';
            settingsButton.type = 'button';
            settingsButton.setAttribute('data-bs-toggle', 'dropdown');
            settingsButton.setAttribute('aria-expanded', 'false');
            settingsButton.setAttribute('aria-label', 'Opciones del grupo');
            settingsButton.title = 'Opciones';
            settingsButton.innerHTML = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">'
                + '<circle cx="12" cy="5" r="1.5" fill="#374151"/>'
                + '<circle cx="12" cy="12" r="1.5" fill="#374151"/>'
                + '<circle cx="12" cy="19" r="1.5" fill="#374151"/>'
                + '</svg>';

            const dropdownMenu = document.createElement('ul');
            dropdownMenu.className = 'dropdown-menu dropdown-menu-end';
            dropdownMenu.style.minWidth = '180px';
            dropdownMenu.style.padding = '6px 0';
            const items = [
                { text: 'Administrar grupo', action: () => abrirAccionesGrupo(grupo.GrupoId) },
                { text: 'Importar alumnos (masivo)', action: () => abrirImportarAlumnos(grupo.GrupoId) },
                { text: 'Crear aviso grupal', action: () => crearAvisoGrupal(grupo.GrupoId) },
                { text: 'Editar grupo', action: () => showEditarGrupoPrompt(grupo) },
                { text: 'Eliminar grupo', action: () => eliminarGrupo(grupo.GrupoId) }
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

            // manual toggle for dropdown so it works even if Bootstrap's JS isn't initializing dynamic elements
            settingsButton.addEventListener('click', function (ev) {
                ev.stopPropagation();
                // close other dropdowns
                document.querySelectorAll('.card-layout .dropdown-menu.show').forEach(dm => {
                    if (dm !== dropdownMenu) dm.classList.remove('show');
                });

                const isShown = dropdownMenu.classList.toggle('show');
                settingsButton.setAttribute('aria-expanded', isShown ? 'true' : 'false');
            });

            // close dropdown when clicking a menu item
            dropdownMenu.addEventListener('click', function (ev) {
                // allow item handlers to run (they call preventDefault), then hide
                setTimeout(() => {
                    dropdownMenu.classList.remove('show');
                    settingsButton.setAttribute('aria-expanded', 'false');
                }, 0);
            });

            // assemble card content
            card.appendChild(row);
            card.appendChild(cta);

            // When clicking the card (except on the settings button or the menu), redirect to group materias
            card.addEventListener('click', function (e) {
                if (e.target.closest('.dropdown-menu') || e.target === settingsButton || e.target.closest('button')) return;
                try {
                    window.location.href = `/Docente/GrupoMaterias?grupoId=${grupo.GrupoId}`;
                } catch (err) {
                    console.warn('No se pudo redirigir:', err);
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

// new abrirAccionesGrupo with improved error reporting
async function abrirAccionesGrupo(grupoId) {
    try {
        window.docenteIdGlobal = docenteIdGlobal;

        // Try API endpoint first
        try {
            const resp = await fetch(`/api/Grupos/ObtenerGruposMateriasDocente?docenteId=${docenteIdGlobal}`);
            if (resp.ok) {
                const grupos = await resp.json();
                if (Array.isArray(grupos)) {
                    const grupo = grupos.find(g => parseInt(g.GrupoId) === parseInt(grupoId) || parseInt(g.GrupoId) === grupoId);
                    if (grupo) {
                        if (typeof showGrupoActionsModal === 'function') { showGrupoActionsModal(grupo); return; }
                    }
                }
            } else {
                const txt = await resp.text();
                console.warn('API /api/Grupos/ObtenerGruposMateriasDocente failed', resp.status, txt);
                // continue to fallback
            }
        } catch (apiErr) {
            console.warn('Error calling API endpoint:', apiErr);
            // continue to fallback
        }

        // Fallback to MVC controller endpoints
        let fallbackErrMsg = '';
        const respGr = await fetch(`/Grupos/ObtenerGrupos?docenteId=${docenteIdGlobal}`);
        if (!respGr.ok) {
            const body = await respGr.text().catch(() => '');
            fallbackErrMsg += `ObtenerGrupos failed ${respGr.status}: ${body}\n`;
            throw new Error(fallbackErrMsg || 'No se pudieron obtener grupos (fallback)');
        }

        const gruposSimple = await respGr.json();
        const grupoSimple = gruposSimple.find(g => parseInt(g.GrupoId) === parseInt(grupoId) || parseInt(g.GrupoId) === grupoId);
        if (!grupoSimple) throw new Error('Grupo no encontrado (fallback)');

        // get materias for that group
        let materias = [];
        try {
            const respMat = await fetch(`/Grupos/ObtenerMateriasPorGrupo?grupoId=${grupoId}`);
            if (respMat.ok) {
                materias = await respMat.json();
            } else {
                const t = await respMat.text().catch(() => '');
                console.warn('ObtenerMateriasPorGrupo failed', respMat.status, t);
                fallbackErrMsg += `ObtenerMateriasPorGrupo failed ${respMat.status}: ${t}\n`;
            }
        } catch (matErr) {
            console.warn('Error fetching materias for group:', matErr);
            fallbackErrMsg += `Error fetching materias: ${matErr.message || matErr}\n`;
        }

        const grupoObj = {
            GrupoId: grupoSimple.GrupoId,
            NombreGrupo: grupoSimple.NombreGrupo,
            Descripcion: grupoSimple.Descripcion,
            Materias: Array.isArray(materias) ? materias.map(m => ({ MateriaId: m.MateriaId || m.materiaId || m.materiaId, NombreMateria: m.NombreMateria || m.nombreMateria || m.NombreMateria, Descripcion: m.Descripcion || m.descripcion })) : []
        };

        if (typeof showGrupoActionsModal === 'function') { showGrupoActionsModal(grupoObj); return; }

        throw new Error('No hay función para mostrar modal de acciones de grupo.');

    } catch (err) {
        console.error('Error al abrir acciones de grupo:', err);
        const msg = (err && err.message) ? err.message : String(err);
        Swal.fire({ icon: 'error', title: 'Error', html: `No se pudieron obtener detalles del grupo.<br><pre style="text-align:left;white-space:pre-wrap">${msg}</pre>` });
    }
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
