// Cargar materias que fueron creadas sin un grupo a la vista principal.
async function cargarMateriasSinGrupo() {
    try {
        //const response = await fetch(`/Materias/ObtenerMateriasSinGrupo?docenteId=${docenteIdGlobal}`);

        const response = await fetch(`/Materias/ObtenerMateriasSinGrupoPorUsuario`);
        if (!response.ok) throw new Error('Error en la respuesta');
        const materiasSinGrupo = await response.json();
        const listaMateriasSinGrupo = document.getElementById("listaMateriasSinGrupo");
        if (!listaMateriasSinGrupo) return;

        // Limpiar contenido anterior
        listaMateriasSinGrupo.innerHTML = "";

        if (!materiasSinGrupo || materiasSinGrupo.length === 0) {
            const mensaje = document.createElement("p");
            mensaje.classList.add("text-center", "text-muted");
            mensaje.textContent = "No hay materias registradas.";
            listaMateriasSinGrupo.appendChild(mensaje);
            return;
        }

        materiasSinGrupo.forEach((materia, index) => {
            const card = document.createElement('div');
            card.className = 'rounded card-layout';

            const title = document.createElement('div');
            title.className = 'card-title';
            title.textContent = materia.NombreMateria;

            const subtitle = document.createElement('div');
            subtitle.className = 'card-subtitle';
            subtitle.textContent = materia.Descripcion || '';

            card.appendChild(title);
            if (materia.Descripcion) card.appendChild(subtitle);

            // agregar nombre del docente si viene
            if (materia.DocenteId || materia.DocenteId === 0) {
                const nombreDocente = materia.DocenteNombre || materia.DocenteNombre || '';
                if (nombreDocente) {
                    const d = document.createElement('div');
                    d.className = 'card-docente';
                    d.textContent = 'Docente: ' + nombreDocente;
                    card.appendChild(d);
                }
            }


            // Redirect to materia details when clicking the card
            card.addEventListener('click', function (e) {
                // avoid redirect if user clicked an inner actionable element in future
                if (e.target.closest('a') || e.target.closest('button')) return;
                try {
                    // preserve group context
                    window.location.href = `/Materias/MateriaDetalles?materiaId=${materia.MateriaId}`;
                } catch (err) {
                    console.warn('No se pudo redirigir a materia:', err);
                }
            });

            listaMateriasSinGrupo.appendChild(card);
        });

    } catch (error) {
        console.error('Error al cargar materias sin grupo:', error);
        //Swal.fire({
        //    title: "Error al cargar materias",
        //    html: "Reintentando en <b></b> segundos...",
        //    timer: 4000,
        //    timerProgressBar: true,
        //    allowOutsideClick: false,
        //    didOpen: () => {
        //        Swal.showLoading();
        //        const timer = Swal.getPopup().querySelector("b");
        //        let timerInterval = setInterval(() => {
        //            timer.textContent = `${Math.floor(Swal.getTimerLeft() / 1000)}`;
        //        }, 100);
        //    },
        //    willClose: () => clearInterval(timerInterval)
        //}).then((result) => {
        //    if (result.dismiss === Swal.DismissReason.timer) {
        //        cargarMateriasSinGrupo();
        //    }
        //});
    }
}
