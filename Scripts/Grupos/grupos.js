async function cargarGrupos() {
    try {
        //const response = await fetch(`/Grupos/ObtenerGrupos?docenteId=${docenteIdGlobal}`);
        const response = await fetch(`/Grupos/ObtenerGruposPorUsuario`);
        if (!response.ok) throw new Error('Error al obtener grupos');
        const grupos = await response.json();
        const listaGrupos = document.getElementById("listaGrupos");
        if (!listaGrupos) return;
        listaGrupos.innerHTML = "";

        if (!grupos || grupos.length === 0) {
            const mensaje = document.createElement("p");
            mensaje.classList.add("text-center", "text-muted");
            mensaje.textContent = "No hay grupos registrados.";
            listaGrupos.appendChild(mensaje);
            return;
        }

        grupos.forEach((grupo, index) => {
            const card = document.createElement('div');
            card.className = 'rounded card-layout';
            card.style.position = 'relative';

            // left icon
            const ico = document.createElement('div');
            ico.className = 'me-3';
            ico.innerHTML = '<i class="fas fa-graduation-cap fa-2x" style="color:#0d6efd"></i>';

            // text
            const text = document.createElement('div');
            text.style.flex = '1';
            const title = document.createElement('div');
            title.className = 'card-title';
            title.textContent = grupo.NombreGrupo;
            const subtitle = document.createElement('div');
            subtitle.className = 'card-subtitle';
            subtitle.textContent = grupo.Descripcion || '';
            text.appendChild(title);
            if (grupo.Descripcion) text.appendChild(subtitle);

            // create a row for icon + text
            const row = document.createElement('div');
            row.style.display = 'flex';
            row.style.width = '100%';
            row.appendChild(ico);
            row.appendChild(text);

            // removed settings dropdown (options moved inside Materias view)

            // assemble card content
            card.appendChild(row);

            // When clicking the card (except on any interactive button/anchor), redirect to group materias
            card.addEventListener('click', function (e) {
                if (e.target.closest('button') || e.target.closest('a')) return;
                try {
                    //window.location.href = `/Docente/GrupoMaterias?grupoId=${grupo.GrupoId}`;
                    window.location.href = `/Grupos/GrupoMaterias?grupoId=${grupo.GrupoId}`;
                } catch (err) {
                    console.warn('No se pudo redirigir:', err);
                }
            });

            listaGrupos.appendChild(card);
        });
    } catch (error) {
        console.error(error);
        Swal.fire({
            position: "top-end",
            icon: "error",
            title: "Error al cargar los grupos.",
            showConfirmButton: false,
            timer: 2000
        });
    }
}
