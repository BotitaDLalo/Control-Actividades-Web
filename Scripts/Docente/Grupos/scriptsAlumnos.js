// Lightweight, robust alumnos script: defines cargarAlumnosAsignados, render and import handling.
var div = document.getElementById('docente-datos');
var docenteIdGlobal = div && div.dataset ? div.dataset.docenteid : null;

// Ensure global materiaId is available for this script: try window, localStorage, or URL param
if (typeof window.materiaIdGlobal === 'undefined' || window.materiaIdGlobal === null || window.materiaIdGlobal === 'undefined' || window.materiaIdGlobal === '') {
    try {
        var fromStorage = localStorage.getItem('materiaIdSeleccionada');
        if (fromStorage) window.materiaIdGlobal = fromStorage;
        else {
            var qp = new URLSearchParams(window.location.search);
            var qm = qp.get('materiaId') || qp.get('MateriaId');
            if (qm) window.materiaIdGlobal = qm;
        }
    } catch (e) { /* ignore */ }
}

function renderAlumnosTable(alumnos) {
    var cont = document.getElementById('listaAlumnosAsignados');
    if (!cont) return;
    cont.innerHTML = '';
    if (!alumnos || alumnos.length === 0) {
        cont.innerHTML = '<p class="text-muted">No hay alumnos asignados a esta materia.</p>';
        return;
    }
// This is a new comment added for clarity
    var table = document.createElement('table'); table.className = 'table table-striped';
    var thead = document.createElement('thead');
    // Removed Estatus column as requested
    thead.innerHTML = '<tr><th>Nombre</th><th>Apellidos</th><th>Email</th><th>Acciones</th></tr>';
    table.appendChild(thead);
    var tbody = document.createElement('tbody');
    alumnos.forEach(function (a) {
        var tr = document.createElement('tr');
        var nombre = a.Nombre || a.nombre || '';
        var ap = (a.ApellidoPaterno || a.apellidoPaterno || '') + ' ' + (a.ApellidoMaterno || a.apellidoMaterno || '');
        // try multiple possible fields for email
        var email = '';
        if (a.Email) email = a.Email;
        else if (a.email) email = a.email;
        else if (a.Correo) email = a.Correo;
        else if (a.correo) email = a.correo;
        else if (a.UserName) email = a.UserName;
        else if (a.userName) email = a.userName;
        else if (a.IdentityUser && (a.IdentityUser.Email || a.IdentityUser.email)) email = a.IdentityUser.Email || a.IdentityUser.email;
        // fallback: sometimes the alumno object is nested inside another object
        else if (a.Alumno && (a.Alumno.Email || a.Alumno.email || a.Alumno.Correo)) email = a.Alumno.Email || a.Alumno.email || a.Alumno.Correo || '';
        // Do not show Estatus column — only show basic info
        tr.innerHTML = '<td>' + nombre + '</td><td>' + ap.trim() + '</td><td>' + email + '</td>';
        var tdAcc = document.createElement('td');
        // action group: delete + estatus dropdown
        var grupoAcc = document.createElement('div'); grupoAcc.className = 'btn-group';

        // eliminar button
        if (a.AlumnoMateriaId || a.alumnoMateriaId) {
            var delBtn = document.createElement('button'); delBtn.className = 'btn btn-sm btn-danger'; delBtn.textContent = 'Eliminar';
            delBtn.addEventListener('click', function () { eliminardelgrupo(a.AlumnoMateriaId || a.alumnoMateriaId); });
            grupoAcc.appendChild(delBtn);
        } else {
            var delBtn = document.createElement('button'); delBtn.className = 'btn btn-sm btn-danger'; delBtn.textContent = 'Eliminar';
            delBtn.addEventListener('click', function () { eliminardelgrupo(a.AlumnoId || a.alumnoId || (a.Alumno && a.Alumno.AlumnoId)); });
            grupoAcc.appendChild(delBtn);
        }

        // removed estatus dropdown — only delete button remains
        tdAcc.appendChild(grupoAcc);
        tr.appendChild(tdAcc);
        tbody.appendChild(tr);
    });
    table.appendChild(tbody);
    cont.appendChild(table);
    // No global dropdown handlers needed since dropdown was removed
}

