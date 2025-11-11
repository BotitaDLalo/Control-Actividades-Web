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
        document.getElementById('tituloActividad').innerText = data.NombreActividad || 'Sin título';
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
            document.getElementById('estadoEntrega').innerHTML = '<p>Ya entregado. Fecha: ' + new Date(data.FechaEntrega).toLocaleString() + '</p>';
            document.getElementById('entregaForm').style.display = 'none';
            if (data.Calificacion !== null) {
                document.getElementById('calificacionAlumno').innerHTML = '<p>Calificación: ' + data.Calificacion + '</p>';
            }
        }
    } catch (e) { console.error(e); }
}

async function enviarEntrega() {
    const respuesta = document.getElementById('respuesta').value.trim();
    if (!respuesta) { alert('Escribe una respuesta.'); return; }

    const payload = {
        ActividadId: actividadIdGlobal,
        AlumnoId: alumnoIdGlobal,
        Respuesta: respuesta,
        FechaEntrega: new Date().toISOString()
    };

    try {
        const resp = await fetch('/api/Alumnos/RegistrarEnvioActividadAlumno', {
            method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload)
        });
        if (!resp.ok) throw new Error('Error al enviar');
        const data = await resp.json();
        alert('Enviado correctamente');
        verificarEnvio();
    } catch (e) { alert('Error: ' + e.message); }
}
