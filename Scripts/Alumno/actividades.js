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
            const actividadItem = document.createElement('div');
            actividadItem.className = 'actividad-item';

            // convert description urls to links if any
            const descripcionConEnlaces = (act.Descripcion || '').toString().replace(/(https?:\/\/[^\s]+)/g, '<a href="$1" target="_blank">$1</a>');

            // estado: for alumnos we assume published since controller filters
            const fechaCreacion = act.FechaCreacion || act.FechaCreacionActividad || '';
            const fechaLimite = act.FechaLimite || act.FechaLimiteActividad || act.FechaLimite;

            actividadItem.innerHTML = `
                <div class="actividad-header">
                    <div class="icono">??</div>
                    <div class="info">
                        <strong>${act.NombreActividad || act.Nombre || ''}</strong>
                        <p class="fecha-publicado">Publicado: ${fechaCreacion ? new Date(fechaCreacion).toLocaleString() : ''}</p>
                        <p class="puntaje" style="font-weight:bold; color:#d35400">Puntaje: ${act.Puntaje || act.puntaje || 0}</p>
                        <p class="actividad-descripcion oculto">${descripcionConEnlaces}</p>
                        <p class="ver-completo">Ver completo</p>
                    </div>
                    <div class="fecha-entrega">
                        <strong>Fecha de entrega:</strong><br>
                        ${fechaLimite ? new Date(fechaLimite).toLocaleString() : 'No disponible'}
                    </div>
                    <div class="botones-container">
                        <button class="btn-ir-actividades btn btn-primary" data-id="${act.ActividadId || act.actividadId || 0}">Ver / Entregar</button>
                    </div>
                </div>
            `;

            // toggle description
            const verCompleto = actividadItem.querySelector('.ver-completo');
            const descripcion = actividadItem.querySelector('.actividad-descripcion');
            if (verCompleto && descripcion) {
                verCompleto.addEventListener('click', () => {
                    if (descripcion.classList.contains('oculto')) {
                        descripcion.classList.remove('oculto');
                        descripcion.classList.add('visible');
                    } else {
                        descripcion.classList.remove('visible');
                        descripcion.classList.add('oculto');
                    }
                });
            }

            // attach button event
            const btn = actividadItem.querySelector('.btn-ir-actividades');
            if (btn) btn.addEventListener('click', function () {
                const id = this.dataset.id || this.getAttribute('data-id');
                irAActividad(id);
            });

            cont.appendChild(actividadItem);
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
