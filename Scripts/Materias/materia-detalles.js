function cambiarSeccion(seccion) {
    const params = new URLSearchParams(window.location.search);

    const materiaId = params.get('materiaId');
    if (!materiaId) return;

    document.querySelectorAll('.seccion').forEach(div => div.style.display = 'none');
    const seccionMostrar = document.getElementById(`seccion-${seccion}`);
    if (seccionMostrar) {
        seccionMostrar.style.display = 'block';
    }

// --- Importar alumnos desde Excel (copiado desde scriptsAlumnos.js) ---
function createAndOpenFileInputForMateria(grupoId) {
    if (window._importDialogOpen) return;
    window._importDialogOpen = true;

    var input = document.createElement('input');
    input.type = 'file';
    input.accept = '.xlsx,.xls';
    input.style.display = 'none';
    document.body.appendChild(input);

    input.addEventListener('change', async function (ev) {
        try {
            var file = ev.target.files && ev.target.files[0];
            if (!file) return;
            var fd = new FormData();
            fd.append('file', file);
            if (grupoId) fd.append('GrupoId', grupoId);
            try {
                var materiaId = null;
                try { materiaId = window.materiaIdGlobal || new URLSearchParams(window.location.search).get('materiaId'); } catch(e){}
                if (materiaId) fd.append('MateriaId', materiaId);
            } catch(e){}

            console.log('ImportarAlumnosExcel: enviando', file.name, 'MateriaId=', fd.get('MateriaId'), 'GrupoId=', fd.get('GrupoId'));

            var resp;
            try {
                resp = await fetch('/api/Alumnos/ImportarAlumnosExcel', { method: 'POST', body: fd, credentials: 'same-origin' });
            } catch (fetchErr) {
                console.error('Fetch error importar alumnos:', fetchErr);
                alert('Error de red al enviar el archivo. Revisa la consola.');
                return;
            }

            var json = await resp.json().catch(function(){return {};});
            if (!resp.ok) {
                var txt = '';
                try { txt = await resp.text(); } catch (e) { txt = ''; }
                console.error('Import failed', resp.status, txt);
                alert((json && json.mensaje) ? json.mensaje : ('Error importar (HTTP ' + resp.status + '). Revisa la consola.'));
                return;
            }

            try {
                var totalLeidos = json.TotalLeidos || json.Total || 0;
                var agregadosCount = (json.Agregados && Array.isArray(json.Agregados)) ? json.Agregados.length : (json.AgregadosCount || 0);
                var omitidosCount = (json.Omitidos && Array.isArray(json.Omitidos)) ? json.Omitidos.length : (json.OmitidosCount || 0);
                var noEncontradosCount = (json.NoEncontrados && Array.isArray(json.NoEncontrados)) ? json.NoEncontrados.length : (json.NoEncontradosCount || 0);
                var summary = `Total leídos: ${totalLeidos}\nAgregados: ${agregadosCount}\nOmitidos: ${omitidosCount}\nNo encontrados: ${noEncontradosCount}`;
                if (window.Swal && typeof Swal.fire === 'function') {
                    Swal.fire('Importación completa', summary.replace(/\n/g, '<br/>'), 'success');
                } else {
                    alert(summary.replace(/\n/g, '\n'));
                }
            } catch (e) {
                console.warn('No se pudo construir resumen de importación', e);
                if (window.Swal && typeof Swal.fire === 'function') Swal.fire('Importación completa', 'Importación finalizada', 'success');
                else alert('Importación completada');
            }

            // Si la API devuelve la lista de alumnos importados, intentar recargar la lista local
            if (json && Array.isArray(json.Alumnos) && json.Alumnos.length > 0) {
                try { if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(json.Alumnos); } catch(e){}
            } else {
                try { if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(fd.get('MateriaId') || null); } catch(e){}
            }

        } catch (e) {
            console.error(e); alert('Error al subir archivo');
        } finally {
            try { input.remove(); } catch (e) {}
            window._importDialogOpen = false;
        }
    });

    setTimeout(function () { try { input.click(); } catch (e) { console.error(e); window._importDialogOpen = false; } }, 10);
}

function initImportButtonOnMateria() {
    var btn = document.getElementById('btnImportarAlumnos');
    if (!btn) return;
    btn.addEventListener('click', function (e) { e.preventDefault(); createAndOpenFileInputForMateria(); });
}

if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', initImportButtonOnMateria);
else initImportButtonOnMateria();
    // Fin de la función cambiarSeccion

    document.querySelectorAll('.tab-button').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`button[onclick="cambiarSeccion('${seccion}')"]`).classList.add('active');

    // Cargar datos si se seleccionan secciones dinámicas
    if (seccion === "actividades") {
        cargarActividadesDeMateria(materiaId);
    }
    else if (seccion === "alumnos") {
        cargarAlumnosAsignados(materiaId);
    }
    else if (seccion === "avisos") {
        cargarAvisosDeMateria(materiaId);
    }
    else if (seccion === "entregables") {
        cargarEntregablesDeMateria(materiaId);
    }
}





