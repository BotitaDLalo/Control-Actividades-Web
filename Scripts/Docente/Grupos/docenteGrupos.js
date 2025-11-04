var div = document.getElementById("docente-datos");
var docenteIdGlobal = div.dataset.docenteid;

//Crea un nuevo grupo, con la posibilidad de agregar una materia sin grupo, y crear directamente varias materia para ese grupo
async function guardarGrupo() {
    const nombre = document.getElementById("nombreGrupo").value;
    const descripcion = document.getElementById("descripcionGrupo").value;
    const color = "#2196F3";
    const checkboxes = document.querySelectorAll(".materia-checkbox:checked");

    
    if (nombre.trim() === '') {
        Swal.fire({
            position: "top-end",
            icon: "question",
            title: "Ingrese nombre del grupo.",
            showConfirmButton: false,
            timer: 2500
        });
        return;
    }

    // Obtener IDs de materias seleccionadas en los checkboxes
    const materiasSeleccionadas = Array.from(checkboxes).map(cb => cb.value);

    // Obtener materias creadas en los inputs
    const materiasNuevas = [];
    document.querySelectorAll(".materia-item").forEach(materiaDiv => {
        const nombreMateria = materiaDiv.querySelector(".nombreMateria").value.trim();
        const descripcionMateria = materiaDiv.querySelector(".descripcionMateria").value.trim();
        if (nombreMateria) {
            materiasNuevas.push({ NombreMateria: nombreMateria, Descripcion: descripcionMateria });
        }
    });

    // Crear el grupo en la base de datos
    const response = await fetch('/Grupos/CrearGrupo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            NombreGrupo: nombre,
            Descripcion: descripcion,
            CodigoColor: color,
            DocenteId: docenteIdGlobal
        })
    });
    
    if (response.ok) {
        const grupoCreado = await response.json();
        const grupoId = grupoCreado.grupoId;

        // Guardar materias nuevas directamente asociadas al grupo
        for (const materia of materiasNuevas) {
            const responseMateria = await fetch('/Materias/CrearMateria', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    NombreMateria: materia.NombreMateria,
                    Descripcion: materia.Descripcion,
                    CodigoColor: color, // Enviamos el color de la materia
                    DocenteId: docenteIdGlobal
                })
            });

            if (responseMateria.ok) {
                const materiaCreada = await responseMateria.json();
                materiasSeleccionadas.push(materiaCreada.materiaId);
            }
        }

        // Asociar materias seleccionadas al grupo
        if (materiasSeleccionadas.length > 0) {
            //console.log("Checkboxes seleccionados:"); Revisar que los ids se estén pasando
            checkboxes.forEach(cb => console.log(cb.value, cb.checked));
            await asociarMateriasAGrupo(grupoId, materiasSeleccionadas);
        }

        Swal.fire({
            position: "top-end",
            icon: "success",
            title: "Grupo registrado correctamente.",
            showConfirmButton: false,
            timer: 2000
        });
        document.getElementById("gruposForm").reset();
        cargarGrupos();
        cargarMateriasSinGrupo();
        cargarMaterias();
    } else {
        Swal.fire({
            position: "top-end",
            icon: "error",
            title: "Error al registrar grupo.",
            showConfirmButton: false,
            timer: 2000
        });
    }
}

async function asociarMateriasAGrupo(grupoId, materiasSeleccionadas) {
    try {
        const response = await fetch('/Materias/AsociarMateriasAGrupo', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                GrupoId: grupoId,
                MateriaIds: materiasSeleccionadas
            })
        });

        if (response.ok) {
            const data = await response.json();
            console.log(data.mensaje || "Materias asociadas correctamente");
        } else {
            console.error("Error al asociar materias al grupo");
        }
    } catch (error) {
        console.error("Error en la solicitud:", error);
    }
}


//funcion que ayuda a agregar materias nuevas para el grupo
function agregarMateria() {
    const materiasContainer = document.getElementById("listaMaterias");

    const materiaDiv = document.createElement("div");
    materiaDiv.classList.add("materia-item");

    materiaDiv.innerHTML = `
        <input type="text" placeholder="Nombre de la Materia" class="nombreMateria">
        <input type="text" placeholder="Descripción" class="descripcionMateria">
        <button type="button" onclick="removerDeLista(this)">❌</button>
    `;

    materiasContainer.appendChild(materiaDiv);
}

// Remover materia del formulario antes de enviarla
function removerDeLista(button) {
    button.parentElement.remove();
}



//Funcion para obtener los grupos de la base de datos y mostrarlos

