
function manejarErrorSesion() {
    let timerInterval;
    Swal.fire({
        title: "Parece que se perdió la conexión con tu sesión.",
        html: "La cerraremos por seguridad y podrás volver a iniciar sesión en: <b></b>.",
        timer: 5000,
        timerProgressBar: true,
        allowOutsideClick: false,
        position: "center",
        didOpen: () => {
            Swal.showLoading();
            const timer = Swal.getPopup().querySelector("b");
            timerInterval = setInterval(() => {
                timer.textContent = `${Math.floor(Swal.getTimerLeft() / 1000)} segundos`;
            }, 100);
        },
        willClose: () => {
            clearInterval(timerInterval);
            cerrarSesion();
        }
    }).then((result) => {
        if (result.dismiss === Swal.DismissReason.timer) {
            console.log("Cerrando sesión automáticamente.");
        }
    });
}

