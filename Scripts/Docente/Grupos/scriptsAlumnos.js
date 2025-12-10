// Lightweight, robust alumnos script: defines cargarAlumnosAsignados, render and import handling.
var div = document.getElementById('docente-datos');
var docenteIdGlobal = div && div.dataset ? div.dataset.docenteid : null;

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

// NOTE: Estatus change UI/endpoint was removed; no client-side function needed.

// Import button handling
document.addEventListener('DOMContentLoaded', function () {
    var btn = document.getElementById('btnImportarAlumnos');
    function createAndOpenFileInput(grupoId) {
        var input = document.getElementById('fileImportarAlumnos');
        if (input) { try { input.remove(); } catch (e) { } }
        input = document.createElement('input'); input.type = 'file'; input.accept = '.xlsx,.xls'; input.id = 'fileImportarAlumnos'; input.style.display = 'none';
        document.body.appendChild(input);
        input.addEventListener('change', async function (ev) {
            var file = ev.target.files && ev.target.files[0]; if (!file) return;
            var fd = new FormData(); fd.append('file', file);
            if (grupoId) fd.append('GrupoId', grupoId);
            if (typeof materiaIdGlobal !== 'undefined' && materiaIdGlobal) fd.append('MateriaId', materiaIdGlobal);
            try {
                var resp = await fetch('/api/Alumnos/ImportarAlumnosExcel', { method: 'POST', body: fd });
                var json = await resp.json().catch(function(){return {};});
                if (!resp.ok) { alert(json.mensaje || 'Error importar'); return; }

                // Mostrar resumen detallado de la importación (si la API lo retorna)
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

                // Si la API devuelve la lista de alumnos importados, renderizarlos directamente
                if (json && Array.isArray(json.Alumnos) && json.Alumnos.length > 0) {
                    if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(json.Alumnos);
                } else {
                    if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(materiaIdGlobal);
                }
            } catch (e) { console.error(e); alert('Error al subir archivo'); }
        });
        setTimeout(function () { try { input.click(); } catch (e) { console.error(e); } }, 10);
    }

    if (btn) btn.addEventListener('click', function (e) { e.preventDefault(); createAndOpenFileInput(); });
});
