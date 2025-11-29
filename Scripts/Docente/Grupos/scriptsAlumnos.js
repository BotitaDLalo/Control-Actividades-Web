var div = document.getElementById("docente-datos");
var docenteIdGlobal = div.dataset.docenteid;

// Prevent this script from initializing twice (bundles + direct include)
if (window.__docente_scriptsAlumnosInitialized) {
    console.warn('scriptsAlumnos already initialized, skipping duplicate load');
    // stop executing duplicate script
    throw new Error('scriptsAlumnos duplicate load prevented');
} 
window.__docente_scriptsAlumnosInitialized = true;

// Esperar a que el DOM esté completamente cargado antes de ejecutar el código
document.addEventListener("DOMContentLoaded", function () {

    // Cargar alumnos asignados a la materia
    cargarAlumnosAsignados();

    //Cargar actividades a la materia
    
    document.addEventListener("click", async function (event) {
        if (event.target.id === "btnAsignarAlumno") {
            const correo = document.getElementById("buscarAlumno").value.trim();
            if (!correo) {
                Swal.fire({
                    position: "top-end",
                    icon: "question",
                    title: "Ingrese un correo valido.",
                    showConfirmButton: false,
                    timer: 2500
                });
                return;
            }

            try {
                const response = await fetch(`/Materias/AsignarAlumnoMateria?correo=${correo}&materiaId=${materiaIdGlobal}`, {
                    method: "POST"
                });

                const data = await response.json();

                if (!response.ok) {
                    Swal.fire({
                        title: "Error",
                        text: data.mensaje || "Error al asignar alumno.",
                        icon: "error",
                        confirmButtonColor: "#d33",
                    });
                    return;
                }
                document.getElementById("buscarAlumno").value = "";
                Swal.fire({
                    position: "top-end",
                    title: "Asignado",
                    text: "Alumno asignado correctamente.",
                    icon: "success",
                    timer: 2500
                });
                cargarAlumnosAsignados(materiaIdGlobal);
            } catch (error) {
                Swal.fire({
                    title: "Error",
                    text: "Hubo un problema al asignar al alumno. Inténtalo de nuevo.",
                    icon: "error",
                    confirmButtonColor: "#d33",
                });
            }
        }

        // Nota: no usar document-level handler para abrir archivo múltiples veces
    });

    // --- File import handling: use a single dedicated click handler and a single file input ---
    let isSelectingFile = false;

    function getOrCreateFileInput() {
        let input = document.getElementById('fileImportarAlumnos');
        if (!input) {
            input = document.createElement('input');
            input.type = 'file';
            input.id = 'fileImportarAlumnos';
            input.accept = '.xlsx,.xls';
            input.style.display = 'none';
            document.body.appendChild(input);
        }

        // attach change handler only once
        if (!input.dataset.importHandlerAttached) {
            input.addEventListener('change', async function (e) {
                const file = e.target.files[0];
                if (!file) return;

                const formData = new FormData();
                formData.append('file', file);
                formData.append('MateriaId', materiaIdGlobal);
                if (grupoIdGlobal && grupoIdGlobal !== '0') formData.append('GrupoId', grupoIdGlobal);

                try {
                    const resp = await fetch('/api/Alumnos/ImportarAlumnosExcel', {
                        method: 'POST',
                        body: formData
                    });

                    const result = await resp.json();
                    if (!resp.ok) {
                        Swal.fire('Error', result.mensaje || 'Error al importar alumnos', 'error');
                        return;
                    }

                    let summary = `Total leídos: ${result.TotalLeidos}\nAgregados: ${result.Agregados.length}\nOmitidos: ${result.Omitidos.length}\nNo encontrados: ${result.NoEncontrados.length}`;
                    Swal.fire('Importación completa', summary.replace(/\n/g, '<br/>'), 'success');

                    cargarAlumnosAsignados(materiaIdGlobal);

                    // Limpiar input
                    input.value = '';
                } catch (err) {
                    console.error(err);
                    Swal.fire('Error', 'Error al subir el archivo', 'error');
                } finally {
                    isSelectingFile = false;
                }
            });
            input.dataset.importHandlerAttached = '1';
        }

        return input;
    }

    // Attach single click handler to the import button (if exists)
    const btnImport = document.getElementById('btnImportarAlumnos');
    if (btnImport) {
        btnImport.addEventListener('click', function (ev) {
            ev.stopPropagation();
            ev.preventDefault();
            if (isSelectingFile) return;
            isSelectingFile = true;
            const input = getOrCreateFileInput();
            // Small timeout to ensure flag set before any other handler
            setTimeout(() => {
                input.click();
            }, 10);
            // safety: reset flag after 2s in case change doesn't fire
            setTimeout(() => { isSelectingFile = false; }, 2000);
        });
    }


    // Funcionalidad de búsqueda de alumnos en tiempo real (sugerencias de correo)
    const inputBuscar = document.getElementById("buscarAlumno");
    const listaSugerencias = document.getElementById("sugerenciasAlumnos");
    let indexSugerenciaSeleccionada = -1;

    if (inputBuscar) {
        inputBuscar.addEventListener("input", async function () {
            const query = inputBuscar.value.trim();
            if (query.length < 3) {
                listaSugerencias.innerHTML = "";
                return;
            }

            try {
                const response = await fetch(`/Materias/BuscarAlumnosPorCorreo?query=${query}`);
                if (!response.ok) throw new Error("Error al buscar alumnos");

                const alumnos = await response.json();
                listaSugerencias.innerHTML = "";

                if (alumnos.length === 0) {
                    listaSugerencias.innerHTML = `<li class="list-group-item text-muted">No se encontraron resultados</li>`;
                    return;
                }

                alumnos.forEach((alumno, index) => {
                    const li = document.createElement("li");
                    li.classList.add("list-group-item", "list-group-item-action");
                    li.textContent = `${alumno.Nombre} ${alumno.ApellidoPaterno} ${alumno.ApellidoMaterno} - ${alumno.Email}`;

                    li.addEventListener("click", function () {
                        inputBuscar.value = alumno.Email;
                        listaSugerencias.innerHTML = "";
                    });

                    if (index === indexSugerenciaSeleccionada) {
                        li.classList.add("active");
                    }

                    listaSugerencias.appendChild(li);
                });

            } catch (error) {
                console.error("Error al buscar alumnos:", error);
            }
        });

        // Navegación con teclas en las sugerencias
        inputBuscar.addEventListener("keydown", function (e) {
            const sugerencias = listaSugerencias.getElementsByTagName("li");

            if (e.key === "ArrowDown") {
                if (indexSugerenciaSeleccionada < sugerencias.length - 1) {
                    indexSugerenciaSeleccionada++;
                    actualizarSugerencias();
                }
            } else if (e.key === "ArrowUp") {
                if (indexSugerenciaSeleccionada > 0) {
                    indexSugerenciaSeleccionada--;
                    actualizarSugerencias();
                }
            } else if (e.key === "Enter" && indexSugerenciaSeleccionada >= 0) {
                const selectedSugerencia = sugerencias[indexSugerenciaSeleccionada];
                if (selectedSugerencia) {
                    const correo = selectedSugerencia.textContent.split(" - ")[1];
                    if (correo) {
                        inputBuscar.value = correo;
                        listaSugerencias.innerHTML = "";
                    }
                }
            }
        });
        function actualizarSugerencias() {
            const sugerencias = listaSugerencias.getElementsByTagName("li");
            for (let i = 0; i < sugerencias.length; i++) {
                sugerencias[i].classList.remove("active");
            }
            if (indexSugerenciaSeleccionada >= 0 && indexSugerenciaSeleccionada < sugerencias.length) {
                sugerencias[indexSugerenciaSeleccionada].classList.add("active");
            }
        }
        document.addEventListener("click", function (event) {
            if (!inputBuscar.contains(event.target) && !listaSugerencias.contains(event.target)) {
                listaSugerencias.innerHTML = "";
            }
        });
    }

});

