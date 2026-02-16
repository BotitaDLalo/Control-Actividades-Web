var actividadesData = {};
var docenteIdGlobal = localStorage.getItem("docenteId");
var materiaIdGlobal = localStorage.getItem("materiaIdSeleccionada");
var grupoIdGlobal = localStorage.getItem("grupoIdSeleccionado");
var actividadIdGlobal = localStorage.getItem("actividadSeleccionada");
var puntajeMaximo = null;

// Helper: elimina backdrops residuales y restaura el body para evitar overlay gris pegado
function _cleanupModalBackdrops() {
    try {
        // jQuery if available
        if (window.jQuery) {
            window.jQuery('.modal-backdrop').remove();
        } else {
            var b = document.querySelectorAll('.modal-backdrop');
            b.forEach(function (el) { el.parentNode && el.parentNode.removeChild(el); });
        }
        // quitar clase que evita scroll
        document.body.classList.remove('modal-open');
        document.body.style.overflow = '';
        document.body.style.paddingRight = '';
    } catch (e) { console.warn('cleanup modal backdrops error', e); }
}

function parseServerDate(dateVal) {
    if (!dateVal) return null;
    // If already a Date
    if (dateVal instanceof Date) return dateVal;
    // If number (milliseconds or ticks)
    if (typeof dateVal === 'number') return new Date(dateVal);

    if (typeof dateVal === 'string') {
        // Trim
        var s = dateVal.trim();
        // If string looks like /Date(1620000000000)/ (ASP.NET JSON date)
        var msMatch = s.match(/\/Date\((-?\d+)(?:[-+][0-9]+)?\)\/?/);
        if (msMatch) {
            var ms = parseInt(msMatch[1], 10);
            if (!isNaN(ms)) return new Date(ms);
        }

        // If string is a plain number in quotes
        if (/^\d+$/.test(s)) {
            var n = parseInt(s, 10);
            return new Date(n);
        }

        // Try ISO parse
        var dIso = new Date(s);
        if (!isNaN(dIso.getTime())) return dIso;

        // Try replacing space between date and time
        var s2 = s.replace(' ', 'T');
        var dIso2 = new Date(s2);
        if (!isNaN(dIso2.getTime())) return dIso2;

        // Last resort: try Date.parse and create
        var parsed = Date.parse(s);
        if (!isNaN(parsed)) return new Date(parsed);
    }

    return null;
}

function formatDateToLocale(dateVal) {
    var d = parseServerDate(dateVal);
    if (!d) {
        // if value exists, return raw so it's visible for debugging
        if (dateVal) return String(dateVal);
        return 'No disponible';
    }
    try {
        return d.toLocaleString('es-ES');
    } catch (e) {
        return d.toString();
    }
}


