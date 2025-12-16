// Activación segura al cargar el DOM
document.addEventListener('DOMContentLoaded', function () {
    // Use server-side proxy endpoint to avoid exposing API key in client
    const apiUrl = '/api/IA/MejorarDescripcion';

    function mostrarOpcionesSugerencias(texto) {
        const cont = document.getElementById('sugerenciasLista');
        if (!cont) return;
        if (!texto || typeof texto !== 'string') {
            cont.innerHTML = '<p class="text-muted">No se recibieron sugerencias válidas</p>';
            return;
        }
        const opciones = texto.split(/\d\./).filter(op => op.trim());
        let html = '';

        opciones.forEach((op, index) => {
            html += `
            <div class="list-group-item d-flex align-items-start">
                <div class="me-3 mt-1"> <!-- Contenedor del radio -->
                    <input class="form-check-input" type="radio" 
                           name="opcionDescripcion" id="opcion${index}" 
                           value="${op.trim().replace(/\"/g, '&quot;')}">
                </div>
                <div class="w-100"> <!-- Contenedor del texto -->
                    <label class="form-check-label fw-normal" for="opcion${index}">
                        ${op.trim()}
                    </label>
                </div>
            </div>`;
        });

        cont.innerHTML = html || '<p class="text-muted">No se recibieron sugerencias válidas</p>';
    }

    async function obtenerRecomendaciones(nombre, descripcion) {
        if (!nombre || !descripcion) {
            throw new Error('Se necesita un título y descripción para mostrar sugerencias');
        }

        const prompt = `resuelve "${nombre}", Descripción: "${descripcion}",\n ` ;

        // Call server endpoint which proxies the request to Google
        const resp = await fetch(apiUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Nombre: nombre, Descripcion: descripcion, model: 'gemini-2.5-flash' })
        });

        const textResp = await resp.text();
        let data = null;
        try { data = textResp ? JSON.parse(textResp) : null; } catch (e) { data = textResp; }

        if (!resp.ok) {
            // server may return structured error
            const msg = data && data.mensaje ? data.mensaje : (typeof data === 'string' ? data : `HTTP ${resp.status}`);
            throw new Error(msg || 'Error al consultar la API');
        }

        // data should be the JSON returned by Google (proxied)
        if (!data || !data.candidates || !data.candidates[0] || !data.candidates[0].content) {
            throw new Error('Respuesta inesperada de la API');
        }

        const text = data.candidates[0].content.parts[0].text || '';
        return limpiarTexto(text);
    }

    function limpiarTexto(texto) {
        if (!texto) return '';
        return texto
            .replace(/\*\*/g, '')
            .replace(/^\"|\"$/g, '')
            .replace(/<br>/g, '\n')
            .replace(/\n+/g, '\n')
            .trim();
    }

    const btnSugerenciasEl = document.getElementById('btnSugerencias');
    const listaEl = document.getElementById('sugerenciasLista');
    if (btnSugerenciasEl && listaEl) {
        btnSugerenciasEl.addEventListener('click', async () => {
            const nombre = (document.getElementById('nombre') || {}).value || '';
            const descripcion = (document.getElementById('descripcion') || {}).value || '';

            listaEl.innerHTML = `
                <div class="text-center py-4">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <p class="mt-2">Generando sugerencias...</p>
                </div>`;

            try {
                // quick ping to server endpoint to provide clearer error if route missing
                try {
                    const pingResp = await fetch('/api/IA/ping');
                    if (!pingResp.ok) {
                        const pText = await pingResp.text().catch(() => '');
                        console.warn('Ping failed:', pingResp.status, pText);
                        // proceed: the server may still respond to POST even if ping returns non-OK
                    }
                } catch (pingErr) {
                    console.error('Ping error:', pingErr);
                    listaEl.innerHTML = `<div class="alert alert-danger">No se pudo contactar el endpoint del servidor: ${pingErr.message}</div>`;
                    return;
                }

                const sugerencias = await obtenerRecomendaciones(nombre.trim(), descripcion.trim());
                mostrarOpcionesSugerencias(sugerencias);
            } catch (error) {
                console.error('Error al obtener sugerencias:', error);
                listaEl.innerHTML = `<div class="alert alert-danger">${error.message || 'Error al generar sugerencias'}</div>`;
            }
        });
    }

    const btnAplicarEl = document.getElementById('btnAplicarSugerencia');
    if (btnAplicarEl) {
        btnAplicarEl.addEventListener('click', function () {
            const seleccionado = document.querySelector('input[name="opcionDescripcion"]:checked');
            const descripcionTextarea = document.getElementById('descripcion');

            if (seleccionado && descripcionTextarea) {
                descripcionTextarea.value = seleccionado.value;
                try {
                    const m = document.getElementById('sugerenciasModal');
                    if (m && window.bootstrap && typeof bootstrap.Modal.getInstance === 'function') {
                        const modal = bootstrap.Modal.getInstance(m);
                        if (modal && typeof modal.hide === 'function') modal.hide();
                    }
                } catch (e) { /* ignore */ }
            } else {
                alert('¡Por favor selecciona una opción antes de continuar!');
            }
        });
    }
});
