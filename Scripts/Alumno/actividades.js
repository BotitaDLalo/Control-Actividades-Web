document.addEventListener('DOMContentLoaded', function () {
    cargarActividadesAlumno();
});

async function cargarActividadesAlumno() {
    const cont = document.getElementById('listaActividadesAlumno');
    if (!cont) {
        console.error('Elemento listaActividadesAlumno no encontrado en el DOM.');
        return;
    }
    cont.innerHTML = '<p>Cargando...</p>';
    try {
        if (typeof materiaIdGlobal === 'undefined' || materiaIdGlobal === 0 || materiaIdGlobal === null) {
            throw new Error('ID de materia no definido.');
        }
        const url = `/Actividades/ObtenerActividadesPorMateria?materiaId=${materiaIdGlobal}`;
        console.debug('Cargando actividades desde:', url);
        const resp = await fetch(url);
        const text = await resp.text();
        let data = null;
        try { data = text ? JSON.parse(text) : null; } catch (e) { data = text; }
        if (!resp.ok) {
            const msg = (data && data.mensaje) ? data.mensaje : (typeof data === 'string' ? data : `HTTP ${resp.status}`);
            throw new Error(msg || 'No se pudieron obtener las actividades');
        }
        // data puede venir como objeto con clave o como arreglo
        let actividades = data;
        if (!Array.isArray(actividades)) {
            // buscar primer array dentro del objeto
            const arr = Array.isArray(data) ? data : (data && typeof data === 'object' ? Object.keys(data).map(k => data[k]).find(v => Array.isArray(v)) : null);
            actividades = arr || [];
        }

        if (!actividades || actividades.length === 0) {
            cont.innerHTML = '<p>No hay actividades.</p>';
            return;
        }
        cont.innerHTML = '';
        actividades.forEach(act => {
            const div = document.createElement('div');
            div.className = 'actividad-item';
            div.innerHTML = `
                <h4>${act.NombreActividad}</h4>
                <p>${act.Descripcion}</p>
                <p>Entrega: ${act.FechaLimite ? new Date(act.FechaLimite).toLocaleString() : 'No disponible'}</p>
                <button class="btn btn-primary" onclick="irAActividad(${act.ActividadId || act.actividadId || 0})">Ver / Entregar</button>
            `;
            cont.appendChild(div);
        });
    } catch (e) {
        console.error('Error cargarActividadesAlumno:', e);
        cont.innerHTML = `<p>Error: ${e.message || 'No se pudieron mostrar las actividades'}</p>`;
    }
}

function irAActividad(id) {
    localStorage.setItem('actividadSeleccionada', id);
    window.location.href = `/Alumno/ActividadDetalle?actividadId=${id}`;
}
