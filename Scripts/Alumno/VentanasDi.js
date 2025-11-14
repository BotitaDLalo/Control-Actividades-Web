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

                if (nombreVista === "Avisos") {
                    console.log('ES AVISOS');
                    // Esperar un poco para asegurar que los <script> internos del parcial se ejecuten
                    setTimeout(() => {
                        console.log("Intentando ejecutar inicializarAvisos() después del render...");
                        if (typeof inicializarAvisos === "function") {
                            inicializarAvisos();
                        } else {
                            console.warn("inicializarAvisos todavía no está disponible, reintentando...");
                            setTimeout(() => {
                                if (typeof inicializarAvisos === "function") {
                                    console.log("inicializarAvisos cargada al segundo intento");
                                    inicializarAvisos();
                                } else {
                                    console.error("inicializarAvisos no se cargó.");
                                }
                            }, 300);
                        }
                    }, 200);
                       
                }

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