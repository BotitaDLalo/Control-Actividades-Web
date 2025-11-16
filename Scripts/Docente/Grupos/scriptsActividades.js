var div = document.getElementById("docente-datos");
var docenteIdGlobal = div ? div.dataset.docenteid : null;

// Esperar a que el DOM esté completamente cargado antes de ejecutar el código
document.addEventListener("DOMContentLoaded", function () {

    cargarActividadesDeMateria();

});

// Función que registra una nueva actividad
async function registrarActividad() {
    let nombre = document.getElementById("nombre").value.trim();
    let descripcion = document.getElementById("descripcion").value.trim();
    let fechaHoraLimite = document.getElementById("fechaHoraLimite").value;
    let puntaje = parseInt(document.getElementById("puntaje").value, 10);

    // Referencia al botón para mostrar estado
    var btn = document.querySelector('#crearActividadModal .btn-primary');
    var originalBtnHtml = btn ? btn.innerHTML : null;

    // Validaciones básicas
    if (!nombre || !descripcion || !fechaHoraLimite || isNaN(puntaje)) {
        Swal.fire({
            icon: "warning",
            title: "Campos incompletos",
            text: "Por favor, completa todos los campos antes de continuar."
        });
        return;
    }

    // Validar que la fecha límite sea mayor a la fecha actual
    let fechaActual = new Date();
    let fechaLimite = new Date(fechaHoraLimite);
    if (fechaLimite <= fechaActual) {
        Swal.fire({
            icon: "warning",
            title: "Fecha inválida",
            text: "La fecha límite debe ser posterior a la fecha actual."
        });
        return;
    }

    // Validar materiaIdGlobal
    if (typeof materiaIdGlobal === 'undefined' || !materiaIdGlobal) {
        Swal.fire({ icon: 'error', title: 'Error en materia', text: 'No se ha identificado la materia seleccionada.' });
        return;
    }

    let actividad = {
        NombreActividad: nombre,
        Descripcion: descripcion,
        FechaLimite: fechaHoraLimite,
        TipoActividadId: 1, // Cambiar si se obtiene dinámicamente
        Puntaje: puntaje,
        MateriaId: parseInt(materiaIdGlobal, 10)
    };

    try {
        // Deshabilitar botón y mostrar spinner
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Publicando...';
        }

        let response = await fetch("/Materias/CrearActividad", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(actividad)
        });

        // Leer respuesta como texto y tratar de parsear JSON (más robusto ante respuestas HTML)
        const text = await response.text();
        let data = null;
        try { data = text ? JSON.parse(text) : null; } catch (e) { data = null; }

        if (!response.ok) {
            const mensaje = data && data.mensaje ? data.mensaje : (text || `Error HTTP: ${response.status}`);
            throw new Error(mensaje);
        }

        Swal.fire({
            position: "top-end",
            title: "Actividad creada",
            text: "La actividad ha sido publicada correctamente.",
            icon: "success",
            timer: 1500,
            showConfirmButton: false
        });

        // Cerrar modal si está abierto (Bootstrap 4/5)
        try {
            if (window.jQuery && $('#crearActividadModal').modal) {
                $('#crearActividadModal').modal('hide');
            } else if (window.bootstrap) {
                var modalEl = document.getElementById('crearActividadModal');
                var modal = bootstrap.Modal.getInstance(modalEl);
                if (modal) modal.hide();
            }
        } catch (e) { console.warn('No se pudo cerrar el modal:', e); }

        // limpiar formulario
        try { document.getElementById("actividadesForm").reset(); } catch (e) { }

        // recargar lista de actividades
        setTimeout(function () { cargarActividadesDeMateria(); }, 300);

    } catch (error) {
        console.error("Error:", error);
        Swal.fire({
            position: "top-end",
            title: "Error al crear la actividad",
            text: error.message || "Ocurrió un problema al crear la actividad.",
            icon: "error",
            timer: 4000,
            showConfirmButton: true
        });
    } finally {
        // Rehabilitar botón
        if (btn) {
            btn.disabled = false;
            if (originalBtnHtml) btn.innerHTML = originalBtnHtml;
        }
    }
}



