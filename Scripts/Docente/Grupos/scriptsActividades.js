var div = document.getElementById("docente-datos");
var docenteIdGlobal = div ? div.dataset.docenteid : null;
// cache últimas actividades cargadas para filtrado instantáneo
var actividadesCacheGlobal = null;

// Esperar a que el DOM esté completamente cargado antes de ejecutar el código
document.addEventListener("DOMContentLoaded", function () {

    // inject filter control into UI and attach listener
    try {
        var seccionAct = document.getElementById('seccion-actividades');
        if (seccionAct) {
            // If a control with id 'filtroActividades' already exists on the page,
            // reuse it (attach listeners / restore state) instead of injecting a new one.
            var existing = document.getElementById('filtroActividades');
            if (existing) {
                try {
                    var saved = localStorage.getItem('filtroActividades');
                    if (saved) existing.value = saved;
                } catch (e) { }

                function aplicarFiltroDesdeSelectExistente() {
                    console.debug('Filtro existente cambiado a', existing.value);
                    try { localStorage.setItem('filtroActividades', existing.value); } catch(e){}

                    if (actividadesCacheGlobal && Array.isArray(actividadesCacheGlobal) && actividadesCacheGlobal.length>0) {
                        renderizarActividades(actividadesCacheGlobal);
                    }

                    var midToUse = window.materiaIdGlobal || materiaIdGlobal || null;
                    setTimeout(function(){ cargarActividadesDeMateria(midToUse, true); }, 0);
                }

                existing.addEventListener('change', aplicarFiltroDesdeSelectExistente);
                existing.addEventListener('input', aplicarFiltroDesdeSelectExistente);
                existing.addEventListener('click', function(){ setTimeout(aplicarFiltroDesdeSelectExistente, 0); });
                try { setTimeout(aplicarFiltroDesdeSelectExistente, 10); } catch(e){}
            } else {
                var container = document.createElement('div');
                container.style.marginTop = '10px';
                container.style.display = 'flex';
                container.style.gap = '8px';
                container.style.alignItems = 'center';

                var label = document.createElement('label'); label.style.margin = '0'; label.textContent = 'Filtro: ';
                var select = document.createElement('select'); select.id = 'filtroActividades'; select.className = 'form-select'; select.style.width = 'auto'; select.style.display = 'inline-block';
                [['all','Todas'], ['borrador','Borradores'], ['publicada','Publicadas'], ['programada','Programadas'] ].forEach(function(opt){
                    var o = document.createElement('option'); o.value = opt[0]; o.textContent = opt[1]; select.appendChild(o);
                });
                container.appendChild(label); container.appendChild(select);
                // try to insert before divider if present, otherwise append
                var ref = seccionAct.querySelector('.divider');
                if (ref) seccionAct.insertBefore(container, ref); else seccionAct.appendChild(container);

                // restore previous selection if any
                try {
                    var saved = localStorage.getItem('filtroActividades');
                    if (saved) select.value = saved;
                } catch (e) { }

                function aplicarFiltroDesdeSelect() {
                    console.debug('Filtro cambiado a', select.value);
                    try { localStorage.setItem('filtroActividades', select.value); } catch(e){}

                    // If we have cached activities, render them immediately for snappy UX
                    if (actividadesCacheGlobal && Array.isArray(actividadesCacheGlobal) && actividadesCacheGlobal.length>0) {
                        renderizarActividades(actividadesCacheGlobal);
                    }

                    // Always refresh from server to ensure data is up-to-date and filtering uses latest values
                    // pass explicit materia id to avoid relying on globals
                    var midToUse = window.materiaIdGlobal || materiaIdGlobal || null;
                    setTimeout(function(){ cargarActividadesDeMateria(midToUse, true); }, 0);
                    // don't toggle tabs here; just refresh data. A global delegated listener ensures changes trigger reload.
                }

                select.addEventListener('change', aplicarFiltroDesdeSelect);
                // also listen to 'input' and 'click' for scenarios where change may not fire
                select.addEventListener('input', aplicarFiltroDesdeSelect);
                select.addEventListener('click', function(){ setTimeout(aplicarFiltroDesdeSelect, 0); });
                // trigger once to apply restored selection
                try { setTimeout(aplicarFiltroDesdeSelect, 10); } catch(e){}
            }
        }
    } catch(e){ console.warn('No se pudo inyectar o inicializar filtroActividades', e); }

    cargarActividadesDeMateria();
    

});

    // Función que registra una nueva actividad
