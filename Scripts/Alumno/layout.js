function toggleMenu(event) {
    event.preventDefault(); // Evitar el comportamiento predeterminado del enlace
    var menu = document.getElementById("user-menu");
    menu.classList.toggle("show");
}

// Ocultar el menú si se hace clic fuera de él
document.addEventListener("click", function (e) {
    var menu = document.getElementById("user-menu");
    var userIcon = document.getElementById("user-icon");

    if (!menu.contains(e.target) && !userIcon.contains(e.target)) {
        menu.classList.remove("show");
    }
});
