(function(){
    async function cargar() {
        try {
            const params = new URLSearchParams(window.location.search);
            const grupoId = params.get('grupoId');
            if (!grupoId) return;

            // use the Docente controller endpoint which includes DocenteNombre in the response
            const resp = await fetch(`/Docente/ObtenerMateriasPorGrupo?grupoId=${grupoId}`);
            if (!resp.ok) {
                document.getElementById('listaMateriasGrupo').innerText = 'Error al cargar materias';
                return;
            }

            const materias = await resp.json();
            console.debug('materias por grupo response', materias);
            const cont = document.getElementById('listaMateriasGrupo');
            cont.innerHTML = '';

            if (!materias || materias.length === 0) {
                cont.innerHTML = '<p>No hay materias en este grupo.</p>';
                return;
            }

            materias.forEach(m => {
                const card = document.createElement('div');
                card.className = 'rounded card-layout';
                // ensure absolute-positioned children (docente name) are placed relative to this card
                card.style.position = 'relative';

                const title = document.createElement('div');
                title.className = 'card-title';
                title.textContent = m.NombreMateria || m.nombreMateria || '';

                const subtitle = document.createElement('div');
                subtitle.className = 'card-subtitle';
                subtitle.textContent = m.Descripcion || m.descripcion || '';

                card.appendChild(title);
                if (subtitle.textContent) card.appendChild(subtitle);

                // show docente name if provided
                const docenteName = m.DocenteNombre || m.docenteNombre || m.Docente || '';
                if (docenteName) {
                    const docenteDiv = document.createElement('div');
                    docenteDiv.className = 'card-docente';
                    docenteDiv.textContent = 'Docente: ' + docenteName;
                    card.appendChild(docenteDiv);
                }

                cont.appendChild(card);
            });

        } catch (err) {
            console.error(err);
            document.getElementById('listaMateriasGrupo').innerText = 'Error';
        }
    }

    document.addEventListener('DOMContentLoaded', cargar);
})();