async function registrarActividad(enviarAhora) {
    let nombre = document.getElementById("nombre").value.trim();
    let descripcion = document.getElementById("descripcion").value.trim();
        // ahora fecha y hora se seleccionan por separado
        let fechaInput = document.getElementById('fechaLimite');
        let horaInput = document.getElementById('horaLimite');
        let fechaHoraLimite = '';
        if (fechaInput && horaInput) {
            fechaHoraLimite = fechaInput.value && horaInput.value ? `${fechaInput.value}T${horaInput.value}` : '';
        }
    let puntajeInput = document.getElementById("puntaje");
    let puntaje = null;
    if (puntajeInput && puntajeInput.value !== '') {
        puntaje = parseInt(puntajeInput.value, 10);
        if (isNaN(puntaje)) puntaje = null;
    }

    // Referencia al botón para mostrar estado
    var btn = document.querySelector('#crearActividadModal .btn-primary');
    var originalBtnHtml = btn ? btn.innerHTML : null;

    // Validaciones básicas
    if (!nombre || !descripcion || !fechaHoraLimite) {
        Swal.fire({
            icon: "warning",
            title: "Campos incompletos",
            text: "Por favor, completa todos los campos antes de continuar. Si la actividad no tiene puntaje marca 'Sin puntaje'."
        });
        return;
    }

    // Validar que la fecha límite sea mayor a la fecha actual
    let fechaActual = new Date();
    let fechaLimite = new Date(fechaHoraLimite);
    if (fechaLimite <= fechaActual) {
        Swal.fire({
            icon: "warning",
            title: "Fecha inválida",
            text: "La fecha límite debe ser posterior a la fecha actual."
        });
        return;
    }

    // Validar materiaIdGlobal
    if (typeof materiaIdGlobal === 'undefined' || !materiaIdGlobal) {
        Swal.fire({ icon: 'error', title: 'Error en materia', text: 'No se ha identificado la materia seleccionada.' });
        return;
    }

    let actividad = {
        NombreActividad: nombre,
        Descripcion: descripcion,
        FechaLimite: fechaHoraLimite,
        Puntaje: (puntaje === null || puntaje === 0) ? 0 : puntaje,
        MateriaId: parseInt(materiaIdGlobal, 10)
    };
    // enviarAhora = true => publicar; false => borrador; undefined/null => publicar (por compatibilidad)
    actividad.Enviado = (typeof enviarAhora === 'boolean') ? enviarAhora : true;
    // publicar ahora / despues / borrador
    try {
        // previously we used estatus/fecha programada; modal now compact => nothing to read
    } catch (e) { }

    try {
        // Deshabilitar botón y mostrar spinner
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Publicando...';
        }

        let response = await fetch("/Materias/CrearActividad", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(actividad)
        });

        // Leer respuesta como texto y tratar de parsear JSON (más robusto ante respuestas HTML)
        const text = await response.text();
        let data = null;
        try { data = text ? JSON.parse(text) : null; } catch (e) { data = null; }

        if (!response.ok) {
            const mensaje = data && data.mensaje ? data.mensaje : (text || `Error HTTP: ${response.status}`);
            throw new Error(mensaje);
        }

        Swal.fire({ position: "top-end", title: "Actividad creada", text: actividad.Enviado ? "La actividad ha sido publicada correctamente." : "La actividad fue guardada como borrador.", icon: "success", timer: 1500, showConfirmButton: false });

        // Cerrar modal si está abierto (Bootstrap 4/5)
        try {
            if (window.jQuery && $('#crearActividadModal').modal) {
                $('#crearActividadModal').modal('hide');
            } else if (window.bootstrap) {
                var modalEl = document.getElementById('crearActividadModal');
                var modal = bootstrap.Modal.getInstance(modalEl);
                if (modal) modal.hide();
            }
        } catch (e) { console.warn('No se pudo cerrar el modal:', e); }

        // limpiar formulario
        try { document.getElementById("actividadesForm").reset(); } catch (e) { }

        // recargar lista de actividades
        setTimeout(function () { cargarActividadesDeMateria(); }, 300);

    } catch (error) {
        console.error("Error:", error);
        Swal.fire({
            position: "top-end",
            title: "Error al crear la actividad",
            text: error.message || "Ocurrió un problema al crear la actividad.",
            icon: "error",
            timer: 4000,
            showConfirmButton: true
        });
    } finally {
        // Rehabilitar botón
        if (btn) {
            btn.disabled = false;
            if (originalBtnHtml) btn.innerHTML = originalBtnHtml;
        }
    }
}



