(function () {

    let modalDetalle;
    let modalDetalleEl;

    document.addEventListener("DOMContentLoaded", function () {

        modalDetalleEl = document.getElementById("modalDetalleEvento");
        modalDetalle = bootstrap.Modal.getOrCreateInstance(modalDetalleEl);

    });

    window.abrirModalDetalle = async function (eventoId) {
        try {

            // Cerrar modal del día antes
            const modalDiaEl = document.getElementById("modalEvento");
            const modalDia = bootstrap.Modal.getInstance(modalDiaEl);
            if (modalDia) modalDia.hide();

            const resp = await fetch(`/EventosAgenda/ObtenerEventoPorId?id=${eventoId}`);
            if (!resp.ok) {
                Swal.fire("Error", "No se pudo cargar el evento", "error");
                return;
            }

            const payload = await resp.json();

            if (payload.mensaje) {
                Swal.fire("Aviso", payload.mensaje, "info");
                return;
            }

            const { evento, gruposConMaterias, materiasSueltas, esPersonal } = payload;

            // --- DATOS PRINCIPALES ---
            document.getElementById("detalle-titulo").textContent = evento.titulo;
            document.getElementById("detalle-descripcion").textContent = evento.descripcion || "";

            const opciones = {
                year: "numeric",
                month: "short",
                day: "numeric",
                hour: "2-digit",
                minute: "2-digit"
            };

            document.getElementById("detalle-fecha-inicio").textContent =
                new Date(evento.fechaInicio).toLocaleString("es-MX", opciones);

            document.getElementById("detalle-fecha-final").textContent =
                new Date(evento.fechaFinal).toLocaleString("es-MX", opciones);

            document.getElementById("detalle-color").textContent = evento.color;

            // --- DESTINATARIOS ---
            const contDest = document.getElementById("detalle-destinatarios");
            const contGrupos = document.getElementById("detalle-grupos");
            const contMaterias = document.getElementById("detalle-materias");
            const ulGrupos = document.getElementById("lista-grupos");
            const ulMaterias = document.getElementById("lista-materias");

            ulGrupos.innerHTML = "";
            ulMaterias.innerHTML = "";

            if (esPersonal || (!gruposConMaterias.length && !materiasSueltas.length)) {
                contDest.style.display = "none";
            } else {
                contDest.style.display = "block";

                // Grupos
                if (gruposConMaterias.length) {
                    contGrupos.style.display = "block";

                    gruposConMaterias.forEach(g => {
                        const liGrupo = document.createElement("li");
                        liGrupo.innerHTML = `<strong>${g.nombre}</strong>`;

                        const ulMat = document.createElement("ul");

                        g.materias.forEach(m => {
                            const li = document.createElement("li");
                            li.textContent = m.nombre;
                            li.className = m.isSelected
                                ? "materia-selected"
                                : "materia-not-selected";
                            ulMat.appendChild(li);
                        });

                        liGrupo.appendChild(ulMat);
                        ulGrupos.appendChild(liGrupo);
                    });
                } else {
                    contGrupos.style.display = "none";
                }

                // Materias sueltas
                if (materiasSueltas.length) {
                    contMaterias.style.display = "block";

                    materiasSueltas.forEach(m => {
                        const li = document.createElement("li");
                        li.textContent = m.nombre;
                        ulMaterias.appendChild(li);
                    });
                } else {
                    contMaterias.style.display = "none";
                }
            }

            modalDetalleEl.dataset.eventoId = evento.eventoId;
            modalDetalle.show();

        } catch (err) {
            console.error(err);
            Swal.fire("Error", "Error al cargar el detalle", "error");
        }
    };

    // ELIMINAR EVENTO
    const btnEliminarEvento = document.getElementById("btnEliminarEvento");

    if (btnEliminarEvento) {
        btnEliminarEvento.addEventListener("click", async function () {

            const id = modalDetalleEl.dataset.eventoId;

            if (!id) {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "Evento no encontrado"
                });
                return;
            }

            const confirmacion = await Swal.fire({
                title: "Eliminar evento",
                text: "Este evento no se podrá recuperar",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Sí, eliminar",
                cancelButtonText: "Cancelar"
            });

            if (!confirmacion.isConfirmed) return;

            try {
                const resp = await fetch(`/EventosAgenda/EliminarEvento/${id}`, {
                    method: "DELETE"
                });

                if (!resp.ok) {
                    throw new Error("Error al eliminar");
                }

                Swal.fire({
                    icon: "success",
                    title: "Evento eliminado",
                    timer: 1500,
                    showConfirmButton: false
                });

                // Cerrar modal detalle
                modalDetalle.hide();

                // Notificar al sistema
                document.dispatchEvent(new CustomEvent("eventoEliminado"));

            } catch (err) {
                console.error(err);
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "No se pudo eliminar el evento"
                });
            }
        });
    }

})();