async function cargarGrupos() {
    const response = await fetch(`/Grupos/ObtenerGrupos?docenteId=${docenteIdGlobal}`);
    if (response.ok) {
        const grupos = await response.json();
        const listaGrupos = document.getElementById("listaGrupos");
        listaGrupos.innerHTML = "";

        if (grupos.length === 0) {
            const mensaje = document.createElement("p");
            mensaje.classList.add("text-center", "text-muted");
            mensaje.textContent = "No hay grupos registrados.";
            listaGrupos.appendChild(mensaje);
            return;
        }

        grupos.forEach(grupo => {
            const card = document.createElement("div");
            card.classList.add("card", "bg-primary", "text-white", "mb-3");
            card.style.cursor = "pointer";
            card.style.maxWidth = "30em";
            card.style.height = "6em";
            card.style.display = "block";
            card.style.alignItems = "center";
            card.style.transition = "transform 0.3s ease, box-shadow 0.3s ease";
            card.onmouseover = () => {
                card.style.transform = "translateY(-10px)";
                card.style.boxShadow = "0 10px 20px rgba(0, 0, 0, 0.2)";
            };
            card.onmouseout = () => {
                card.style.transform = "";
                card.style.boxShadow = "";
            };

            // 📌 Imagen del grupo
            const img = document.createElement("img");
            img.classList.add("card-img-top");
            img.src = "/Content/Iconos/1-26.svg";
            img.alt = "Grupo";
            img.style.maxWidth = "25%";
            img.style.margin = "auto";
            img.style.padding = "0.5em";
            img.style.borderRadius = "0.7em";

            // 📌 Contenedor del contenido
            const cardBody = document.createElement("div");
            cardBody.classList.add("card-body");
            cardBody.style.display = "flex";
            cardBody.style.justifyContent = "space-between";
            cardBody.style.alignItems = "center";
            cardBody.style.flex = "1";
            cardBody.style.overflow = "hidden";

            // 📌 Sección de texto
            const textSection = document.createElement("div");
            textSection.classList.add("text-section");
            textSection.style.maxWidth = "100%";
            textSection.style.overflow = "hidden";
            textSection.style.display = "flex";
            textSection.style.flexDirection = "column";
            textSection.style.justifyContent = "center";

            const title = document.createElement("h5");
            title.classList.add("card-title");
            title.textContent = grupo.NombreGrupo;
            title.style.whiteSpace = "nowrap";
            title.style.overflow = "hidden";
            title.style.textOverflow = "ellipsis";
            title.style.margin = "0";
            title.style.fontWeight = "bold";

            const description = document.createElement("p");
            description.classList.add("card-text");
            description.textContent = grupo.Descripcion || "Sin descripción";
            description.style.whiteSpace = "nowrap";
            description.style.overflow = "hidden";
            description.style.textOverflow = "ellipsis";
            description.style.margin = "0";

            textSection.appendChild(title);
            textSection.appendChild(description);

            // 📌 Sección del botón (Icono de engranaje)
            const lista = document.getElementById('listaGrupos');

            const ctaSection = document.createElement("div");
            ctaSection.classList.add("cta-section", "dropdown", "dropend");
            ctaSection.style.maxWidth = "40%";
            ctaSection.style.display = "flex";
            ctaSection.style.flexDirection = "column";
            ctaSection.style.justifyContent = "center";
            ctaSection.style.position = "relative";

            const settingsButton = document.createElement("button");
            settingsButton.classList.add("btn", "btn-link", "text-white", "p-0");
            settingsButton.type = "button";
            settingsButton.setAttribute("data-bs-toggle", "dropdown");
            settingsButton.setAttribute("aria-expanded", "false");
            settingsButton.style.width = "3em";
            settingsButton.style.height = "3em";
            settingsButton.style.display = "flex";
            settingsButton.style.alignItems = "center";
            settingsButton.style.justifyContent = "center";
            settingsButton.style.border = "none";
            settingsButton.style.outline = "none";
            settingsButton.style.textDecoration = "none";

            const settingsIcon = document.createElement("i");
            settingsIcon.classList.add("fas", "fa-cog");
            settingsIcon.style.fontSize = "1.5em";



            // ... (el código anterior hasta crear el settingsButton)

            // Crear el menú dropdown con clases de Bootstrap
            const dropdownMenu = document.createElement("ul");
            dropdownMenu.classList.add("dropdown-menu", "dropdown-menu-end", "mt-2");

            // Añadir items al dropdown
            const dropdownItems = [
                { text: "Eliminar", action: () => eliminarGrupo(grupo.GrupoId) },
                { text: "Aviso Grupal", action: () => crearAvisoGrupal(grupo.GrupoId) },
                { text: "Desactivar", action: () => console.log("Opción 3 seleccionada") }
            ];

            dropdownItems.forEach(item => {
                const li = document.createElement("li");
                const dropdownItem = document.createElement("a");
                dropdownItem.classList.add("dropdown-item");
                dropdownItem.href = "#";
                dropdownItem.textContent = item.text;
                dropdownItem.addEventListener("click", (e) => {
                    e.preventDefault();
                    item.action();
                });
                li.appendChild(dropdownItem);
                dropdownMenu.appendChild(li);
            });

            // Ensamblar todos los elementos
            settingsButton.appendChild(settingsIcon);
            ctaSection.appendChild(settingsButton);
            ctaSection.appendChild(dropdownMenu);
            lista.appendChild(ctaSection);

            // 📌 Contenedor de materias (inicialmente oculto)
            const materiasContainer = document.createElement("div");
            materiasContainer.id = `materiasContainer-${grupo.GrupoId}`;
            materiasContainer.classList.add("materias-container");
            materiasContainer.style.display = "none";
            materiasContainer.style.paddingLeft = "20px";
            materiasContainer.style.marginBottom = "20px";

            // 📌 Evento al hacer clic en la tarjeta
            card.onclick = () => {
                handleCardClick(grupo.GrupoId);
            };

            // 📌 Estructura final
            cardBody.appendChild(textSection);
            cardBody.appendChild(ctaSection);

            const contentWrapper = document.createElement("div");
            contentWrapper.style.display = "flex";
            contentWrapper.style.width = "100%";
            contentWrapper.appendChild(img);
            contentWrapper.appendChild(cardBody);

            card.appendChild(contentWrapper);

            listaGrupos.appendChild(card);
            listaGrupos.appendChild(materiasContainer);
        });
    } else {
        Swal.fire({
            title: "Error al cargar los grupos.",
            html: "Reintentando en <b></b> segundos...",
            timer: 4000,
            timerProgressBar: true,
            allowOutsideClick: false,
            showCancelButton: true,
            cancelButtonText: "Cerrar sesión",
            didOpen: () => {
                Swal.showLoading();
                const timer = Swal.getPopup().querySelector("b");
                let interval = setInterval(() => {
                    timer.textContent = `${Math.floor(Swal.getTimerLeft() / 1000)}`;
                }, 100);
            },
            willClose: () => clearInterval(timerInterval)
        }).then((result) => {
            if (result.dismiss === Swal.DismissReason.timer) {
                cargarGrupos();
            } else if (result.dismiss === Swal.DismissReason.cancel) {
                cerrarSesion();
            }
        });
    }

    document.getElementById('gruposModal').addEventListener('hidden.bs.modal', cargarGrupos);
}




