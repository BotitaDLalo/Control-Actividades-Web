var actividadesData = {};
var docenteIdGlobal = localStorage.getItem("docenteId");
var materiaIdGlobal = localStorage.getItem("materiaIdSeleccionada");
var grupoIdGlobal = localStorage.getItem("grupoIdSeleccionado");
var actividadIdGlobal = localStorage.getItem("actividadSeleccionada");
var puntajeMaximo = null;

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
                    var nombreElem = document.getElementById("nombreActividad");
                    var descElem = document.getElementById("descripcionActividad");
                    var fechaCreacionElem = document.getElementById("fechaCreacion");
                    var fechaLimiteElem = document.getElementById("fechaLimite");
                    var tipoElem = document.getElementById("tipoActividad");
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

                    if (tipoElem) tipoElem.innerText = data.TipoActividad || "No disponible";
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

function AlumnosDeMateriaParaActividades() {
    return fetch("/Actividades/AlumnosParaCalificarActividades?materiaId=" + materiaIdGlobal)
        .then(function (response) {
            if (!response.ok) throw new Error("No se pudieron cargar los alumnos.");
            return response.json();
        })
        .then(function (alumnos) {
            localStorage.setItem("alumnos_materia_" + materiaIdGlobal, JSON.stringify(alumnos));
            console.log("Alumnos guardados en localStorage:", alumnos);
        })
        .catch(function (error) {
            console.error("Error al cargar alumnos:", error);
        });
}

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
            // soportar dos formas: antiguo (AlumnoActividadId + Entrega) o nuevo (EntregaId, AlumnoId, Respuesta)
            var nombre = alumno.Nombre || alumno.Nombres || '';
            var apeP = alumno.ApellidoPaterno || alumno.ApellidoPaterno || alumno.ApellidoMaterno ? (alumno.ApellidoPaterno || '') : '';
            var apeM = alumno.ApellidoMaterno || alumno.ApellidoMaterno || '';
            var fechaEntrega = alumno.FechaEntrega ? (parseServerDate(alumno.FechaEntrega) ? parseServerDate(alumno.FechaEntrega).toLocaleDateString('es-ES') : 'Sin entregar') : 'Sin entregar';
            var fechaCalificacion = (alumno.FechaCalificacionAsignada || alumno.FechaCalificacionAsignada) ? (parseServerDate(alumno.FechaCalificacionAsignada).toLocaleDateString('es-ES')) : (alumno.FechaCalificacionAsignada ? parseServerDate(alumno.FechaCalificacionAsignada).toLocaleDateString('es-ES') : 'Sin calificar');

            // id para verRespuesta: preferir EntregaId, sino AlumnoActividadId o AlumnoActividadId dentro de objeto
            var idParaVer = alumno.AlumnoActividadId || alumno.AlumnoActividad || alumno.AlumnoActividadId || alumno.AlumnoId;
            // id de entregable (EntregableId) usado para calificar
            var entregableId = alumno.Entrega && alumno.Entrega.EntregaId ? alumno.Entrega.EntregaId : (alumno.EntregaId || alumno.EntregableId || 0);

            var fechaCalifMostrar = 'Sin calificar';
            if (alumno.FechaCalificado) fechaCalifMostrar = formatDateToLocale(alumno.FechaCalificado);
            var comentarioMostrar = alumno.Comentario ? ('Comentario: ' + escapeHtml(alumno.Comentario)) : '';

            var alumnoHTML =
                '<div class="list-group-item d-flex justify-content-between align-items-center">' +
                '<div style="flex:1"><h5 class="mb-1" style="font-weight: bold; color: #333;">' + (nombre + ' ' + (alumno.ApellidoPaterno || '') + ' ' + (alumno.ApellidoMaterno || '')).trim() + '</h5>' +
                '<p class="mb-1" style="color: #777;">Entregó: ' + fechaEntrega + '</p>' +
                '<p class="mb-1" style="color: #777;">Calificado: ' + fechaCalifMostrar + '</p>' +
                '<p class="mb-1" style="color: #777;">' + comentarioMostrar + '</p></div>' +
                '<div style="display:flex;gap:8px">' +
                '<button class="btn btn-primary btn-sm" onclick="verRespuesta(' + (idParaVer || 0) + ',' + (alumno.AlumnoId || 0) + ')">Ver Respuesta</button>' +
                '<button class="btn btn-warning btn-sm" onclick="abrirModalCalificar(' + (entregableId || 0) + ', ' + puntajeMaximo + ')">Calificar</button>' +
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
                    if (parsed.Respuesta) {
                        html += '<div><strong>Respuesta:</strong><pre>' + escapeHtml(parsed.Respuesta) + '</pre></div>';
                    } else if (typeof parsed === 'string') {
                        html += '<div><strong>Respuesta:</strong><pre>' + escapeHtml(parsed) + '</pre></div>';
                    }
                }
            } catch (err) {
                // no es JSON
                html = '<div><strong>Respuesta:</strong><pre>' + escapeHtml(String(respuestaRaw)) + '</pre></div>';
            }
        } else {
            html = '<p>No hay respuesta registrada.</p>';
        }

        var textoElem = document.getElementById('texto-respuesta');
        if (textoElem) textoElem.innerHTML = html;
        var modalEl = document.getElementById('respuestaModal');
        if (modalEl) {
            var modal = new bootstrap.Modal(modalEl);
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
            var modal = new bootstrap.Modal(modalEl);
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
        var entregaId = parseInt(document.getElementById('entregaId').value || '0', 10);
        var cal = parseInt(document.getElementById('calificacion').value || '0', 10);
        if (!entregaId || isNaN(cal)) { alert('Entrega o calificación inválida'); return; }
        try {
            var comentario = (document.getElementById('comentario') || {}).value || '';
            const resp = await fetch('/api/Actividades/AsignarCalificacion', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ EntregaId: entregaId, Calificacion: cal, Comentario: comentario })
            });
            if (!resp.ok) throw new Error('Error al guardar la calificación');
            // cerrar modal
            var modalEl = document.getElementById('calificarModal');
            var modal = bootstrap.Modal.getInstance(modalEl);
            if (modal) modal.hide();

            Swal.fire({ icon: 'success', title: 'Calificación guardada', timer: 1200, showConfirmButton: false });
            // recargar lista
            prepararAlumnosYActividades();
        } catch (err) {
            console.error(err);
            Swal.fire('Error', err.message || 'No se pudo guardar', 'error');
        }
    });
});