//Carga los alumnos a la materia y los muestra en el div
async function cargarAlumnosAsignados(materiaIdGlobal) {
    try {
        // Hacer la petición al servidor
        const response = await fetch(`/Materias/ObtenerAlumnosPorMateria?materiaId=${materiaIdGlobal}`);

        if (!response.ok) {
            throw new Error("No se pudieron cargar los alumnos.");
        }

        // Convertir la respuesta a JSON
        const alumnos = await response.json();

        // Seleccionar el contenedor donde se mostrará la lista
        const contenedor = document.getElementById("listaAlumnosAsignados");
        contenedor.innerHTML = ""; // Limpiar contenido anterior
        // Verificar si hay alumnos
        if (alumnos.length === 0) {
            contenedor.innerHTML = `<p class="text-muted">No hay alumnos asignados a esta materia.</p>`;
            return;
        }

        // Crear la lista de alumnos
        alumnos.forEach(alumno => {
            //  Crear el div del alumno
            const divAlumno = document.createElement("div");
            divAlumno.classList.add("d-flex", "justify-content-between", "align-items-center", "p-2", "mb-2");
            divAlumno.style.background = "#f8f9fa"; // Color de fondo
            divAlumno.style.borderRadius = "8px"; // Bordes redondeados

            //  Agregar el nombre del alumno
            const spanNombre = document.createElement("span");
            spanNombre.textContent = `${alumno.ApellidoPaterno} ${alumno.ApellidoMaterno} ${alumno.Nombre}`;
            divAlumno.appendChild(spanNombre);

            //  Contenedor de botón
            const divBotones = document.createElement("div");

            //  Botón "Eliminar del grupo" dentro de un menú desplegable
            const dropdown = document.createElement("div");
            dropdown.classList.add("dropdown");

            const btnDropdown = document.createElement("button");
            btnDropdown.classList.add("btn", "btn-danger", "btn-sm", "dropdown-toggle");
            btnDropdown.textContent = "Opciones";
            btnDropdown.setAttribute("data-bs-toggle", "dropdown");

            const dropdownMenu = document.createElement("ul");
            dropdownMenu.classList.add("dropdown-menu");

            const eliminarItem = document.createElement("li");
            const eliminarLink = document.createElement("a");
            eliminarLink.classList.add("dropdown-item");
            eliminarLink.href = "#";
            eliminarLink.textContent = "Eliminar del grupo";
            eliminarLink.onclick = function () {
                eliminardelgrupo(alumno.alumnoMateriaId);
            };

            eliminarItem.appendChild(eliminarLink);
            dropdownMenu.appendChild(eliminarItem);
            dropdown.appendChild(btnDropdown);
            dropdown.appendChild(dropdownMenu);

            divBotones.appendChild(dropdown);
            divAlumno.appendChild(divBotones);

            // Agregar alumno a la lista
            contenedor.appendChild(divAlumno);
        });

    } catch (error) {
        console.error("Error al cargar alumnos:", error);
    }
}


//Elimina Alumno del grupo
async function eliminardelgrupo(alumnoMateriaId) {
    try {
        const confirmacion = await Swal.fire({
            title: "¿Estás seguro?",
            text: "Esta acción eliminará al alumno del grupo.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar"
        });

        if (!confirmacion.isConfirmed) return;

        const response = await fetch(`/Materias/EliminarAlumnoDeMateria?idEnlace=${alumnoMateriaId}`, {
            method: "DELETE",
        });

        if (!response.ok) {
            throw new Error("No se pudo eliminar al alumno del grupo.");
        }

        Swal.fire({
            position: "top-end",
            title: "Eliminado",
            text: "El alumno ha sido eliminado del grupo correctamente.",
            icon: "success",
            timer: 2500
        });

        cargarAlumnosAsignados(materiaIdGlobal); // Recargar la lista

    } catch (error) {
        Swal.fire({
            position: "top-end",
            title: "Error",
            text: "Hubo un problema al eliminar al alumno.",
            icon: "error",
            timer: 2500
        });

        console.error("Error al eliminar alumno:", error);
    }
}
