document.addEventListener("DOMContentLoaded", async function () {
    console.log("Cargando clases...");
    await cargarClases();
});

async function obtenerAlumnoId() {
    return alumnoIdGlobal; 
}

// Función para unirse a una clase con código
async function unirseAClase() {
    var codigoAcceso = document.getElementById("codigoAccesoInput").value.trim();
    var alumnoId = alumnoIdGlobal; // Asegúrate de que este valor esté definido correctamente

    if (!codigoAcceso) {
        Swal.fire({
            icon: "warning",
            title: "Código requerido",
            text: "Por favor, ingresa un código de acceso.",
            position: "center"
        });
        return;
    }

    try {
       var response = await fetch('AlumnoApi/UnirseAClase', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                AlumnoId: alumnoId,
                CodigoAcceso: codigoAcceso
            })
        });

        var data = await response.json();

        if (response.ok) {
            Swal.fire({
                icon: "success",
                title: "Unido con éxito",
                text: data.mensaje,
                position: "center"
            }).then(() => {
                closeModal(); // Cierra el modal (si aplica)
                cargarClases(); // ✅ Recargar las clases después de unirse
            });

        } else {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: data.mensaje,
                position: "center"
            });
        }

    } catch (error) {
        console.error("Error al intentar unirse a la clase:", error);
        Swal.fire({
            icon: "error",
            title: "Error",
            text: "Ocurrió un error. Intenta nuevamente.",
            position: "center"
        });
    }
}

// Cargar clases del alumno
async function cargarClases() {
    const alumnoId = await obtenerAlumnoId();
    //console.log("Cargando clases del alumno con ID:", alumnoId);

    try {
        var response = await fetch(`/Alumno/ObtenerClases?alumnoId=${alumnoId}`);
        var clases = await response.json();

        console.log('Clases:',clases);

        var contenedor = document.getElementById("contenedorClases");
        if (!contenedor) {
            console.error("No se encontró el elemento contenedorClases en el HTML.");
            return;
        }

        contenedor.innerHTML = ""; // Limpiar contenido previo

        if (!clases.length) {
            console.warn("No hay clases para mostrar.");
            contenedor.innerHTML = "<p>No tienes clases registradas.</p>";
            return;
        }

        clases.forEach(clase => {
            agregarCardClase(clase);
        });

    } catch (error) {
        console.error("Error al cargar las clases:", error);
        alert("Ocurrió un error al cargar las clases.");
    }
}

//l45 Agregar clase a la vista
function agregarCardClase(clase) {

    let id = clase.Id;
    if (!clase.Nombre) {
        console.warn("Intento de agregar clase sin nombre.");
        return;
    }

    const contenedor = document.getElementById("contenedorClases");
    if (!contenedor) {
        console.error("No se encontró el elemento contenedorClases en el HTML.");
        return;
    }

    const card = document.createElement("div");
    card.classList.add("class-card");

    const etiqueta = clase.esGrupo ? "Grupo" : "Materia";

    card.innerHTML = `
        
            <p class="card-etiqueta">
                <img class="iconos-nav2" src="/Content/Iconos/TABLAB.svg" alt="Icono de Grupo" />
                ${etiqueta} - ${clase.Nombre}
            </p>
    `;

    //72 Si la clase es una materia, hacer clic para ir a su página
    if (!clase.esGrupo) {
        card.addEventListener("click", function () {
            window.location.href = `/Alumno/Clase?tipo=materia&id=${id}`;
        });
    }

    // Si es un grupo con materias, agregar la vista de materias en formato de cards cuadradas
    if (clase.esGrupo && clase.materias && clase.materias.length > 0) {
        let contenedorMaterias = document.createElement("div");
        contenedorMaterias.classList.add("materias-grid"); // Nueva clase CSS para el grid
        contenedorMaterias.style.display = "none"; // Inicialmente oculto

        clase.materias.forEach(materia => {
            let materiaCard = document.createElement("div");
            materiaCard.classList.add("materia-card");

            materiaCard.innerHTML = `
                <div class="card-container1">
                    <p class="card-etiqueta">
                    <img class="iconos-nav2" src="/Content/Iconos/TABLAB.svg" alt="Icono de Grupo" />
                    ${materia.Nombre}</p>
                     <hr class="card-separator">
            <img class="iconos-nav2" src="/Content/Iconos/TABLA-26.svg" alt="Icono de Horario" />
            <img class="iconos-nav2" src="/Content/Iconos/PAR-26.svg" alt="Icono de Participación" />
            <img class="iconos-nav2" src="/Content/Iconos/ESTRELLA-26.svg" alt="Icono de Favorito" />

                </div>
            `;

            // ✅ Agregar evento para ir a la página de la materia
            materiaCard.addEventListener("click", function (event) {
                event.stopPropagation(); // Evita que el grupo también se active
                window.location.href = `/Alumno/Clase?tipo=materia&id=${id}`;
            });

            contenedorMaterias.appendChild(materiaCard);
        });

        card.appendChild(contenedorMaterias);

        // Hacer que al hacer clic sobre el grupo se desplieguen/oculten las materias
        card.addEventListener("click", function () {
            contenedorMaterias.style.display = contenedorMaterias.style.display === "none" ? "grid" : "none";
        });
    }

    contenedor.appendChild(card);
}

// Ver clase al hacer clic
function verClase(nombre, esGrupo) {
    if (!nombre || nombre === "undefined") {
        alert("Error: La clase no tiene un nombre válido.");
        return;
    }

    console.log(`Redirigiendo a la clase: ${nombre} (Grupo: ${esGrupo})`);
    const tipo = esGrupo ? 'grupo' : 'materia';
    window.location.href = `/Alumno/Clase?tipo=${tipo}&nombre=${encodeURIComponent(nombre)}`;
}
