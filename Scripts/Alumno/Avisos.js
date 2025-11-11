function inicializarAvisos() {
    console.log("✅ inicializarAvisos() fue llamada");
    console.log("alumnoIdGlobal:", alumnoIdGlobal);
    if (!alumnoIdGlobal) {
        console.error("alumnoIdGlobal no está definido.");
        $("#avisos-container").html("<p>Error al obtener los avisos.</p>");
        return;
    }

    $.get("/Alumno/ObtenerAvisos?alumnoId=" + alumnoIdGlobal, function (data) {
        var avisosHtml = "";
        data.reverse();
        if (data.length > 0) {
            data.forEach(function (aviso) {
                avisosHtml += `
                    <li class="list-group-item">
                        <h5>${aviso.Titulo}</h5>
                        <p>${aviso.Descripcion}</p>
                        <small class="text-muted">${aviso.FechaCreacion}</small>
                    </li>`;
            });
        } else {
            avisosHtml = "<p>No hay avisos disponibles.</p>";
        }
        $("#avisos-container").html(avisosHtml);
    }).fail(function () {
        $("#avisos-container").html("<p>Error al cargar los avisos.</p>");
    });
}

/*
//AVISO COMPONENTE
$(document).ready(function () {

    const contenedor = document.querySelector("#avisos-container");
    if (!contenedor) {
        console.error("No se encontró el contenedor de avisos en el DOM");
        return;
    }

    if (!alumnoIdGlobal || alumnoIdGlobal === "0") {
        console.error("alumnoIdGlobal no está definido.");
        $("#avisos-container").html("<p>Error al obtener los avisos.</p>");
        return;
    }

    const avisosAlumno = new AvisosComponent({
        modo: "alumno",
        container: "#avisos-container",
        alumnoId: alumnoIdGlobal
    });

    avisosAlumno.cargarAvisos();
});


*/
/* Esperamos a que el DOM exista
function inicializarAvisos() {
    var div = document.getElementById("alumno-datos");
    console.log("Tu id es:  " + alumnoIdGlobal)
    const contenedor = document.querySelector("#avisos-container");
    if (!alumnoIdGlobal) {
        console.error("No se pudo obtener el alumnoIdGlobal.");
    } 

    const avisosAlumno = new AvisosComponent({
        modo: "alumno",
        container: "#avisos-container",
        alumnoId: window.alumnoIdGlobal
    });

    avisosAlumno.cargarAvisos();

document.addEventListener("DOMContentLoaded", inicializarAvisos);
}
/*

$(document).ready(function () {
    if (!alumnoIdGlobal) {
        console.error("alumnoIdGlobal no está definido.");
        $("#avisos-container").html("<p>Error al obtener los avisos.</p>");
        return;
    }

    const avisosAlumno = new AvisosComponent({
        modo: "alumno",
        container: "#avisos-container",
        alumnoId: alumnoIdGlobal
    });

    avisosAlumno.cargarAvisos();
});*/
