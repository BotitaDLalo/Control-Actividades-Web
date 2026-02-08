//// Funcion que carga los avisos a la vista.
//let _rolUsuarioCache = null;


/*async function cargaravisosdemateria() {
const listaavisos = document.getelementbyid("listadeavisosdemateria");
try {
const params = new urlsearchparams(window.location.search);

const materiaid = params.get("materiaid");
if (!materiaid) return;

const response = await fetch(
    `/materias/obteneravisos?idmateria=${materiaid}`
);
if (!response.ok) throw new error("no se encontraron avisos.");
const responsejson = await response.json();

const avisos = responsejson.avisos || [];
const rolusuario = responsejson.rolusuario || '';
// guardar en cache y renderizar
_avisoscache = avisos.slice();
_rolusuariocache = rolusuario;
aplicarfiltrosyrender();
// inicializar listeners de filtros (solo una vez)
inicializarfiltrosavisos();
}
catch (error) {
listaavisos.innerhtml = `<p class="aviso-error">${error.message}</p>`;
}

// intentar cargar avisos al cargar el documento
if (typeof document !== 'undefined'){
document.addeventlistener('domcontentloaded', function(){
try {
    cargaravisosdemateria();
}
catch (e) {
    console.warn('error cargando avisos iniciales', e);
}
});
}
}*/
