document.addEventListener("DOMContentLoaded", function () {
    const privacyLink = document.getElementById("privacy-link");
    const privacyModal = document.getElementById("privacy-modal");
    const closeModal = document.querySelector(".close-modal");
    const acceptButton = document.getElementById("modal-accept");

    // Abrir el modal
    privacyLink.addEventListener("click", function (event) {
        event.preventDefault();
        privacyModal.style.display = "flex";
    });

    // Cerrar el modal al hacer clic en la "X"
    closeModal.addEventListener("click", function () {
        privacyModal.style.display = "none";
    });

    // Cerrar el modal al hacer clic en el botón "Aceptar"
    acceptButton.addEventListener("click", function () {
        privacyModal.style.display = "none";
    });

    // Cerrar el modal al hacer clic fuera del contenido
    window.addEventListener("click", function (event) {
        if (event.target === privacyModal) {
            privacyModal.style.display = "none";
        }
    });
});
