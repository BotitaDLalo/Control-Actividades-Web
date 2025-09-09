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
}


function convertirUrlsEnEnlaces(texto) {
    const urlRegex = /(https?:\/\/[^\s]+)/g;
    return texto.replace(urlRegex, '<a href="$1" target="_blank">$1</a>');
}


function formatearFecha(fecha) {
    const dateObj = new Date(fecha);
    return dateObj.toLocaleDateString("es-ES", { day: "2-digit", month: "2-digit", year: "numeric" }) +
        " " + dateObj.toLocaleTimeString("es-ES", { hour: "2-digit", minute: "2-digit" });
}