// Funcion que carga las actividades a la vista.
async function cargarActividadesDeMateria(midParam, forceReload) {
    const listaActividades = document.getElementById("listaActividadesDeMateria");
    if (!listaActividades) return;
    listaActividades.innerHTML = "<p>Cargando actividades...</p>";

    try {
        // Determine materia id: allow caller to pass it as first param, otherwise use global
        var mid = null;
        if (typeof midParam !== 'undefined' && midParam !== null && midParam !== false) {
            mid = midParam;
        } else {
            mid = (typeof materiaIdGlobal !== 'undefined' && materiaIdGlobal) ? materiaIdGlobal : (window.materiaIdGlobal || null);
        }
        // normalize to number/string
        if (typeof mid === 'object' && mid.hasOwnProperty('value')) mid = mid.value;
        // debug
        console.debug('cargarActividadesDeMateria: mid=', mid, 'forceReload=', !!forceReload);
        if (!mid) throw new Error('Materia no definida');
        // include filtro (if set) so server can return filtered results
        var filtroEl = document.getElementById('filtroActividades');
        var filtroVal = filtroEl ? filtroEl.value : null;
        // normalize base path to avoid accidental '//' protocol-relative URLs when appBasePath === '/'
        const rawBasePath = (window.appBasePath || '');
        const basePath = rawBasePath.replace(/\/$/, '');

        // Try multiple endpoints (MVC and API variants) to be resilient to routing differences
        const endpoints = [
            (basePath || '') + `/Materias/ObtenerActividadesPorMateria?materiaId=${mid}` + (filtroVal ? `&filtro=${encodeURIComponent(filtroVal)}` : ''),
            (basePath || '') + `/api/Actividades/ObtenerActividadesPorMateria?materiaId=${mid}` + (filtroVal ? `&filtro=${encodeURIComponent(filtroVal)}` : ''),
            (basePath || '') + `/api/Materias/ObtenerActividadesPorMateria?materiaId=${mid}` + (filtroVal ? `&filtro=${encodeURIComponent(filtroVal)}` : '')
        ];

        let response = null;
        try {
            response = await tryEndpoints(endpoints, { method: 'GET', headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' });
        } catch (err) {
            console.warn('No endpoint responded for actividades:', err);
            listaActividades.innerHTML = '<p class="mensaje-error">No se pudieron obtener actividades del servidor.</p>';
            return;
        }

        const text = await response.text();
        let payload = null;
        try { payload = text ? JSON.parse(text) : null; } catch (e) { payload = null; }

        if (!response.ok) {
            const mensaje = payload && (payload.mensaje || payload.message) ? (payload.mensaje || payload.message) : (text || `Error HTTP: ${response.status}`);
            console.warn('Error fetching actividades:', response.status, mensaje);
            listaActividades.innerHTML = `<p class="mensaje-error">${mensaje}</p>`;
            return;
        }

        if (payload == null) {
            listaActividades.innerHTML = `<p class="mensaje-error">Respuesta inválida del servidor.</p>`;
            console.warn('Respuesta inválida al obtener actividades:', text);
            return;
        }

        console.debug('cargarActividadesDeMateria: payload keys=', payload && typeof payload === 'object' ? Object.keys(payload) : typeof payload);
        if (!Array.isArray(payload)) {
            if (payload.mensaje || payload.message) {
                listaActividades.innerHTML = `<p class="mensaje-error">${payload.mensaje || payload.message}</p>`;
                return;
            }
            if (payload.resultado && Array.isArray(payload.resultado)) {
                actividadesCacheGlobal = payload.resultado;
                console.debug('cargarActividadesDeMateria: usando payload.resultado length=', actividadesCacheGlobal.length);
                renderizarActividades(payload.resultado);
                requestAnimationFrame(function(){ try{ renderizarActividades(actividadesCacheGlobal); }catch(e){} });
                return;
            }
            const arr = Object.keys(payload).map(k => payload[k]).find(v => Array.isArray(v));
            if (arr) {
                actividadesCacheGlobal = arr;
                console.debug('cargarActividadesDeMateria: found array in payload keys, length=', actividadesCacheGlobal.length);
                renderizarActividades(arr);
                requestAnimationFrame(function(){ try{ renderizarActividades(actividadesCacheGlobal); }catch(e){} });
                return;
            }

            listaActividades.innerHTML = `<p class="mensaje-error">No se encontraron actividades.</p>`;
            console.warn('Payload no es array:', payload);
            return;
        }

        // cache payload for instant client-side filtering
        actividadesCacheGlobal = Array.isArray(payload) ? payload : [];
        console.debug('cargarActividadesDeMateria: received array payload length=', actividadesCacheGlobal.length);
        renderizarActividades(actividadesCacheGlobal);
        requestAnimationFrame(function(){ try{ renderizarActividades(actividadesCacheGlobal); }catch(e){} });
    } catch (error) {
        console.error('Error en cargarActividadesDeMateria:', error);
        listaActividades.innerHTML = `<p class="mensaje-error">${error.message}</p>`;
    }
}

//Renderiza actividades despues de confirmar existencia
function renderizarActividades(actividades) {
    const listaActividades = document.getElementById("listaActividadesDeMateria");
    if (!listaActividades) return;
    listaActividades.innerHTML = ""; // Limpiar el contenedor

    if (!actividades || actividades.length === 0) {
        listaActividades.innerHTML = "<p>No hay actividades registradas para esta materia.</p>";
        return;
    }
    // work on a copy to avoid mutating the cached array (reverse is in-place)
    const actividadesToRender = actividades.slice().reverse();

    // get filter
    const filtroEl = document.getElementById('filtroActividades');
    const filtro = filtroEl ? filtroEl.value : 'all';
    // normalize filter: allow plural values from markup (e.g. 'publicadas')
    const filtroNorm = (filtro === 'all') ? 'all' : (String(filtro).endsWith('s') ? String(filtro).slice(0, -1) : String(filtro));

    // apply filter client-side to get the list to actually render
    const filteredActividades = actividadesToRender.filter(actividad => {
        // Normalize 'Enviado' value (may come as boolean, string or number)
        let enviadoVal = actividad.Enviado;

        if (typeof enviadoVal === 'undefined' && actividad.enviado !== undefined) enviadoVal = actividad.enviado;

        const enviadoBool = (enviadoVal === true) || (String(enviadoVal).toLowerCase() === 'true') || (String(enviadoVal) === '1') || (enviadoVal === 1);
        const fechaProgVal = actividad.FechaProgramada || actividad.fechaProgramada || actividad.FechaProgramada;
        const estadoKey = enviadoBool ? 'publicada' : (fechaProgVal ? 'programada' : 'borrador');

        if (filtroNorm === 'all')
            return true;
        if (filtroNorm === 'borrador')
            return estadoKey === 'borrador';
        if (filtroNorm === 'publicada')
            return estadoKey === 'publicada';
        if (filtroNorm === 'programada')
            return estadoKey === 'programada';
        return true;
    });

    console.debug('renderizarActividades: total=', actividadesToRender.length, 'filtradas=', filteredActividades.length, 'filtro=', filtroNorm);

    // render the filtered list
    renderActividadesDirect(filteredActividades);
    // ensure UI repaint in case container was hidden or needs reflow (fixes issue where list only updates after switching tabs)
    try {
        var lista = document.getElementById('listaActividadesDeMateria');
        if (lista) {
            // force reflow
            lista.style.display = 'none';
            // reading offsetHeight forces layout
            void lista.offsetHeight;
            lista.style.display = '';
            // reset scroll
            lista.scrollTop = 0;
        }
    } catch (e) { }
}

// Render a given list of activities directly (no further filtering)
function renderActividadesDirect(listado) {
    const listaActividades = document.getElementById("listaActividadesDeMateria");
    if (!listaActividades) return;
    // assume container already cleared
    listado.forEach(actividad => {
        let enviadoVal = actividad.Enviado;
        if (typeof enviadoVal === 'undefined' && actividad.enviado !== undefined) enviadoVal = actividad.enviado;
        const enviadoBool = (enviadoVal === true) || (String(enviadoVal).toLowerCase() === 'true') || (String(enviadoVal) === '1') || (enviadoVal === 1);
        const fechaProgVal = actividad.FechaProgramada || actividad.fechaProgramada || actividad.FechaProgramada;
        const descripcionActividadConEnlace = convertirUrlsEnEnlaces(actividad.Descripcion);
        const estado = enviadoBool ? 'Publicado' : (fechaProgVal ? 'Programada' : 'Borrador');

        const actividadItem = document.createElement('div');
        actividadItem.classList.add('actividad-item');
        actividadItem.innerHTML = `
            <div class="icono">📋</div>
            <div class="info">
                <strong>${escapeHtml(actividad.NombreActividad)}</strong>
                <p class="fecha-publicado">Publicado: ${formatearFecha(actividad.FechaCreacion)}</p>
                <p class="puntaje" style="font-weight: bold; color: #d35400;">Puntaje: ${actividad.Puntaje}</p>
                <p class="actividad-descripcion oculto">${descripcionActividadConEnlace}</p>
                <p class="ver-completo">Ver completo</p>
            </div>
            <div class="fecha-entrega">
                <strong>Fecha de entrega:</strong><br>
                ${formatearFecha(actividad.FechaLimite)}
            </div>
            <div class="botones-container">
                <button class="btn btn-sm btn-primary btn-ir-actividades" data-id="${actividad.ActividadId}">Ver / Entregar</button>
                <button class="btn btn-sm btn-warning editar-btn" data-id="${actividad.ActividadId}">Editar</button>
                <button class="btn btn-sm btn-danger eliminar-btn" data-id="${actividad.ActividadId}">Eliminar</button>
            </div>
        `;

        const verCompleto = actividadItem.querySelector(".ver-completo");
        const descripcion = actividadItem.querySelector(".actividad-descripcion");
        if (verCompleto && descripcion) {
            verCompleto.addEventListener("click", () => {
                if (descripcion.classList.contains("oculto")) {
                    descripcion.classList.remove("oculto");
                    descripcion.classList.add("visible");
                } else {
                    descripcion.classList.remove("visible");
                    descripcion.classList.add("oculto");
                }
            });
        }

        const btnEliminar = actividadItem.querySelector(".eliminar-btn");
        const btnEditar = actividadItem.querySelector(".editar-btn");
        const btnIrActividad = actividadItem.querySelector(".btn-ir-actividades");
        if (btnEliminar) btnEliminar.addEventListener("click", () => eliminarActividad(actividad.ActividadId));
        if (btnEditar) btnEditar.addEventListener("click", () => editarActividad(actividad.ActividadId));
        if (btnIrActividad) btnIrActividad.addEventListener("click", () => IrAActividad(actividad.ActividadId));

        listaActividades.appendChild(actividadItem);
    });
}


async function IrAActividad(actividadIdSeleccionada) {
   //guardar el id de la materia para acceder a la materia en la que se entro y usarla en otro script
   localStorage.setItem("actividadSeleccionada", actividadIdSeleccionada);
    // Redirige a la ruta que decide la vista según rol en el servidor
    var url = `/Actividades/DetallesActividad?actividadId=${encodeURIComponent(actividadIdSeleccionada)}`;
    window.open(url, '_blank'); // Abrir en nueva pestaña
}
// Funciones para manejar los botones

// helper para intentar una lista de endpoints en secuencia
async function tryEndpoints(endpoints, fetchOptions) {
    for (let i = 0; i < endpoints.length; i++) {
        try {
            const res = await fetch(endpoints[i], fetchOptions);
            if (res.ok) return res;
            // if server returned JSON with message, continue to next but remember last response
            console.warn('Endpoint failed', endpoints[i], res.status);
        } catch (e) {
            console.warn('Fetch error for', endpoints[i], e);
        }
    }
    throw new Error('Ningún endpoint respondió correctamente.');
}

async function eliminarActividad(id) {
    const result = await Swal.fire({
        title: '¿Estás seguro?',
        text: "¡Esta acción no se puede deshacer!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        reverseButtons: true
    });

    if (!result.isConfirmed) {
        Swal.fire({ title: 'Cancelado', text: 'La actividad no fue eliminada.', icon: 'info', timer: 1500, showConfirmButton: false });
        return;
    }

    Swal.fire({ title: 'Eliminando...', text: `Eliminando actividad ${id}`, allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

    const endpoints = [
        `/Actividades/EliminarActividad?id=${id}`,
    //        `/api/Actividades/EliminarActividad?id=${id}`,
    //      `/api/Actividades/EliminarActividad/${id}`,
    //    `/Materias/EliminarActividad?id=${id}`,
    //  `/Materias/EliminarActividad/${id}`
    ];

    try {
        const resp = await tryEndpoints(endpoints,
            {
                method: 'DELETE', headers:
                {
                    'Content-Type': 'application/json'
                }
            });
        const text = await resp.text();
        let data = null; try { data = text ? JSON.parse(text) : null; } catch (e) { data = null; }
        Swal.close();
        Swal.fire('Eliminado!',
            data &&
                data.mensaje ?
                data.mensaje : `La actividad ${id} fue eliminada.`, 'success'
        );
        cargarActividadesDeMateria();
    } catch (error) {
        console.error('Error al eliminar la actividad:', error);
        Swal.close();
        Swal.fire('Error',
            'No se pudo eliminar la actividad.',
            'error'
        );
    }
}


function formatearFecha(fechaStr) {
    try {
        const d = new Date(fechaStr);
        return d.toLocaleString();
    } catch (e) { return fechaStr; }
}

function convertirUrlsEnEnlaces(texto) {
    var urlRegex = /(https?:\/\/[^\n\s]+)/g;
    return (texto || '').replace(urlRegex, '<a href="$1" target="_blank">$1</a>');
}

// ------------------ EDITAR ACTIVIDAD ------------------
function toInputDateTimeValue(dateStr) {
    if (!dateStr) return '';
    var d = new Date(dateStr);
    if (isNaN(d.getTime())) return '';
    // get local offset ISO without seconds
    var pad = function (n) { return n < 10 ? '0' + n : n; };
    var year = d.getFullYear();
    var month = pad(d.getMonth() + 1);
    var day = pad(d.getDate());
    var hours = pad(d.getHours());
    var minutes = pad(d.getMinutes());
    return `${year}-${month}-${day}T${hours}:${minutes}`;
}

async function editarActividad(id) {
    try {
        // Obtener datos de la actividad
        const resp = await fetch(`/Actividades/ObtenerActividadPorId?actividadId=${id}`);
        if (!resp.ok) throw new Error('No se pudo obtener la actividad');
        const data = await resp.json();

        // llenar formulario
        document.getElementById('nombre').value = data.NombreActividad || '';
        document.getElementById('descripcion').value = data.Descripcion || '';
        // llenar fecha y hora por separado
        try {
            var d = document.getElementById('fechaLimite');
            var h = document.getElementById('horaLimite');
            var fechaISO = data.FechaLimite || data.FechaCreacion;
            if (fechaISO) {
                var dt = new Date(fechaISO);
                if (!isNaN(dt)) {
                    if (d) d.value = `${dt.getFullYear()}-${String(dt.getMonth()+1).padStart(2,'0')}-${String(dt.getDate()).padStart(2,'0')}`;
                    if (h) h.value = `${String(dt.getHours()).padStart(2,'0')}:${String(dt.getMinutes()).padStart(2,'0')}`;
                    // store for modal show handler
                    window._editarFechaISO = fechaISO;
                }
            }
        } catch (e) { console.warn(e); }
        document.getElementById('puntaje').value = (data.Puntaje === 0 || data.Puntaje == null) ? '' : data.Puntaje;

        // establecer materia global si no existe
        if (!materiaIdGlobal && window.materiaIdGlobal) materiaIdGlobal = window.materiaIdGlobal;

        // marcar que estamos editando
        window.editingActividadId = id;

        // preparar botón publicar para actualizar
        var btn = document.getElementById('btnPublicarActividad');
        if (btn) {
            btn.textContent = 'Guardar cambios';
            // quitar listeners previos
            var newBtn = btn.cloneNode(true);
            btn.parentNode.replaceChild(newBtn, btn);
            newBtn.addEventListener('click', async function () {
                await actualizarActividad(id);
            });
        }

        // abrir modal
        try {
            var crearModalEl = document.getElementById('crearActividadModal');
            if (crearModalEl && window.bootstrap) {
                var crearModal = new bootstrap.Modal(crearModalEl);
                crearModal.show();
            } else if (window.jQuery && $('#crearActividadModal').modal) {
                $('#crearActividadModal').modal('show');
            }
        } catch (e) { console.warn(e); }

    } catch (err) {
        console.error(err);
        Swal.fire('Error', 'No se pudo cargar la actividad para edición', 'error');
    }
}

function intNull() { return null; }

async function actualizarActividad(id) {
    // leer campos
    let nombre = document.getElementById('nombre').value.trim();
    let descripcion = document.getElementById('descripcion').value.trim();
    let fechaHoraLimite = document.getElementById('fechaHoraLimite').value;
    let puntajeInput = document.getElementById("puntaje");
    let sinPuntajeCheckbox = document.getElementById("sinPuntaje");
    let puntaje = null;
    if (puntajeInput && !puntajeInput.disabled && puntajeInput.value !== '') {
        puntaje = parseInt(puntajeInput.value, 10);
    }

    if (!nombre || !descripcion || !fechaHoraLimite) {
        Swal.fire({ icon: 'warning', title: 'Campos incompletos', text: 'Completa todos los campos.' });
        return;
    }

    const body = {
        NombreActividad: nombre,
        Descripcion: descripcion,
        FechaLimite: fechaHoraLimite,
        Puntaje: (puntaje === null || puntaje === 0) ? 0 : puntaje
    };

    // incluir estatus y fecha programada si aplica
    // modal ya no contiene estatus/fecha programada

    const endpoints = [
        `/api/Actividades/ActualizarActividad?id=${id}`,
        `/api/Actividades/ActualizarActividad/${id}`,
        `/Actividades/ActualizarActividad?id=${id}`,
        `/Actividades/ActualizarActividad/${id}`
    ];

    Swal.fire({ title: 'Guardando...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

    try {
        const resp = await tryEndpoints(endpoints, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
        const text = await resp.text();
        let data = null; try { data = text ? JSON.parse(text) : null; } catch (e) { data = null; }

        Swal.close();
        Swal.fire({ icon: 'success', title: 'Actividad actualizada' });

        // cerrar modal
        try {
            if (window.jQuery && $('#crearActividadModal').modal) {
                $('#crearActividadModal').modal('hide');
            } else if (window.bootstrap) {
                var modalEl = document.getElementById('crearActividadModal');
                var modal = bootstrap.Modal.getInstance(modalEl);
                if (modal) modal.hide();
            }
        } catch (e) { }

        // limpiar estado
        window.editingActividadId = null;
        try { document.getElementById('actividadesForm').reset(); } catch (e) { }

        // recargar lista
        setTimeout(cargarActividadesDeMateria, 300);

    } catch (e) {
        console.error(e);
        Swal.close();
        Swal.fire('Error', e.message || 'No se pudo actualizar la actividad', 'error');
    }
}

// Global delegated listener: ensures changes to the filter select always reload activities
document.addEventListener('change', function (e) {
    try {
        var t = e.target || e.srcElement;
        if (t && t.id === 'filtroActividades') {
            try { localStorage.setItem('filtroActividades', t.value); } catch (err) { }
            var mid = window.materiaIdGlobal || (typeof materiaIdGlobal !== 'undefined' ? materiaIdGlobal : null);
            cargarActividadesDeMateria(mid, true);
        }
    } catch (err) { /* ignore */ }
});