// Funcion que carga las actividades a la vista.
async function cargarActividadesDeMateria() {
    const listaActividades = document.getElementById("listaActividadesDeMateria");
    if (!listaActividades) return;
    listaActividades.innerHTML = "<p>Cargando actividades...</p>"; // Mostrar mensaje de carga

    try {
        const mid = typeof materiaIdGlobal !== 'undefined' ? materiaIdGlobal : (window.materiaIdGlobal || null);
        if (!mid) throw new Error('Materia no definida');
        const response = await fetch(`/Materias/ObtenerActividadesPorMateria?materiaId=${mid}`);
        if (!response.ok) throw new Error("No se encontraron actividades.");
        const actividades = await response.json();
        renderizarActividades(actividades);
    } catch (error) {
        listaActividades.innerHTML = `<p class="mensaje-error">${error.message}</p>`;
    }
}
//Renderiza actividades despues de confirmar existencia
function renderizarActividades(actividades) {
    const listaActividades = document.getElementById("listaActividadesDeMateria");
    if (!listaActividades) return;
    listaActividades.innerHTML = ""; // Limpiar el contenedor

    if (!actividades || actividades.length === 0) {
        listaActividades.innerHTML = "<p>No hay actividades registradas para esta materia.</p>";
        return;
    }
    actividades.reverse();

    actividades.forEach(actividad => {
        const actividadItem = document.createElement("div");
        actividadItem.classList.add("actividad-item");
        const descripcionActividadConEnlace = convertirUrlsEnEnlaces(actividad.Descripcion);

        actividadItem.innerHTML = `
            <div class="actividad-header">
                <div class="icono">📋</div>
                <div class="info">
                    <strong>${actividad.NombreActividad}</strong>
                    <p class="fecha-publicado">Publicado: ${formatearFecha(actividad.FechaCreacion)}</p>
                    <p class="puntaje" style="font-weight: bold; color: #d35400;">Puntaje: ${actividad.Puntaje}</p>
                    <p class="actividad-descripcion oculto">${descripcionActividadConEnlace}</p>
                    <p class="ver-completo">Ver completo</p>
                </div>
                <div class="fecha-entrega">
                    <strong>Fecha de entrega:</strong><br>
                    ${formatearFecha(actividad.FechaLimite)}
                </div>
                <div class="botones-container">
                    <button class="btn-ir-actividades" data-id="${actividad.ActividadId}">Ir a actividad</button>
                    <button class="editar-btn" data-id="${actividad.ActividadId}">Editar</button>
                    <button class="eliminar-btn" data-id="${actividad.ActividadId}">Eliminar</button>
                </div>
            </div>
        `;

        // Mostrar/ocultar descripción al hacer clic en "Ver completo"
        const verCompleto = actividadItem.querySelector(".ver-completo");
        const descripcion = actividadItem.querySelector(".actividad-descripcion");

        verCompleto.addEventListener("click", () => {
            if (descripcion.classList.contains("oculto")) {
                descripcion.classList.remove("oculto");
                descripcion.classList.add("visible");
            } else {
                descripcion.classList.remove("visible");
                descripcion.classList.add("oculto");
            }
        });

        // Agregar eventos a los botones
        const btnEliminar = actividadItem.querySelector(".eliminar-btn");
        const btnEditar = actividadItem.querySelector(".editar-btn");
        const btnIrActividad = actividadItem.querySelector(".btn-ir-actividades");

        btnEliminar.addEventListener("click", () => eliminarActividad(actividad.ActividadId));
        btnEditar.addEventListener("click", () => editarActividad(actividad.ActividadId));
        btnIrActividad.addEventListener("click", () => IrAActividad(actividad.ActividadId));

        listaActividades.appendChild(actividadItem);
    });
}


async function IrAActividad(actividadIdSeleccionada) {
   //guardar el id de la materia para acceder a la materia en la que se entro y usarla en otro script
   localStorage.setItem("actividadSeleccionada", actividadIdSeleccionada);
    // Redirige a la página de detalles de la materia
    window.open(`/Docente/EvaluarActividades`, '_blank'); //Aqui lleva en la url el id de la actividadSeleccionada
}
// Funciones para manejar los botones

// helper para intentar una lista de endpoints en secuencia
async function tryEndpoints(endpoints, fetchOptions) {
    for (let i = 0; i < endpoints.length; i++) {
        try {
            const res = await fetch(endpoints[i], fetchOptions);
            if (res.ok) return res;
            // if server returned JSON with message, continue to next but remember last response
            console.warn('Endpoint failed', endpoints[i], res.status);
        } catch (e) {
            console.warn('Fetch error for', endpoints[i], e);
        }
    }
    throw new Error('Ningún endpoint respondió correctamente.');
}

