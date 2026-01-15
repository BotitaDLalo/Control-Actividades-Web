var div = document.getElementById("docente-datos");
var docenteIdGlobal = 0;
if (div && div.dataset && div.dataset.docenteid) {
    docenteIdGlobal = div.dataset.docenteid;
} else if (localStorage.getItem('docenteId')) {
    docenteIdGlobal = localStorage.getItem('docenteId');
}

//Funcion para guarda las materias registradas sin un grupo
async function guardarMateriaSinGrupo() {
    const nombre = document.getElementById("nombreMateria").value; // Obtenemos el nombre de la materia desde el input
    const descripcion = document.getElementById("descripcionMateria").value; // Obtenemos la descripción de la materia
    const color = "#2196F3"; // Asignamos un color predeterminado para la materia

    if (nombre.trim() === '') { // Verificamos que el nombre de la materia no esté vacío
        Swal.fire({
            position: "top-end",
            icon: "question",
            title: "Ingrese nombre de la materia.",
            showConfirmButton: false,
            timer: 2500
        });// Mostramos una alerta si el nombre está vacío
        return;
    }

    // Enviamos una solicitud POST al servidor para guardar la materia
    const response = await fetch('/Materias/CrearMateria', {
        method: 'POST', // Indicamos que la solicitud será de tipo POST
        headers: { 'Content-Type': 'application/json' }, // Especificamos que el cuerpo de la solicitud será JSON
        body: JSON.stringify({
            NombreMateria: nombre, // Enviamos el nombre de la materia
            Descripcion: descripcion, // Enviamos la descripción de la materia
            CodigoColor: color, // Enviamos el color de la materia
            DocenteId: docenteIdGlobal // Enviamos el docenteId obtenido previamente
        })
    });
    
    if (response.ok) { // Verificamos si la respuesta es exitosa
        Swal.fire({
            position: "top-end",
            icon: "success",
            title: "Materia registrada correctamente.",
            showConfirmButton: false,
            timer: 2000
        });
        ;// Mostramos una alerta de éxito
        const form = document.getElementById("materiasForm");
        if (form) form.reset(); // Limpiamos el formulario
        if (typeof cargarMateriasSinGrupo === 'function') cargarMateriasSinGrupo(); // Recargamos la lista de materias sin grupo
    } else {
        Swal.fire({
            position: "top-end",
            icon: "error",
            title: "Error al registrar materia.",
            showConfirmButton: false,
            timer: 2000
        }); // Mostramos una alerta si hubo un error al guardar
    }
}


//Funcion para cargar materias sin grupo existentes al modal de crear grupo
async function cargarMaterias() {
    try {
        // Hacemos una solicitud GET al servidor para obtener las materias sin grupo
        const response = await fetch(`/Materias/ObtenerMateriasSinGrupo?docenteId=${docenteIdGlobal}`);
        if (response.ok) { // Verificamos si la respuesta es exitosa
            const materias = await response.json(); // Convertimos la respuesta en formato JSON
            const contenedorMaterias = document.getElementById("materiasLista"); // Obtenemos el contenedor donde se mostrarán las materias
            if (!contenedorMaterias) return;
            contenedorMaterias.innerHTML = ""; // Limpiamos cualquier contenido previo en el contenedor

            if (!materias || materias.length === 0) { // Si no hay materias disponibles, mostramos un mensaje
                contenedorMaterias.innerHTML = "<p>No hay materias disponibles.</p>";
                return;
            }

            // Recorremos todas las materias obtenidas y las mostramos en el contenedor
            materias.forEach(materia => {
                const checkbox = document.createElement("input"); // Creamos un checkbox para cada materia
                checkbox.type = "checkbox"; // Definimos que sea un checkbox
                checkbox.className = "materia-checkbox"; // Asignamos una clase para identificarlos
                checkbox.value = materia.MateriaId; // Asignamos el ID de la materia como valor del checkbox  
                const label = document.createElement("label"); // Creamos una etiqueta para el checkbox
                label.appendChild(checkbox); // Añadimos el checkbox a la etiqueta
                label.appendChild(document.createTextNode(" " + (materia.nombreMateria || materia.NombreMateria))); // Añadimos el nombre de la materia a la etiqueta

                const div = document.createElement("div"); // Creamos un contenedor div para cada materia
                div.className = "form-check"; // Asignamos una clase para estilo
                div.appendChild(label); // Añadimos el label al div

                contenedorMaterias.appendChild(div); // Añadimos el div al contenedor
            });
        }
    } catch (error) {
        console.error("Error al cargar materias:", error); // Mostramos un mensaje de error si hay un problema al cargar las materias
    }
}


