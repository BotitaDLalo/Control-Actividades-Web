document.addEventListener('DOMContentLoaded', function () {
    cargarDetalleActividad();
    verificarEnvio();

    document.getElementById('btnEnviar').addEventListener('click', enviarEntrega);
});

async function cargarDetalleActividad() {
    try {
        const resp = await fetch(`/Actividades/ObtenerActividadPorId?actividadId=${actividadIdGlobal}`);
        if (!resp.ok) throw new Error('No se encontró la actividad');
        const data = await resp.json();
        document.getElementById('tituloActividad').innerText = data.NombreActividad || 'Sin t�tulo';
        document.getElementById('descripcionActividad').innerText = data.Descripcion || '';
        document.getElementById('fechaLimite').innerText = new Date(data.FechaLimite).toLocaleString();
    } catch (e) {
        console.error(e);
    }
}

async function verificarEnvio() {
    try {
        const resp = await fetch(`/api/Actividades/ObtenerEnviosActividadesAlumno?ActividadId=${actividadIdGlobal}&AlumnoId=${alumnoIdGlobal}`);
        if (!resp.ok) return;
        const data = await resp.json();
        if (data && data.AlumnoActividadId && data.Status) {
            var estadoHtml = '<p>Ya entregado. Fecha: ' + new Date(data.FechaEntrega).toLocaleString() + '</p>';
            // intentar parsear respuesta para extraer archivos
            try {
                var parsed = JSON.parse(data.Respuesta || 'null');
                if (parsed && parsed.Archivos && Array.isArray(parsed.Archivos) && parsed.Archivos.length > 0) {
                    estadoHtml += '<p>Archivos adjuntos:</p><ul>';
                    parsed.Archivos.forEach(function (a) { estadoHtml += '<li><a href="' + a + '" target="_blank">' + a.split('/').pop() + '</a></li>'; });
                    estadoHtml += '</ul>';
                } else if (parsed && parsed.Respuesta) {
                    estadoHtml += '<div><strong>Respuesta:</strong><pre>' + parsed.Respuesta + '</pre></div>';
                } else {
                    if (data.Respuesta) estadoHtml += '<div><strong>Respuesta:</strong><pre>' + data.Respuesta + '</pre></div>';
                }
            } catch (e) {
                if (data.Respuesta) estadoHtml += '<div><strong>Respuesta:</strong><pre>' + data.Respuesta + '</pre></div>';
            }

            document.getElementById('estadoEntrega').innerHTML = estadoHtml;
            document.getElementById('entregaForm').style.display = 'none';
            if (data.Calificacion !== null) {
                document.getElementById('calificacionAlumno').innerHTML = '<p>Calificaci�n: ' + data.Calificacion + '</p>';
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