async function eliminarActividad(id) {
    const result = await Swal.fire({
        title: '¿Estás seguro?',
        text: "¡Esta acción no se puede deshacer!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        reverseButtons: true
    });

    if (!result.isConfirmed) {
        Swal.fire({ title: 'Cancelado', text: 'La actividad no fue eliminada.', icon: 'info', timer: 1500, showConfirmButton: false });
        return;
    }

    Swal.fire({ title: 'Eliminando...', text: `Eliminando actividad ${id}`, allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

    const endpoints = [
        `/api/Actividades/EliminarActividad?id=${id}`,
        `/api/Actividades/EliminarActividad/${id}`,
        `/Materias/EliminarActividad?id=${id}`,
        `/Materias/EliminarActividad/${id}`
    ];

    try {
        const resp = await tryEndpoints(endpoints, { method: 'DELETE', headers: { 'Content-Type': 'application/json' } });
        const text = await resp.text();
        let data = null; try { data = text ? JSON.parse(text) : null; } catch (e) { data = null; }
        Swal.close();
        Swal.fire('Eliminado!', data && data.mensaje ? data.mensaje : `La actividad ${id} fue eliminada.`, 'success');
        cargarActividadesDeMateria();
    } catch (error) {
        console.error('Error al eliminar la actividad:', error);
        Swal.close();
        Swal.fire('Error', 'No se pudo eliminar la actividad. Revisa la consola para más detalles.', 'error');
    }
}


function formatearFecha(fechaStr) {
    try {
        const d = new Date(fechaStr);
        return d.toLocaleString();
    } catch (e) { return fechaStr; }
}

function convertirUrlsEnEnlaces(texto) {
    var urlRegex = /(https?:\/\/[^\n\s]+)/g;
    return (texto || '').replace(urlRegex, '<a href="$1" target="_blank">$1</a>');
}

// ------------------ EDITAR ACTIVIDAD ------------------
function toInputDateTimeValue(dateStr) {
    if (!dateStr) return '';
    var d = new Date(dateStr);
    if (isNaN(d.getTime())) return '';
    // get local offset ISO without seconds
    var pad = function (n) { return n < 10 ? '0' + n : n; };
    var year = d.getFullYear();
    var month = pad(d.getMonth() + 1);
    var day = pad(d.getDate());
    var hours = pad(d.getHours());
    var minutes = pad(d.getMinutes());
    return `${year}-${month}-${day}T${hours}:${minutes}`;
}

async function editarActividad(id) {
    try {
        // Obtener datos de la actividad
        const resp = await fetch(`/Actividades/ObtenerActividadPorId?actividadId=${id}`);
        if (!resp.ok) throw new Error('No se pudo obtener la actividad');
        const data = await resp.json();

        // llenar formulario
        document.getElementById('nombre').value = data.NombreActividad || '';
        document.getElementById('descripcion').value = data.Descripcion || '';
        document.getElementById('fechaHoraLimite').value = toInputDateTimeValue(data.FechaLimite || data.FechaCreacion);
        document.getElementById('puntaje').value = data.Puntaje || 0;

        // establecer materia global si no existe
        if (!materiaIdGlobal && window.materiaIdGlobal) materiaIdGlobal = window.materiaIdGlobal;

        // marcar que estamos editando
        window.editingActividadId = id;

        // preparar botón publicar para actualizar
        var btn = document.getElementById('btnPublicarActividad');
        if (btn) {
            btn.textContent = 'Guardar cambios';
            // quitar listeners previos
            var newBtn = btn.cloneNode(true);
            btn.parentNode.replaceChild(newBtn, btn);
            newBtn.addEventListener('click', async function () {
                await actualizarActividad(id);
            });
        }

        // abrir modal
        try {
            var crearModalEl = document.getElementById('crearActividadModal');
            if (crearModalEl && window.bootstrap) {
                var crearModal = new bootstrap.Modal(crearModalEl);
                crearModal.show();
            } else if (window.jQuery && $('#crearActividadModal').modal) {
                $('#crearActividadModal').modal('show');
            }
        } catch (e) { console.warn(e); }

    } catch (err) {
        console.error(err);
        Swal.fire('Error', 'No se pudo cargar la actividad para edición', 'error');
    }
}

async function actualizarActividad(id) {
    // leer campos
    let nombre = document.getElementById('nombre').value.trim();
    let descripcion = document.getElementById('descripcion').value.trim();
    let fechaHoraLimite = document.getElementById('fechaHoraLimite').value;
    let puntaje = parseInt(document.getElementById('puntaje').value, 10) || 0;

    if (!nombre || !descripcion || !fechaHoraLimite) {
        Swal.fire({ icon: 'warning', title: 'Campos incompletos', text: 'Completa todos los campos.' });
        return;
    }

    const body = {
        NombreActividad: nombre,
        Descripcion: descripcion,
        FechaLimite: fechaHoraLimite,
        Puntaje: puntaje,
        TipoActividadId: 1
    };

    const endpoints = [
        `/api/Actividades/ActualizarActividad?id=${id}`,
        `/api/Actividades/ActualizarActividad/${id}`,
        `/Actividades/ActualizarActividad?id=${id}`,
        `/Actividades/ActualizarActividad/${id}`
    ];

    Swal.fire({ title: 'Guardando...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

    try {
        const resp = await tryEndpoints(endpoints, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
        const text = await resp.text();
        let data = null; try { data = text ? JSON.parse(text) : null; } catch (e) { data = null; }

        Swal.close();
        Swal.fire({ icon: 'success', title: 'Actividad actualizada' });

        // cerrar modal
        try {
            if (window.jQuery && $('#crearActividadModal').modal) {
                $('#crearActividadModal').modal('hide');
            } else if (window.bootstrap) {
                var modalEl = document.getElementById('crearActividadModal');
                var modal = bootstrap.Modal.getInstance(modalEl);
                if (modal) modal.hide();
            }
        } catch (e) { }

        // limpiar estado
        window.editingActividadId = null;
        try { document.getElementById('actividadesForm').reset(); } catch (e) { }

        // recargar lista
        setTimeout(cargarActividadesDeMateria, 300);

    } catch (e) {
        console.error(e);
        Swal.close();
        Swal.fire('Error', e.message || 'No se pudo actualizar la actividad', 'error');
    }
}