document.addEventListener("DOMContentLoaded", function () {
    // Re-read localStorage at runtime in case another script set the keys after this file was parsed
    actividadIdGlobal = localStorage.getItem("actividadSeleccionada");
    materiaIdGlobal = localStorage.getItem("materiaIdSeleccionada");
    grupoIdGlobal = localStorage.getItem("grupoIdSeleccionado");
    docenteIdGlobal = localStorage.getItem("docenteId");

    if (actividadIdGlobal != null) {
        fetch("/Actividades/ObtenerActividadPorId?actividadId=" + actividadIdGlobal)
            .then(function (response) {
                if (!response.ok) {
                    throw new Error("Error en la respuesta de la API");
                }
                return response.json();
            })
            .then(function (data) {
                console.log('Actividad raw data:', data);
                if (data) {
                    // Exponer datos globalmente para que otras vistas/scripts (EvaluarActividades) puedan leerlos
                    try { window.actividadData = data; } catch (e) { }

                    // Actualizar badge de "Permitir entregas tarde" si viene el campo
                    try {
                        var permitir = null;
                        if (typeof data.PermitirEntregasTarde !== 'undefined') permitir = data.PermitirEntregasTarde;
                        else if (typeof data.PermitirTarde !== 'undefined') permitir = data.PermitirTarde;
                        var badgeEl = document.getElementById('permitirTardeBadge');
                        var btnEl = document.getElementById('togglePermitirTardeBtn');
                        if (badgeEl && permitir !== null) {
                            if (permitir === true || String(permitir).toLowerCase() === 'true') {
                                badgeEl.className = 'badge bg-success';
                                badgeEl.innerText = 'Sí';
                            } else {
                                badgeEl.className = 'badge bg-secondary';
                                badgeEl.innerText = 'No';
                            }
                        }
                        // habilitar botón si existe
                        if (btnEl) {
                            btnEl.disabled = false;
                            // agregar listener robusto aquí (si no existe) para alternar permiso
                            try {
                                if (!btnEl.dataset._toggleAttached) {
                                    btnEl.addEventListener('click', async function () {
                                        try {
                                            var permitir = (badgeEl && badgeEl.innerText === 'Sí');
                                            var nueva = !permitir;
                                            var actividadId = localStorage.getItem('actividadSeleccionada') || (data && data.ActividadId) || (data && data.ActividadId);
                                            if (!actividadId) return alert('Actividad no identificada');
                                            var url = '/api/Actividades/TogglePermitirEntregasTarde?actividadId=' + encodeURIComponent(actividadId) + '&permitir=' + encodeURIComponent(nueva);
                                            var resp = await fetch(url, { method: 'POST', credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                                            if (!resp.ok) {
                                                var txt = await resp.text().catch(() => null);
                                                alert('No se pudo actualizar: ' + (txt || resp.statusText));
                                                return;
                                            }
                                            // actualizar UI local
                                            if (badgeEl) {
                                                if (nueva === true || String(nueva).toLowerCase() === 'true') {
                                                    badgeEl.className = 'badge bg-success';
                                                    badgeEl.innerText = 'Sí';
                                                } else {
                                                    badgeEl.className = 'badge bg-secondary';
                                                    badgeEl.innerText = 'No';
                                                }
                                            }
                                            try { window.actividadData = window.actividadData || {}; window.actividadData.PermitirEntregasTarde = nueva; } catch (e) { }
                                        } catch (e) { console.error(e); alert('Error al actualizar permiso'); }
                                    });
                                    btnEl.dataset._toggleAttached = '1';
                                }
                            } catch (e) { console.warn('No se pudo adjuntar listener toggle', e); }
                        }
                    } catch (e) { console.warn('Error actualizando badge permitirTarde', e); }
                    var nombreElem = document.getElementById("nombreActividad");
                    var descElem = document.getElementById("descripcionActividad");
                    var fechaCreacionElem = document.getElementById("fechaCreacion");
                    var fechaLimiteElem = document.getElementById("fechaLimite");
                    var puntajeElem = document.getElementById("puntajeMaximo");
                    var alumnosEntregadosElem = document.getElementById("alumnosEntregados");
                    var actividadesCalificadasElem = document.getElementById("actividadesCalificadas");

                    if (nombreElem) nombreElem.innerText = data.NombreActividad || "No disponible";
                    if (descElem) descElem.innerText = data.Descripcion || "No disponible";

                    // Log raw date values for debugging
                    console.log('FechaCreacion raw:', data.FechaCreacion);
                    console.log('FechaLimite raw:', data.FechaLimite);

                    if (fechaCreacionElem) fechaCreacionElem.innerText = data.FechaCreacion ? formatDateToLocale(data.FechaCreacion) : "No disponible";
                    if (fechaLimiteElem) fechaLimiteElem.innerText = data.FechaLimite ? formatDateToLocale(data.FechaLimite) : "No disponible";

                    // `Tipo de Actividad` field removed from UI. No assignment needed.
                    if (puntajeElem) puntajeElem.innerText = (data.Puntaje !== undefined && data.Puntaje !== null) ? data.Puntaje : "0";
                    puntajeMaximo = data.Puntaje;
                    if (alumnosEntregadosElem) alumnosEntregadosElem.innerText = data.AlumnosEntregados || "0 de 0";
                    if (actividadesCalificadasElem) actividadesCalificadasElem.innerText = data.ActividadesCalificadas || "0 de 0";

                    // si no teníamos materiaId, obtenerla desde la actividad y guardarla
                    try {
                        if ((!materiaIdGlobal || materiaIdGlobal === 'null' || materiaIdGlobal === 'undefined') && data.MateriaId) {
                            materiaIdGlobal = data.MateriaId;
                            localStorage.setItem('materiaIdSeleccionada', materiaIdGlobal);
                        }
                    } catch (e) { console.warn(e); }
                } else {
                    console.error("No se encontraron datos válidos para esta actividad.");
                }
            })
            .catch(function (error) {
                console.error("Error al obtener los datos de la actividad:", error);
            });
    }

    // Esperar un poco para permitir que materiaIdGlobal se establezca a partir de la actividad
    setTimeout(function () {
        prepararAlumnosYActividades();
    }, 250);
    // Poll para refrescar entregas automáticamente (cada 10s) — evita múltiples intervalos
    try {
        if (!window._actividadEntregasPollingAttached) {
            window._actividadEntregasPollingAttached = true;
            setInterval(function () {
                try { prepararAlumnosYActividades(); } catch (e) { /* noop */ }
            }, 10000);
        }
    } catch (e) { }
});

function prepararAlumnosYActividades() {
    // Cargar lista de alumnos de la materia y luego solicitar las entregas desde la API
    AlumnosDeMateriaParaActividades()
        .then(async function () {
            try {
                var actividadId = localStorage.getItem("actividadSeleccionada");
                if (!actividadId) return;
                const resp = await fetch('/api/Actividades/ObtenerAlumnosEntregables?actividadId=' + encodeURIComponent(actividadId));
                if (!resp.ok) throw new Error('No se pudieron obtener las entregas');
                const data = await resp.json();
                // data should be RespuestaAlumnosEntregables with AlumnosEntregables list
                actividadesData = data || {};

                // construir arrays entregados / noEntregados para compatibilidad con el renderizado
                const alumnos = JSON.parse(localStorage.getItem('alumnos_materia_' + materiaIdGlobal) || '[]');
                const entregados = Array.isArray(data.AlumnosEntregables) ? data.AlumnosEntregables : (data.entregados || []);
                const entregadosAlumnoIds = entregados.map(e => e.AlumnoId);
                const noEntregados = alumnos.filter(a => !entregadosAlumnoIds.includes(a.AlumnoId));

                // Actualizar badges de resumen (Alumnos que han entregado / Actividades calificadas)
                try {
                    var alumnosEntregadosElem = document.getElementById('alumnosEntregados');
                    var actividadesCalificadasElem = document.getElementById('actividadesCalificadas');
                    var totalAlumnos = Array.isArray(alumnos) ? alumnos.length : 0;

                    // totalEntregados: preferir valor enviado por la API si existe
                    var totalEntregados = (typeof data.TotalEntregados === 'number') ? data.TotalEntregados : (new Set(entregadosAlumnoIds)).size;

                    // calcular alumnos con al menos una calificación asignada
                    var alumnosConCalif = 0;
                    try {
                        var mapCalif = {};
                        (entregados || []).forEach(function (it) {
                            try {
                                if (it && (it.Calificacion !== null && typeof it.Calificacion !== 'undefined')) {
                                    mapCalif[it.AlumnoId] = true;
                                }
                            } catch (e) { }
                        });
                        alumnosConCalif = Object.keys(mapCalif).length;
                    } catch (e) { alumnosConCalif = 0; }

                    if (alumnosEntregadosElem) alumnosEntregadosElem.innerText = totalEntregados + ' de ' + totalAlumnos;
                    if (actividadesCalificadasElem) actividadesCalificadasElem.innerText = alumnosConCalif + ' de ' + totalAlumnos;
                } catch (e) { console.warn('No se pudieron actualizar badges de entregados/calificadas', e); }

                // adaptar estructura esperada por renderizarAlumnos
                actividadesData.entregados = entregados;
                actividadesData.noEntregados = noEntregados.map(function (a) {
                    return { AlumnoId: a.AlumnoId, Nombre: a.Nombre, ApellidoPaterno: a.ApellidoPaterno, ApellidoMaterno: a.ApellidoMaterno };
                });

                renderizarAlumnos(actividadesData);
            } catch (err) {
                console.error(err);
            }
        })
        .catch(function (err) { console.error(err); });
}

//function AlumnosDeMateriaParaActividades() {
//    return fetch("/Actividades/AlumnosParaCalificarActividades?materiaId=" + materiaIdGlobal)
//        .then(function (response) {
//            if (!response.ok) throw new Error("No se pudieron cargar los alumnos.");
//            return response.json();
//        })
//        .then(function (alumnos) {
//            localStorage.setItem("alumnos_materia_" + materiaIdGlobal, JSON.stringify(alumnos));
//            console.log("Alumnos guardados en localStorage:", alumnos);
//        })
//        .catch(function (error) {
//            console.error("Error al cargar alumnos:", error);
//        });
//}

function obtenerActividadesParaEvaluar() {
    var alumnos = JSON.parse(localStorage.getItem("alumnos_materia_" + materiaIdGlobal) || "[]");
    var actividadId = localStorage.getItem("actividadSeleccionada");

    if (!actividadId || alumnos.length === 0) {
        console.error("No hay datos suficientes para enviar la solicitud.");
        return;
    }

    var requestData = {
        Alumnos: alumnos,
        ActividadId: parseInt(actividadId)
    };

    fetch("/Actividades/ObtenerActividadesParaEvaluar", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(requestData)
    })
        .then(function (response) { return response.json(); })
        .then(function (data) {
            actividadesData = data;
            console.log("Actividades No Entregadas:", data.noEntregados);
            console.log("Actividades Entregadas:", data.entregados);
            renderizarAlumnos(data);
        })
        .catch(function (error) {
            console.error("Error al obtener las actividades:", error);
        });
}

function renderizarAlumnos(data) {
    var listaEntregados = document.getElementById("listaAlumnosEntregados");
    var listaNoEntregados = document.getElementById("listaAlumnosSinEntregar");

    if (listaEntregados) listaEntregados.innerHTML = "";
    if (listaNoEntregados) listaNoEntregados.innerHTML = "";

    if (data.entregados && listaEntregados) {
        data.entregados.forEach(function (alumno) {
            var nombre = alumno.Nombre || alumno.Nombres || '';
            var apeP = alumno.ApellidoPaterno || '';
            var apeM = alumno.ApellidoMaterno || '';
            var fechaEntrega = alumno.FechaEntrega ? (parseServerDate(alumno.FechaEntrega) ? parseServerDate(alumno.FechaEntrega).toLocaleDateString('es-ES') : 'Sin entregar') : 'Sin entregar';
            var fechaCalificacion = (alumno.FechaCalificacionAsignada || alumno.FechaCalificacionAsignada) ? (parseServerDate(alumno.FechaCalificacionAsignada).toLocaleDateString('es-ES')) : (alumno.FechaCalificacionAsignada ? parseServerDate(alumno.FechaCalificacionAsignada).toLocaleDateString('es-ES') : 'Sin calificar');

            // id para verRespuesta: preferir EntregaId, sino AlumnoActividadId o AlumnoActividadId dentro de objeto
            var idParaVer = alumno.AlumnoActividadId || alumno.AlumnoActividad || alumno.AlumnoActividadId || alumno.AlumnoId;
            // id de entregable (EntregableId) usado para calificar
            var entregableId = alumno.Entrega && alumno.Entrega.EntregaId ? alumno.Entrega.EntregaId : (alumno.EntregaId || alumno.EntregableId || 0);

            var fechaCalifMostrar = 'Sin calificar';
            if (alumno.FechaCalificado) fechaCalifMostrar = formatDateToLocale(alumno.FechaCalificado);

            var alumnoHTML =
                '<div class="list-group-item d-flex justify-content-between align-items-center">' +
                '<div style="flex:1"><h5 class="mb-1" style="font-weight: bold; color: #333;">' + (nombre + ' ' + (alumno.ApellidoPaterno || '') + ' ' + (alumno.ApellidoMaterno || '')).trim() + '</h5>' +
                '<p class="mb-1" style="color: #777;">Entregó: ' + fechaEntrega + '</p>' +
                '<p class="mb-1" style="color: #777;">Calificado: ' + fechaCalifMostrar + '</p>' +
                '</div>' +
                '<div style="display:flex;gap:8px">' +
                '<button class="btn btn-primary btn-sm" onclick="verRespuesta(' + (idParaVer ||0) + ',' + (alumno.AlumnoId ||0) + ')">Ver Respuesta</button>' +
                '<button class="btn btn-warning btn-sm" onclick="abrirModalCalificar(' + (entregableId ||0) + ', ' + puntajeMaximo + ')">Calificar</button>' +
                '</div>' +
                '</div>';

            listaEntregados.innerHTML += alumnoHTML;
        });
    }

    if (data.noEntregados && listaNoEntregados) {
        data.noEntregados.forEach(function (alumno) {
            var alumnoHTML =
                '<div class="list-group-item d-flex justify-content-between align-items-center">' +
                '<div><h5 class="mb-1" style="font-weight: bold; color: #333;">' + alumno.Nombre + ' ' + alumno.ApellidoPaterno + ' ' + alumno.ApellidoMaterno + '</h5>' +
                '<p class="mb-1" style="color: #777;">Entregó: Sin entregar</p></div>' +
                '<span class="badge bg-danger">No entregado</span>' +
                '</div>';

            listaNoEntregados.innerHTML += alumnoHTML;
        });
    }
}

function convertirUrlsEnEnlaces(texto) {
    var urlRegex = /(https?:\/\/[^\s]+)/g;
    return texto.replace(urlRegex, '<a href="$1" target="_blank">$1</a>');
}

// Mostrar la respuesta (archivos / texto) de un alumno en el modal
function verRespuesta(alumnoActividadIdSeleccionada, alumnoId) {
    try {
        const lista = actividadesData.entregados || actividadesData.AlumnosEntregables || actividadesData.entregados || [];
        const found = lista.find(function (e) {
            if (!e) return false;
            // match by EntregableId / EntregaId / AlumnoId / AlumnoActividadId
            if (e.EntregableId && e.EntregableId === alumnoActividadIdSeleccionada) return true;
            if (e.EntregaId && e.EntregaId === alumnoActividadIdSeleccionada) return true;
            if (e.AlumnoActividadId && e.AlumnoActividadId === alumnoActividadIdSeleccionada) return true;
            if (alumnoId && e.AlumnoId && e.AlumnoId === alumnoId) return true;
            return false;
        });

        if (!found) {
            console.warn('No se encontró la entrega para identificador', alumnoActividadIdSeleccionada, 'o alumnoId', alumnoId);
            alert('No se encontró la respuesta del alumno.');
            return;
        }

        // intentar extraer texto/archivos
        var respuestaRaw = null;
        if (found.Contenido || found.Respuesta || found.respuesta) respuestaRaw = found.Contenido || found.Respuesta || found.respuesta;
        if (!respuestaRaw && found.Entrega && (found.Entrega.Contenido || found.Entrega.Respuesta)) respuestaRaw = found.Entrega.Contenido || found.Entrega.Respuesta;

        var html = '';
        if (respuestaRaw) {
            try {
                var parsed = typeof respuestaRaw === 'string' ? JSON.parse(respuestaRaw) : respuestaRaw;
                if (parsed) {
                    if (parsed.Archivos && Array.isArray(parsed.Archivos) && parsed.Archivos.length > 0) {
                        html += '<p><strong>Archivos adjuntos:</strong></p><ul>';
                        parsed.Archivos.forEach(function (a) { html += '<li><a href="' + a + '" target="_blank">' + a.split('/').pop() + '</a></li>'; });
                        html += '</ul>';
                    }
                    // Mostrar link si fue enviado (aceptar varias variantes: Link, Enlaces, links)
                    var posiblesLinks = parsed.Link || parsed.Enlaces || parsed.link || parsed.enlaces || parsed.Links || parsed.links || null;
                    function ensureProtocol(u) {
                        if (!u) return u;
                        var s = String(u).trim();
                        if (/^https?:\/\//i.test(s)) return s;
                        // si empieza con www. agregar https://
                        if (/^www\./i.test(s)) return 'https://' + s;
                        return s;
                    }
                    if (posiblesLinks) {
                        if (Array.isArray(posiblesLinks)) {
                            posiblesLinks.forEach(function (ln) {
                                var href = ensureProtocol(ln);
                                html += '<p><strong>Enlace:</strong> <a href="' + href + '" target="_blank">' + escapeHtml(String(ln)) + '</a></p>';
                            });
                        } else {
                            var href = ensureProtocol(posiblesLinks);
                            html += '<p><strong>Enlace:</strong> <a href="' + href + '" target="_blank">' + escapeHtml(String(posiblesLinks)) + '</a></p>';
                        }
                    }
                    if (parsed.Respuesta) {
                        // Convertir posibles URLs dentro del texto en enlaces
                        html += '<div><strong>Respuesta:</strong><pre style="white-space:pre-wrap;">' + convertirUrlsEnEnlaces(escapeHtml(parsed.Respuesta)) + '</pre></div>';
                    } else if (typeof parsed === 'string') {
                        html += '<div><strong>Respuesta:</strong><pre style="white-space:pre-wrap;">' + convertirUrlsEnEnlaces(escapeHtml(parsed)) + '</pre></div>';
                    }
                }
            } catch (err) {
                // no es JSON -> mostrar texto plano pero convertir URLs en enlaces
                var textoEsc = escapeHtml(String(respuestaRaw));
                var textoConEnlaces = convertirUrlsEnEnlaces(textoEsc);
                html = '<div><strong>Respuesta:</strong><pre style="white-space:pre-wrap;">' + textoConEnlaces + '</pre></div>';
            }
        } else {
            html = '<p>No hay respuesta registrada.</p>';
        }

        var textoElem = document.getElementById('texto-respuesta');
        if (textoElem) textoElem.innerHTML = html;
        var modalEl = document.getElementById('respuestaModal');
        if (modalEl) {
            _cleanupModalBackdrops();
            var modal = new bootstrap.Modal(modalEl, { backdrop: true });
            modal.show();
        }
    } catch (e) {
        console.error(e);
        alert('Error al mostrar la respuesta.');
    }
}

function escapeHtml(unsafe) {
    return unsafe
         .replace(/&/g, "&amp;")
         .replace(/</g, "&lt;")
         .replace(/>/g, "&gt;")
         .replace(/\"/g, "&quot;")
         .replace(/'/g, "&#039;");
}

// Abrir modal de calificar y preparar el formulario
function abrirModalCalificar(entregaId, puntajeMaximo) {
    try {
        var entregaInput = document.getElementById('entregaId');
        if (entregaInput) entregaInput.value = entregaId || 0;
        var calInput = document.getElementById('calificacion');
        if (calInput) {
            calInput.min = 0;
            calInput.max = puntajeMaximo || 100;
            calInput.value = '';
        }
        var modalEl = document.getElementById('calificarModal');
        if (modalEl) {
            // limpiar backdrops residuales antes de mostrar
            _cleanupModalBackdrops();
            var modal = new bootstrap.Modal(modalEl, { backdrop: true });
            modal.show();
        }
    } catch (e) { console.error(e); }
}

// Manejar envío de calificación desde el modal (form id = formCalificacion)
document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('formCalificacion');
    if (!form) return;
    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        var entregaId = parseInt(document.getElementById('entregaId').value || '0',10);
        var cal = parseInt(document.getElementById('calificacion').value || '0',10);
        if (!entregaId || isNaN(cal)) { alert('Entrega o calificación inválida'); return; }
        try {
            // No enviar comentario al API
            const resp = await fetch('/api/Actividades/AsignarCalificacion', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ EntregableId: entregaId, Calificacion: cal })
            });
            if (!resp.ok) throw new Error('Error al guardar la calificación');
            // cerrar modal
            var modalEl = document.getElementById('calificarModal');
            var modal = bootstrap.Modal.getInstance(modalEl);
            if (modal) modal.hide();

            Swal.fire({ icon: 'success', title: 'Calificación guardada', timer:1200, showConfirmButton: false });
            // recargar lista
            prepararAlumnosYActividades();
        } catch (err) {
            console.error(err);
            Swal.fire('Error', err.message || 'No se pudo guardar', 'error');
        }
    });
});

        // Inicializar control toggle para permitir entregas tarde
        // Nota: no deshabilitamos el botón aquí para no interferir con la lógica
        // definida en la vista EvaluarActividades (que añade su propio listener).
        document.addEventListener('DOMContentLoaded', function () {
            try {
                var badge = document.getElementById('permitirTardeBadge');
                var actividad = localStorage.getItem('actividadSeleccionada');
                if (!actividad) return;

                // Forzar que el badge muestre 'No' y estilo secundario inicialmente
                function actualizarBadgeForzado() {
                    if (!badge) return;
                    badge.className = 'badge bg-secondary';
                    badge.innerText = 'No';
                }

                try { actualizarBadgeForzado(); } catch (e) { }
                // No añadimos listeners ni deshabilitamos el botón aquí
            } catch (e) { console.warn(e); }
        });
