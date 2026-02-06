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

            // Considerar que la API puede devolver0 como valor por defecto cuando no existe calificación.
            // Tratamos0 como "pendiente" para mejorar la UX, a menos que quieras que0 sea una nota válida.
            var cal = envio.Calificacion;
            var isPending = (cal === null || typeof cal === 'undefined' || Number(cal) ===0);

            if (isPending) {
                calContainer.innerHTML = '<div class="alert alert-info" role="alert" style="display:inline-block;"><strong>Calificación:</strong> <span class="ms-2">En espera de calificar</span></div>';
            } else {
                calContainer.innerHTML = '<div class="badge bg-success" style="font-size:1rem;padding:0.6rem0.9rem;"><strong>Calificación: </strong> <span class="ms-2">' + String(cal) + '</span></div>';
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