// Función para cargar materias de un grupo cuando se hace clic en la card del grupo
async function handleCardClick(grupoId) {
    localStorage.setItem("grupoIdSeleccionado", grupoId); //Se guardar el localstorage el id del grupo seleccionado

    // Ocultar todas las materias de otros grupos
    document.querySelectorAll("[id^='materiasContainer-']").forEach(container => {
        if (container.id !== `materiasContainer-${grupoId}`) {
            container.style.display = "none";
            container.innerHTML = "";
        }
    });

    const materiasContainer = document.getElementById(`materiasContainer-${grupoId}`);

    if (materiasContainer.style.display === "block") {
        // Si las materias están visibles, ocultarlas
        materiasContainer.style.display = "none";
        materiasContainer.innerHTML = "";
    } else {
        // Si están ocultas, obtener las materias y mostrarlas
        const response = await fetch(`/Grupos/ObtenerMateriasPorGrupo?grupoId=${grupoId}`);
        if (response.ok) {
            const materias = await response.json();
            if (materias.length === 0) {
                materiasContainer.innerHTML = "<p>Aún no hay materias registradas para este grupo.</p>";
            } else {
                const rowContainer = document.createElement("div");
                rowContainer.classList.add("row", "g-3");

                materias.forEach(materia => {
                    const col = document.createElement("div");
                    col.classList.add("col-md-3"); // Ajusta el tamaño de la tarjeta en la fila

                    const card = document.createElement("div");
                    card.classList.add("card", "bg-light", "mb-3", "shadow-sm");
                    card.style.maxWidth = "100%";

                    // Header
                    const header = document.createElement("div");
                    header.classList.add("card-header", "bg-primary", "text-white", "fs-4");
                    header.style.display = "flex";
                    header.style.justifyContent = "space-between";
                    header.textContent = materia.NombreMateria;

                    // Crear el dropdown
                    const dropdown = document.createElement("div");
                    dropdown.classList.add("dropdown");

                    const button = document.createElement("button");
                    button.classList.add("btn", "btn-link", "p-0", "text-white");
                    button.setAttribute("data-bs-toggle", "dropdown");
                    button.setAttribute("aria-expanded", "false");

                    const icon = document.createElement("i");
                    icon.classList.add("fas", "fa-ellipsis-v");
                    button.appendChild(icon);

                    const ul = document.createElement("ul");
                    ul.classList.add("dropdown-menu", "dropdown-menu-end");

                    const editLi = document.createElement("li");
                    const editLink = document.createElement("a");
                    editLink.classList.add("dropdown-item");
                    editLink.href = "#";
                    editLink.onclick = () => editarMateria(materia.MateriaId);
                    editLink.textContent = "Editar";
                    editLi.appendChild(editLink);

                    const deleteLi = document.createElement("li");
                    const deleteLink = document.createElement("a");
                    deleteLink.classList.add("dropdown-item");
                    deleteLink.href = "#";
                    deleteLink.onclick = () => eliminarMateria(materia.MateriaId);
                    deleteLink.textContent = "Eliminar";
                    deleteLi.appendChild(deleteLink);

                    // Añadir los elementos al menú desplegable
                    ul.appendChild(editLi);
                    ul.appendChild(deleteLi);

                    // Añadir el botón y el menú al dropdown
                    dropdown.appendChild(button);
                    dropdown.appendChild(ul);

                    // Añadir el dropdown al header
                    header.appendChild(dropdown);

                    // Body
                    const body = document.createElement("div");
                    body.classList.add("card-body");

                    const title = document.createElement("h5");
                    title.classList.add("card-title");

                    const description = document.createElement("p");
                    description.classList.add("card-text");
                    description.textContent = materia.Descripcion || "Sin descripción";
                    
                    body.appendChild(title);
                    body.appendChild(description);

                    // Actividades Recientes - Crear una sección para las actividades
                    if (materia.ActividadesRecientes && materia.ActividadesRecientes.length > 0) {
                        const actividadesContainer = document.createElement("div");
                        actividadesContainer.classList.add("mt-3"); // Margen superior para separar las actividades

                        materia.ActividadesRecientes.forEach(actividad => {
                            const actividadItem = document.createElement("div");
                            actividadItem.classList.add("actividad-item");

                            const fechaFormateada = new Date(actividad.fechaCreacion).toLocaleDateString('es-ES', {
                                day: '2-digit',
                                month: '2-digit',
                                year: 'numeric'
                            });

                            const actividadLink = document.createElement("a");
                            actividadLink.href = "#";
                            actividadLink.classList.add("actividad-link");
                            actividadLink.textContent = actividad.nombreActividad;
                            actividadLink.setAttribute("data-id", actividad.actividadId, materia.MateriaId);

                            const actividadFecha = document.createElement("p");
                            actividadFecha.classList.add("actividad-fecha");
                            actividadFecha.textContent = `Asignada: ${fechaFormateada}`;

                            actividadItem.appendChild(actividadLink);
                            actividadItem.appendChild(actividadFecha);

                            actividadesContainer.appendChild(actividadItem);
                        });

                        body.appendChild(actividadesContainer); // Agregar actividades al cuerpo de la tarjeta
                    }

                    // Footer
                    const footer = document.createElement("div");
                    footer.classList.add("card-footer", "d-flex", "justify-content-between", "align-items-center");

                    const btnVerMateria = document.createElement("button");
                    btnVerMateria.classList.add("btn", "btn-sm", "btn-primary");
                    btnVerMateria.textContent = "Ver Materia";
                    btnVerMateria.onclick = () => irAMateria(materia.MateriaId);

                    // Contenedor de iconos
                    const iconContainer = document.createElement("div");
                    iconContainer.classList.add("d-flex", "gap-2");

                    const icons = [
                        { src: "https://cdn-icons-png.flaticon.com/512/1828/1828817.png", title: "Ver Actividades", onclick: () => irAMateria(materia.MateriaId, 'actividades') },
                        { src: "https://cdn-icons-png.flaticon.com/512/847/847969.png", title: "Ver Integrantes", onclick: () => irAMateria(materia.MateriaId, 'alumnos') },
                    ];

                    icons.forEach(({ src, title, onclick }) => {
                        const img = document.createElement("img");
                        img.classList.add("icon-action");
                        img.src = src;
                        img.alt = title;
                        img.title = title;
                        img.onclick = onclick;
                        iconContainer.appendChild(img);
                    });

                    footer.appendChild(btnVerMateria);
                    footer.appendChild(iconContainer);

                    // Construcción de la card
                    card.appendChild(header);
                    card.appendChild(body);
                    card.appendChild(footer);
                    col.appendChild(card);

                    // Agregar la columna al contenedor de la fila
                    rowContainer.appendChild(col);
                });
                materiasContainer.appendChild(rowContainer);
            }
            materiasContainer.style.display = "block";
        } else {
            Swal.fire({
                position: "top-end",
                icon: "error",
                title: "Error al obtener las materias del grupo.",
                showConfirmButton: false,
                timer: 2000
            });
        }
    }
}




