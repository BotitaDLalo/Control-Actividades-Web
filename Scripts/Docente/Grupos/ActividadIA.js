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
        // Try to handle several possible shapes of 'texto' returned by the IA
        let opciones = [];

        // If `texto` looks like JSON, try to parse and extract textual parts
        const trimmed = texto.trim();
        if (trimmed.startsWith('{') || trimmed.startsWith('[')) {
            try {
                const parsed = JSON.parse(trimmed);
                // If it's a response with candidates -> extract each candidate text
                if (Array.isArray(parsed.candidates) && parsed.candidates.length) {
                    parsed.candidates.forEach(c => {
                        try {
                            if (c && c.content && Array.isArray(c.content.parts)) {
                                const t = c.content.parts.map(p => p.text || p).join(' ');
                                if (t && typeof t === 'string') opciones.push(t.trim());
                            } else if (typeof c === 'string') {
                                opciones.push(c.trim());
                            }
                        } catch (e) { /* ignore per candidate */ }
                    });
                } else if (parsed.content && Array.isArray(parsed.content.parts)) {
                    opciones.push(parsed.content.parts.map(p => p.text || p).join(' '));
                } else if (parsed.parts && Array.isArray(parsed.parts)) {
                    opciones.push(parsed.parts.map(p => p.text || p).join(' '));
                } else {
                    // as a last resort, stringify
                    opciones.push(String(parsed));
                }
            } catch (e) {
                // invalid JSON - fallback to raw text
            }
        }

        // If we didn't get options from JSON parse, try to split by numbered list or blank lines
        if (opciones.length === 0) {
            // split by patterns like "1.", "2.", etc
            if (/\d+\./.test(texto)) {
                opciones = texto.split(/\d+\.[\s\t]*/).map(s => s.trim()).filter(Boolean);
            }
            // if still not, split by double newlines
            if (opciones.length === 0) {
                opciones = texto.split(/\n\s*\n/).map(s => s.trim()).filter(Boolean);
            }
        }

        // If parsing produced a single chunk that itself contains a numbered list or JSON-wrapped text,
        // try to extract inner content and split it by numbered list.
        if (opciones.length === 1) {
            const single = opciones[0];
            // if looks like JSON inside, try parse
            if (/\{\s*"parts"|"content"|"candidates"/.test(single)) {
                try {
                    const inner = JSON.parse(single);
                    if (inner && inner.candidates && Array.isArray(inner.candidates)) {
                        inner.candidates.forEach(c => {
                            try {
                                if (c && c.content && Array.isArray(c.content.parts)) {
                                    const t = c.content.parts.map(p => p.text || p).join(' ');
                                    if (t) opciones.push(t.trim());
                                }
                            } catch (e) { }
                        });
                    } else if (inner && inner.parts && Array.isArray(inner.parts)) {
                        opciones = inner.parts.map(p => p.text || p).filter(Boolean);
                    } else if (typeof inner === 'string') {
                        opciones = [inner.trim()];
                    }
                } catch (e) { /* ignore */ }
            }

            // if still a single string but contains numbered items, split it
            if (opciones.length === 1 && /\d+\./.test(opciones[0])) {
                opciones = opciones[0].split(/\d+\.[\s\t]*/).map(s => s.trim()).filter(Boolean);
            }
        }

        // If still only one large chunk, try to split into sentences to create up to 3 suggestions
        if (opciones.length <= 1) {
            const sentenceParts = texto.replace(/\n+/g, ' ').split(/(?<=[\.\?\!])\s+/).map(s => s.trim()).filter(Boolean);
            if (sentenceParts.length >= 3) {
                opciones = [sentenceParts.slice(0, Math.ceil(sentenceParts.length/3)).join(' '), sentenceParts.slice(Math.ceil(sentenceParts.length/3), Math.ceil(2*sentenceParts.length/3)).join(' '), sentenceParts.slice(Math.ceil(2*sentenceParts.length/3)).join(' ')].map(s=>s.trim()).filter(Boolean);
            }
        }

        // Normalize each option: try to parse JSON, extract "text" fields, unescape sequences
        function normalizeOption(raw) {
            if (!raw || typeof raw !== 'string') return '';
            let s = raw.trim();

            // try parse as JSON directly
            if (s.startsWith('{') || s.startsWith('[')) {
                try {
                    const p = JSON.parse(s);
                    // candidates -> parts
                    if (Array.isArray(p.candidates) && p.candidates.length) {
                        return p.candidates.map(c => {
                            if (c && c.content && Array.isArray(c.content.parts)) return c.content.parts.map(pp => pp.text || pp).join(' ');
                            return typeof c === 'string' ? c : '';
                        }).filter(Boolean).join('\n\n').trim();
                    }
                    if (p.content && Array.isArray(p.content.parts)) return p.content.parts.map(pp => pp.text || pp).join(' ').trim();
                    if (p.parts && Array.isArray(p.parts)) return p.parts.map(pp => pp.text || pp).join(' ').trim();
                    // fallback stringify
                    return String(p);
                } catch (e) {
                    // continue to other strategies
                }
            }

            // try to extract all "text": "..." occurrences (handles escaped quotes)
            try {
                const re = /"text"\s*:\s*"((?:[^"\\]|\\.)*)"/g;
                let m; const parts = [];
                while ((m = re.exec(s)) !== null) {
                    try {
                        // unescape via JSON parsing of quoted string
                        const un = JSON.parse('"' + m[1].replace(/"/g, '\\"') + '"');
                        parts.push(un);
                    } catch (e) {
                        parts.push(m[1].replace(/\\n/g, '\n').replace(/\\"/g, '"'));
                    }
                }
                if (parts.length) return parts.join(' ').replace(/\\n/g, '\n').trim();
            } catch (e) { }

            // if contains sequences like \"parts\": ... possibly double-escaped JSON, try to find inner JSON and parse
            try {
                const innerMatch = s.match(/\{[^\{]*\"parts\"[\s\S]*\}/);
                if (innerMatch) {
                    try {
                        const p2 = JSON.parse(innerMatch[0]);
                        if (p2 && p2.parts && Array.isArray(p2.parts)) return p2.parts.map(pp => pp.text || pp).join(' ').trim();
                    } catch (e) { }
                }
            } catch (e) { }

            // remove leading numeric bullets and normalize escaped newlines
            s = s.replace(/^\d+\.?\s*/, '').replace(/\\n/g, '\n').trim();
            return s;
        }

        opciones = opciones.map(normalizeOption).filter(Boolean).slice(0, 3);

        if (!opciones.length) {
            cont.innerHTML = '<p class="text-muted">No se recibieron sugerencias válidas</p>';
            return;
        }

        // Build html: place input inside label to make whole card clickable
        let html = '<div class="row g-2">';
        opciones.forEach((op, index) => {
            const safeText = op.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
            const title = `Sugerencia ${index+1}`;
            html += `
            <div class="col-12">
              <label class="card suggestion-card p-2 d-block" style="cursor:pointer;">
                <div class="card-body d-flex gap-3 align-items-start">
                  <div class="form-check mt-1">
                    <input class="form-check-input" type="radio" name="opcionDescripcion" id="opcion${index}" value="${safeText}">
                  </div>
                  <div class="flex-grow-1">
                    <div class="fw-semibold suggestion-title">${title}</div>
                    <div class="text-muted suggestion-text mt-1">${safeText.replace(/\n/g, '<br/>')}</div>
                  </div>
                </div>
              </label>
            </div>`;
        });
        html += '</div>';

        cont.innerHTML = html;
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

                // Open suggestions modal programmatically while keeping crearActividadModal open in background
                try {
                    const sugerenciasEl = document.getElementById('sugerenciasModal');
                    if (sugerenciasEl && window.bootstrap) {
                        const sModal = bootstrap.Modal.getInstance(sugerenciasEl) || new bootstrap.Modal(sugerenciasEl, { backdrop: 'static' });
                        sModal.show();
                    }
                } catch (e) { console.warn(e); }
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
                // fill description
                descripcionTextarea.value = seleccionado.value;
                // fill nombre if empty
                try {
                    const nombreEl = document.getElementById('nombre');
                    if (nombreEl && (!nombreEl.value || nombreEl.value.trim().length === 0)) {
                        let txt = seleccionado.value.replace(/\n+/g, ' ').trim();
                        let first = txt.split(/[\.\!\?]\s/)[0] || txt;
                        first = first.length > 120 ? first.slice(0, 117) + '...' : first;
                        nombreEl.value = first;
                    }
                } catch (e) { console.warn(e); }

                // Close only suggestions modal; keep crearActividadModal open in background
                try {
                    const mEl = document.getElementById('sugerenciasModal');
                    if (mEl && window.bootstrap) {
                        const modal = bootstrap.Modal.getInstance(mEl) || new bootstrap.Modal(mEl);
                        if (modal && typeof modal.hide === 'function') modal.hide();
                    }
                    // return focus to description field
                    setTimeout(() => {
                        try {
                            const descripcionEl = document.getElementById('descripcion');
                            if (descripcionEl) descripcionEl.focus();
                        } catch (e) { }
                    }, 120);
                } catch (e) { /* ignore */ }
            } else {
                alert('¡Por favor selecciona una opción antes de continuar!');
            }
        });
    }
});
