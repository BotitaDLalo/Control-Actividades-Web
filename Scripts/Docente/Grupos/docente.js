var div = document.getElementById("docente-datos");
var docenteIdGlobal = div && div.dataset ? div.dataset.docenteid : null;
var materiasPorCrear = []; // Lista de materias a crear
// Evitar doble declaración si el script se carga dos veces
if (typeof window.intentosAcceder === 'undefined') {
    window.intentosAcceder = 0;
}
var intentosAcceder = window.intentosAcceder;


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


function verActividad(actividadIdSeleccionada, materiaId) {
    // Guardar actividad siempre
    localStorage.setItem("actividadSeleccionada", actividadIdSeleccionada);

    // Guardar materia solo si viene definida (evitar guardar "undefined")
    if (typeof materiaId !== 'undefined' && materiaId !== null && String(materiaId).trim() !== '') {
        localStorage.setItem("materiaIdSeleccionada", materiaId);
    }

    // Redirige a la página de detalles de la materia
    window.open(`/Docente/EvaluarActividades`, '_blank');
}