//Funciones de contenedor de grupo
function editarGrupo(id) {
    alert("Editar grupo " + id); // Muestra una alerta indicando que el grupo será editado
}

async function eliminarGrupo(grupoId) {
    Swal.fire({
        title: "¿Qué deseas eliminar?",
        text: "Elige si deseas eliminar solo el grupo o también las materias que contiene.",
        icon: "warning",
        showCancelButton: true,
        showDenyButton: true,
        confirmButtonText: "Eliminar solo grupo",
        denyButtonText: "Eliminar grupo y materias",
        cancelButtonText: "Cancelar"
    }).then(async (result) => {
        if (result.isConfirmed) {
            // Llamar al controlador que elimina solo el grupo
            const response = await fetch(`/Grupos/EliminarGrupo?grupoId=${grupoId}`, { method: "DELETE" });
            if (response.ok) {
                await Swal.fire({
                    position: "top-end",
                    icon: "success",
                    title: "El grupo ha sido eliminado.",
                    showConfirmButton: false,
                    timer: 2000
                });
                inicializar();
            } else {
                await Swal.fire({
                    position: "top-end",
                    icon: "error",
                    title: "No se pudo eliminar el grupo.",
                    showConfirmButton: false,
                    timer: 2000
                });
            }
        } else if (result.isDenied) {
            // Llamar al nuevo controlador que elimina grupo y materias
            const response = await fetch(`/Grupos/EliminarGrupoConMaterias?grupoId=${grupoId}`, { method: "DELETE" });
            if (response.ok) {
                await Swal.fire({
                    position: "top-end",
                    icon: "success",
                    title: "El grupo y sus materias han sido eliminados.",
                    showConfirmButton: false,
                    timer: 2000
                });
                inicializar();
            } else {
                await Swal.fire({
                    position: "top-end",
                    icon: "error",
                    title: "No se pudo eliminar el grupo y sus materias.",
                    showConfirmButton: false,
                    timer: 2000
                });
            }
        }
    });
}


