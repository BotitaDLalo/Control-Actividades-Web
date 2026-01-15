document.addEventListener("DOMContentLoaded", function () {
    //Cargar los avisos asinados a la materia
    cargarAvisosDeMateria();
});

//Funcion para publicar un aviso
async function publicarAviso() {
    // Obtener valores de los inputs
    let titulo = document.getElementById("titulo").value.trim();
    let descripcion = document.getElementById("descripcionAviso").value.trim();

    // Validar que los campos no estén vacíos
    if (!titulo || !descripcion) {
        Swal.fire({
            position: "top-end",
            title: "Campos vacíos",
            text: "Por favor, completa todos los campos.",
            icon: "warning",
            timer: 2500,
            showConfirmButton: false
        });
        return;
    }

    // Variables globales que ya tienes en tu archivo .js
    let docenteId = docenteIdGlobal;
    let grupoId = grupoIdGlobal;
    let materiaId = materiaIdGlobal;

    // Crear objeto con los datos a enviar
    let avisoData = {
        DocenteId: docenteId,
        Titulo: titulo,
        Descripcion: descripcion,
        GrupoId: grupoId,
        MateriaId: materiaId
    };

    try {
        // Enviar datos al controlador
        let response = await fetch("/Materias/CrearAviso", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(avisoData)
        });

        let result = await response.json();

        if (response.ok) {
            Swal.fire({
                position: "top-end",
                title: "Aviso creado",
                text: "El aviso ha sido publicado correctamente.",
                icon: "success",
                timer: 3000,
                showConfirmButton: false
            });

            setTimeout(() => {
                document.getElementById("avisosForm").reset();//Resetear  formulario
                cargarAvisosDeMateria();
            }, 3000);

        } else {
            Swal.fire({
                position: "top-end",
                title: "Error",
                text: result.mensaje || "Error al crear el aviso.",
                icon: "error",
                timer: 3000,
                showConfirmButton: false
            });
        }
    } catch (error) {
        console.error("Error:", error);
        Swal.fire({
            position: "top-end",
            title: "Error",
            text: "Hubo un problema al enviar el aviso.",
            icon: "error",
            timer: 3000,
            showConfirmButton: false
        });
    }
}



// Funcion que carga los avisos a la vista.
async function cargarAvisosDeMateria() {
    const listaAvisos = document.getElementById("listaDeAvisosDeMateria");
    if (!listaAvisos) return;
    try {
        const response = await fetch(`/Materias/ObtenerAvisos?IdMateria=${materiaIdGlobal}`);
        if (!response.ok) throw new Error("No se encontraron avisos.");
        const avisos = await response.json();
        renderizarAvisos(avisos);
    } catch (error) {
        listaAvisos.innerHTML = `<p class="aviso-error">${error.message}</p>`;
    }
}

function renderizarAvisos(avisos) {
    const listaAvisos = document.getElementById("listaDeAvisosDeMateria");
    listaAvisos.innerHTML = ""; // Limpiar el contenedor

    if (avisos.length === 0) {
        listaAvisos.innerHTML = "<p>No hay avisos registrados para esta materia.</p>";
        return;
    }
    avisos.reverse();


    avisos.forEach(aviso => {
        const avisoItem = document.createElement("div");
        avisoItem.classList.add("aviso-item");
        //const descripcionAvisoConEnlace = convertirUrlsEnEnlaces(aviso.Descripcion);

        avisoItem.innerHTML = `
            <div class="aviso-header">
                <div class="aviso-icono">📢</div>
                <div class="aviso-info">
                    <strong>${aviso.Titulo}</strong>
                    <p class="aviso-fecha-publicado">Publicado: ${aviso.FechaCreacion}</p>
                    <p class="ver-completo">Ver completo</p>
                </div>
                <div class="aviso-botones-container">
                    <button class="aviso-editar-btn" data-id="${aviso.AvisoId}">Editar</button>
                    <button class="aviso-eliminar-btn" data-id="${aviso.AvisoId}">Eliminar</button>
                </div>
            </div>
            <div>
                <p class="actividad-descripcion oculto">${aviso.Descripcion}</p>
            </div>
        `;

        // Mostrar/ocultar descripción al hacer clic en "Ver completo"
        const verCompleto = avisoItem.querySelector(".ver-completo");
        const descripcion = avisoItem.querySelector(".actividad-descripcion");
        
        verCompleto.addEventListener("click", () => {
            // Alternar entre mostrar y ocultar la descripción
            if (descripcion.classList.contains("oculto")) {
                descripcion.classList.remove("oculto");
                descripcion.classList.add("visible");
            } else {
                descripcion.classList.remove("visible");
                descripcion.classList.add("oculto");
            }
        });

        // Agregar eventos a los botones
        const btnEliminar = avisoItem.querySelector(".aviso-eliminar-btn");
        const btnEditar = avisoItem.querySelector(".aviso-editar-btn");

        btnEliminar.addEventListener("click", () => eliminarAviso(aviso.AvisoId));
        btnEditar.addEventListener("click", () => editarAviso(aviso.AvisoId));

        listaAvisos.appendChild(avisoItem);
    });
}

