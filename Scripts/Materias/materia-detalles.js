function cambiarSeccion(seccion) {
    const params = new URLSearchParams(window.location.search);

    const materiaId = params.get('materiaId');
    if (!materiaId) return;

    document.querySelectorAll('.seccion').forEach(div => div.style.display = 'none');
    const seccionMostrar = document.getElementById(`seccion-${seccion}`);
    if (seccionMostrar) {
        seccionMostrar.style.display = 'block';
    }

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