function agregarMateriaAlGrupo(id) {
    alert("Agregar Materia Al Grupo " + id); // Muestra una alerta indicando que el grupo será desactivado
}


function crearAvisoGrupal(id) {
    Swal.fire({
        title: "Crear Aviso",
        html:
            '<input id="tituloAviso" class="swal2-input" placeholder="Título del aviso">' +
            '<textarea id="descripcionAviso" class="swal2-textarea" placeholder="Descripción del aviso"></textarea>',
        showCancelButton: true,
        confirmButtonText: "Crear",
        cancelButtonText: "Cancelar",
        preConfirm: () => {
            const titulo = document.getElementById("tituloAviso").value.trim();
            const descripcion = document.getElementById("descripcionAviso").value.trim();

            if (!titulo || !descripcion) {
                Swal.showValidationMessage("Debes completar todos los campos");
                return false;
            }
            return { titulo, descripcion };
        }
    }).then((result) => {
        if (result.isConfirmed) {
            const datos = {
                GrupoId: id,
                Titulo: result.value.titulo,
                Descripcion: result.value.descripcion,
                DocenteId: docenteIdGlobal
            };

            fetch("/Materias/CrearAvisoPorGrupo", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(datos)
            })
                .then(response => response.json())
                .then(data => {
                    if (data.mensaje) {
                        Swal.fire("Éxito", data.mensaje, "success");
                    } else {
                        Swal.fire("Error", "No se pudo crear el aviso", "error");
                    }
                })
                .catch(error => {
                    console.error("Error al enviar el aviso:", error);
                    Swal.fire("Error", "Ocurrió un error al crear el aviso", "error");
                });
        }
    });
}
