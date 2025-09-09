var div = document.getElementById("docente-datos");
var docenteIdGlobal = div.dataset.docenteid;
function copiarCodigoAcceso() {
    const codigoElemento = document.getElementById("codigoAcceso");
    const codigo = codigoElemento.innerText;

    // Crear un input temporal para copiar el texto
    const inputTemp = document.createElement("input");
    document.body.appendChild(inputTemp);
    inputTemp.value = codigo;
    inputTemp.select();
    document.execCommand("copy");
    document.body.removeChild(inputTemp);

    // Cambiar temporalmente el ícono para indicar que se copió
    const icono = document.querySelector(".copiar-icono");
    icono.classList.remove("fa-copy");
    icono.classList.add("fa-check");

    setTimeout(() => {
        icono.classList.remove("fa-check");
        icono.classList.add("fa-copy");
    }, 2000); // Volver al ícono de copiar después de 2 segundos
}
