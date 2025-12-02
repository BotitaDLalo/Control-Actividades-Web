var div = document.getElementById("docente-datos");
var docenteIdGlobal = div && div.dataset ? div.dataset.docenteid : null;

// Helpers for localStorage keys
function keyForGrupo(grupoId) { return grupoId ? `alumnos_grupo_${grupoId}` : null; }
function keyForMateria(materiaId) { return materiaId ? `alumnos_materia_${materiaId}` : null; }

function saveAlumnosToStorage(alumnos, grupoId, materiaId) {
    try {
        if (grupoId) {
            localStorage.setItem(keyForGrupo(grupoId), JSON.stringify(alumnos));
        } else if (materiaId) {
            localStorage.setItem(keyForMateria(materiaId), JSON.stringify(alumnos));
        }
    } catch (e) { console.warn('No se pudo guardar alumnos en storage', e); }
}


function loadAlumnosFromStorage(grupoId, materiaId) {
    try {
        if (grupoId) {
            const v = localStorage.getItem(keyForGrupo(grupoId));
            return v ? JSON.parse(v) : null;
        }
        if (materiaId) {
            const v = localStorage.getItem(keyForMateria(materiaId));
            return v ? JSON.parse(v) : null;
        }
    } catch (e) { console.warn('No se pudo leer alumnos de storage', e); }
    return null;
}

// Render table and optionally save to storage
function renderAlumnosTable(alumnos, grupoId, materiaId) {
    try {
        const contenedor = document.getElementById("listaAlumnosAsignados");
        if (!contenedor) return;
        contenedor.innerHTML = '';

        if (!alumnos || alumnos.length === 0) {
            contenedor.innerHTML = `<p class="text-muted">No hay alumnos asignados a esta materia.</p>`;
            return;
        }

        const anyHasEmail = alumnos.some(a => (a.Email || a.email));
        const table = document.createElement('table');
        table.className = 'table table-striped';
        const thead = document.createElement('thead');
        const headRow = document.createElement('tr');
        const thNombre = document.createElement('th'); thNombre.textContent = 'Nombre'; headRow.appendChild(thNombre);
        const thApellidos = document.createElement('th'); thApellidos.textContent = 'Apellidos'; headRow.appendChild(thApellidos);
        if (anyHasEmail) { const thEmail = document.createElement('th'); thEmail.textContent = 'Email'; headRow.appendChild(thEmail); }
        const thAcciones = document.createElement('th'); thAcciones.textContent = 'Acciones'; headRow.appendChild(thAcciones);
        thead.appendChild(headRow);
        table.appendChild(thead);

        const tbody = document.createElement('tbody');
        alumnos.forEach(alumno => {
            const tr = document.createElement('tr');
            const nombre = alumno.Nombre || alumno.nombre || '';
            const ap = alumno.ApellidoPaterno || alumno.apellidoPaterno || '';
            const am = alumno.ApellidoMaterno || alumno.apellidoMaterno || '';

            const tdNombre = document.createElement('td'); tdNombre.textContent = nombre || '';
            const tdApellidos = document.createElement('td'); tdApellidos.textContent = `${ap} ${am}`.trim();
            tr.appendChild(tdNombre);
            tr.appendChild(tdApellidos);

            if (anyHasEmail) {
                const tdEmail = document.createElement('td'); tdEmail.textContent = alumno.Email || alumno.email || '';
                tr.appendChild(tdEmail);
            }

            const tdAcc = document.createElement('td');
            const alumnoMateriaId = alumno.AlumnoMateriaId || alumno.alumnoMateriaId || null;
            if (alumnoMateriaId) {
                const btn = document.createElement('button');
                btn.className = 'btn btn-sm btn-danger';
                btn.textContent = 'Eliminar';
                btn.addEventListener('click', function (e) {
                    e.preventDefault();
                    eliminardelgrupo(alumnoMateriaId);
                });
                tdAcc.appendChild(btn);
            }
            tr.appendChild(tdAcc);
            tbody.appendChild(tr);
        });

        table.appendChild(tbody);
        contenedor.appendChild(table);

        // save to storage for offline/fast reload
        try { saveAlumnosToStorage(alumnos, grupoId, materiaId); } catch (e) { console.warn(e); }
    } catch (e) { console.error('Error rendering alumnos table', e); }
}

