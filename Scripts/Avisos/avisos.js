//// Funcion que carga los avisos a la vista.
//let _avisosCache = null; // cache local de avisos para filtrar en cliente
//let _rolUsuarioCache = null;
//async function cargarAvisosDeMateria() {
//    const listaAvisos = document.getElementById("listaDeAvisosDeMateria");
//    try {
//        const params = new URLSearchParams(window.location.search);

//        const materiaId = params.get("materiaId");
//        if (!materiaId) return;

//        const response = await fetch(
//            `/Materias/ObtenerAvisos?IdMateria=${materiaId}`
//        );
//        if (!response.ok) throw new Error("No se encontraron avisos.");
//        const responseJson = await response.json();

//        const avisos = responseJson.avisos || [];
//        const rolUsuario = responseJson.RolUsuario || '';
//        // guardar en cache y renderizar
//        _avisosCache = avisos.slice();
//        _rolUsuarioCache = rolUsuario;
//        aplicarFiltrosYRender();
//        // inicializar listeners de filtros (solo una vez)
//        inicializarFiltrosAvisos();
//    }
//    catch (error) {
//        listaAvisos.innerHTML = `<p class="aviso-error">${error.message}</p>`;
//    }

//    // intentar cargar avisos al cargar el documento
//    if (typeof document !== 'undefined'){
//        document.addEventListener('DOMContentLoaded', function(){
//        try {
//            cargarAvisosDeMateria();
//        }
//        catch (e) {
//            console.warn('Error cargando avisos iniciales', e);
//        }
//        });
//    }
//}

//function inicializarFiltrosAvisos() {
//    if (window._filtrosAvisosInicializados) return;
//    window._filtrosAvisosInicializados = true;
//    const inputTitulo = document.getElementById('buscarAvisoTitulo');
//    const fechaDesde = document.getElementById('fechaDesdeAviso');
//    const fechaHasta = document.getElementById('fechaHastaAviso');
//    const btnLimpiar = document.getElementById('btnLimpiarFiltrosAvisos');
//    if (inputTitulo) inputTitulo.addEventListener('input', aplicarFiltrosYRender);
//    if (fechaDesde) fechaDesde.addEventListener('change', aplicarFiltrosYRender);
//    if (fechaHasta) fechaHasta.addEventListener('change', aplicarFiltrosYRender);
//    if (btnLimpiar) btnLimpiar.addEventListener('click', function(){
//        if (inputTitulo) inputTitulo.value = '';
//        if (fechaDesde) fechaDesde.value = '';
//        if (fechaHasta) fechaHasta.value = '';
//        aplicarFiltrosYRender();
//    });
//}

//function aplicarFiltrosYRender(){
//  if (!_avisosCache) return;
//  const inputTitulo = document.getElementById('buscarAvisoTitulo');
//  const fechaDesde = document.getElementById('fechaDesdeAviso');
//  const fechaHasta = document.getElementById('fechaHastaAviso');
//  const term = inputTitulo && inputTitulo.value ? inputTitulo.value.trim().toLowerCase() : '';
//  const desde = fechaDesde && fechaDesde.value ? new Date(fechaDesde.value) : null;
//  const hasta = fechaHasta && fechaHasta.value ? new Date(fechaHasta.value) : null;
//  const filtrados = _avisosCache.filter(function(a){
//    try{
//      // filtro por título
//      if (term){
//        const titulo = (a.Titulo || '').toString().toLowerCase();
//        if (titulo.indexOf(term) === -1) return false;
//      }
//      // filtro por fecha (asume Campo FechaCreacionIso, FechaCreacion o Fecha)
//      if (desde || hasta){
//        const rawFecha = a.FechaCreacionIso || a.FechaCreacion || a.Fecha || a.fecha || null;
//        if (!rawFecha) return false;

//        // función para parsear fechas retornadas por el servidor de forma robusta
//        function parseServerFecha(s){
//          if (!s) return null;
//          // si es ISO directo
//          const dIso = new Date(s);
//          if (!isNaN(dIso.getTime())) return dIso;

