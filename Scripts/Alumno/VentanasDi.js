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

    // Sólo cargamos por AJAX la vista de Avisos (parcial _Avisos).
    // Para Actividades y otras pestañas preferimos usar el HTML estático ya presente
    // en la vista `DetalleMateria` y ejecutar las funciones cliente correspondientes.
    if (nombreVista === "Avisos") {
        console.log('Cargando parcial de Avisos');
        fetch(`/Alumno/${nombreVista}?alumnoId=${alumnoIdGlobal}`)
            .then(response => response.text())
            .then(html => {
                const target = document.getElementById(`contenido${nombreVista}`);
                if (target) target.innerHTML = html;

                // Ejecutar inicialización del script de avisos si existe
                setTimeout(() => {
                    if (typeof inicializarAvisos === "function") {
                        inicializarAvisos();
                    } else {
                        console.warn("inicializarAvisos no disponible después de cargar parcial.");
                    }
                }, 200);
            })
            .catch(error => console.error(`Error al cargar ${nombreVista}:`, error));

        return;
    }

    if (nombreVista === "Actividades") {
        // No sobreescribir el HTML de actividades; simplemente invocar la carga de actividades
        // que utiliza el contenedor existente `#listaActividadesAlumno`.
        console.log('Mostrar Actividades y cargar contenido estático');
        try {
            if (typeof cargarActividadesAlumno === 'function') {
                cargarActividadesAlumno();
            }
        } catch (e) {
            console.error('Error al invocar cargarActividadesAlumno:', e);
        }
        return;
    }

    // Para otras pestañas (alumnos, calificaciones) no hacemos AJAX por defecto; dejamos
    // que el contenido ya presente en la vista o los scripts asociados se encarguen.
    console.log(`No se realiza carga AJAX para ${nombreVista}, mostrar contenido estático.`);
}