// Exponer funciones al scope global por si los scripts están encapsulados en bundles
try {
    if (typeof window !== 'undefined') {
        window.cargarAlumnosAsignados = window.cargarAlumnosAsignados || null; // placeholder until function declared
        window.eliminardelgrupo = window.eliminardelgrupo || null;
    }
} catch (e) { /* ignore */ }

// Prevent this script from initializing twice (bundles + direct include)
if (window.__docente_scriptsAlumnosInitialized) {
    console.warn('scriptsAlumnos already initialized, skipping duplicate load');
    // stop executing duplicate script
    // previously we threw here which aborted script execution in some pages
    // simply skip duplicate initialization and allow existing handlers to remain
    // do not abort to avoid breaking other scripts on the page
} 
window.__docente_scriptsAlumnosInitialized = true;

// Esperar a que el DOM esté completamente cargado antes de ejecutar el código
document.addEventListener("DOMContentLoaded", function () {
    // Cargar alumnos asignados a la materia (intenta usar variable global o localStorage)
    try {
        const mid = typeof materiaIdGlobal !== 'undefined' && materiaIdGlobal ? materiaIdGlobal : (window.materiaIdGlobal || localStorage.getItem('materiaIdSeleccionada') || null);
        if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(mid);
    } catch (e) {
        try { if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(); } catch (_) { }
    }

    //Cargar actividades a la materia
    
    document.addEventListener("click", async function (event) {
        if (event.target.id === "btnAsignarAlumno") {
            const correo = document.getElementById("buscarAlumno").value.trim();
            if (!correo) {
                Swal.fire({
                    position: "top-end",
                    icon: "question",
                    title: "Ingrese un correo valido.",
                    showConfirmButton: false,
                    timer: 2500
                });
                return;
            }

            try {
                const response = await fetch(`/Materias/AsignarAlumnoMateria?correo=${correo}&materiaId=${materiaIdGlobal}`, {
                    method: "POST"
                });

                const data = await response.json();

                if (!response.ok) {
                    Swal.fire({
                        title: "Error",
                        text: data.mensaje || "Error al asignar alumno.",
                        icon: "error",
                        confirmButtonColor: "#d33",
                    });
                    return;
                }
                document.getElementById("buscarAlumno").value = "";
                Swal.fire({
                    position: "top-end",
                    title: "Asignado",
                    text: "Alumno asignado correctamente.",
                    icon: "success",
                    timer: 2500
                });
                cargarAlumnosAsignados(materiaIdGlobal);
            } catch (error) {
                Swal.fire({
                    title: "Error",
                    text: "Hubo un problema al asignar al alumno. Inténtalo de nuevo.",
                    icon: "error",
                    confirmButtonColor: "#d33",
                });
            }
        }

        // Nota: no usar document-level handler para abrir archivo múltiples veces
    });

    // --- File import handling: use a single dedicated click handler and a single file input ---
    let isSelectingFile = false;

    function getOrCreateFileInput() {
        let input = document.getElementById('fileImportarAlumnos');
        if (!input) {
            input = document.createElement('input');
            input.type = 'file';
            input.id = 'fileImportarAlumnos';
            input.accept = '.xlsx,.xls';
            input.style.display = 'none';
            document.body.appendChild(input);
            console.debug('scriptsAlumnos: created hidden file input #fileImportarAlumnos');
        }

        // attach change handler only once
        if (!input.dataset.importHandlerAttached) {
            console.debug('scriptsAlumnos: attaching change handler to file input');
            input.addEventListener('change', async function (e) {
                const file = e.target.files[0];
                console.debug('scriptsAlumnos: file input changed, file:', file && file.name);
                if (!file) return;

                const formData = new FormData();
                formData.append('file', file);
                formData.append('MateriaId', materiaIdGlobal);
                // include GrupoId when available (either variable or localStorage)
                const gid = (typeof grupoIdGlobal !== 'undefined' && grupoIdGlobal) ? grupoIdGlobal : (localStorage.getItem('grupoIdSeleccionado') || null);
                if (gid && gid !== '0') formData.append('GrupoId', gid);

                try {
                    console.debug('scriptsAlumnos: uploading file to /api/Alumnos/ImportarAlumnosExcel', { MateriaId: materiaIdGlobal, GrupoId: gid });
                    const resp = await fetch('/api/Alumnos/ImportarAlumnosExcel', {
                        method: 'POST',
                        body: formData
                    });

                    const result = await resp.json();
                    if (!resp.ok) {
                        Swal.fire('Error', result.mensaje || 'Error al importar alumnos', 'error');
                        return;
                    }

                    let summary = `Total leídos: ${result.TotalLeidos}\nAgregados: ${result.Agregados.length}\nOmitidos: ${result.Omitidos.length}\nNo encontrados: ${result.NoEncontrados.length}`;
                    Swal.fire('Importación completa', summary.replace(/\n/g, '<br/>'), 'success');

                    // If API returned the list of alumnos, render them directly. Otherwise refetch by materiaId.
                    if (result && Array.isArray(result.Alumnos) && result.Alumnos.length > 0) {
                        cargarAlumnosAsignados(result.Alumnos);
                    } else {
                        const mid2 = typeof materiaIdGlobal !== 'undefined' && materiaIdGlobal ? materiaIdGlobal : (window.materiaIdGlobal || localStorage.getItem('materiaIdSeleccionada') || null);
                        cargarAlumnosAsignados(mid2);
                    }

                    // Limpiar input
                    input.value = '';
                } catch (err) {
                    console.error(err);
                    Swal.fire('Error', 'Error al subir el archivo', 'error');
                } finally {
                    isSelectingFile = false;
                }
            });
            input.dataset.importHandlerAttached = '1';
        }

        return input;
    }

    // Attach single click handler to the import button (if exists)
    const btnImport = document.getElementById('btnImportarAlumnos');
    if (btnImport) {
        btnImport.addEventListener('click', function (ev) {
            ev.stopPropagation();
            ev.preventDefault();
            if (isSelectingFile) return;
            isSelectingFile = true;
            const input = getOrCreateFileInput();
            setTimeout(() => { input.click(); }, 10);
            setTimeout(() => { isSelectingFile = false; }, 2000);
        });
    }
    // If for any reason the button wasn't found at binding time (dynamic rendering),
    // delegate clicks on document to handle it.
    document.addEventListener('click', function (ev) {
        try {
            if (ev.target && ev.target.id === 'btnImportarAlumnos') {
                console.debug('scriptsAlumnos: import button clicked (delegated)');
                ev.stopPropagation();
                ev.preventDefault();
                if (isSelectingFile) return;
                isSelectingFile = true;
                const input = getOrCreateFileInput();
                setTimeout(() => { input.click(); }, 10);
                setTimeout(() => { isSelectingFile = false; }, 2000);
            }
        } catch (e) { console.warn('Error handling delegated import click', e); }
    });


    // Funcionalidad de búsqueda de alumnos en tiempo real (sugerencias de correo)
    const inputBuscar = document.getElementById("buscarAlumno");
    const listaSugerencias = document.getElementById("sugerenciasAlumnos");
    let indexSugerenciaSeleccionada = -1;

    if (inputBuscar) {
        inputBuscar.addEventListener("input", async function () {
            const query = inputBuscar.value.trim();
            if (query.length < 3) {
                listaSugerencias.innerHTML = "";
                return;
            }

            try {
                const response = await fetch(`/Materias/BuscarAlumnosPorCorreo?query=${query}`);
                if (!response.ok) throw new Error("Error al buscar alumnos");

                const alumnos = await response.json();
                listaSugerencias.innerHTML = "";

                if (alumnos.length === 0) {
                    listaSugerencias.innerHTML = `<li class="list-group-item text-muted">No se encontraron resultados</li>`;
                    return;
                }

                alumnos.forEach((alumno, index) => {
                    const li = document.createElement("li");
                    li.classList.add("list-group-item", "list-group-item-action");
                    li.textContent = `${alumno.Nombre} ${alumno.ApellidoPaterno} ${alumno.ApellidoMaterno} - ${alumno.Email}`;

                    li.addEventListener("click", function () {
                        inputBuscar.value = alumno.Email;
                        listaSugerencias.innerHTML = "";
                    });

                    if (index === indexSugerenciaSeleccionada) {
                        li.classList.add("active");
                    }

                    listaSugerencias.appendChild(li);
                });

            } catch (error) {
                console.error("Error al buscar alumnos:", error);
            }
        });

        // Navegación con teclas en las sugerencias
        inputBuscar.addEventListener("keydown", function (e) {
            const sugerencias = listaSugerencias.getElementsByTagName("li");

            if (e.key === "ArrowDown") {
                if (indexSugerenciaSeleccionada < sugerencias.length - 1) {
                    indexSugerenciaSeleccionada++;
                    actualizarSugerencias();
                }
            } else if (e.key === "ArrowUp") {
                if (indexSugerenciaSeleccionada > 0) {
                    indexSugerenciaSeleccionada--;
                    actualizarSugerencias();
                }
            } else if (e.key === "Enter" && indexSugerenciaSeleccionada >= 0) {
                const selectedSugerencia = sugerencias[indexSugerenciaSeleccionada];
                if (selectedSugerencia) {
                    const correo = selectedSugerencia.textContent.split(" - ")[1];
                    if (correo) {
                        inputBuscar.value = correo;
                        listaSugerencias.innerHTML = "";
                    }
                }
            }
        });
        function actualizarSugerencias() {
            const sugerencias = listaSugerencias.getElementsByTagName("li");
            for (let i = 0; i < sugerencias.length; i++) {
                sugerencias[i].classList.remove("active");
            }
            if (indexSugerenciaSeleccionada >= 0 && indexSugerenciaSeleccionada < sugerencias.length) {
                sugerencias[indexSugerenciaSeleccionada].classList.add("active");
            }
        }
        document.addEventListener("click", function (event) {
            if (!inputBuscar.contains(event.target) && !listaSugerencias.contains(event.target)) {
                listaSugerencias.innerHTML = "";
            }
        });
    }

});

