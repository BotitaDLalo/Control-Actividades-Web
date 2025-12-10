var div = document.getElementById("docente-datos");
var docenteIdGlobal = div && div.dataset ? div.dataset.docenteid : null;

var misCursosBtn = document.getElementById("misCursos-dropdownBtn");
var misCursosMenu = document.getElementById("misCursos-dropdownMenu");
if (misCursosBtn) {
    misCursosBtn.addEventListener("click", function () {
        if (!misCursosMenu) return;
        misCursosMenu.style.display = misCursosMenu.style.display === "block" ? "none" : "block";
    });
}

// Ocultar el menú si se hace clic fuera de él
document.addEventListener("click", function (event) {
    var dropdown = document.querySelector(".misCursos-dropdown");
    if (!dropdown) return;
    if (!dropdown.contains(event.target)) {
        if (misCursosMenu) misCursosMenu.style.display = "none";
    }
});



