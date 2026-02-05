// Obtener el ID del docente almacenado en localStorage
var respTxt = '-';
//let docenteIdGlobal = localStorage.getItem("docenteId");

document.addEventListener("DOMContentLoaded", function () {
    const urlParams = new URLSearchParams(window.location.search);
    const materiaId = urlParams.get('materiaId');
    const seccion = urlParams.get('seccion') || 'avisos';

    // Cargar datos de encabezado de materia (no depende de docenteId)
    async function loadMateriaHeader() {
        try {
            const id = (typeof materiaIdGlobal !== 'undefined' && materiaIdGlobal) ? materiaIdGlobal : materiaId;
            if (!id) return;

            // Preferir API que no requiere docenteId
            const resp = await fetch(`/api/Materias/ObtenerMateriaUnica?id=${encodeURIComponent(id)}`);
            if (resp.ok) {
                const data = await resp.json();
                populateHeader(data);
                return;
            }

            // Fallback: intentar endpoint MVC que requiere docenteId (si está disponible)
            const docId = (typeof docenteIdGlobal !== 'undefined' && docenteIdGlobal) ? docenteIdGlobal : 0;
            const resp2 = await fetch(`/Materias/ObtenerDetallesMateria?materiaId=${encodeURIComponent(id)}&docenteId=${encodeURIComponent(docId)}`);
            if (resp2.ok) {
                const data2 = await resp2.json();
                populateHeader(data2);
            }
        } catch (error) {
            console.error('Error al obtener los datos de la materia:', error);
        }
    }

    function populateHeader(data) {
        if (!data) return;
        const name = data.NombreMateria || data.Nombre || data.nombreMateria;
        const codigo = data.CodigoAcceso || data.Codigo || data.codigoAcceso;
        const color = data.CodigoColor || data.codigoColor || '#d63384';
        if (name) {
            const el = document.getElementById('materiaNombre');
            if (el) el.innerText = name;
        }
        if (codigo) {
            const el = document.getElementById('codigoAcceso');
            if (el) el.innerText = codigo;
        }
        try { document.querySelector('.materia-header').style.backgroundColor = color; } catch (e) { }
    }

    // iniciar carga del header
    loadMateriaHeader();

async function cargarEntregablesPorActividad(actividadId) {
    var cont = document.getElementById('listaEntregables');
    if (!cont) return;
    cont.innerHTML = '<p class="text-muted">Cargando entregables...</p>';

    try {
        // Intentar ambos endpoints: MVC y API
        var endpoints = [
            `/Actividades/ObtenerAlumnosEntregables?actividadId=${encodeURIComponent(actividadId)}`,
            `/api/Actividades/ObtenerAlumnosEntregables?actividadId=${encodeURIComponent(actividadId)}`
        ];

        var resp = null;
        for (var i = 0; i < endpoints.length; i++) {
            try {
                resp = await fetch(endpoints[i]);
                if (resp.ok) break;
            } catch (e) { resp = null; }
        }

        if (!resp || !resp.ok) {
            cont.innerHTML = '<p class="text-danger">No se pudieron cargar los entregables.</p>';
            return;
        }

        var data = await resp.json();
        // data puede venir dentro de propiedades (si es MVC devuelve Ok(object))
        // Normalizar al formato esperado por renderEntregablesForActivity
        var normalized = data;
        // Si la respuesta es el objeto RespuestaAlumnosEntregables
        if (data && (data.AlumnosEntregables || typeof data.TotalEntregados !== 'undefined')) {
            normalized = data;
        } else if (data && data.d) {
            normalized = data.d;
        }

        renderEntregablesForActivity(normalized, cont);
    } catch (err) {
        console.error('Error al cargar entregables por actividad:', err);
        cont.innerHTML = '<p class="text-danger">Error al cargar entregables.</p>';
    }
}

    // Mostrar sección solicitada
    cambiarSeccion(seccion);
    var html = '<div class="table-responsive"><table class="table table-sm table-entregables"><thead><tr><th>Alumno</th><th>Respuesta</th><th>Fecha</th><th>Calificación</th><th>Acciones</th></tr></thead><tbody>';

});



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
        const raw = await resp.json();
        // Normalizar distintas formas de respuesta: puede ser un array directo o un objeto { Actividades: [...] }
        let actividades = [];
        if (Array.isArray(raw)) actividades = raw;
        else if (raw && Array.isArray(raw.Actividades)) actividades = raw.Actividades;
        else if (raw && Array.isArray(raw.resultado)) actividades = raw.resultado;
        else {
            // intentar encontrar la primera propiedad que sea un array
            const arr = raw && typeof raw === 'object' ? Object.keys(raw).map(k => raw[k]).find(v => Array.isArray(v)) : null;
            if (arr) actividades = arr;
        }

        if (!actividades || actividades.length === 0) {
            cont.innerHTML = '<p class="text-muted">No hay actividades para esta materia.</p>';
            if (sel) sel.innerHTML = '<option value="0">-- Sin actividades --</option>';
            return;
        }
        // cache and populate select
        actividadesCache = actividades;
        if (sel) {
            sel.innerHTML = '<option value="all">-- Todas las actividades --</option><option value="0">-- Seleccione una actividad --</option>';
            actividades.forEach(a => {
                var opt = document.createElement('option');
                opt.value = a.ActividadId || a.actividadId || a.ActividadId;
                opt.textContent = a.NombreActividad || a.nombreActividad || ('Actividad ' + opt.value);
                sel.appendChild(opt);
            });
            sel.onchange = function () {
                var val = this.value || '0';
                if (val === 'all') {
                    // mostrar todos los entregables agrupados por actividad
                    cargarTodosEntregables(actividades);
                    return;
                }
                var id = parseInt(val || '0');
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
        // Al cargar actividades, por defecto mostramos todos los entregables agrupados
        try { cargarTodosEntregables(actividades); } catch (e) { /* noop */ }
    } catch (err) {
        console.error(err);
        cont.innerHTML = '<p class="text-danger">Error al cargar actividades.</p>';
        if (sel) sel.innerHTML = '<option value="0">-- Error --</option>';
    }
}

