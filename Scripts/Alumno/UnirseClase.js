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
       // Use API endpoint that returns object with IDs so we can redirect into the class
       var response = await fetch('/api/Alumnos/UnirseAClaseM', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                AlumnoId: alumnoId,
                CodigoAcceso: codigoAcceso
            })
        });
        // Try to parse JSON safely
        let data = null;
        try { data = await response.json(); } catch (e) { data = null; }

        if (response.ok) {
            // If API returns detailed info (UnirseAClaseM), redirect into the class
            const esGrupo = (data && (data.EsGrupo === true || data.EsGrupo === 'True' || data.EsGrupo === 'true'));
            // Close modal safely
            try {
                const modalEl = document.getElementById('unirseModal');
                if (modalEl) {
                    const m = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                    m.hide();
                }
            } catch (e) { try { document.getElementById('unirseModal').classList.remove('show'); } catch (__) { } }

            // If Grupo object present
            if (data && data.Grupo && (data.Grupo.GrupoId || data.Grupo.grupoId)) {
                const gid = data.Grupo.GrupoId || data.Grupo.grupoId;
                // redirect to group view
                window.location.href = `/Alumno/Clase?tipo=grupo&id=${encodeURIComponent(gid)}`;
                return;
            }

            // If Materia object present
            if (data && data.Materia && (data.Materia.MateriaId || data.Materia.materiaId)) {
                const mid = data.Materia.MateriaId || data.Materia.materiaId;
                window.location.href = `/Alumno/Clase?tipo=materia&id=${encodeURIComponent(mid)}`;
                return;
            }

            // Fallback: reload classes list and show success
            cargarClases();
            Swal.fire({ icon: 'success', title: 'Unido con éxito', text: (data && data.mensaje) ? data.mensaje : 'Te has unido a la clase.' });
        } else {
            const msg = (data && data.mensaje) ? data.mensaje : (data && data.Message) ? data.Message : 'Código inválido o ya estás inscrito.';
            Swal.fire({ icon: 'error', title: 'Error', text: msg, position: 'center' });
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
            // render empty materias list as well
            renderMisMaterias([]);
            return;
        }

        // Render only groups in the main area; materias se listan en la columna derecha
        var grupos = clases.filter(function (c) { return c.esGrupo === true || c.esGrupo === 'true' || c.esGrupo === true; });
        if (!grupos || grupos.length === 0) {
            contenedor.innerHTML = '<div class="col-12"><div class="card border-0 shadow-sm p-4 text-center text-muted"><p class="mb-0">No tienes grupos registrados.</p></div></div>';
        } else {
            grupos.forEach(function (clase) { agregarCardClase(clase); });
        }

        // Build materias list from returned clases
        try {
            var materiasMap = {};
            clases.forEach(function (c) {
                // handle group with Materias/Materias or materias property
                var inner = c.Materias || c.materias || c.Materias || null;
                if (c.esGrupo && Array.isArray(inner)) {
                    inner.forEach(function (m) {
                        var id = m.Id || m.MateriaId || m.id || m.MateriaId || m.Id;
                        var nombre = m.Nombre || m.NombreMateria || m.nombre || '';
                        if (id && !materiasMap[id]) materiasMap[id] = nombre;
                    });
                } else if (!c.esGrupo) {
                    var id = c.Id || c.id;
                    var nombre = c.Nombre || c.NombreMateria || c.nombre || '';
                    if (id && !materiasMap[id]) materiasMap[id] = nombre;
                }
            });

            var materiasArr = Object.keys(materiasMap).map(function (k) { return { Id: k, Nombre: materiasMap[k] }; });
            renderMisMaterias(materiasArr);
        } catch (e) { console.warn('No se pudieron procesar materias', e); renderMisMaterias([]); }

    } catch (error) {
        console.error("Error al cargar las clases:", error);
        alert("Ocurrió un error al cargar las clases.");
    }
}

// Función para enviar archivo/texto como entrega
async function enviarEntrega(actividadId) {
    try {
        var alumnoId = alumnoIdGlobal;
        if (!alumnoId) { alert('Alumno no identificado'); return; }

        var inputFile = document.getElementById('fileEntrega_' + actividadId);
        var texto = document.getElementById('textoEntrega_' + actividadId)?.value || '';

        var form = new FormData();
        form.append('ActividadId', actividadId);
        form.append('AlumnoId', alumnoId);
        form.append('Respuesta', texto);

        if (inputFile && inputFile.files && inputFile.files.length > 0) {
            for (var i = 0; i < inputFile.files.length; i++) {
                form.append('files', inputFile.files[i]);
            }
        }

        var resp = await fetch('/api/Alumnos/SubirEntrega', { method: 'POST', body: form });
        if (!resp.ok) {
            var txt = await resp.text().catch(() => '');
            throw new Error(txt || 'Error al subir entrega');
        }
        var json = await resp.json().catch(() => null);
        Swal.fire('Enviado', (json && json.mensaje) ? json.mensaje : 'Entrega registrada', 'success');
        // recargar estado de la actividad
        if (typeof cargarClases === 'function') cargarClases();
    } catch (e) {
        console.error(e);
        Swal.fire('Error', e.message || 'No se pudo enviar la entrega', 'error');
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

    // Show only the type (Grupo / Materia) in the main cards area. The full
    // materia names are shown in the right column "Mis materias".
    card.innerHTML = `
            <div class="card-body">
                <p class="card-etiqueta">
                    <img class="iconos-nav2" src="/Content/Iconos/TABLAB.svg" alt="Icono de Grupo" />
                    ${etiqueta}
                </p>
            </div>
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

// Render lista lateral de materias
function renderMisMaterias(materias) {
    var cont = document.getElementById('misMaterias');
    if (!cont) return;
    cont.innerHTML = '';
    if (!materias || materias.length === 0) {
        cont.innerHTML = '<div class="list-group-item text-muted">No estás inscrito en ninguna materia.</div>';
        return;
    }
    materias.forEach(function (m) {
        var a = document.createElement('a');
        a.className = 'list-group-item list-group-item-action';
        a.href = '/Alumno/Clase?tipo=materia&id=' + encodeURIComponent(m.Id || m.MateriaId || m.id);
        a.textContent = m.Nombre || m.NombreMateria || ('Materia ' + (m.Id || ''));
        cont.appendChild(a);
    });
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
