document.addEventListener("DOMContentLoaded", function () {
    //Cargar los avisos asinados a la materia
    // inyectar controles de filtro (nombre + fechas)
    try {
        inputHasta.type = 'date';
        inputHasta.id = 'filtroAvisoHasta';
        inputHasta.className = 'form-control form-control-sm';
        inputHasta.style.width = '150px';

        var btn = document.createElement('button');
        btn.className = 'btn btn-sm btn-primary';
        btn.textContent = 'Filtrar';

        btn.addEventListener('click', function () {
            cargarAvisosDeMateria();
        });

        filtroDiv.appendChild(inputNombre);
        filtroDiv.appendChild(inputDesde);
        filtroDiv.appendChild(inputHasta);
        filtroDiv.appendChild(btn);

        // intentar insertar antes de la lista si existe
        var lista = document.getElementById('listaDeAvisosDeMateria');
        if (lista) lista.parentNode.insertBefore(filtroDiv, lista);
        else document.body.insertBefore(filtroDiv, document.body.firstChild);
    } catch (e)
    {
        console.warn('No se pudo insertar filtros de avisos', e);
    }

    cargarAvisosDeMateria();
});

function escapeHtml(s)
{
    if (!s) return '';
    return String(s).replace(/[&<>"'`]/g, function (m)
                                            {
                                                return (
                                                    { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;', '`': '&#96;' }
                                                )[m];
                                            });
}

let _avisosCache = []; // cache local de avisos para filtrar en cliente

//Funcion para publicar un aviso
async function publicarAviso() {
    // Obtener valores de los inputs
    let titulo = document.getElementById("titulo").value.trim();
    let descripcion = document.getElementById("descripcionAviso").value.trim();
    let enlaces = document.getElementById("enlacesAviso").value.trim();
    let fechaInicio = document.getElementById("fechaInicioAviso").value;
    let fechaFin = document.getElementById("fechaFinAviso").value;
    let frecuenciaDias = parseInt(document.getElementById("frecuenciaDias").value || 0);

    // Validar que los campos no estén vacíos
    if (!titulo || !descripcion || !fechaInicio || !fechaFin) {
        document.getElementById("avisosForm").classList.add("was-validated");
        return;
    }

    let hoy = new Date().toISOString().split("T")[0];

    if (fechaInicio < hoy) {
        Swal.fire({
            icon: "warning",
            title: "Fecha inválida",
            text: "La fecha de inicio no puede ser menor a hoy."
        });
        return;
    }

    if (fechaFin < fechaInicio) {
        Swal.fire({
            icon: "warning",
            title: "Fechas inválidas",
            text: "La fecha de fin debe ser mayor o igual a la fecha de inicio."
        });
        return;
    }

    if (frecuenciaDias < 1) {
        Swal.fire({
            icon: "warning",
            title: "Frecuencia inválida",
            text: "La frecuencia debe ser al menos 1 día."
        });
        return;
    }

    if (enlaces) {
        let linksArray = enlaces.split("\n");

        for (let link of linksArray) {
            link = link.trim();
            if (link && !link.startsWith("https")) {
                Swal.fire({
                    icon: "warning",
                    title: "Enlace inválido",
                    text: "Todos los enlaces deben comenzar con https."
                });
                return;
            }
        }
    }

    // Crear objeto con los datos a enviar
    let avisoData = {
        Titulo: titulo,
        Descripcion: descripcion,
        Enlaces: enlaces,
        GrupoId: grupoIdGlobal,
        MateriaId: materiaIdGlobal,
        FechaInicio: fechaInicio,
        FechaFin: fechaFin,
        FrecuenciaDias: frecuenciaDias
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
                text: "El aviso ha creado correctamente.",
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
                icon: "error",
                title: "Error",
                text: result.mensaje || "Error al crear el aviso.",
                timer: 3000,
                showConfirmButton: false
            });
        }
    } catch (error) {
        console.error("Error:", error);
        Swal.fire({            
            icon: "error",
            title: "Error",
            text: "Hubo un problema al enviar el aviso.",
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
        } 

        // Guardar en cache
        _avisosCache = avisos || [];

        // Renderizar todo inicialmente
        renderizarAvisos(_avisosCache);

        // Activar filtros
        inicializarFiltrosAvisos();

    } catch (error) {
        listaAvisos.innerHTML = `<p class="aviso-error">${error.message}</p>`;
    }
}

/* --- ENLACES --- */
function renderizarEnlaces(enlaces) {
    if (!enlaces) return '';

    const lineas = enlaces.split('\n')
        .map(l => l.trim())
        .filter(l => l);

    if (lineas.length === 0) return '';

    let html = '<div class="aviso-enlaces mt-2"><strong>Recursos:</strong><ul>';

    lineas.forEach(link => {
        const safeLink = escapeHtml(link);
        html += `
            <li>
                <a href="${safeLink}" target="_blank" rel="noopener noreferrer">
                    ${safeLink}
                </a>
            </li>`;
    });

    html += '</ul></div>';

    return html;
}

/*ACTIVO | PROGRAMADO | FINALIZADO*/
function obtenerBadgeEstado(estado) {

    switch (estado) {
        case "Activo":
            return `<span class="badge bg-success">Activo</span>`;

        case "Programado":
            return `<span class="badge bg-warning text-dark">Programado</span>`;

        case "Finalizado":
            return `<span class="badge bg-secondary">Finalizado</span>`;

        default:
            return '';
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
 
        //const descripcionAvisoConEnlace = convertirUrlsEnEnlaces(aviso.Descripcion);

        const avisoItem = document.createElement('div');
        const enlacesHtml = renderizarEnlaces(aviso.Enlaces);
        const badgeEstado = window.esDocente
            ? obtenerBadgeEstado(aviso.Estado)
            : '';

        avisoItem.className = 'aviso-item';

        const botonesHtml = window.esDocente
            ? `
                <div class="aviso-botones-lateral">
                    <button class="btn btn-warning btn-editar" data-id="${aviso.AvisoId}">
                        Editar
                    </button>
                    <button class="btn btn-danger btn-eliminar" data-id="${aviso.AvisoId}">
                        Eliminar
                    </button>
                </div>
              `
            : '';

        const fechaCreacionDocente = window.esDocente 
            ? `
                 <div class="aviso-fecha-publicado">
                    <small>
                        Creado: ${aviso.FechaCreacion}
                    </small>
                </div>
              `
            : '';
           
        // Crear card
        avisoItem.innerHTML = `
                                <div class="aviso-info">
                                    <div class="aviso-icono d-flex justify-content-between align-items-center">
                                        <div>
                                            📢 <strong>${escapeHtml(aviso.Titulo)}</strong>
                                        </div>
                                        ${badgeEstado}
                                    </div>

                                    <div class="aviso-descripcion visible">
                                        ${escapeHtml(aviso.Descripcion)}
                                    </div>

                                    ${enlacesHtml}

                                    <div class="aviso-fechas mt-2">
                                        <small>
                                            <strong>Inicio:</strong> ${aviso.FechaInicio || '-'} |
                                            <strong>Fin:</strong> ${aviso.FechaFin || '-'}
                                        </small>
                                    </div>

                                    <div class="aviso-frecuencia">
                                        <small>
                                            Recordatorio cada ${aviso.FrecuenciaDias} día(s)
                                        </small>
                                    </div>

                                    ${fechaCreacionDocente}
                                </div>
                                ${botonesHtml}
                            `;

        // botones
        if (window.esDocente) {
            avisoItem.querySelectorAll('.btn-eliminar')
                .forEach(b => b.addEventListener('click', () => eliminarAviso(aviso.AvisoId)));

            avisoItem.querySelectorAll('.btn-editar')
                .forEach(b => b.addEventListener('click', () => editarAviso(aviso.AvisoId)));
        }

        listaAvisos.appendChild(avisoItem);
    });
}


function inicializarFiltrosAvisos() {
    if (window._filtrosAvisosInicializados) return;
    window._filtrosAvisosInicializados = true;

    const inputTitulo = document.getElementById('buscarAvisoTitulo');
    const fechaDesde = document.getElementById('fechaDesdeAviso');
    const fechaHasta = document.getElementById('fechaHastaAviso');
    const btnLimpiar = document.getElementById('btnLimpiarFiltrosAvisos');

    if (inputTitulo) inputTitulo.addEventListener('input', aplicarFiltrosYRender);
    if (fechaDesde) fechaDesde.addEventListener('change', aplicarFiltrosYRender);
    if (fechaHasta) fechaHasta.addEventListener('change', aplicarFiltrosYRender);

    if (btnLimpiar) btnLimpiar.addEventListener('click', function () {
        if (inputTitulo) inputTitulo.value = '';
        if (fechaDesde) fechaDesde.value = '';
        if (fechaHasta) fechaHasta.value = '';
        aplicarFiltrosYRender();
    });
}

function aplicarFiltrosYRender() {
    if (!_avisosCache.length) {
        renderizarAvisos([]);
        return;
    }

    const term = document.getElementById('buscarAvisoTitulo')?.value.toLowerCase() || '';
    const desdeValue = document.getElementById('fechaDesdeAviso')?.value;
    const hastaValue = document.getElementById('fechaHastaAviso')?.value;

    const desde = desdeValue ? new Date(desdeValue + "T00:00:00") : null;
    const hasta = hastaValue ? new Date(hastaValue + "T23:59:59") : null;

    const filtrados = _avisosCache.filter(a => {

        //Filtro por título
        if (term && !(a.Titulo || '').toLowerCase().includes(term))
            return false;

        // Fecha
        if (desde || hasta) {

            if (!a.FechaCreacionIso) return false;

            const fechaAviso = new Date(a.FechaCreacionIso);
            if (isNaN(fechaAviso)) return false;

            if (desde && fechaAviso < desde)
                return false;

            if (hasta && fechaAviso > hasta)
                return false;
        }

        return true;
    });

    renderizarAvisos(filtrados);
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

//Obtiene los datos de un aviso desde su id
async function editarAviso(avisoId) {

    try {
        const response = await fetch(`/Materias/ObtenerAvisoPorId?avisoId=${avisoId}`);
        if (!response.ok) throw new Error("No se pudo obtener el aviso.");

        const aviso = await response.json();

        // llenar campos del modal
        document.getElementById("editarAvisoId").value = aviso.AvisoId;
        document.getElementById("editarTituloAviso").value = aviso.Titulo;
        document.getElementById("editarDescripcionAviso").value = aviso.Descripcion;
        document.getElementById("editarEnlacesAviso").value = aviso.Enlaces || '';
        document.getElementById("editarFechaInicioAviso").value = aviso.FechaInicio;
        document.getElementById("editarFechaFinAviso").value = aviso.FechaFin;
        document.getElementById("editarFrecuenciaDiasAviso").value = aviso.FrecuenciaDias;

        // mostrar modal
        const modal = new bootstrap.Modal(document.getElementById("editarAvisoModal"));
        modal.show();

    } catch (error) {
        Swal.fire("Error", error.message, "error");
    }
}

// Edita el aviso
async function guardarEdicionAviso() {

    const form = document.getElementById("editarAvisosForm");

    if (!form.checkValidity()) {
        form.classList.add("was-validated");
        return;
    }

    const data = {
        AvisoId: document.getElementById("editarAvisoId").value,
        Titulo: document.getElementById("editarTituloAviso").value.trim(),
        Descripcion: document.getElementById("editarDescripcionAviso").value.trim(),
        Enlaces: document.getElementById("editarEnlacesAviso").value.trim(),
        FechaInicio: document.getElementById("editarFechaInicioAviso").value,
        FechaFin: document.getElementById("editarFechaFinAviso").value,
        FrecuenciaDias: parseInt(document.getElementById("editarFrecuenciaDiasAviso").value) || 0
    };

    let hoy = new Date().toISOString().split("T")[0];

    if (data.FechaInicio < hoy) {
        Swal.fire({
            icon: "warning",
            title: "Fecha inválida",
            text: "La fecha de inicio no puede ser menor a hoy."
        });
        return;
    }

    if (data.FechaFin < data.FechaInicio) {
        Swal.fire({
            icon: "warning",
            title: "Fechas inválidas",
            text: "La fecha de fin debe ser mayor o igual a la fecha de inicio."
        });
        return;
    }

    if (data.FrecuenciaDias < 1) {
        Swal.fire({
            icon: "warning",
            title: "Frecuencia inválida",
            text: "La frecuencia debe ser al menos 1 día."
        });
        return;
    }

    if (data.Enlaces) {
        let linksArray = data.Enlaces.split("\n");

        for (let link of linksArray) {
            link = link.trim();
            if (link && !link.startsWith("https")) {
                Swal.fire({
                    icon: "warning",
                    title: "Enlace inválido",
                    text: "Todos los enlaces deben comenzar con https."
                });
                return;
            }
        }
    }

    try {
        const response = await fetch("/Materias/EditarAviso", {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        });

        if (!response.ok)
            throw new Error("No se pudo actualizar el aviso.");

        Swal.fire({
            toast: true,
            position: "top-end",
            icon: "success",
            title: "Aviso editado correctamente",
            showConfirmButton: false,
            timer: 3000
        });

        bootstrap.Modal
            .getInstance(document.getElementById("editarAvisoModal"))
            .hide();

        form.classList.remove("was-validated");
        form.reset();

        cargarAvisosDeMateria();

    } catch (error) {
        Swal.fire("Error", error.message, "error");
    }
}