// Carga entregables: lista de actividades y un botón para ver entregables por actividad
var actividadesCache = [];
async function cargarEntregablesDeMateria(materiaId) {
    var sel = document.getElementById('selectActividadEntregables');
    var cont = document.getElementById('listaEntregables');
    if (!cont) return;
    cont.innerHTML = '<p class="text-muted">Cargando actividades...</p>';
    try {
        const resp = await fetch(`/api/Actividades/ObtenerActividadesPorMateria?materiaId=${encodeURIComponent(materiaId)}`);
        if (!resp.ok) throw new Error('No se pudieron cargar actividades');
        const actividades = await resp.json();
        if (!actividades || actividades.length === 0) {
            cont.innerHTML = '<p class="text-muted">No hay actividades para esta materia.</p>';
            if (sel) sel.innerHTML = '<option value="0">-- Sin actividades --</option>';
            return;
        }
        // cache and populate select
        actividadesCache = actividades;
        if (sel) {
            sel.innerHTML = '<option value="0">-- Seleccione una actividad --</option>';
            actividades.forEach(a => {
                var opt = document.createElement('option');
                opt.value = a.ActividadId || a.actividadId || a.ActividadId;
                opt.textContent = a.NombreActividad || a.nombreActividad || ('Actividad ' + opt.value);
                sel.appendChild(opt);
            });
            sel.onchange = function () {
                var id = parseInt(this.value || '0');
                if (id > 0) cargarEntregablesPorActividad(id);
                else cont.innerHTML = '<p class="text-muted">Selecciona una actividad para ver los entregables.</p>';
            };
        }
        // attach search input behavior
        var busc = document.getElementById('buscarActividadEntregables');
        if (busc) {
            var debounceTimer = null;
            busc.value = '';
            busc.oninput = function () {
                clearTimeout(debounceTimer);
                var q = this.value.trim().toLowerCase();
                debounceTimer = setTimeout(function () {
                    // filter actividadesCache
                    if (!actividadesCache || actividadesCache.length === 0) return;
                    var matches = actividadesCache.filter(function (it) {
                        var nombre = (it.NombreActividad || it.nombreActividad || '') + '';
                        return nombre.toLowerCase().indexOf(q) !== -1;
                    });
                    // repoblar select with matches
                    sel.innerHTML = '<option value="0">-- Seleccione una actividad --</option>';
                    matches.forEach(function (a) {
                        var opt = document.createElement('option');
                        opt.value = a.ActividadId || a.actividadId || a.ActividadId;
                        opt.textContent = a.NombreActividad || a.nombreActividad || ('Actividad ' + opt.value);
                        sel.appendChild(opt);
                    });
                    // if only one match, select it
                    if (matches.length === 1) {
                        sel.value = matches[0].ActividadId || matches[0].actividadId || matches[0].ActividadId;
                        sel.dispatchEvent(new Event('change'));
                    }
                }, 250);
            };
        }
        cont.innerHTML = '<p class="text-muted">Selecciona una actividad para ver los entregables.</p>';
    } catch (err) {
        console.error(err);
        cont.innerHTML = '<p class="text-danger">Error al cargar actividades.</p>';
        if (sel) sel.innerHTML = '<option value="0">-- Error --</option>';
    }
}
