document.addEventListener('DOMContentLoaded', function () {

    async function obtenerRecomendaciones(nombre, descripcion) {
        const resp = await fetch('/api/IA/MejorarDescripcion', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Nombre: nombre,
                Descripcion: descripcion
            })
        });

        if (!resp.ok) {
            throw new Error("Error al generar sugerencias");
        }

        const data = await resp.json();

        return data.candidates?.[0]?.content?.parts?.[0]?.text || '';
    }

    function mostrarOpcionesSugerencias(texto) {
        const cont = document.getElementById('sugerenciasLista');
        if (!cont) return;

        if (!texto) {
            cont.innerHTML = '<p class="text-muted">No se recibieron sugerencias</p>';
            return;
        }

        const opciones = texto
            .split(/\n\s*\n/) // ← tu bloque 7or69g
            .map(t => t.trim())
            .filter(Boolean)
            .slice(0, 3);

        if (!opciones.length) {
            cont.innerHTML = '<p class="text-muted">No se recibieron sugerencias</p>';
            return;
        }

        let html = '<div class="row g-2">';
        opciones.forEach((op, index) => {
            const safe = op
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;');

            html += `
            <div class="col-12">
              <label class="card suggestion-card p-2 d-block" style="cursor:pointer;">
                <div class="card-body d-flex gap-3 align-items-start">
                  <div class="form-check mt-1">
                    <input class="form-check-input" type="radio"
                           name="opcionDescripcion"
                           value="${safe}">
                  </div>
                  <div class="flex-grow-1">
                    <div class="fw-semibold">Sugerencia ${index + 1}</div>
                    <div class="text-muted mt-1">${safe.replace(/\n/g, '<br>')}</div>
                  </div>
                </div>
              </label>
            </div>`;
        });
        html += '</div>';

        cont.innerHTML = html;
    }

    const btn = document.getElementById('btnSugerencias');
    const lista = document.getElementById('sugerenciasLista');

    if (btn && lista) {
        btn.addEventListener('click', async () => {

            const nombre = document.getElementById('nombre')?.value || '';
            const descripcion = document.getElementById('descripcion')?.value || '';

            lista.innerHTML = `
                <div class="text-center py-4">
                    <div class="spinner-border text-primary"></div>
                    <p class="mt-2">Generando sugerencias...</p>
                </div>`;

            try {
                const texto = await obtenerRecomendaciones(nombre.trim(), descripcion.trim());
                mostrarOpcionesSugerencias(texto);
                const sugerenciasEl = document.getElementById('sugerenciasModal');
                if (sugerenciasEl && window.bootstrap) {
                    const modal = bootstrap.Modal.getInstance(sugerenciasEl)
                        || new bootstrap.Modal(sugerenciasEl);
                    modal.show();
                }
            }
            catch (err) {
                lista.innerHTML =
                    `<div class="alert alert-danger">${err.message}</div>`;
            }
        });
    }

    const btnAplicar = document.getElementById('btnAplicarSugerencia');
    if (btnAplicar) {
        btnAplicar.addEventListener('click', function () {

            const seleccionado = document.querySelector('input[name="opcionDescripcion"]:checked');
            const descripcionTextarea = document.getElementById('descripcion');

            if (!seleccionado || !descripcionTextarea) {
                alert('Selecciona una opción');
                return;
            }

            descripcionTextarea.value = seleccionado.value;

            const modalEl = document.getElementById('sugerenciasModal');
            if (modalEl && window.bootstrap) {
                const modal = bootstrap.Modal.getInstance(modalEl)
                    || new bootstrap.Modal(modalEl);
                modal.hide();
            }
        });
    }

});