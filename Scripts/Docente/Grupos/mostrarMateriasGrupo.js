(function(){
    async function cargar() {
        try {
            const params = new URLSearchParams(window.location.search);
            const grupoId = params.get('grupoId');
            if (!grupoId) return;

            const resp = await fetch(`/Grupos/ObtenerMateriasPorGrupo?grupoId=${grupoId}`);
            if (!resp.ok) {
                document.getElementById('listaMateriasGrupo').innerText = 'Error al cargar materias';
                return;
            }

            const materias = await resp.json();
            const cont = document.getElementById('listaMateriasGrupo');
            cont.innerHTML = '';

            if (!materias || materias.length === 0) {
                cont.innerHTML = '<p>No hay materias en este grupo.</p>';
                return;
            }

            materias.forEach(m => {
                const card = document.createElement('div');
                card.className = 'rounded card-layout';

                const title = document.createElement('div');
                title.className = 'card-title';
                title.textContent = m.NombreMateria || m.nombreMateria || '';

                const subtitle = document.createElement('div');
                subtitle.className = 'card-subtitle';
                subtitle.textContent = m.Descripcion || m.descripcion || '';

                card.appendChild(title);
                if (subtitle.textContent) card.appendChild(subtitle);

                cont.appendChild(card);
            });

        } catch (err) {
            console.error(err);
            document.getElementById('listaMateriasGrupo').innerText = 'Error';
        }
    }

    document.addEventListener('DOMContentLoaded', cargar);
})();
