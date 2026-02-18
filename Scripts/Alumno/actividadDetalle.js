document.addEventListener('DOMContentLoaded', function () {
    cargarDetalleActividad();
    verificarEnvio();

    var btn = document.getElementById('btnEnviar');
    if (btn) btn.addEventListener('click', enviarEntrega);
});

function parseServerDateFlexible(val) {
    if (!val) return null;
    if (val instanceof Date) return val;
    // Handle /Date(123456789)/
    try {
        var s = String(val).trim();
        var msMatch = s.match(/\/(?:Date)?\((-?\d+)(?:[-+]\d+)?\)\//);
        if (msMatch) return new Date(parseInt(msMatch[1],10));
        // plain number
        if (/^-?\d+$/.test(s)) return new Date(parseInt(s,10));
        var d = new Date(s);
        if (!isNaN(d.getTime())) return d;
    } catch (e) { }
    return null;
}

function formatDateForUser(d) {
    if (!d) return '—';
    try {
        // show date + short time
        return d.toLocaleString('es-ES', { year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' });
    } catch (e) {
        return d.toString();
    }
}

async function cargarDetalleActividad() {
    try {
        let resp = await fetch(`/api/Actividades/ObtenerActividad?id=${actividadIdGlobal}`);
        if (!resp.ok) resp = await fetch(`/Actividades/ObtenerActividadPorId?actividadId=${actividadIdGlobal}`);
        if (!resp.ok) throw new Error('No se encontró la actividad');

        const text = await resp.text();
        let data = null;
        try { data = text ? JSON.parse(text) : null; } catch (e) { data = null; }
        if (!data) try { data = resp.ok ? JSON.parse(text) : null; } catch (e) { data = null; }
        if (!data) throw new Error('Respuesta inválida del servidor al obtener actividad');

        document.getElementById('tituloActividad').innerText = data.NombreActividad || 'Sin título';
        document.getElementById('descripcionActividad').innerText = data.Descripcion || '';

        let fechaVal = data.FechaLimite || data.fechaLimite || data.FechaLimiteString || null;
        let fechaText = '—';
        const d = parseServerDateFlexible(fechaVal);
        if (d) fechaText = formatDateForUser(d);
        document.getElementById('fechaLimite').innerText = fechaText;
    } catch (e) {
        console.error(e);
    }
}

// Escuchar evento de calificación para refrescar vista del alumno si corresponde
window.addEventListener('entregableCalificado', function (ev) {
    try { verificarEnvio(); } catch (e) { console.error(e); }
});

// También escuchar cambios en localStorage (cuando docente guarda y guarda marca en localStorage)
window.addEventListener('storage', function (e) { try { if (e.key === 'entregableCalificado') verificarEnvio(); } catch (err) { console.error(err); } });

async function verificarEnvio() {
    try {
        const resp = await fetch(`/api/Alumnos/ObtenerEnviosActividadesAlumno?ActividadId=${actividadIdGlobal}&AlumnoId=${alumnoIdGlobal}`);
        if (!resp.ok) return;
        const data = await resp.json();
        const envio = Array.isArray(data) && data.length >0 ? data[0] : (data || null);
        if (envio) {
            var d = parseServerDateFlexible(envio.FechaEntrega || envio.fechaEntrega || envio.FechaEntrega);
            var fechaTexto = formatDateForUser(d);
            var estadoHtml = '<p>Ya entregado. Fecha: ' + fechaTexto + '</p>';
            try {
                var parsed = JSON.parse(envio.Contenido || 'null');
                if (parsed && parsed.Archivos && Array.isArray(parsed.Archivos) && parsed.Archivos.length >0) {
                    estadoHtml += '<p>Archivos adjuntos:</p><ul>';
                    parsed.Archivos.forEach(function (a) { estadoHtml += '<li><a href="' + a + '" target="_blank">' + a.split('/').pop() + '</a></li>'; });
                    estadoHtml += '</ul>';
                } else if (parsed && parsed.Respuesta) {
                    estadoHtml += '<div><strong>Respuesta:</strong><pre>' + escapeHtml(parsed.Respuesta) + '</pre></div>';
                } else {
                    if (envio.Contenido) estadoHtml += '<div><strong>Respuesta:</strong><pre>' + escapeHtml(envio.Contenido) + '</pre></div>';
                }
            } catch (e) {
                if (envio.Contenido) estadoHtml += '<div><strong>Respuesta:</strong><pre>' + escapeHtml(envio.Contenido) + '</pre></div>';
            }

            document.getElementById('estadoEntrega').innerHTML = estadoHtml;
            var ef = document.getElementById('entregaForm'); if (ef) ef.style.display = 'none';
            if (envio.Calificacion !== null && envio.Calificacion !== undefined) {
                document.getElementById('calificacionAlumno').innerHTML = '<p>Calificación: <strong>' + envio.Calificacion + '</strong></p>';
            } else {
                document.getElementById('calificacionAlumno').innerHTML = '<p>Calificación: <em>Pendiente de calificar</em></p>';
            }
        } else {
            // show form
            var ef2 = document.getElementById('entregaForm'); if (ef2) ef2.style.display = '';
        }
    } catch (e) { console.error(e); }
}

async function enviarEntrega() {
    const respuesta = (document.getElementById('respuesta') || {}).value.trim();
    const respuestaLink = (document.getElementById('respuestaLink') || {}).value || '';
    const fileInput = document.getElementById('fileEntrega');
    if (!respuesta && !respuestaLink && (!fileInput || !fileInput.files || fileInput.files.length ===0)) { alert('Agrega una respuesta, un link o un archivo.'); return; }

    const form = new FormData();
    form.append('ActividadId', actividadIdGlobal);
    form.append('AlumnoId', alumnoIdGlobal);
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
        const resp = await fetch('/Alumno/SubirEntrega', { method: 'POST', body: form });
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
    return String(unsafe).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/\"/g, "&quot;").replace(/'/g, "&#039;");
}
