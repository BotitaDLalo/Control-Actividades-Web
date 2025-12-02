//const apiKey = "AIzaSyAkcRmqwYgV1M4cr5bOHh0symB38KP8yMY";
const apiKey = "AIzaSyABBr-kmTcgPrduKMmLhRLLSnlipQRoMqk";



const apiUrl = `https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=${apiKey}`;

function mostrarOpcionesSugerencias(texto) {
    const opciones = texto.split(/\d\./).filter(op => op.trim());
    let html = '';

    opciones.forEach((op, index) => {
        html += `
        <div class="list-group-item d-flex align-items-start">
            <div class="me-3 mt-1"> <!-- Contenedor del radio -->
                <input class="form-check-input" type="radio" 
                       name="opcionDescripcion" id="opcion${index}" 
                       value="${op.trim().replace(/"/g, '&quot;')}">
            </div>
            <div class="w-100"> <!-- Contenedor del texto -->
                <label class="form-check-label fw-normal" for="opcion${index}">
                    ${op.trim()}
                </label>
            </div>
        </div>`;
    });

    document.getElementById('sugerenciasLista').innerHTML = html ||
        '<p class="text-muted">No se recibieron sugerencias válidas</p>';
}

// Guardar referencias y añadir listeners con comprobación de existencia
const btnSugerenciasEl = document.getElementById('btnSugerencias');
if (btnSugerenciasEl) {
    btnSugerenciasEl.addEventListener('click', async () => {
    const nombre = document.getElementById('nombre').value;
    const descripcion = document.getElementById('descripcion').value;

    // Mostrar spinner de Bootstrap
    document.getElementById('sugerenciasLista').innerHTML = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p class="mt-2">Generando sugerencias...</p>
        </div>`;

    try {
        const sugerencias = await obtenerRecomendaciones(nombre, descripcion);
        mostrarOpcionesSugerencias(sugerencias);
    } catch (error) {
        document.getElementById('sugerenciasLista').innerHTML = `
            <div class="alert alert-danger">
                Error al generar sugerencias: ${error.message}
            </div>`;
    }
    });
}


async function obtenerRecomendaciones(nombre, descripcion) {
    const prompt = `A partir de la siguiente actividad: Título: "${nombre}", Descripción: "${descripcion}", 
    genera tres versiones mejoradas de la descripción, más claras, completas y bien estructuradas. 
    Cada versión debe incluir detalles útiles sin cambiar el significado original. 
    Devuelve SOLO las tres versiones numeradas (1, 2 y 3) sin texto adicional.
    Sino hay nombre y descripción solo mostrar un aviso:  Se necesita un titulo y descripción para mostrar sugerencias"`;

    const requestBody = {
        contents: [{
            parts: [{
                text: prompt
            }]
        }]
    };

    try {
        const response = await fetch(apiUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestBody)
        });

        if (!response.ok) {
            throw new Error(`Error HTTP! estado: ${response.status}`);
        }

        const data = await response.json();
        console.log("Respuesta completa:", data);

        // Extrae el texto de la respuesta
        const text = data.candidates[0].content.parts[0].text;
        return limpiarTexto(text);

    } catch (error) {
        console.error("Error:", error);
        return "⚠️ Error al obtener sugerencias. Inténtalo de nuevo.";
    }
}

// FUNCIÓN PARA LIMPIAR TEXTO
function limpiarTexto(texto) {
    return texto
        .replace(/\*\*/g, '')
        .replace(/^"|"$/g, '')
        .replace(/<br>/g, '\n')
        .replace(/\n+/g, '\n')
        .trim();
}

document.getElementById('btnAplicarSugerencia').addEventListener('click', function () {
    const seleccionado = document.querySelector('input[name="opcionDescripcion"]:checked');
    const descripcionTextarea = document.getElementById('descripcion');

    if (seleccionado && descripcionTextarea) {
        descripcionTextarea.value = seleccionado.value;
        bootstrap.Modal.getInstance('#sugerenciasModal').hide();
    } else {
        alert('¡Por favor selecciona una opción antes de continuar!');
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
            try { const m = document.getElementById('sugerenciasModal'); if (m) bootstrap.Modal.getInstance(m)?.hide(); } catch (e) { /* ignore */ }
        } else {
            alert('¡Por favor selecciona una opción antes de continuar!');
        }
    });
}
