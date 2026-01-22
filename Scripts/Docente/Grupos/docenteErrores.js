var div = document.getElementById("docente-datos");
var docenteIdGlobal = div && div.dataset ? div.dataset.docenteid : null;

//Funcion que detecta errores generales
function alertaDeErroresGenerales(error) {
    // Mensaje de error por defecto
    let mensajeError = "Ocurrió un error inesperado.";

    // Si el error tiene un mensaje, lo usamos
    if (error && error.message) {
        mensajeError = error.message;
    }

    // Enlace para enviar un correo con el error incluido en el cuerpo
    const enlaceCorreo = `mailto:soporte@tuempresa.com?subject=Error%20en%20la%20aplicación
        &body=Hola,%20tengo%20un%20problema%20en%20la%20aplicación.%0A%0ADetalles%20del%20error:%0A${encodeURIComponent(mensajeError)}
        %0A%0APor%20favor,%20ayuda.`.replace(/\s+/g, ''); // Limpia espacios innecesarios

    // Mostrar alerta
    Swal.fire({
        icon: "error",
        title: "Oops...",
        text: mensajeError,
        position: "center",
        allowOutsideClick: false//, // Evita que se cierre con un clic afuera
        //footer: `<a href="${enlaceCorreo}" target="_blank">Si el problema persiste, contáctanos.</a>`
    });
}

//function AlertaCierreSesion() {
//    let timerInterval;
//    Swal.fire({
//        title: "Parece que se perdió la conexión con tu sesión.",
//        html: "La cerraremos por seguridad y podrás volver a iniciar sesión en <b></b>.",
//        timer: 5000,
//        timerProgressBar: true,
//        position: "center",
//        allowOutsideClick: false, 
//        didOpen: () => {
//            Swal.showLoading();
//            const timer = Swal.getPopup().querySelector("b");
//            timerInterval = setInterval(() => {
//                timer.textContent = `${Math.floor(Swal.getTimerLeft() / 1000)} segundos`;
//            }, 100);
//        },
//        willClose: () => {
//            clearInterval(timerInterval);
//            cerrarSesion();
//        }
//    }).then((result) => {
//        if (result.dismiss === Swal.DismissReason.timer) {
//            console.log("Cerrando sesión automáticamente.");
//        }
//    });
//}

// Mostrar alerta cuando la sesión no está disponible y forzar cierre.
function AlertaCierreSesion() {
    // Alerta deshabilitada intencionalmente para docentes.
    // Código original comentado para mantener referencia y poder revertir fácilmente.
    /*
    let timerInterval;
    Swal.fire({
        title: "Parece que se perdió la conexión con tu sesión.",
        html: "La cerraremos por seguridad y podrás volver a iniciar sesión en <b></b>.",
        timer: 3500,
        timerProgressBar: true,
        position: "center",
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
            const timer = Swal.getPopup().querySelector("b");
            timerInterval = setInterval(() => {
                if (timer) timer.textContent = `${Math.ceil(Swal.getTimerLeft() / 1000)}s`;
            }, 200);
        },
        willClose: () => {
            clearInterval(timerInterval);
            try { cerrarSesion(); } catch (e) { window.location.href = '/Cuenta/IniciarSesion'; }
        }
    }).then((result) => {
        // no-op
    });
    */

    // No-op: evitar mostrar modal y no forzar cierre de sesión aquí.
    console.log('AlertaCierreSesion desactivada (modal comentado).');
}


// Función para cerrar sesión
async function cerrarSesion() {
    try {
        // Realiza una solicitud POST al endpoint de cierre de sesión
        const response = await fetch('/Cuenta/CerrarSesion', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded', // Especifica el tipo de contenido
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value // Obtiene el token de verificación CSRF
            }
        });

        if (response.ok) {
            console.log("Sesión cerrada correctamente."); // Mensaje en consola indicando que la sesión se cerró con éxito
            window.location.href = "/Cuenta/IniciarSesion"; // Redirige al usuario a la página de inicio de sesión
        } else {
            // En caso de error en la respuesta del servidor, muestra un mensaje de alerta con SweetAlert2
            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "No se pudo cerrar sesión.",
                position: "center",
                allowOutsideClick: false//,// Evita que la alerta se cierre al hacer clic fuera de ella
                // footer: '<a href="mailto:soporte@tuempresa.com?subject=Problema%20con%20cierre%20de%20sesión&body=Hola,%20tengo%20un%20problema%20al%20cerrar%20sesión.%20Por%20favor,%20ayuda." target="_blank">Si el problema persiste, contáctanos.</a>'
            });
        }
    } catch (error) {
        // Captura cualquier error inesperado (por ejemplo, problemas de conexión) y muestra una alerta
        alertaDeErroresGenerales(error);
    }
}