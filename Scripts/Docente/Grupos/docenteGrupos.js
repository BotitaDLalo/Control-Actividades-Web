var div = document.getElementById("docente-datos");
var docenteIdGlobal = div.dataset.docenteid;

// cargarGrupos: obtiene la lista de grupos y actualiza #listaGrupos
async function cargarGrupos() {
    try {
        const resp = await fetch('/Grupos/ObtenerGrupos?docenteId=' + encodeURIComponent(docenteIdGlobal));
        if (!resp.ok) return;
        const list = await resp.json().catch(() => []);
        const cont = document.getElementById('listaGrupos');
        if (!cont) return;
        cont.innerHTML = '';
        if (!Array.isArray(list) || list.length ===0) {
            cont.innerHTML = '<p class="text-muted">No tiene grupos.</p>';
            return;
        }
        list.forEach(function(g) {
            const card = document.createElement('div');
            card.className = 'card mb-3 min-vh-50 position-relative custom-card';
            card.style.maxWidth = '540px';
            card.innerHTML = `
                <div class="dropdown position-absolute top-0 end-0 m-2">
                    <button type="button" aria-expanded="false" data-bs-toggle="dropdown" class="btn btn-light btn-sm position-absolute top-0 end-0 m-2 custom-button-settings ">
                        <svg fill="#000000" width="25px" height="25px" viewBox="002424" xmlns="http://www.w3.org/2000/svg">
                            <circle cx="12" cy="17.5" r="1.5" />
                            <circle cx="12" cy="12" r="1.5" />
                            <circle cx="12" cy="6.5" r="1.5" />
                        </svg>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li>
                            <button type="button" class="dropdown-item btn-editar-grupo" data-id="${g.GrupoId}" data-nombre="${htmlEscape(g.NombreGrupo)}" data-desc="${htmlEscape(g.Descripcion)}">Editar</button>
                        </li>
                        <li>
                            <button type="button" class="dropdown-item text-danger btn-eliminar-grupo" data-id="${g.GrupoId}">Eliminar</button>
                        </li>
                    </ul>
                </div>
                <div class="card-link text-decoration-none text-reset" data-href="/Grupos/GrupoMaterias?grupoId=${g.GrupoId}">
                    <div class="row g-0 h-100">
                        <div class="col-md-4 d-flex align-items-center justify-content-center p-3" style="min-height:180px; background: -webkit-linear-gradient(270deg, #1db7f7,#1db7f7,#06b0f9); background: linear-gradient(270deg, #1db7f7,#1db7f7,#06b0f9);">
                            <img src="/Content/Iconos/grupo_2.svg" class="img-fluid rounded-start" style="object-fit: cover;" alt="..." />
                        </div>
                        <div class="col-md-8 d-flex flex-column">
                            <div class="card-body d-flex flex-column h-100">
                                <div class="flex-grow-1">
                                    <h5 class="card-title">${escapeHtmlText(g.NombreGrupo)}</h5>
                                    <p class="card-text"><small class="text-body-secondary"></small></p>
                                    <p class="card-text limit-lines">${escapeHtmlText(g.Descripcion || '')}</p>
                                </div>
                            </div>
                            <div class="card-bottom ">
                                <p class="card-text"><small class="text-body-secondary">Sin actividades</small></p>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            cont.appendChild(card);
        });
        // Reattach handlers (delegated handlers in Index.cshtml will catch these)
    } catch (e) { console.warn('cargarGrupos error', e); }
}

function htmlEscape(str) { return String(str || '').replace(/"/g,'&quot;'); }
function escapeHtmlText(str) { return String(str || '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }

// eliminarGrupo: llama al endpoint y recarga
async function eliminarGrupo(grupoId) {
    if (!confirm('¿Eliminar grupo?')) return;
    try {
        const r = await fetch('/Grupos/EliminarGrupo?grupoId=' + encodeURIComponent(grupoId), { method: 'DELETE' });
        if (!r.ok) {
            const txt = await r.text().catch(()=>r.statusText);
            alert('No se pudo eliminar: ' + (txt || r.status));
            return;
        }
        alert('Grupo eliminado');
        if (typeof cargarGrupos === 'function') cargarGrupos();
    } catch (e) {
        console.error(e);
        alert('Error al eliminar grupo');
    }
}

// Expose for other scripts
window.cargarGrupos = window.cargarGrupos || cargarGrupos;
window.eliminarGrupo = window.eliminarGrupo || eliminarGrupo;

// Load groups on DOM ready
if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', function() { try { if (typeof cargarGrupos === 'function') cargarGrupos(); } catch (e) { } });
else try { if (typeof cargarGrupos === 'function') cargarGrupos(); } catch (e) { }

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
    console.debug('showEditarGrupoPrompt invoked with:', grupo);
    // If the modal partial is available, use it (more reliable than Swal)
    try {
        var grupoObj = grupo || {};
        if (typeof grupo === 'number' || typeof grupo === 'string') {
            grupoObj = { GrupoId: parseInt(grupo,10) };
        }

        // If the GrupoActionsModal helper exists, open it and switch to config tab
        if (typeof window.showGrupoActionsModal === 'function') {
            try {
                window.showGrupoActionsModal(grupoObj);
                return; // modal will handle saving
            } catch (e) {
                console.warn('showGrupoActionsModal failed, falling back to inline editor', e);
            }
        }

        // si sólo tenemos id, obtener datos actuales del servidor (opcional)
        (async function () {
            try {
                if ((!grupoObj.NombreGrupo || !grupoObj.Descripcion) && grupoObj.GrupoId) {
                    // intentar obtener info rápida desde API MVC endpoint ObtenerGrupos
                    try {
                        var resp = await fetch('/Grupos/ObtenerGrupos?docenteId=' + (window.docenteIdGlobal || ''));
                        if (resp.ok) {
                            var list = await resp.json().catch(() => []);
                            if (Array.isArray(list)) {
                                var found = list.find(function (g) { return (g.GrupoId || g.grupoId) == grupoObj.GrupoId; });
                                if (found) {
                                    grupoObj.NombreGrupo = found.NombreGrupo || found.nombreGrupo || found.Nombre || found.nombre || '';
                                    grupoObj.Descripcion = found.Descripcion || found.descripcion || '';
                                }
                            }
                        }
                    } catch (e) { console.warn('Error fetching grupos for edit fallback', e); }
                }
            } catch (e) { console.warn('Error preparing grupoObj in showEditarGrupoPrompt', e); }

            // Use SweetAlert modal if available, otherwise fallback to prompt
            var formValues = null;
            if (typeof Swal !== 'undefined' && Swal && typeof Swal.fire === 'function') {
                var swalResult = await Swal.fire({
                    title: 'Editar Grupo',
                    html: `
                        <input id="swal-nombre-grupo" class="swal2-input" placeholder="Nombre del grupo" value="${(grupoObj.NombreGrupo || '').replace(/"/g, '&quot;')}">
                        <textarea id="swal-desc-grupo" class="swal2-textarea" placeholder="Descripción">${(grupoObj.Descripcion || '')}</textarea>
                    `,
                    focusConfirm: false,
                    showCancelButton: true,
                    confirmButtonText: 'Guardar',
                    cancelButtonText: 'Cancelar',
                    preConfirm: () => {
                        const nombre = document.getElementById('swal-nombre-grupo').value.trim();
                        if (!nombre) {
                            Swal.showValidationMessage('El nombre es obligatorio');
                            return false;
                        }
                        return {
                            NombreGrupo: nombre,
                            Descripcion: document.getElementById('swal-desc-grupo').value.trim()
                        };
                    }
                });

                if (swalResult && swalResult.value) formValues = swalResult.value;
            } else {
                // fallback to prompt
                try {
                    var nuevoNombre = prompt('Nombre del grupo', grupoObj.NombreGrupo || '');
                    if (nuevoNombre === null) {
                        formValues = null;
                    } else {
                        var nuevaDesc = prompt('Descripción', grupoObj.Descripcion || '');
                        if (nuevaDesc === null) nuevaDesc = grupoObj.Descripcion || '';
                        formValues = { NombreGrupo: nuevoNombre.trim(), Descripcion: nuevaDesc.trim() };
                    }
                } catch (e) {
                    console.warn('Fallback prompt failed', e);
                    formValues = null;
                }
            }

            if (formValues) {
                try {
                    const payload = { GrupoId: grupoObj.GrupoId, NombreGrupo: formValues.NombreGrupo, Descripcion: formValues.Descripcion };
                    console.debug('Updating grupo payload:', payload);
                    const r = await fetch('/api/Grupos/ActualizarGrupo', {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    });
                    if (!r.ok) {
                        const t = await r.text().catch(() => '');
                        throw new Error(t || 'Error al actualizar');
                    }

                    if (typeof Swal !== 'undefined' && Swal && typeof Swal.fire === 'function') {
                        Swal.fire({ position: 'top-end', icon: 'success', title: 'Grupo actualizado', showConfirmButton: false, timer:1400 });
                    } else {
                        alert('Grupo actualizado');
                    }

                    if (typeof cargarGrupos === 'function') cargarGrupos();
                } catch (err) {
                    console.error('Error actualizar grupo', err);
                    if (typeof Swal !== 'undefined' && Swal && typeof Swal.fire === 'function') {
                        Swal.fire({ icon: 'error', title: 'No se pudo actualizar', text: err.message || '' });
                    } else {
                        alert('No se pudo actualizar: ' + (err.message || ''));
                    }
                }
            }
        })();
    } catch (e) {
        console.error('showEditarGrupoPrompt uncaught error', e);
    }
}

// Expose utilities to global scope for other scripts/inline handlers
window.docenteIdGlobal = window.docenteIdGlobal || (document.getElementById('docente-datos') && document.getElementById('docente-datos').dataset ? document.getElementById('docente-datos').dataset.docenteid : null);

// ensure function reference available globally
window.showEditarGrupoPrompt = window.showEditarGrupoPrompt || showEditarGrupoPrompt;

console.debug('docenteGrupos.js loaded. showEditarGrupoPrompt available:', typeof window.showEditarGrupoPrompt === 'function');