var div = document.getElementById("docente-datos");
var docenteIdGlobal = div && div.dataset ? div.dataset.docenteid : null;
let materiasPorCrear = []; // Lista de materias a crear
let intentosAcceder = 0;


//Funcion que obtiene informacion del docente.
async function obtenerDocenteId() {
    try {
        // Hacemos una solicitud para obtener el docenteId desde el servidor
        const response = await fetch('/Cuenta/ObtenerDocenteId'); // Llamar al controlador
        const data = await response.json(); // Convertimos la respuesta en formato JSON
        if (data.docenteId) {
            docenteIdGlobal = data.docenteId; // Guardamos el docenteId en la variable 
            localStorage.setItem("docenteId", docenteIdGlobal); // Guardamos el docenteIdGlobal en el almacenamiento local

            // Alerta con diseño de Toast
            const Toast = Swal.mixin({
                toast: true,
                position: "top-end",
                showConfirmButton: false,
                timer: 1500,
                timerProgressBar: true,
                didOpen: (toast) => {
                    toast.onmouseleave = Swal.resumeTimer;
                }
            });

            Toast.fire({
                icon: "success",
                title: "Todo correcto."
            });

        }
    } catch (error) {
        AlertaCierreSesion(); // Si existe un error, cierra la sesión
    }
}





// Función para asociar materias al grupo
async function asociarMateriasAGrupo(grupoId, materias) {
    const response = await fetch('/Grupos/AsociarMaterias', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ GrupoId: grupoId, MateriaIds: materias })
    });

    if (!response.ok) {
        Swal.fire({
            position: "top-end",
            icon: "error",
            title: "Error al asociar materias con grupo.",
            showConfirmButton: false,
            timer: 2000
        });
    }
}



// Función para redirigir a la vista Materias dentro del controlador Docente
function irAMateria(materiaIdSeleccionada, seccion = 'avisos') {
    //guardar el id de la materia en localstorage para obtenerla en otros scripts
    localStorage.setItem("materiaIdSeleccionada", materiaIdSeleccionada);
    // Redirige a la página de detalles de la materia
    window.location.href = `/Docente/MateriasDetalles?materiaId=${materiaIdSeleccionada}&seccion=${seccion}`;
}



async function inicializar() {
    //await obtenerDocenteId();
    if (docenteIdGlobal != null) { 
        cargarMateriasSinGrupo(docenteIdGlobal);
        cargarGrupos(docenteIdGlobal)
    } else {
        AlertaCierreSesion();
    }
}

//Prioriza la ejecucion al cargar index
// Llamar a la función inicializadora cuando se cargue la página
document.addEventListener("DOMContentLoaded", () => {
    inicializar(); // Carga inicial de datos
    // Se ejecuta solo cuando se abre el modal
    const gruposModal = document.getElementById("gruposModal");
    if (gruposModal) {
        gruposModal.addEventListener("shown.bs.modal", cargarMaterias);
    }

    // Delegación de eventos: escucha los clics en el contenedor padre
    document.body.addEventListener("click", function (event) {
        let link = event.target.closest(".actividad-link"); // Detecta si el clic fue en un enlace de actividad
        if (link) {
            event.preventDefault(); // Evita la recarga de la página si es un <a>
            let actividadId = link.getAttribute("data-id"); // Obtener el ID correcto
            verActividad(actividadId);
        }
    });
});


function verActividad(actividadIdSeleccionada, materiaId) {
    localStorage.setItem("materiaIdSeleccionada", materiaId);
    //guardar el id de la materia para acceder a la materia en la que se entro y usarla en otro script
    localStorage.setItem("actividadSeleccionada", actividadIdSeleccionada);
    // Redirige a la página de detalles de la materia
    window.open(`/Docente/EvaluarActividades`, '_blank'); //Aqui lleva en la url el id de la actividadSeleccionada
}