// Cargar entregables de todas las actividades y agruparlos
async function cargarTodosEntregables(actividades) {
    var cont = document.getElementById('listaEntregables');
    if (!cont) return;
    cont.innerHTML = '<p class="text-muted">Cargando entregables de todas las actividades...</p>';

    try {
        if (!Array.isArray(actividades) || actividades.length === 0) {
            cont.innerHTML = '<p class="text-muted">No hay actividades para mostrar entregables.</p>';
            return;
        }

        // Realizar peticiones en paralelo (pero limitar en caso de muchas actividades)
        const maxParallel = 10;
        const chunks = [];
        for (let i = 0; i < actividades.length; i += maxParallel) chunks.push(actividades.slice(i, i + maxParallel));

        const resultsMap = {};
        for (const chunk of chunks) {
            const promises = chunk.map(a => {
                const id = a.ActividadId || a.actividadId || a.ActividadId;
                return fetch(`/api/Actividades/ObtenerAlumnosEntregables?actividadId=${encodeURIComponent(id)}`)
                    .then(r => r.ok ? r.json().catch(() => null) : null)
                    .then(data => ({ id: id, meta: a, data: data }))
                    .catch(() => ({ id: id, meta: a, data: null }));
            });

            const res = await Promise.all(promises);
            res.forEach(r => { resultsMap[r.id] = r; });
        }

        // Construir vista agrupada
        renderEntregablesGrouped(resultsMap, actividades, cont);
    } catch (err) {
        console.error('Error al cargar todos los entregables:', err);
        cont.innerHTML = '<p class="text-danger">Error al cargar entregables.</p>';
    }
}

function renderEntregablesGrouped(resultsMap, actividades, container) {
    container.innerHTML = '';
    // Mostrar resumen total
    let total = 0;
    actividades.forEach(a => {
        const id = a.ActividadId || a.actividadId || a.ActividadId;
        const r = resultsMap[id];
        if (r && r.data && Array.isArray(r.data.AlumnosEntregables)) total += r.data.AlumnosEntregables.length;
    });
    const header = document.createElement('div');
    header.innerHTML = `<p><strong>Total entregables:</strong> ${total} &nbsp; <strong>Actividades:</strong> ${actividades.length}</p>`;
    container.appendChild(header);

    actividades.forEach(a => {
        const id = a.ActividadId || a.actividadId || a.ActividadId;
        const r = resultsMap[id];
        const activityTitle = a.NombreActividad || a.nombreActividad || ('Actividad ' + id);

        const actDiv = document.createElement('div');
        actDiv.className = 'actividad-entregables mb-3';
        const h = document.createElement('h6');
        h.textContent = activityTitle;
        actDiv.appendChild(h);

        if (!r || !r.data || !r.data.AlumnosEntregables || r.data.AlumnosEntregables.length === 0) {
            const p = document.createElement('p'); p.className = 'text-muted'; p.textContent = 'No hay entregables.'; actDiv.appendChild(p);
        } else {
            const list = document.createElement('div'); list.className = 'list-group';
            (r.data.AlumnosEntregables || []).forEach(ent => {
                const item = document.createElement('div'); item.className = 'list-group-item d-flex justify-content-between align-items-start';
                const left = document.createElement('div');
                left.innerHTML = `<div><strong>${ent.NombreUsuario || (ent.Nombres + ' ' + ent.ApellidoPaterno)}</strong></div><div class="small text-muted">Entregado: ${ent.FechaEntrega ? new Date(ent.FechaEntrega).toLocaleString() : '—'}</div>`;
                const right = document.createElement('div'); right.className = 'd-flex gap-2 align-items-center';
                const btn = document.createElement('button'); btn.className = 'btn btn-sm btn-primary btn-ver-entrega'; btn.textContent = 'Ver';
                btn.dataset.entregaid = ent.EntregaId || 0; btn.dataset.respuesta = ent.Respuesta || '';
                const badge = document.createElement('span'); badge.className = 'badge bg-secondary'; badge.textContent = (typeof ent.Calificacion !== 'undefined' && ent.Calificacion !== null) ? ent.Calificacion : '—';
                right.appendChild(btn); right.appendChild(badge);
                item.appendChild(left); item.appendChild(right);
                list.appendChild(item);
            });
            // attach listeners
            list.querySelectorAll('.btn-ver-entrega').forEach(function (b) {
                b.addEventListener('click', function () {
                    var respuesta = this.dataset.respuesta || '';
                    var nombre = this.parentNode.parentNode.querySelector('strong') ? this.parentNode.parentNode.querySelector('strong').innerText : '';
                    Swal.fire({ title: 'Respuesta de ' + nombre, html: '<pre style="text-align:left; white-space:pre-wrap;">' + (respuesta || 'Sin respuesta') + '</pre>', width: 800 });
                });
            });
            actDiv.appendChild(list);
        }

        container.appendChild(actDiv);
    });
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
