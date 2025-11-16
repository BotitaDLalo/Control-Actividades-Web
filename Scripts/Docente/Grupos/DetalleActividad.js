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
        // If string looks like /Date(1620000000000)/
        var msMatch = s.match(/\\/?Date\\(([-\\d]+)(?:[-+][0-9]+)?\\)\\/?/);
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
    if (actividadIdGlobal != null && materiaIdGlobal != null) {
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
                } else {
                    console.error("No se encontraron datos válidos para esta actividad.");
                }
            })
            .catch(function (error) {
                console.error("Error al obtener los datos de la actividad:", error);
            });
    }

    prepararAlumnosYActividades();
});

function prepararAlumnosYActividades() {
    AlumnosDeMateriaParaActividades()
        .then(obtenerActividadesParaEvaluar)
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
            var fechaEntrega = alumno.FechaEntrega ? (parseServerDate(alumno.FechaEntrega) ? parseServerDate(alumno.FechaEntrega).toLocaleDateString('es-ES') : 'Sin entregar') : 'Sin entregar';
            var fechaCalificacion = alumno.Entrega && alumno.Entrega.FechaCalificacionAsignada ? (parseServerDate(alumno.Entrega.FechaCalificacionAsignada) ? parseServerDate(alumno.Entrega.FechaCalificacionAsignada).toLocaleDateString('es-ES') : 'Sin calificar') : 'Sin calificar';

            var alumnoHTML =
                '<div class="list-group-item d-flex justify-content-between align-items-center">' +
                '<div><h5 class="mb-1" style="font-weight: bold; color: #333;">' + alumno.Nombre + ' ' + alumno.ApellidoPaterno + ' ' + alumno.ApellidoMaterno + '</h5>' +
                '<p class="mb-1" style="color: #777;">Entregó: ' + fechaEntrega + '</p></div>' +
                '<span class="badge bg-success">Entregado</span>' +
                '<button class="btn btn-primary btn-sm" onclick="verRespuesta(' + alumno.AlumnoActividadId + ')">Ver Respuesta</button>' +
                '<button class="btn btn-warning btn-sm" onclick="abrirModalCalificar(' + (alumno.Entrega ? alumno.Entrega.EntregaId : 0) + ', ' + puntajeMaximo + ')">Calificar</button>' +
                '<p class="mb-1" style="color: #777;">Calificado el: ' + fechaCalificacion + '</p>' +
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
