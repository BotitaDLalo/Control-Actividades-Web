document.addEventListener("DOMContentLoaded", function () {
    //Cargar los avisos asinados a la materia
    // inyectar controles de filtro (nombre + fechas)
    try {
        var cont = document.getElementById('seccion-avisos') || document.body;
        var filtroDiv = document.createElement('div');
        filtroDiv.style.display = 'flex';
        filtroDiv.style.gap = '8px';
        filtroDiv.style.marginBottom = '10px';

        var inputNombre = document.createElement('input');
        inputNombre.id = 'filtroAvisoNombre';
        inputNombre.placeholder = 'Buscar por título...';
        inputNombre.className = 'form-control form-control-sm';
        inputNombre.style.width = '220px';

        var inputDesde = document.createElement('input');
        inputDesde.type = 'date';
        inputDesde.id = 'filtroAvisoDesde';
        inputDesde.className = 'form-control form-control-sm';
        inputDesde.style.width = '150px';

        var inputHasta = document.createElement('input');
        inputHasta.type = 'date';
        inputHasta.id = 'filtroAvisoHasta';
        inputHasta.className = 'form-control form-control-sm';
        inputHasta.style.width = '150px';

        var btn = document.createElement('button');
        btn.className = 'btn btn-sm btn-primary';
        btn.textContent = 'Filtrar';
        btn.addEventListener('click', function(){ cargarAvisosDeMateria(); });

        filtroDiv.appendChild(inputNombre);
        filtroDiv.appendChild(inputDesde);
        filtroDiv.appendChild(inputHasta);
        filtroDiv.appendChild(btn);

        // intentar insertar antes de la lista si existe
        var lista = document.getElementById('listaDeAvisosDeMateria');
        if (lista) lista.parentNode.insertBefore(filtroDiv, lista);
        else document.body.insertBefore(filtroDiv, document.body.firstChild);
    } catch(e) { console.warn('No se pudo insertar filtros de avisos', e); }

    cargarAvisosDeMateria();
});

function escapeHtml(s) { if (!s) return ''; return String(s).replace(/[&<>"'`]/g, function (m) { return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;', '`': '&#96;' })[m]; }); }

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
        const payload = await response.json();
        // payload puede venir como un arreglo directo o como { avisos: [...], RolUsuario: ... }
        let avisos = [];
        if (Array.isArray(payload)) {
            avisos = payload;
        } else if (payload && Array.isArray(payload.avisos)) {
            avisos = payload.avisos;
        } else if (payload && Array.isArray(payload.resultado)) {
            avisos = payload.resultado;
        } else {
            // intentar extraer la primera propiedad que sea un array
            const arr = payload && typeof payload === 'object' ? Object.keys(payload).map(k => payload[k]).find(v => Array.isArray(v)) : null;
            if (arr) avisos = arr;
        }

        // aplicar filtro cliente si hay controles
        try {
            var nombre = (document.getElementById('filtroAvisoNombre') || {}).value || '';
            var desde = (document.getElementById('filtroAvisoDesde') || {}).value || '';
            var hasta = (document.getElementById('filtroAvisoHasta') || {}).value || '';
            if (nombre || desde || hasta) {
                avisos = avisos.filter(function(a){
                    var ok = true;
                    if (nombre) ok = ok && (a.Titulo || '').toLowerCase().indexOf(nombre.toLowerCase()) !== -1;
                    if (desde) {
                        var f = new Date(a.FechaCreacion);
                        var d = new Date(desde);
                        if (!isNaN(f)) ok = ok && f >= d;
                    }
                    if (hasta) {
                        var f2 = new Date(a.FechaCreacion);
                        var h = new Date(hasta);
                        // incluir todo el día
                        h.setHours(23,59,59,999);
                        if (!isNaN(f2)) ok = ok && f2 <= h;
                    }
                    return ok;
                });
            }
        } catch(e) { console.warn(e); }

        renderizarAvisos(avisos);
    } catch (error) {
        listaAvisos.innerHTML = `<p class="aviso-error">${error.message}</p>`;
    }
}

function renderizarAvisos(avisos) {
    const listaAvisos = document.getElementById("listaDeAvisosDeMateria");
    if (!listaAvisos) return;
    listaAvisos.innerHTML = ""; // Limpiar el contenedor

    if (!avisos || avisos.length === 0) {
        listaAvisos.innerHTML = "<p>No hay avisos registrados para esta materia.</p>";
        return;
    }

    // asegurarse que es array y clonarlo antes de invertir para no mutar origen
    const items = avisos.slice().reverse();

    items.forEach(aviso => {
<<<<<<< Updated upstream
        const avisoItem = document.createElement("div");
        avisoItem.classList.add("aviso-item");
        //const descripcionAvisoConEnlace = convertirUrlsEnEnlaces(aviso.Descripcion);

        avisoItem.innerHTML = `
            <div class="aviso-header">
                <div class="aviso-icono">📢</div>
                <div class="aviso-info">
                    <strong>${aviso.Titulo}</strong>
                    <p class="aviso-fecha-publicado">Publicado: ${aviso.FechaCreacion || aviso.FechaCreacion}</p>
                    <p class="ver-completo">Ver completo</p>
                </div>
                <div class="aviso-botones-container">
                    <button class="aviso-editar-btn" data-id="${aviso.AvisoId}">Editar</button>
                    <button class="aviso-eliminar-btn" data-id="${aviso.AvisoId}">Eliminar</button>
                </div>
=======
        const avisoItem = document.createElement('div');
        avisoItem.className = 'aviso-item';
        // Crear card
        avisoItem.innerHTML = `
            <div class="aviso-icono">📢</div>
            <div class="aviso-info">
                <strong>${escapeHtml(aviso.Titulo)}</strong>
                <div class="aviso-descripcion oculto">${escapeHtml(aviso.Descripcion)}</div>
                <div class="aviso-fecha-publicado">Publicado: ${aviso.FechaCreacion || aviso.FechaCreacion}</div>
                <div class="ver-completo">Ver completo</div>
>>>>>>> Stashed changes
            </div>
            <div style="display:flex;flex-direction:column;gap:8px;margin-left:12px">
                <button class="btn btn-sm btn-outline-primary btn-editar" data-id="${aviso.AvisoId}">Editar</button>
                <button class="btn btn-sm btn-outline-danger btn-eliminar" data-id="${aviso.AvisoId}">Eliminar</button>
            </div>
        `;

        // toggle descripcion
        const ver = avisoItem.querySelector('.ver-completo');
        const desc = avisoItem.querySelector('.aviso-descripcion');
        if (ver && desc) ver.addEventListener('click', () => { desc.classList.toggle('oculto'); desc.classList.toggle('visible'); });

        // botones
        avisoItem.querySelectorAll('.btn-eliminar').forEach(b => b.addEventListener('click', () => eliminarAviso(aviso.AvisoId)));
        avisoItem.querySelectorAll('.btn-editar').forEach(b => b.addEventListener('click', () => editarAviso(aviso.AvisoId)));

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
