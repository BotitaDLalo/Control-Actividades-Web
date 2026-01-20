function inicializarAvisos() {
    console.log("✅ inicializarAvisos() fue llamada");
    console.log("alumnoIdGlobal:", alumnoIdGlobal);
    if (!alumnoIdGlobal) {
        console.error("alumnoIdGlobal no está definido.");
        $("#avisos-container").html("<p>Error al obtener los avisos.</p>");
        return;
    }

    // include materiaId/grupoId if present so server returns scoped avisos
    var qs = 'alumnoId=' + encodeURIComponent(alumnoIdGlobal);
    try { if (typeof materiaIdGlobal !== 'undefined' && materiaIdGlobal) qs += '&materiaId=' + encodeURIComponent(materiaIdGlobal); } catch(e){}
    try { if (typeof grupoIdGlobal !== 'undefined' && grupoIdGlobal) qs += '&grupoId=' + encodeURIComponent(grupoIdGlobal); } catch(e){}

    $.get('/Alumno/ObtenerAvisos?' + qs, function (data) {
        var avisosHtml = "";
        if (Array.isArray(data)) data = data.slice().reverse();
        if (data.length > 0) {
            data.forEach(function (aviso) {
                avisosHtml += `
                    <li class="list-group-item">
                    <div class="aviso-header">
                        <div class="aviso-icono">📢</div>
                        <div class="aviso-info">
                            <strong>${aviso.Titulo}</strong>
                            <p>${aviso.Descripcion}</p>
                            <p class="aviso-fecha-publicado">Publicado: ${aviso.FechaCreacion}</p>
                        </div>
                    </div>
                    </li>`;


               
            });
        } else {
            avisosHtml = "<p>No hay avisos disponibles.</p>";
        }
        $("#avisos-container").html(avisosHtml);
    }).fail(function () {
        $("#avisos-container").html("<p>Error al cargar los avisos.</p>");
    });
}

