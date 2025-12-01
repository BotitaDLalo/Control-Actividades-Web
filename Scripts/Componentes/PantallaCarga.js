    function mostrarLoader() {
        $("#loader").addClass("visible");
    }

function ocultarLoader() {
    setTimeout(() => {
        $("#loader").removeClass("visible");
    }, 10); // retrasa x milisegundos para ver la animación
}


    // Mostrar loader automáticamente durante cualquier petición AJAX
    $(document).ajaxStart(function () {
        mostrarLoader();
    });

    $(document).ajaxStop(function () {
        ocultarLoader();
    });

    // Mostrar loader en cambio de página
    window.addEventListener('beforeunload', function () {
        mostrarLoader();
    });

    //Carga para fetch
    if (window.fetch) {
        const originalFetch = window.fetch;
        window.fetch = async function (...args) {
            try {
                mostrarLoader();
                const response = await originalFetch.apply(this, args);
                return response;
            } catch (error) {
                throw error;
            } finally {
                ocultarLoader();
            }
        };
    }
