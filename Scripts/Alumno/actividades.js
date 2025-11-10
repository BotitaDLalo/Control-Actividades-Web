document.addEventListener('DOMContentLoaded', function () {
    cargarActividadesAlumno();
});

async function cargarActividadesAlumno() {
    const cont = document.getElementById('listaActividadesAlumno');
    cont.innerHTML = '<p>Cargando...</p>';
    try {
        const resp = await fetch(`/Actividades/ObtenerActividadesPorMateria?materiaId=${materiaIdGlobal}`);
        if (!resp.ok) throw new Error('No se pudieron obtener las actividades');
        const data = await resp.json();
        if (!data || data.length === 0) {
            cont.innerHTML = '<p>No hay actividades.</p>';
            return;
        }
        cont.innerHTML = '';
        data.forEach(act => {
            const div = document.createElement('div');
            div.className = 'actividad-item';
            div.innerHTML = `
                <h4>${act.NombreActividad}</h4>
                <p>${act.Descripcion}</p>
                <p>Entrega: ${new Date(act.FechaLimite).toLocaleString()}</p>
                <button class="btn btn-primary" onclick="irAActividad(${act.ActividadId})">Ver / Entregar</button>
            `;
            cont.appendChild(div);
        });
    } catch (e) {
        cont.innerHTML = `<p>Error: ${e.message}</p>`;
    }
}

function irAActividad(id) {
    localStorage.setItem('actividadSeleccionada', id);
    window.location.href = `/Alumno/ActividadDetalle?actividadId=${id}`;
}
