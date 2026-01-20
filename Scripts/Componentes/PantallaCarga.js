function mostrarLoader() {
    $("#loader").addClass("visible");
}

function ocultarLoader() {
    setTimeout(() => {
        $("#loader").removeClass("visible");
    });
}

// Mostrar loader en cambio de página
window.addEventListener('beforeunload', function () {
    mostrarLoader();
});