async function cargarAlumnosAsignados(materiaOrAlumnos) {
    var cont = document.getElementById('listaAlumnosAsignados');
    if (!cont) return;
    try {
        if (Array.isArray(materiaOrAlumnos)) { renderAlumnosTable(materiaOrAlumnos); return; }
        var materiaId = (typeof materiaOrAlumnos !== 'undefined' && materiaOrAlumnos) ? materiaOrAlumnos : (typeof materiaIdGlobal !== 'undefined' ? materiaIdGlobal : (window.materiaIdGlobal || null));
        if (!materiaId) { cont.innerHTML = '<p class="text-muted">No hay materia seleccionada.</p>'; return; }
        var resp = await fetch('/Materias/ObtenerAlumnosPorMateria?materiaId=' + encodeURIComponent(materiaId));
        if (!resp.ok) { cont.innerHTML = '<p class="text-danger">Error al cargar alumnos.</p>'; return; }
        var data = await resp.json();
        var alumnos = [];
        if (data) {
            if (Array.isArray(data)) alumnos = data; // backward compat
            else if (Array.isArray(data.alumnos)) alumnos = data.alumnos;
        }
        renderAlumnosTable(alumnos);
    } catch (e) { console.error('Error cargarAlumnosAsignados', e); }
}

async function eliminardelgrupo(enlaceId) {
    if (!enlaceId) return;
    if (!confirm('¿Eliminar alumno?')) return;
    try {
        var r = await fetch('/Materias/EliminarAlumnoDeMateria?idEnlace=' + encodeURIComponent(enlaceId), { method: 'DELETE' });
        if (!r.ok) throw new Error('No eliminado');
        alert('Alumno eliminado');
        if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(materiaIdGlobal);
    } catch (e) { console.error(e); alert('Error al eliminar alumno'); }
}

// Expose globally
window.cargarAlumnosAsignados = cargarAlumnosAsignados;
window.eliminardelgrupo = eliminardelgrupo;

// Search / suggestions for asignar alumno
function renderSugerencias(items) {
    var ul = document.getElementById('sugerenciasAlumnos');
    if (!ul) return;
    ul.innerHTML = '';
    if (!items || items.length === 0) {
        ul.style.display = 'none';
        return;
    }
    items.forEach(function (it) {
        var li = document.createElement('li');
        li.className = 'list-group-item list-group-item-action';
        var display = (it.Nombre || '') + ' ' + (it.ApellidoPaterno || '') + ' ' + (it.ApellidoMaterno || '');
        display = display.trim();
        if (!display) display = it.Email || it.email || it.UserName || '';
        li.textContent = display + (it.Email ? (' — ' + it.Email) : '');
        li.addEventListener('click', function () {
            var input = document.getElementById('buscarAlumno');
            if (input) input.value = it.Email || it.email || '';
            ul.innerHTML = '';
            ul.style.display = 'none';
        });
        ul.appendChild(li);
    });
    ul.style.display = 'block';
}