//          // intentar parsear formato en español como "6 de diciembre de 2025 14:23:00"
//          try{
//            const txt = s.toString().toLowerCase().replace(/[.,]/g,'');
//            // buscar año (4 dígitos)
//            const yearMatch = txt.match(/(20\d{2}|19\d{2})/);
//            const year = yearMatch ? parseInt(yearMatch[0],10) : null;
//            // buscar día (1-2 dígitos) antes de 'de'
//            const dayMatch = txt.match(/(\b\d{1,2})\s+de\s+/);
//            const day = dayMatch ? parseInt(dayMatch[1],10) : null;
//            // mapa de meses en español
//            const meses = { 'enero':0,'ene':0,'febrero':1,'feb':1,'marzo':2,'mar':2,'abril':3,'abr':3,'mayo':4,'may':4,'junio':5,'jun':5,'julio':6,'jul':6,'agosto':7,'ago':7,'septiembre':8,'sep':8,'setiembre':8,'octubre':9,'oct':9,'noviembre':10,'nov':10,'diciembre':11,'dic':11 };
//            // buscar mes por nombre
//            let mes = null;
//            for (const m in meses){ if (txt.indexOf(' ' + m + ' ') !== -1) { mes = meses[m]; break; } }
//            if (year != null && day != null && mes != null){
//              // intentar extraer hora si existe
//              const timeMatch = txt.match(/(\d{1,2}:\d{2}:?\d{0,2})/);
//              let hh=0, mm=0, ss=0;
//              if (timeMatch){
//                const parts = timeMatch[1].split(':'); hh = parseInt(parts[0]||0,10); mm = parseInt(parts[1]||0,10); ss = parseInt(parts[2]||0,10);
//              }
//              return new Date(year, mes, day, hh, mm, ss);
//            }
//          }catch(e){ /* fallthrough */ }
//          return null;
//        }

//        const f = parseServerFecha(rawFecha);
//        if (!f || isNaN(f.getTime())) return false;
//        // comparar sin hora: normalizar horas a 0
//        const fN = new Date(f.getFullYear(), f.getMonth(), f.getDate());
//        if (desde){ const dN = new Date(desde.getFullYear(), desde.getMonth(), desde.getDate()); if (fN < dN) return false; }
//        if (hasta){ const hN = new Date(hasta.getFullYear(), hasta.getMonth(), hasta.getDate()); if (fN > hN) return false; }
//      }
//      return true;
//    }catch(e){ return false; }
//  });
//  renderizarAvisos(filtrados, _rolUsuarioCache);
//}

////Funcion para publicar un aviso
//async function publicarAviso() {
//    // Obtener valores de los inputs
//    let titulo = document.getElementById("titulo").value.trim();
//    let descripcion = document.getElementById("descripcionAviso").value.trim();

//    // Validar que los campos no estén vacíos
//    if (!titulo || !descripcion) {
//        Swal.fire({
//            position: "top-end",
//            title: "Campos vacíos",
//            text: "Por favor, completa todos los campos.",
//            icon: "warning",
//            timer: 2500,
//            showConfirmButton: false,
//            customClass: {
//                title: 'sweetAlertTitleCustom',
//                popup: 'sweetAlertBgCustom'
//            }

//        });
//        return;
//    }

//    const params = new URLSearchParams(window.location.search);

//    let grupoId = params.get("grupoId");
//    if (!grupoId) {
//        grupoId = 0;
//    }

//    const materiaId = params.get("materiaId");
//    if (!materiaId) return;

//    // Crear objeto con los datos a enviar
//    let avisoData = {
//        //DocenteId: docenteId,
//        Titulo: titulo,
//        Descripcion: descripcion,
//        GrupoId: grupoId,
//        MateriaId: materiaId,
//    };

//    try {
//        // Enviar datos al controlador
//        let response = await fetch("/Materias/CrearAviso", {
//            method: "POST",
//            headers: {
//            "Content-Type": "application/json",
//            },
//            body: JSON.stringify(avisoData),
//        });

//        let result = await response.json();

