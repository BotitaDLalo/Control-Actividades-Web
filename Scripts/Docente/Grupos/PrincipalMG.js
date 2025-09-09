var div = document.getElementById("docente-datos");
var docenteIdGlobal = div.dataset.docenteid;

document.getElementById("misCursos-dropdownBtn").addEventListener("click", function () {
    let menu = document.getElementById("misCursos-dropdownMenu");
    menu.style.display = menu.style.display === "block" ? "none" : "block";
});

// Ocultar el menú si se hace clic fuera de él
document.addEventListener("click", function (event) {
    let dropdown = document.querySelector(".misCursos-dropdown");
    if (!dropdown.contains(event.target)) {
        document.getElementById("misCursos-dropdownMenu").style.display = "none";
    }
});



