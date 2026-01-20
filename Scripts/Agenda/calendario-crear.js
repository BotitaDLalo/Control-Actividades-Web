(function () {

    let modalCrear, formEvento;

    document.addEventListener("DOMContentLoaded", function () {
        console.log("Inicializando modal de crear evento");

        // Modal bootstrap
        const modalEl = document.getElementById("modalCrearEvento");
        modalCrear = bootstrap.Modal.getOrCreateInstance(modalEl);

        formEvento = document.getElementById("formEvento");

        // Cargar grupos y materias
        cargarGruposMaterias();

        // Activar tabs
        activarTabs();

        // Submit del formulario
        formEvento.addEventListener("submit", handleSubmit);

        // Limpiar formulario al cerrar
        modalEl.addEventListener("hidden.bs.modal", limpiarFormularioEvento);
    });

    // --- FUNCIONES PRINCIPALES ---

    async function cargarGruposMaterias() {
        try {
            const resp = await fetch("/EventosAgenda/ObtenerGruposYMaterias");
            const data = await resp.json();

            const contGrupos = document.getElementById("contenedorGrupos");
            const contSueltas = document.getElementById("contenedorMateriasSueltas");

            contGrupos.innerHTML = "";
            contSueltas.innerHTML = "";

            // Grupos
            data.grupos.forEach(grupo => {
                const divGrupo = document.createElement("div");
                divGrupo.classList.add("grupo-item");

                divGrupo.innerHTML = `
                <div class="grupo-header">
                    <label>
                        <input type="checkbox" class="chk-grupo" data-grupo="${grupo.GrupoId}">
                        <strong>${grupo.NombreGrupo}</strong>
                    </label>
                    <button type="button" class="btn-expandir" data-grupo="${grupo.GrupoId}">▶</button>
                </div>
                <div class="materias-del-grupo hidden"></div>
                `;

                const contMaterias = divGrupo.querySelector(".materias-del-grupo");
                grupo.Materias.forEach(mat => {
                    const divMat = document.createElement("div");
                    divMat.innerHTML = `
                        <label>
                            <input type="checkbox" class="chk-materia" data-grupo="${grupo.GrupoId}" data-materia="${mat.MateriaId}">
                            ${mat.NombreMateria}
                        </label>
                    `;
                    contMaterias.appendChild(divMat);
                });

                contGrupos.appendChild(divGrupo);
            });

            // Materias sueltas
            data.materiasSueltas.forEach(mat => {
                const divMat = document.createElement("div");
                divMat.classList.add("materia-suelta-item");
                divMat.innerHTML = `
                    <label>
                        <input type="checkbox" class="chk-materia-suelta" data-materia="${mat.MateriaId}">
                        ${mat.NombreMateria}
                    </label>
                `;
                contSueltas.appendChild(divMat);
            });

            activarExpandibles();
            activarLogicaCheckBoxes();

        } catch (err) {
            console.error("Error cargando grupos y materias:", err);
        }
    }

    function activarTabs() {
        document.querySelectorAll("#modalCrearEvento .nav-link-custom").forEach(btn => {
            btn.addEventListener("click", function () {
                const tabId = this.dataset.tab;

                document.querySelectorAll("#modalCrearEvento .nav-link-custom").forEach(b => b.classList.remove("active"));
                this.classList.add("active");

                document.querySelectorAll("#modalCrearEvento .tab-pane").forEach(tab => tab.classList.remove("active", "show"));
                document.getElementById(tabId).classList.add("active", "show");
            });
        });
    }

    function activarExpandibles() {
        document.querySelectorAll(".btn-expandir").forEach(boton => {
            boton.addEventListener("click", function () {
                const contenedorMaterias = this.closest(".grupo-item").querySelector(".materias-del-grupo");
                const estaOculto = contenedorMaterias.classList.contains("hidden");
                contenedorMaterias.classList.toggle("hidden", !estaOculto);
                this.textContent = estaOculto ? "▼" : "▶";
            });
        });
    }

    function activarLogicaCheckBoxes() {
        // Grupo -> materias
        document.querySelectorAll(".chk-grupo").forEach(chkGrupo => {
            chkGrupo.addEventListener("change", function () {
                const grupoId = this.dataset.grupo;
                document.querySelectorAll(`.chk-materia[data-grupo="${grupoId}"]`)
                    .forEach(chk => chk.checked = this.checked);
            });
        });

        // Materias -> grupo
        document.querySelectorAll(".chk-materia").forEach(chk => {
            chk.addEventListener("change", function () {
                const grupoId = this.dataset.grupo;
                const todas = document.querySelectorAll(`.chk-materia[data-grupo="${grupoId}"]`);
                const marcadas = document.querySelectorAll(`.chk-materia[data-grupo="${grupoId}"]:checked`);
                const chkGrupo = document.querySelector(`.chk-grupo[data-grupo="${grupoId}"]`);
                chkGrupo.checked = (marcadas.length === todas.length);
            });
        });
    }

    async function handleSubmit(e) {
        e.preventDefault();

        const formData = new FormData(formEvento);

        // Grupos seleccionados
        const grupos = [...document.querySelectorAll('.chk-grupo:checked')].map(x => x.dataset.grupo).join(',');

        // Materias seleccionadas dentro de grupos
        const materias = [...document.querySelectorAll('.chk-materia:checked')].map(x => x.dataset.materia);

        // Materias sueltas
        const materiasSueltas = [...document.querySelectorAll('.chk-materia-suelta:checked')].map(x => x.dataset.materia);

        const todasLasMaterias = [...materias, ...materiasSueltas].join(',');

        formData.append("GruposSeleccionados", grupos);
        formData.append("MateriasSeleccionadas", todasLasMaterias);

        try {
            const response = await fetch("/EventosAgenda/CrearEvento", {
                method: "POST",
                body: formData
            });

            if (response.ok) {
                Swal.fire({ title: "Evento creado correctamente", icon: "success" });
                limpiarFormularioEvento();
                modalCrear.hide();

                // Disparar evento global para que el calendario refresque
                document.dispatchEvent(new CustomEvent('eventoCreado'));

            } else {
                Swal.fire({ icon: "error", title: "Error", text: "Ocurrió un error al crear el evento" });
            }

        } catch (err) {
            console.error("Error creando evento:", err);
            Swal.fire({ icon: "error", title: "Error", text: "Ocurrió un error al crear el evento" });
        }
    }

    function limpiarFormularioEvento() {
        formEvento.reset();
        document.querySelectorAll("#contenedorGrupos input, #contenedorMateriasSueltas input").forEach(chk => chk.checked = false);
    }

})();
