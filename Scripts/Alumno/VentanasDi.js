document.addEventListener("DOMContentLoaded", function () {
    console.log("Detalle de clase cargado...");

    document.getElementById("avisos-tab").addEventListener("click", () => cargarContenido("avisos", "Avisos"));
    document.getElementById("actividades-tab").addEventListener("click", () => cargarContenido("actividades", "Actividades"));
    document.getElementById("alumnos-tab").addEventListener("click", () => cargarContenido("alumnos", "Alumnos"));
    document.getElementById("calificaciones-tab").addEventListener("click", () => cargarContenido("calificaciones", "Calificaciones"));

    // Cargar Avisos por defecto
    cargarContenido("avisos", "Avisos");
});

function cargarContenido(seccion, nombreVista) {
    console.log(`Cargando ${nombreVista}...`);

    if (nombreVista === "Avisos") {
        console.log('ES AVISOS');
        fetch(`/Alumno/${nombreVista}?alumnoId=${alumnoIdGlobal}`)
            .then(response => response.text())
            .then(html => {
                document.getElementById(`contenido${nombreVista}`).innerHTML = html;
            })
            .catch(error => console.error(`Error al cargar ${nombreVista}:`, error));
    } else {
        fetch(`/Alumno/${nombreVista}`)
            .then(response => response.text())
            .then(html => {
                document.getElementById(`contenido${nombreVista}`).innerHTML = html;
            })
            .catch(error => console.error(`Error al cargar ${nombreVista}:`, error));
    }
}