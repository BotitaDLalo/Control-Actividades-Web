
var actividadesData = {};
var docenteIdGlobal = localStorage.getItem("docenteId");
var materiaIdGlobal = localStorage.getItem("materiaIdSeleccionada");
var grupoIdGlobal = localStorage.getItem("grupoIdSeleccionado");
var actividadIdGlobal = localStorage.getItem("actividadSeleccionada");
var puntajeMaximo = null;

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
                    if (fechaCreacionElem) fechaCreacionElem.innerText = data.FechaCreacion ? new Date(data.FechaCreacion).toLocaleDateString("es-ES") : "No disponible";
                    if (fechaLimiteElem) fechaLimiteElem.innerText = data.FechaLimite ? new Date(data.FechaLimite).toLocaleDateString("es-ES") : "No disponible";
                    if (tipoElem) tipoElem.innerText = data.TipoActividad || "No disponible";
                    if (puntajeElem) puntajeElem.innerText = data.Puntaje || "0";
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
            var fechaEntrega = alumno.FechaEntrega ? new Date(alumno.FechaEntrega).toLocaleDateString("es-ES") : "Sin entregar";
            var fechaCalificacion = alumno.Entrega && alumno.Entrega.FechaCalificacionAsignada ? new Date(alumno.Entrega.FechaCalificacionAsignada).toLocaleDateString("es-ES") : "Sin calificar";

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
