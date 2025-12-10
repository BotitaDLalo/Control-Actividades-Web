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
        const left = document.createElement('div');
        left.innerHTML = `<div><strong>${a.NombreUsuario || (a.Nombres + ' ' + a.ApellidoPaterno)}</strong></div><div class="small text-muted">Entregado: ${a.FechaEntrega ? new Date(a.FechaEntrega).toLocaleString() : '—'}</div>`;
        const right = document.createElement('div'); right.className = 'd-flex gap-2 align-items-center';
        const btn = document.createElement('button'); btn.className = 'btn btn-sm btn-primary btn-ver-entrega'; btn.textContent = 'Ver';
        btn.dataset.entregaid = a.EntregaId || 0; btn.dataset.respuesta = a.Respuesta || '';
        btn.dataset.alumnonombre = (a.NombreUsuario || (a.Nombres + ' ' + a.ApellidoPaterno)) || '';
        const badge = document.createElement('span'); badge.className = 'badge bg-secondary'; badge.textContent = (a.Calificacion >= 0 ? a.Calificacion : '—');
        right.appendChild(btn); right.appendChild(badge);
        item.appendChild(left); item.appendChild(right);
        list.appendChild(item);
    });
    // attach listeners for ver buttons
    list.querySelectorAll('.btn-ver-entrega').forEach(function (b) {
        b.addEventListener('click', function () {
            var entregaId = parseInt(this.dataset.entregaid || '0');
            var respuesta = this.dataset.respuesta || '';
            var nombre = this.dataset.alumnonombre || '';
            Swal.fire({ title: 'Respuesta de ' + nombre, html: '<pre style="text-align:left; white-space:pre-wrap;">' + (respuesta || 'Sin respuesta') + '</pre>', width: 800 });
        });
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
