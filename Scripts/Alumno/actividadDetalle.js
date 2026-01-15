document.addEventListener('DOMContentLoaded', function () {
    cargarDetalleActividad();
    verificarEnvio();

    document.getElementById('btnEnviar').addEventListener('click', enviarEntrega);
});

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
            const d = new Date(fechaVal);
            fechaText = isNaN(d.getTime()) ? String(fechaVal) : d.toLocaleString();
        }
        document.getElementById('fechaLimite').innerText = fechaText;
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
        const envio = Array.isArray(data) && data.length > 0 ? data[0] : (data || null);
        if (envio) {
            var estadoHtml = '<p>Ya entregado. Fecha: ' + new Date(envio.FechaEntrega).toLocaleString() + '</p>';
            try {
                var parsed = JSON.parse(envio.Contenido || 'null');
                if (parsed && parsed.Archivos && Array.isArray(parsed.Archivos) && parsed.Archivos.length > 0) {
                    estadoHtml += '<p>Archivos adjuntos:</p><ul>';
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
            if (envio.Calificacion !== null && envio.Calificacion !== undefined) {
                document.getElementById('calificacionAlumno').innerHTML = '<p>Calificación: ' + envio.Calificacion + '</p>';
            } else {
                document.getElementById('calificacionAlumno').innerHTML = '<p>Calificación: <em>Pendiente de calificar</em></p>';
            }
        }
    } catch (e) { console.error(e); }
}

async function enviarEntrega() {
    const respuesta = document.getElementById('respuesta').value.trim();
    const fileInput = document.getElementById('fileEntrega');
    if (!respuesta && (!fileInput || !fileInput.files || fileInput.files.length === 0)) { alert('Agrega una respuesta o un archivo.'); return; }

    const form = new FormData();
    form.append('ActividadId', actividadIdGlobal);
    form.append('AlumnoId', alumnoIdGlobal);
    form.append('Respuesta', respuesta);
    form.append('FechaEntrega', new Date().toISOString());

    if (fileInput && fileInput.files && fileInput.files.length > 0) {
        for (let i = 0; i < fileInput.files.length; i++) {
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