//        if (response.ok) {
//            Swal.fire({
//                position: "top-end",
//                title: "Aviso creado",
//                text: "El aviso ha sido publicado correctamente.",
//                icon: "success",
//                timer: 3000,
//                showConfirmButton: false,
//                customClass: {
//                    title: 'sweetAlertTitleCustom',
//                    popup: 'sweetAlertBgCustom'
//                },
//            });

//            setTimeout(() => {
//                document.getElementById("avisosForm").reset(); //Resetear  formulario
//                cargarAvisosDeMateria();
//            }, 3000);
//        }
//        else {
//            Swal.fire({
//                position: "top-end",
//                title: "Error",
//                text: result.mensaje || "Error al crear el aviso.",
//                icon: "error",
//                timer: 3000,
//                showConfirmButton: false,
//                customClass: {
//                    title: 'sweetAlertTitleCustom',
//                    popup: 'sweetAlertBgCustom'
//                },
//            });
//        }
//    }
//    catch (error) {
//        console.error("Error:", error);
//        Swal.fire({
//            position: "top-end",
//            title: "Error",
//            text: "Hubo un problema al enviar el aviso.",
//            icon: "error",
//            timer: 3000,
//            showConfirmButton: false,
//            customClass: {
//                title: 'sweetAlertTitleCustom',
//                popup: 'sweetAlertBgCustom'
//            }
//        });
//    }
//}



//function renderizarAvisos(avisos, rolUsuario) {
//    const listaAvisos = document.getElementById("listaDeAvisosDeMateria");
//    listaAvisos.innerHTML = ""; // Limpiar el contenedor

//    if (avisos.length === 0) {
//    listaAvisos.innerHTML =
//        "<p>No hay avisos registrados para esta materia.</p>";
//    return;
//    }
//    avisos.reverse();

//    avisos.forEach((aviso) => {
//    const avisoItem = document.createElement("div");
//    avisoItem.classList.add("aviso-item");
//    //const descripcionAvisoConEnlace = convertirUrlsEnEnlaces(aviso.Descripcion);

//    let elemento = `
//            <div class="aviso-header">
//                <div class="aviso-icono">📢</div>
//                <div class="aviso-info">
//                    <strong>${aviso.Titulo}</strong>
//                    <p class="aviso-fecha-publicado">Publicado: ${aviso.FechaCreacion}</p>
//                    <p class="ver-completo">Ver completo</p>
//                </div>
//        `;

//    if (rolUsuario === "Docente") {
//      let opcionRol = `
//                <div class="aviso-botones-container">
//                    <button class="aviso-editar-btn" data-id="${aviso.AvisoId}">Editar</button>
//                    <button class="aviso-eliminar-btn" data-id="${aviso.AvisoId}">Eliminar</button>
//                </div>
//            </div>
//        `;

//      elemento += opcionRol;
//    }

//    let descripcionP = `
//        <div>
//        <p class="actividad-descripcion oculto">${aviso.Descripcion}</p>
//        </div>
//        `;

//    elemento += descripcionP;

//    avisoItem.innerHTML = elemento;

//    // Mostrar/ocultar descripción al hacer clic en "Ver completo"
//    const verCompleto = avisoItem.querySelector(".ver-completo");
//    const descripcion = avisoItem.querySelector(".actividad-descripcion");

//    verCompleto.addEventListener("click", () => {
//      // Alternar entre mostrar y ocultar la descripción
//      if (descripcion.classList.contains("oculto")) {
//        descripcion.classList.remove("oculto");
//        descripcion.classList.add("visible");
//      } else {
//        descripcion.classList.remove("visible");
//        descripcion.classList.add("oculto");
//      }
//    });

//    // Agregar eventos a los botones
//    const btnEliminar = avisoItem.querySelector(".aviso-eliminar-btn");
//    if (btnEliminar != null) {
//      btnEliminar.addEventListener("click", () => eliminarAviso(aviso.AvisoId));
//    }

//    const btnEditar = avisoItem.querySelector(".aviso-editar-btn");
//    if (btnEditar != null) {
//      btnEditar.addEventListener("click", () => editarAviso(aviso.AvisoId));
//    }

//    listaAvisos.appendChild(avisoItem);
//  });
//}