async function eliminarAviso(avisoId) {
    // Mostrar una confirmación antes de proceder con la eliminación
    const confirmacion = await Swal.fire({
        title: '¿Estás seguro de eliminar este aviso?',
        text: "¡Esta acción no se puede deshacer!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    });

    // Si el usuario confirma la eliminación, proceder con la solicitud DELETE
    if (confirmacion.isConfirmed) {
        try {
            // Hacer la solicitud DELETE para eliminar el aviso
            const response = await fetch(`/Materias/EliminarAviso?id=${avisoId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            // Verificar si la respuesta fue exitosa
            if (response.ok) {
                // Mostrar mensaje de éxito
                Swal.fire({
                    icon: 'success',
                    title: 'Aviso eliminado con éxito',
                    showConfirmButton: false,
                    timer: 1500
                });
                cargarAvisosDeMateria(); // Recargar los avisos después de eliminar
            } else {
                // Si la respuesta no es exitosa, mostrar un error
                const errorData = await response.json();
                Swal.fire({
                    icon: 'error',
                    title: 'Error al eliminar el aviso',
                    text: errorData.mensaje,
                    showConfirmButton: true
                });
            }
        } catch (error) {
            // En caso de error en la solicitud
            Swal.fire({
                icon: 'error',
                title: 'Error al conectar con el servidor',
                text: 'Por favor, intente nuevamente.',
                showConfirmButton: true
            });
        }
    }
}

//Edita un aviso desde su id
async function editarAviso(avisoId) {
    try {
        // Obtener datos actuales del aviso
        const response = await fetch(`/Materias/ObtenerAvisoPorId?avisoId=${avisoId}`);
        if (!response.ok) throw new Error("No se pudo obtener el aviso.");

        const aviso = await response.json();

        // Mostrar SweetAlert con los datos actuales
        const { value: formValues } = await Swal.fire({
            title: "Editar Aviso",
            html: `
                <input id="swal-titulo" class="swal2-input" placeholder="Título" value="${aviso.Titulo}">
                <textarea id="swal-descripcion" class="swal2-textarea" placeholder="Descripción">${aviso.Descripcion}</textarea>
            `,
            focusConfirm: false,
            showCancelButton: true,
            confirmButtonText: "Guardar Cambios",
            cancelButtonText: "Cancelar",
            preConfirm: () => {
                return {
                    titulo: document.getElementById("swal-titulo").value.trim(),
                    descripcion: document.getElementById("swal-descripcion").value.trim()
                };
            }
        });

        if (!formValues) return; // Si el usuario cancela, no hacer nada

        // Enviar los cambios al backend
        const updateResponse = await fetch(`/Materias/EditarAviso`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                avisoId,
                titulo: formValues.titulo,
                descripcion: formValues.descripcion,
                docenteId: docenteIdGlobal
            })
        });

        if (!updateResponse.ok) throw new Error("No se pudo actualizar el aviso.");

        Swal.fire("Actualizado", "El aviso ha sido editado correctamente.", "success");

        // Recargar avisos para reflejar los cambios
        cargarAvisosDeMateria();

    } catch (error) {
        Swal.fire("Error", error.message, "error");
    }
}
