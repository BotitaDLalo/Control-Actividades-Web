// Obtener el ID del docente almacenado en localStorage
//let docenteIdGlobal = localStorage.getItem("docenteId");

document.addEventListener("DOMContentLoaded", function () {
    if (materiaIdGlobal && docenteIdGlobal) {
        fetch(`/Materias/ObtenerDetallesMateria?materiaId=${materiaIdGlobal}&docenteId=${docenteIdGlobal}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error("Error en la respuesta de la API");
                }
                return response.json();
            })
            .then(data => {
                if (data.NombreMateria && data.CodigoAcceso && data.CodigoColor) {
                    document.getElementById("materiaNombre").innerText = data.NombreMateria;
                    document.getElementById("codigoAcceso").innerText = data.CodigoAcceso;
                    document.querySelector(".materia-header").style.backgroundColor = data.CodigoColor;
                } else {
                    console.error("No se encontraron datos válidos para esta materia.");
                }
            })
            .catch(error => console.error("Error al obtener los datos de la materia:", error));
    }

    const urlParams = new URLSearchParams(window.location.search);
    const materiaId = urlParams.get('materiaId'); 
    const seccion = urlParams.get('seccion') || 'avisos'; 

    cambiarSeccion(seccion);  

});



function cambiarSeccion(seccion) {
    document.querySelectorAll('.seccion').forEach(div => div.style.display = 'none');
    const seccionMostrar = document.getElementById(`seccion-${seccion}`);
    if (seccionMostrar) {
        seccionMostrar.style.display = 'block';
    }

    document.querySelectorAll('.tab-button').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`button[onclick="cambiarSeccion('${seccion}')"]`).classList.add('active');

    // Cargar datos si se seleccionan secciones dinámicas
    if (seccion === "actividades") {
        cargarActividadesDeMateria(materiaIdGlobal);
    }
    if (seccion === "alumnos") {
        cargarAlumnosAsignados(materiaIdGlobal);
    }
    if (seccion === "avisos") {
        cargarAvisosDeMateria(materiaIdGlobal);
    }
    if (seccion === "entregables") {
        cargarEntregablesDeMateria(materiaIdGlobal);
    }
}


function convertirUrlsEnEnlaces(texto) {
    const urlRegex = /(https?:\/\/[^\s]+)/g;
    return texto.replace(urlRegex, '<a href="$1" target="_blank">$1</a>');
}

// Carga entregables: lista de actividades y un botón para ver entregables por actividad
async function cargarEntregablesDeMateria(materiaId) {
    const cont = document.getElementById('listaEntregables');
    if (!cont) return;
    cont.innerHTML = '<p>Cargando entregables...</p>';
    try {
        const resp = await fetch(`/api/Actividades/ObtenerActividadesPorMateria?materiaId=${encodeURIComponent(materiaId)}`);
        if (!resp.ok) throw new Error('No se pudieron cargar actividades');
        const actividades = await resp.json();
        if (!actividades || actividades.length === 0) {
            cont.innerHTML = '<p class="text-muted">No hay actividades para esta materia.</p>';
            return;
        }
        // Render list of activities with a button to load entregables
        cont.innerHTML = '';
        actividades.forEach(act => {
            const card = document.createElement('div');
            card.className = 'actividad-card mb-2';
            card.innerHTML = `
                <div class="actividad-head d-flex justify-content-between align-items-center" style="padding:10px; border:1px solid #e0e0e0; border-radius:6px; background:#fff;">
                    <div>
                        <strong>${act.NombreActividad || act.NombreActividad}</strong>
                        <div class="text-muted small">Fecha límite: ${act.FechaLimite || ''}</div>
                    </div>
                    <div>
                        <button class="btn btn-sm btn-primary ver-entregables-btn" data-actividadid="${act.ActividadId}">Ver entregables</button>
                    </div>
                </div>
                <div class="entregables-list mt-2" id="entregables_activity_${act.ActividadId}" style="display:none; padding:8px;"></div>
            `;
            cont.appendChild(card);
        });

        // attach handlers
        cont.querySelectorAll('.ver-entregables-btn').forEach(btn => {
            btn.addEventListener('click', async function () {
                const actividadId = this.dataset.actividadid;
                const target = document.getElementById('entregables_activity_' + actividadId);
                if (!target) return;
                if (target.style.display === 'block') { target.style.display = 'none'; return; }
                target.style.display = 'block';
                target.innerHTML = '<p class="text-muted">Cargando entregables...</p>';
                try {
                    const r = await fetch(`/api/Actividades/ObtenerAlumnosEntregables?actividadId=${encodeURIComponent(actividadId)}`);
                    if (!r.ok) throw new Error('No se pudieron cargar entregables');
                    const data = await r.json();
                    renderEntregablesForActivity(data, target);
                } catch (e) {
                    target.innerHTML = `<p class="text-danger">Error al cargar entregables</p>`;
                }
            });
        });

    } catch (err) {
        console.error(err);
        cont.innerHTML = '<p class="text-danger">Error al cargar entregables.</p>';
    }
}

function renderEntregablesForActivity(data, container) {
    if (!data || (!data.AlumnosEntregables && typeof data.TotalEntregados === 'undefined')) {
        container.innerHTML = '<p class="text-muted">No hay entregables para esta actividad.</p>';
        return;
    }

    const header = document.createElement('div');
    header.innerHTML = `<p><strong>Entregados:</strong> ${data.TotalEntregados || 0} &nbsp; <strong>Puntaje:</strong> ${data.Puntaje || 0}</p>`;
    container.innerHTML = '';
    container.appendChild(header);

    if (!data.AlumnosEntregables || data.AlumnosEntregables.length === 0) {
        const p = document.createElement('p'); p.className = 'text-muted'; p.textContent = 'Aún no hay entregables recibidos.'; container.appendChild(p); return;
    }

    const list = document.createElement('div');
    list.className = 'list-group';
    data.AlumnosEntregables.forEach(a => {
        const item = document.createElement('div');
        item.className = 'list-group-item d-flex justify-content-between align-items-start';
        item.innerHTML = `
            <div>
                <div><strong>${a.NombreUsuario || (a.Nombres + ' ' + a.ApellidoPaterno)}</strong></div>
                <div class="small text-muted">Entregado: ${a.FechaEntrega ? new Date(a.FechaEntrega).toLocaleString() : '—'}</div>
            </div>
            <div class="d-flex gap-2">
                <button class="btn btn-sm btn-primary" onclick="verRespuestaEntrega(${a.EntregaId || 0})">Ver</button>
                <span class="badge bg-secondary">${a.Calificacion >= 0 ? a.Calificacion : '—'}</span>
            </div>
        `;
        list.appendChild(item);
    });
    container.appendChild(list);
}

function verRespuestaEntrega(entregaId) {
    // Reutilizar modal respuesta existente (if present in EvaluarActividades) or show simple alert
    // Try to open respuestaModal if exists
    const modalEl = document.getElementById('respuestaModal');
    if (modalEl && typeof bootstrap !== 'undefined') {
        // fetch entrega details
        fetch(`/api/Actividades/ObtenerAlumnosEntregables?actividadId=${entregaId}`)
            .then(r => r.json()).then(d => { console.log(d); })
            .catch(() => {});
        var modal = new bootstrap.Modal(modalEl);
        modal.show();
        return;
    }
    alert('Ver respuesta: ' + entregaId);
}


function formatearFecha(fecha) {
    const dateObj = new Date(fecha);
    return dateObj.toLocaleDateString("es-ES", { day: "2-digit", month: "2-digit", year: "numeric" }) +
        " " + dateObj.toLocaleTimeString("es-ES", { hour: "2-digit", minute: "2-digit" });
}