document.addEventListener('DOMContentLoaded', function () {
    var buscar = document.getElementById('buscarAlumno');
    var btnAsignar = document.getElementById('btnAsignarAlumno');
    var debounceTimer = null;

    if (buscar) {
        buscar.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            var q = this.value.trim();
            if (!q) { renderSugerencias([]); return; }
            debounceTimer = setTimeout(async function () {
                try {
                    var resp = await fetch('/Materias/BuscarAlumnosPorCorreo?query=' + encodeURIComponent(q));
                    if (!resp.ok) { renderSugerencias([]); return; }
                    var data = await resp.json();
                    renderSugerencias(data || []);
                } catch (e) { console.error('Error buscar alumnos', e); renderSugerencias([]); }
            }, 300);
        });

        // hide suggestions when clicking outside
        document.addEventListener('click', function (ev) {
            var ul = document.getElementById('sugerenciasAlumnos');
            if (!ul) return;
            if (!ev.target.closest('#sugerenciasAlumnos') && ev.target !== buscar) {
                ul.innerHTML = '';
                ul.style.display = 'none';
            }
        });
    }

    if (btnAsignar) {
        btnAsignar.addEventListener('click', async function () {
            var input = document.getElementById('buscarAlumno');
            if (!input) return;
            var correo = input.value.trim();
            if (!correo) { alert('Ingresa el correo del alumno'); return; }
            if (!window.materiaIdGlobal) {
                alert('No se pudo identificar la materia');
                return;
            }
            try {
                var body = new URLSearchParams();
                body.append('correo', correo);
                body.append('materiaId', window.materiaIdGlobal);
                var resp = await fetch('/Materias/AsignarAlumnoMateria', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: body.toString() });
                if (!resp.ok) {
                    var txt = await resp.text().catch(()=>'');
                    alert('Error al asignar alumno: ' + (txt || resp.status));
                    return;
                }
                // success -> recargar lista y limpiar
                alert('Alumno asignado correctamente');
                if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(window.materiaIdGlobal);
                input.value = '';
                renderSugerencias([]);
            } catch (e) { console.error(e); alert('Error al asignar alumno'); }
        });
    }
});

// NOTE: Estatus change UI/endpoint was removed; no client-side function needed.

// Import button handling (attach safely whether DOMContentLoaded already fired or not)
function _initImportButton() {
    try {
        var btn = document.getElementById('btnImportarAlumnos');
        if (!btn) return;

        function createAndOpenFileInput(grupoId) {
            if (window._importDialogOpen) return;
            window._importDialogOpen = true;

            var input = document.getElementById('fileImportarAlumnos');
            if (input) { try { input.remove(); } catch (e) { } }
            input = document.createElement('input'); input.type = 'file'; input.accept = '.xlsx,.xls'; input.id = 'fileImportarAlumnos'; input.style.display = 'none';
            document.body.appendChild(input);
            input.addEventListener('change', async function (ev) {
                try {
                    var file = ev.target.files && ev.target.files[0]; if (!file) return;
                    var fd = new FormData(); fd.append('file', file);
                    if (grupoId) fd.append('GrupoId', grupoId);
                    if (typeof window.materiaIdGlobal !== 'undefined' && window.materiaIdGlobal) fd.append('MateriaId', window.materiaIdGlobal);
                    console.log('ImportarAlumnosExcel: Enviando archivo', file.name, 'MateriaId=', window.materiaIdGlobal, 'GrupoId=', grupoId);
                    var resp;
                    try {
                        // include credentials so cookie authentication is sent
                        resp = await fetch('/api/Alumnos/ImportarAlumnosExcel', { method: 'POST', body: fd, credentials: 'same-origin' });
                    } catch (fetchErr) {
                        console.error('Fetch error importar alumnos:', fetchErr);
                        alert('Error de red al enviar el archivo. Revisa la consola.');
                        return;
                    }
                    var json = await resp.json().catch(function(){return {};});
                    if (!resp.ok) {
                        // Try to show response text for debugging
                        var txt = '';
                        try { txt = await resp.text(); } catch (e) { txt = ''; }
                        console.error('Import failed', resp.status, txt);
                        alert((json && json.mensaje) ? json.mensaje : ('Error importar (HTTP ' + resp.status + '). Revisa la consola.'));
                        return;
                    }

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

                    if (json && Array.isArray(json.Alumnos) && json.Alumnos.length > 0) {
                        if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(json.Alumnos);
                    } else {
                        if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(window.materiaIdGlobal);
                    }
                } catch (e) {
                    console.error(e); alert('Error al subir archivo');
                } finally {
                    try { input.remove(); } catch (e) { }
                    window._importDialogOpen = false;
                }
            });
            setTimeout(function () { try { input.click(); } catch (e) { console.error(e); window._importDialogOpen = false; } }, 10);
        }

        btn.addEventListener('click', function (e) { e.preventDefault(); createAndOpenFileInput(); });
    } catch (e) { console.error('_initImportButton error', e); }
}

if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', _initImportButton);
else _initImportButton();