//Carga los alumnos a la materia y los muestra en el div
async function cargarAlumnosAsignados(materiaOrAlumnos) {
    try {
        const contenedor = document.getElementById("listaAlumnosAsignados");
        if (!contenedor) return;

        let alumnos = null;

        // Si recibimos directamente un array de alumnos (resultado de la importación), renderizarlo
        if (Array.isArray(materiaOrAlumnos)) {
            alumnos = materiaOrAlumnos;
            // attempt to determine context ids for storage
            const gid = (typeof grupoIdGlobal !== 'undefined' && grupoIdGlobal) ? grupoIdGlobal : (localStorage.getItem('grupoIdSeleccionado') || null);
            const mid = (typeof materiaIdGlobal !== 'undefined' && materiaIdGlobal) ? materiaIdGlobal : (window.materiaIdGlobal || localStorage.getItem('materiaIdSeleccionada') || null);
            // render and save
            renderAlumnosTable(alumnos, gid, mid);
            return;
        } else {
            // Determinar si debemos consultar por grupo o por materia.
            // Preferir grupo si existe en contexto (grupoIdGlobal or localStorage)
            const gid = (typeof grupoIdGlobal !== 'undefined' && grupoIdGlobal) ? grupoIdGlobal : (localStorage.getItem('grupoIdSeleccionado') || null);
            const midCandidate = (typeof materiaOrAlumnos !== 'undefined' && materiaOrAlumnos) ? materiaOrAlumnos : (typeof materiaIdGlobal !== 'undefined' && materiaIdGlobal ? materiaIdGlobal : (window.materiaIdGlobal || localStorage.getItem('materiaIdSeleccionada') || null));

            // Try load from storage first so UI is instant
            const cached = loadAlumnosFromStorage(gid, midCandidate);
            if (cached && Array.isArray(cached) && cached.length > 0) {
                alumnos = cached;
                // render cached immediately
                renderAlumnosTable(alumnos, gid, midCandidate);
                // (we'll still fetch fresh data below to update storage)
            }

            if (gid) {
                // solicitar lista de alumnos por grupo (API espera POST body { GrupoId })
                const resp = await fetch('/api/Alumnos/ObtenerListaAlumnosGrupo', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ GrupoId: parseInt(gid, 10) })
                });
                if (!resp.ok) {
                    let txt = '';
                    try { txt = await resp.text(); } catch (e) { }
                    throw new Error(txt || 'No se pudieron cargar los alumnos del grupo.');
                }
                alumnos = await resp.json();
                // If server returned some alumnos, update storage and render. If empty, keep cached if exists.
                if (Array.isArray(alumnos) && alumnos.length > 0) {
                    saveAlumnosToStorage(alumnos, gid, null);
                    renderAlumnosTable(alumnos, gid, null);
                } else if (cached && Array.isArray(cached) && cached.length > 0) {
                    // keep showing cached data
                    renderAlumnosTable(cached, gid, null);
                } else {
                    // render empty state
                    renderAlumnosTable([], gid, null);
                }
            } else {
                // fallback a materia
                const materiaIdToUse = midCandidate;
                if (!materiaIdToUse) {
                    contenedor.innerHTML = `<p class="text-muted">No hay materia o grupo seleccionado para mostrar alumnos.</p>`;
                    return;
                }

                // Hacer la petición al servidor por materia
                const response = await fetch(`/Materias/ObtenerAlumnosPorMateria?materiaId=${materiaIdToUse}`);
                if (!response.ok) {
                    // intentar leer mensaje del servidor
                    let txt = '';
                    try { txt = await response.text(); } catch (e) { }
                    throw new Error(txt || "No se pudieron cargar los alumnos.");
                }

                // Convertir la respuesta a JSON
                alumnos = await response.json();
                if (!Array.isArray(alumnos)) {
                    alumnos = [];
                }
                if (alumnos.length > 0) {
                    saveAlumnosToStorage(alumnos, null, materiaIdToUse);
                    renderAlumnosTable(alumnos, null, materiaIdToUse);
                } else if (cached && Array.isArray(cached) && cached.length > 0) {
                    renderAlumnosTable(cached, null, materiaIdToUse);
                } else {
                    renderAlumnosTable([], null, materiaIdToUse);
                }
            }

    } catch (error) {
        console.error("Error al cargar alumnos:", error);
    }
}


//Elimina Alumno del grupo
async function eliminardelgrupo(alumnoMateriaId) {
    try {
        const confirmacion = await Swal.fire({
            title: "¿Estás seguro?",
            text: "Esta acción eliminará al alumno del grupo.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar"
        });

        if (!confirmacion.isConfirmed) return;

        const response = await fetch(`/Materias/EliminarAlumnoDeMateria?idEnlace=${alumnoMateriaId}`, {
            method: "DELETE",
        });

        if (!response.ok) {
            throw new Error("No se pudo eliminar al alumno del grupo.");
        }

        Swal.fire({
            position: "top-end",
            title: "Eliminado",
            text: "El alumno ha sido eliminado del grupo correctamente.",
            icon: "success",
            timer: 2500
        });

        cargarAlumnosAsignados(materiaIdGlobal); // Recargar la lista

    } catch (error) {
        Swal.fire({
            position: "top-end",
            title: "Error",
            text: "Hubo un problema al eliminar al alumno.",
            icon: "error",
            timer: 2500
        });

        console.error("Error al eliminar alumno:", error);
    }
}
