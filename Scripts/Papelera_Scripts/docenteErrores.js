//var div = document.getElementById("docente-datos");
//var docenteIdGlobal = div && div.dataset ? div.dataset.docenteid : null;

////Funcion que detecta errores generales
//function alertaDeErroresGenerales(error) {
//    // Mensaje de error por defecto
//    let mensajeError = "Ocurrió un error inesperado.";

//    // Si el error tiene un mensaje, lo usamos
//    if (error && error.message) {
//        mensajeError = error.message;
//    }

//    // Enlace para enviar un correo con el error incluido en el cuerpo
//    const enlaceCorreo = `mailto:soporte@tuempresa.com?subject=Error%20en%20la%20aplicación
//        &body=Hola,%20tengo%20un%20problema%20en%20la%20aplicación.%0A%0ADetalles%20del%20error:%0A${encodeURIComponent(mensajeError)}
//        %0A%0APor%20favor,%20ayuda.`.replace(/\s+/g, ''); // Limpia espacios innecesarios

//    // Mostrar alerta
//    Swal.fire({
//        icon: "error",
//        title: "Oops...",
//        text: mensajeError,
//        position: "center",
//        allowOutsideClick: false//, // Evita que se cierre con un clic afuera
//        //footer: `<a href="${enlaceCorreo}" target="_blank">Si el problema persiste, contáctanos.</a>`
//    });
//}
