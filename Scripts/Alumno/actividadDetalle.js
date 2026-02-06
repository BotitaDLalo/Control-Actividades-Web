document.addEventListener('DOMContentLoaded', function () {
    cargarDetalleActividad();
    verificarEnvio();

    document.getElementById('btnEnviar').addEventListener('click', enviarEntrega);
});

function parseNetDate(value) {
    if (!value) return null;
    // Handle /Date(1234567890)/ format
    if (typeof value === 'string') {
        var m = value.match(/\/Date\((\d+)\)/);
        if (m) return new Date(parseInt(m[1],10));
        // Try ISO
        var d = new Date(value);
        if (!isNaN(d.getTime())) return d;
        // Try numeric string
        if (!isNaN(Number(value))) return new Date(Number(value));
        return null;
    }
    if (typeof value === 'number') return new Date(value);
    if (value instanceof Date) return value;
    return null;
}

function formatDateNice(d) {
    if (!d) return '';
    try {
        return d.toLocaleString('es-ES', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
    } catch (e) {
        return d.toString();
    }
}

async function cargarDetalleActividad() {
    try {
        // Intentar primero endpoint API (más consistente), si falla, intentar el endpoint MVC
        let resp = await fetch(`/api/Actividades/ObtenerActividad?id=${actividadIdGlobal}`);
        if (!resp.ok) {
            resp = await fetch(`/Actividades/ObtenerActividadPorId?actividadId=${actividadIdGlobal}`);
        }
        if (!resp.ok) throw new Error('No se encontró la actividad');

        // Leer como texto y parsear con tolerancia (algunos endpoints pueden devolver HTML en error)
        const text = await resp.text();
        let data = null;
        try { data = text ? JSON.parse(text) : null; } catch (e) { data = null; }
        if (!data) {
            try { data = resp.ok ? JSON.parse(text) : null; } catch (e) { data = null; }
        }
        if (!data) throw new Error('Respuesta inválida del servidor al obtener actividad');
        document.getElementById('tituloActividad').innerText = data.NombreActividad || 'Sin título';
        document.getElementById('descripcionActividad').innerText = data.Descripcion || '';
        // FechaLimite puede venir como string ISO o como objeto; manejar ambos casos
        let fechaVal = data.FechaLimite || data.fechaLimite || data.FechaLimiteString || null;
        let fechaText = '';
        if (fechaVal) {
            const d = parseNetDate(fechaVal);
            fechaText = d ? formatDateNice(d) : String(fechaVal);
        }
        // Mejor presentación: etiqueta y estilo
        var el = document.getElementById('fechaLimite');
        if (el) {
            if (fechaText) {
                el.innerText = fechaText;
                el.classList.add('text-primary');
            } else {
                el.innerText = 'Sin fecha límite';
                el.classList.add('text-muted');
            }
        }
    } catch (e) {
        console.error(e);
    }
}

// Escuchar evento de calificación para refrescar vista del alumno si corresponde
window.addEventListener('entregableCalificado', function (ev) {
    try {
        var detalle = ev && ev.detail;
        if (!detalle) return;
        // Si el alumno está viendo esta entrega, refrescar la sección de calificación
        // Intentamos identificar si el alumno actual corresponde a la entrega (no siempre disponible en cliente)
        // Forzar recarga parcial: volver a llamar a verificarEnvio
        verificarEnvio();
    } catch (e) { console.error(e); }
});

// También escuchar cambios en localStorage (cuando docente guarda y guarda marca en localStorage)
window.addEventListener('storage', function (e) {
    try {
        if (e.key === 'entregableCalificado') {
            // recargar vista de envío
            verificarEnvio();
        }
    } catch (err) { console.error(err); }
});

async function verificarEnvio() {
    try {
        const resp = await fetch(`/api/Alumnos/ObtenerEnviosActividadesAlumno?ActividadId=${actividadIdGlobal}&AlumnoId=${alumnoIdGlobal}`);
        if (!resp.ok) return;
        const data = await resp.json();
        const envio = Array.isArray(data) && data.length >0 ? data[0] : (data || null);
        if (envio) {
            var fechaEnt = parseNetDate(envio.FechaEntrega) || (envio.FechaEntrega ? new Date(envio.FechaEntrega) : null);
            var estadoHtml = '<p><strong>Ya entregado.</strong> Fecha: ' + (fechaEnt ? formatDateNice(fechaEnt) : String(envio.FechaEntrega || '—')) + '</p>';
            try {
                var parsed = JSON.parse(envio.Contenido || 'null');
                if (parsed && parsed.Archivos && Array.isArray(parsed.Archivos) && parsed.Archivos.length >0) {
                    estadoHtml += '<p><strong>Archivos adjuntos:</strong></p><ul>';
                    parsed.Archivos.forEach(function (a) { estadoHtml += '<li><a href="' + a + '" target="_blank">' + a.split('/').pop() + '</a></li>'; });
                    estadoHtml += '</ul>';
                } else if (parsed && parsed.Respuesta) {
                    estadoHtml += '<div><strong>Respuesta:</strong><pre>' + parsed.Respuesta + '</pre></div>';
                } else {
                    if (envio.Contenido) estadoHtml += '<div><strong>Respuesta:</strong><pre>' + envio.Contenido + '</pre></div>';
                }
            } catch (e) {
                if (envio.Contenido) estadoHtml += '<div><strong>Respuesta:</strong><pre>' + envio.Contenido + '</pre></div>';
            }

            document.getElementById('estadoEntrega').innerHTML = estadoHtml;
            document.getElementById('entregaForm').style.display = 'none';
            // Mostrar calificación o estado pendiente
            var calContainer = document.getElementById('calificacionAlumno');
            if (!calContainer) return;

            // Elements for progress and badge
            var progressEl = document.getElementById('progressCircle');
            var gradeValueEl = document.getElementById('gradeValue');
            var gradeBadgeEl = document.getElementById('gradeBadge');
            var comentarioEl = document.getElementById('comentarioDocente');

            // Considerar que la API puede devolver0 como valor por defecto cuando no existe calificación.
            // Tratamos0 como "pendiente" para mejorar la UX, a menos que quieras que0 sea una nota válida.
            var cal = envio.Calificacion;
            var isPending = (cal === null || typeof cal === 'undefined' || Number(cal) ===0);

            if (isPending) {
                // Reset progress and badge
                if (gradeValueEl) gradeValueEl.innerText = '--%';
                if (progressEl) progressEl.style.background = 'conic-gradient(#e0e0e00deg360deg)';
                if (gradeBadgeEl) {
                    gradeBadgeEl.className = 'grade-badge';
                    gradeBadgeEl.innerHTML = '<div class="badge" style="background:#6c757d">En espera</div>';
                }
                calContainer.innerHTML = '<div class="alert alert-info" role="alert" style="display:inline-block;"><strong>Calificación:</strong> <span class="ms-2">En espera de calificar</span></div>';
                if (comentarioEl) comentarioEl.innerHTML = '<strong>Comentario del docente:</strong><div style="margin-top:4px;color:#666;">Aún no hay comentarios.</div>';
            } else {
                var percent = Math.round(Number(cal));
                if (isNaN(percent)) percent =0;
                if (percent <0) percent =0;
                if (percent >100) percent =100;

                // choose color state
                var color = '#2e7d32'; // green
                var stateClass = 'grade-good';
                if (percent <60) { color = '#d32f2f'; stateClass = 'grade-bad'; }
                else if (percent <81) { color = '#f5a623'; stateClass = 'grade-medium'; }

                // Update progress circle visually using conic-gradient
                if (gradeValueEl) gradeValueEl.innerText = percent + '%';
                if (progressEl) {
                    var angle = Math.round((percent /100) *360);
                    progressEl.style.background = 'conic-gradient(' + color + '0deg ' + angle + 'deg, #e9eef2 ' + angle + 'deg360deg)';
                    progressEl.setAttribute('aria-valuenow', String(percent));
                    progressEl.setAttribute('aria-valuemin', '0');
                    progressEl.setAttribute('aria-valuemax', '100');
                }

                // Badge with color and numeric percent (emojis removed)
                if (gradeBadgeEl) {
                    gradeBadgeEl.className = 'grade-badge ' + stateClass;
                    gradeBadgeEl.innerHTML = '<div class="badge" role="status" aria-label="Calificación ' + percent + '"><span>' + percent + '</span></div>';
                }

                // render main calificacion container with emphasis
                calContainer.innerHTML = '<div style="display:flex;gap:12px;align-items:center;">' +
                '<div style="font-size:1.15rem;font-weight:700;color:' + color + '">Calificación: <span style="margin-left:8px;">' + percent + '</span></div>' +
                '</div>';

                // Comentario del docente (si existe)
                var comentario = (envio.Comentario && String(envio.Comentario).trim().length >0) ? envio.Comentario : null;
                if (comentarioEl) {
                    comentarioEl.innerHTML = '<strong>Comentario del docente:</strong> <div style="margin-top:6px;color:#444;">' + escapeHtml(comentario || 'El docente no ha agregado comentarios.') + '</div>';
                }
            }
        }
    } catch (e) { console.error(e); }
}

async function enviarEntrega() {
    const respuesta = document.getElementById('respuesta').value.trim();
    const respuestaLink = (document.getElementById('respuestaLink') || {}).value || '';
    const fileInput = document.getElementById('fileEntrega');
    if (!respuesta && !respuestaLink && (!fileInput || !fileInput.files || fileInput.files.length ===0)) { alert('Agrega una respuesta, un link o un archivo.'); return; }

    const form = new FormData();
    form.append('ActividadId', actividadIdGlobal);
    form.append('AlumnoId', alumnoIdGlobal);
    // Enviar contenido estructurado: Respuesta (texto) y Link opcional
    const payload = { Respuesta: respuesta };
    if (respuestaLink) payload.Link = respuestaLink;
    form.append('Respuesta', JSON.stringify(payload));
    form.append('FechaEntrega', new Date().toISOString());

    if (fileInput && fileInput.files && fileInput.files.length >0) {
        for (let i =0; i < fileInput.files.length; i++) {
            form.append('files', fileInput.files[i]);
        }
    }

    try {
        const resp = await fetch('/api/Alumnos/SubirEntrega', { method: 'POST', body: form });
        if (!resp.ok) {
            const txt = await resp.text().catch(() => '');
            throw new Error(txt || 'Error al subir entrega');
        }
        const data = await resp.json().catch(() => null);
        Swal.fire('Enviado', (data && data.mensaje) ? data.mensaje : 'Entrega registrada', 'success');
        verificarEnvio();
    } catch (e) { Swal.fire('Error', e.message || 'No se pudo enviar', 'error'); }
}

function escapeHtml(unsafe) {
    if (!unsafe) return '';
    return String(unsafe)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/\"/g, "&quot;")
        .replace(/'/g, "&#039;");
}