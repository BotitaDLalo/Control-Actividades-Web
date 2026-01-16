    (function () {
        // Reference counter to support nested requests
        let __loaderCounter = 0;
        let __loaderSafetyTimeout = null;
        const SAFETY_TIMEOUT_MS = 8000; // force-hide after this time

        function mostrarLoader() {
            __loaderCounter++;
            // show immediately
            $("#loader").addClass("visible");
            // reset safety timeout
            if (__loaderSafetyTimeout) clearTimeout(__loaderSafetyTimeout);
            __loaderSafetyTimeout = setTimeout(() => {
                // Force hide if something got stuck
                __loaderCounter = 0;
                $("#loader").removeClass("visible");
                __loaderSafetyTimeout = null;
                console.warn('Loader hidden by safety timeout');
            }, SAFETY_TIMEOUT_MS);
        }

        function ocultarLoader() {
            // small delay to let UI update
            setTimeout(() => {
                __loaderCounter = Math.max(0, __loaderCounter - 1);
                if (__loaderCounter === 0) {
                    $("#loader").removeClass("visible");
                    if (__loaderSafetyTimeout) {
                        clearTimeout(__loaderSafetyTimeout);
                        __loaderSafetyTimeout = null;
                    }
                }
            }, 10);
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

        //Carga para fetch - wrap requests to increment/decrement counter
        if (window.fetch) {
            const originalFetch = window.fetch.bind(window);
            window.fetch = async function (...args) {
                mostrarLoader();
                try {
                    const response = await originalFetch(...args);
                    return response;
                } catch (error) {
                    throw error;
                } finally {
                    ocultarLoader();
                }
            };
        }
    })();
