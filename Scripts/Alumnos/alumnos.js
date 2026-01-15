
//AsignarAlumnoMateria

async function AsignarAlumnoMateria() {
    const correo = document.getElementById("buscarAlumno").value.trim();

    if (!correo) {
        alert("Ingrese un correo");
        return;
    }

    try {

        const params = new URLSearchParams(window.location.search);

        const materiaId = params.get('materiaId');
        if (!materiaId) return;

        const response = await fetch("/Materias/AsignarAlumnoMateria", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ correo,  materiaId})
        });

        const data = await response.json();

        if (data.success) {

            cargarAlumnosAsignados(materiaId);

            Swal.fire({
                icon: 'success',
                //title: '¡Éxito!',
                text: 'Alumno asignado correctamente',
                timer: 2000,
                showConfirmButton: false
            });
        }
    } catch (err) {
        return;
    } finally {
        document.getElementById("buscarAlumno").value = "";
    }
}



async function cargarAlumnosAsignados(materiaOrAlumnos) {
    var cont = document.getElementById('listaAlumnosAsignados');
    if (!cont) return;
    try {
        if (Array.isArray(materiaOrAlumnos)) { renderAlumnosTable(materiaOrAlumnos); return; }
        //var materiaId = (typeof materiaOrAlumnos !== 'undefined' && materiaOrAlumnos) ? materiaOrAlumnos : (typeof materiaIdGlobal !== 'undefined' ? materiaIdGlobal : (window.materiaIdGlobal || null));
        //if (!materiaId) { cont.innerHTML = '<p class="text-muted">No hay materia seleccionada.</p>'; return; }

        const params = new URLSearchParams(window.location.search);

        const materiaId = params.get('materiaId');
        if (!materiaId) return;

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

async function EliminarAlumnoDeMateria(enlaceId) {
    if (!enlaceId) return;
    if (!confirm('¿Eliminar alumno?')) return;
    try {
        const params = new URLSearchParams(window.location.search);

        const materiaId = params.get('materiaId');
        if (!materiaId) return;


        var r = await fetch('/Materias/EliminarAlumnoDeMateria?idEnlace=' + encodeURIComponent(enlaceId), { method: 'DELETE' });
        if (!r.ok) throw new Error('No eliminado');
        alert('Alumno eliminado');
        if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(materiaId);
    } catch (e) { console.error(e); alert('Error al eliminar alumno'); }
    finally {
        document.getElementById("buscarAlumno").value = "";
    }
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
            delBtn.addEventListener('click', function () { EliminarAlumnoDeMateria(a.AlumnoMateriaId || a.alumnoMateriaId); });
            grupoAcc.appendChild(delBtn);
        } else {
            var delBtn = document.createElement('button'); delBtn.className = 'btn btn-sm btn-danger'; delBtn.textContent = 'Eliminar';
            delBtn.addEventListener('click', function () { EliminarAlumnoDeMateria(a.AlumnoId || a.alumnoId || (a.Alumno && a.Alumno.AlumnoId)); });
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
