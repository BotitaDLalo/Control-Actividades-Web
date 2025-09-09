function mostrarModalConfirmacion(mensaje, titulo = "Confirmación", callback) {
    document.getElementById("modalTitle").innerText = titulo;
    document.getElementById("modalMensaje").innerText = mensaje;

    let btnAceptar = document.getElementById("btnAceptar");
    btnAceptar.onclick = function () {
        callback(); // Ejecuta la función pasada como argumento
        let modal = new bootstrap.Modal(document.getElementById('modalConfirmacion'));
        modal.hide();
    };

    let modal = new bootstrap.Modal(document.getElementById('modalConfirmacion'));
    modal.show();
}