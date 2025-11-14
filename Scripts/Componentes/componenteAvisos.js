class AvisosComponent {
    constructor(config) {
        this.modo = config.modo; // "docente" o "alumno"
        this.container = document.querySelector(config.container);
        this.materiaId = config.materiaId || null;
        this.alumnoId = config.alumnoId || null;
        this.docenteId = config.docenteId || null;
    }

    async cargarAvisos() {
        try {
            let url = "";
            if (this.modo === "docente" && this.materiaId) {
                url = `/Materias/ObtenerAvisos?IdMateria=${this.materiaId}`;
            } else if (this.modo === "alumno" && this.alumnoId) {
                url = `/Alumno/ObtenerAvisos?IdMateria=${this.materiaId}`;
            } else {
                throw new Error("Faltan parámetros para cargar los avisos.");
            }

            const response = await fetch(url);
            if (!response.ok) throw new Error("No se pudieron obtener los avisos.");

            const avisos = await response.json();
            this.renderizarAvisos(avisos);
        } catch (error) {
            this.container.innerHTML = `<p class="aviso-error text-danger">${error.message}</p>`;
        }
    }

    renderizarAvisos(avisos) {
        this.container.innerHTML = "";

        if (!avisos || avisos.length === 0) {
            this.container.innerHTML = "<p>No hay avisos disponibles.</p>";
            return;
        }

        avisos.reverse();

        avisos.forEach(aviso => {
            const avisoItem = document.createElement("div");
            avisoItem.classList.add("list-group-item", "aviso-item");

            avisoItem.innerHTML = `
                <div class="aviso-header d-flex justify-content-between align-items-start">
                    <div>
                        <div class="aviso-icono">📢</div>
                        <strong>${aviso.Titulo}</strong>
                        <p class="aviso-fecha-publicado text-muted">Publicado: ${aviso.FechaCreacion}</p>

                        <p class="aviso-fecha-publicado text-muted">Publicado: ${new Date(aviso.FechaCreacion).toLocaleString()}</p>
                        <p class="ver-completo text-primary" style="cursor:pointer;">Ver completo</p>
                    </div>
                    ${this.modo === "docente" ? `
                    <div class="aviso-botones-container">
                        <button class="aviso-editar-btn btn btn-warning btn-sm" data-id="${aviso.AvisoId}">Editar</button>
                        <button class="aviso-eliminar-btn btn btn-danger btn-sm" data-id="${aviso.AvisoId}">Eliminar</button>
                    </div>` : ""}
                </div>
                <div>
                    <p class="actividad-descripcion oculto">${aviso.Descripcion}</p>
                </div>
            `;

            // Mostrar/ocultar descripción
            const verCompleto = avisoItem.querySelector(".ver-completo");
            const descripcion = avisoItem.querySelector(".actividad-descripcion");
            verCompleto.addEventListener("click", () => {
                descripcion.classList.toggle("oculto");
                descripcion.classList.toggle("visible");
            });

            // Acciones solo si es docente
            if (this.modo === "docente") {
                const btnEditar = avisoItem.querySelector(".aviso-editar-btn");
                const btnEliminar = avisoItem.querySelector(".aviso-eliminar-btn");

                btnEditar.addEventListener("click", () => this.editarAviso(aviso.AvisoId));
                btnEliminar.addEventListener("click", () => this.eliminarAviso(aviso.AvisoId));
            }

            this.container.appendChild(avisoItem);
        });
    }

    async eliminarAviso(avisoId) {
        const confirmacion = await Swal.fire({
            title: '¿Eliminar aviso?',
            text: "No podrás revertir esto.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        });

        if (!confirmacion.isConfirmed) return;

        try {
            const response = await fetch(`/Materias/EliminarAviso?id=${avisoId}`, { method: 'DELETE' });
            if (response.ok) {
                Swal.fire('Eliminado', 'El aviso fue eliminado correctamente.', 'success');
                this.cargarAvisos();
            } else {
                Swal.fire('Error', 'No se pudo eliminar el aviso.', 'error');
            }
        } catch {
            Swal.fire('Error', 'Hubo un problema al eliminar el aviso.', 'error');
        }
    }

    async editarAviso(avisoId) {
        try {
            const response = await fetch(`/Materias/ObtenerAvisoPorId?avisoId=${avisoId}`);
            if (!response.ok) throw new Error("No se pudo obtener el aviso.");
            const aviso = await response.json();

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
                preConfirm: () => ({
                    titulo: document.getElementById("swal-titulo").value.trim(),
                    descripcion: document.getElementById("swal-descripcion").value.trim()
                })
            });

            if (!formValues) return;

            const updateResponse = await fetch(`/Materias/EditarAviso`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    AvisoId: avisoId,
                    Titulo: formValues.titulo,
                    Descripcion: formValues.descripcion,
                    DocenteId: this.docenteId
                })
            });

            if (!updateResponse.ok) throw new Error("No se pudo actualizar el aviso.");
            Swal.fire("Actualizado", "El aviso ha sido editado correctamente.", "success");
            this.cargarAvisos();
        } catch (error) {
            Swal.fire("Error", error.message, "error");
        }
    }
}

window.AvisosComponent = AvisosComponent;