// Cargar materias que fueron creadas sin un grupo a la vista principal.
async function cargarMateriasSinGrupo() {
    try {
        const response = await fetch(`/Materias/ObtenerMateriasSinGrupo?docenteId=${docenteIdGlobal}`);
        if (!response.ok) throw new Error('Error en la respuesta');
        const materiasSinGrupo = await response.json();
        const listaMateriasSinGrupo = document.getElementById("listaMateriasSinGrupo");
        if (!listaMateriasSinGrupo) return;

        // Limpiar contenido anterior
        listaMateriasSinGrupo.innerHTML = "";

        if (!materiasSinGrupo || materiasSinGrupo.length === 0) {
            const mensaje = document.createElement("p");
            mensaje.classList.add("text-center", "text-muted");
            mensaje.textContent = "No hay materias registradas.";
            listaMateriasSinGrupo.appendChild(mensaje);
            return;
        }

        materiasSinGrupo.forEach((materia, index) => {
            const card = document.createElement('div');
            card.className = 'rounded card-layout';

            const title = document.createElement('div');
            title.className = 'card-title';
            title.textContent = materia.NombreMateria;

            const subtitle = document.createElement('div');
            subtitle.className = 'card-subtitle';
            subtitle.textContent = materia.Descripcion || '';

            card.appendChild(title);
            if (materia.Descripcion) card.appendChild(subtitle);

            // agregar nombre del docente si viene
            if (materia.DocenteId || materia.DocenteId === 0) {
                const nombreDocente = materia.DocenteNombre || materia.DocenteNombre || '';
                if (nombreDocente) {
                    const d = document.createElement('div');
                    d.className = 'card-docente';
                    d.textContent = 'Docente: ' + nombreDocente;
                    card.appendChild(d);
                }
            }

            listaMateriasSinGrupo.appendChild(card);
        });

    } catch (error) {
        console.error('Error al cargar materias sin grupo:', error);
        Swal.fire({
            title: "Error al cargar materias",
            html: "Reintentando en <b></b> segundos...",
            timer: 4000,
            timerProgressBar: true,
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
                const timer = Swal.getPopup().querySelector("b");
                let timerInterval = setInterval(() => {
                    timer.textContent = `${Math.floor(Swal.getTimerLeft() / 1000)}`;
                }, 100);
            },
            willClose: () => clearInterval(timerInterval)
        }).then((result) => {
            if (result.dismiss === Swal.DismissReason.timer) {
                cargarMateriasSinGrupo();
            }
        });
    }
}


//Funcion para editar nombre y descripcion de una materia.
async function editarMateria(MateriaId, NombreMateria, Descripcion) {

    if (Descripcion === null || Descripcion === "null" || Descripcion === undefined) {
        Descripcion = "";
    }

    const { value: formValues } = await Swal.fire({
        title: "Editar Materia",
        html: `
            <div style="display: flex; flex-direction: column; gap: 10px; text-align: left;">
                <div style="display: flex; align-items: center; gap: 10px;">
                    <label for="swal-nombre" style="width: 100px;">Materia</label>
                    <input id="swal-nombre" class="swal2-input"  placeholder="Nombre" value="${NombreMateria}">
                </div>
                <div style="display: flex; align-items: center; gap: 5px;">
                    <label for="swal-descripcion" style="width: 100px;">Descripción</label>
                    <input id="swal-descripcion" class="swal2-input" placeholder="Descripción" value="${Descripcion}">
                </div>
            </div>
        `,
        focusConfirm: false,
        showCancelButton: true,
        confirmButtonText: "Guardar",
        cancelButtonText: "Cancelar",
        preConfirm: () => {
            const nombre = document.getElementById("swal-nombre").value.trim();

            if(!nombre) {
                Swal.showValidationMessage("Por favor, ingresa un nombre para la materia.");
                return false;
            }

            return {
                NombreMateria: document.getElementById("swal-nombre").value, // Nombre correcto
                Descripcion: document.getElementById("swal-descripcion").value // Nombre correcto
            };
        }
    });

    if (formValues) {
        // Enviar los datos al servidor para actualizar la materia
        const response = await fetch(`/Materias/ActualizarMateria?materiaId=${MateriaId}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(formValues)
        });
        console.log("Código de respuesta:", response.status);
        console.log("Tipo de contenido:", response.headers.get("content-type"));

        const text = await response.text();
        console.log("Respuesta del servidor:", text);


        if (response.ok) {
            Swal.fire({
                position: "top-end",
                icon: "success",
                title: "Materia actualizada correctamente",
                showConfirmButton: false,
                timer: 2000
            });
            if (typeof cargarGrupos === 'function') cargarGrupos();
            if (typeof cargarMaterias === 'function') cargarMaterias(); // Recargar la lista de materias
            if (typeof cargarMateriasSinGrupo === 'function') cargarMateriasSinGrupo();
        } else {
            Swal.fire({
                position: "top-end",
                icon: "error",
                title: "Error al actualizar la materia",
                showConfirmButton: false,
                timer: 2000
            });
        }
    }
}



async function eliminarMateria(MateriaId) {
    const confirmacion = await Swal.fire({
        title: "¿Estás seguro?",
        text: "No podrás recuperar esta materia después de eliminarla.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí, eliminar",
        cancelButtonText: "Cancelar"
    });

    if (confirmacion.isConfirmed) {
        try {
            const response = await fetch(`/Materias/EliminarMateria/${MateriaId}`, {
                method: "DELETE"
            });

            const resultado = await response.json();

            if (response.ok) {
                await Swal.fire({
                    position: "top-end",
                    icon: "success",
                    title: resultado.mensaje || "Eliminado.",
                    showConfirmButton: false,
                    timer: 2000
                });
                // Se ejecuta funcion inicializar para actualizar vista completa
                if (typeof inicializar === 'function') inicializar();
            } else {
                await Swal.fire({
                    position: "top-end",
                    icon: "error",
                    title: resultado.mensaje || "No se pudo eliminar el grupo y sus materias.",
                    showConfirmButton: false,
                    timer: 2000
                });
            }
        } catch (error) {
            await Swal.fire({
                position: "top-end",
                icon: "error",
                title: "Ocurrio un problema al eliminar la materia.",
                showConfirmButton: false,
                timer: 2000
            });
        }
    }
}

