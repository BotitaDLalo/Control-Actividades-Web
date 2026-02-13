// Lightweight, robust alumnos script: defines cargarAlumnosAsignados, render and import handling.
var div = document.getElementById('docente-datos');
var docenteIdGlobal = div && div.dataset ? div.dataset.docenteid : null;

// Helper: try to resolve current MateriaId from multiple sources (window, hidden input, URL)
function resolveMateriaId() {
 try {
 var id = null;
 if (typeof window.materiaIdGlobal !== 'undefined' && window.materiaIdGlobal) {
 id = parseInt(window.materiaIdGlobal,10);
 if (!isNaN(id) && id >0) return id;
 }
 var hid = document.getElementById('materiaIdHidden');
 if (hid && hid.value) {
 var v = parseInt(hid.value,10);
 if (!isNaN(v) && v >0) return v;
 }
 // fallback: try URL querystring 'materiaId'
 try {
 var params = new URLSearchParams(window.location.search);
 if (params.has('materiaId')) {
 var pv = parseInt(params.get('materiaId'),10);
 if (!isNaN(pv) && pv >0) return pv;
 }
 } catch (e) { }
 return null;
 } catch (e) { return null; }
}

// If you want to remove the alumno search UI from the MateriaDetalles view
// remove any search-related elements that may be rendered by server or other scripts.

 // keyboard navigation is attached inside setupAlumnoSearch when elements exist


// Initialize search/assign UI
function setupAlumnoSearch() {
 var buscar = document.getElementById('buscarAlumno');
 var btnAsignar = document.getElementById('btnAsignarAlumno');
 var sugerencias = document.getElementById('sugerenciasAlumnos');
 if (!buscar || !sugerencias) return;

 var debounceTimer = null;
 buscar.addEventListener('input', function () {
 clearTimeout(debounceTimer);
 var q = this.value.trim();
 if (!q) {
 renderSugerencias([]);
 return;
 }
 debounceTimer = setTimeout(async function () {
 try {
 var basePath = (window.appBasePath || '');
 var resp = await fetch('/Materias/BuscarAlumnosPorCorreo?query=' + encodeURIComponent(q), { credentials: 'same-origin' });
 if (!resp.ok) {
 renderSugerencias([]);
 return;
 }
 var data = await resp.json().catch(
 function () {
 return [];
 });
 renderSugerencias(data || []);
 } catch (e) { console.error('Error buscar alumnos', e); renderSugerencias([]); }
 },250);
 });

 // hide suggestions when clicking outside
 document.addEventListener('click', function (ev) {
 if (!sugerencias) return;
 if (!ev.target.closest || (!ev.target.closest('#sugerenciasAlumnos') && ev.target !== buscar)) {
 sugerencias.innerHTML = '';
 sugerencias.style.display = 'none';
 }
 });

 if (btnAsignar) {
 // Insert Excel template download icon next to assign button (if not already present)
 try {
 var container = btnAsignar.parentElement || document.body;
 if (!document.getElementById('btnDescargarPlantillaAlumnos')) {
 var tplBtn = document.createElement('button');
 tplBtn.type = 'button';
 tplBtn.id = 'btnDescargarPlantillaAlumnos';
 tplBtn.className = 'btn btn-sm btn-outline-secondary ms-2';
 tplBtn.title = 'Descargar plantilla de Excel (columnas: Email,Nombre,ApellidoPaterno,ApellidoMaterno). Compatible con Excel2007-2022';
 tplBtn.innerHTML = '<i class="bi bi-file-earmark-excel"></i> Plantilla';
 // prefer to insert after btnAsignar
 if (btnAsignar.nextSibling) container.insertBefore(tplBtn, btnAsignar.nextSibling);
 else container.appendChild(tplBtn);

 tplBtn.addEventListener('click', function () {
 // Generate a CSV template and trigger download. CSV works in Excel2007..2022
 var headers = ['Email','Nombre','ApellidoPaterno','ApellidoMaterno'];
 var csv = headers.join(',') + '\n';
 var blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
 var url = URL.createObjectURL(blob);
 var a = document.createElement('a');
 a.href = url;
 a.download = 'plantilla_alumnos.csv';
 document.body.appendChild(a);
 a.click();
 setTimeout(function () { URL.revokeObjectURL(url); try { a.remove(); } catch (e) { } },500);

 if (window.Swal && typeof Swal.fire === 'function') {
 Swal.fire({
 icon: 'info',
 title: 'Formato de importación',
 html: 'La plantilla se descarga como CSV. Puedes abrirla con Excel (2007-2022) y guardarla como .xlsx si lo prefieres. Columnas requeridas: <b>Email, Nombre, ApellidoPaterno, ApellidoMaterno</b>.',
 confirmButtonText: 'Entendido'
 });
 }
 });
 }
 } catch (e) { console.warn('No se pudo insertar botón plantilla', e); }

 btnAsignar.addEventListener('click', async function () {
 var correo = buscar.value.trim();
 if (!correo) {
 alert('Ingresa el correo del alumno');
 return;
 }
 var materiaId = resolveMateriaId();
 if (!materiaId) {
 alert('No se pudo identificar la materia');
 return;
 }
 try {
 var body = new URLSearchParams();
 body.append('correo', correo);
 body.append('materiaId', materiaId);
 var basePath = (window.appBasePath || '');
 var resp = await fetch('/Materias/AsignarAlumnoMateria',
 {
 method: 'POST',
 headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
 body: body.toString(),
 credentials: 'same-origin'
 });
 if (!resp.ok) {
 var txt = await resp.text().catch(()=>'');
 alert('Error al asignar alumno: ' + (txt || resp.status));
 return;
 }
 var j = await resp.json().catch(()=>null);
 alert((j && j.mensaje) ? j.mensaje : 'Alumno asignado correctamente');

 if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(materiaId);
 buscar.value = '';
 renderSugerencias([]);
 } catch (e) {
 console.error(e);
 alert('Error al asignar alumno');
 }
 });
 }
}

function renderSugerencias(items) {
 var ul = document.getElementById('sugerenciasAlumnos');
 if (!ul) return;
 ul.innerHTML = '';
 if (!items || items.length ===0) {
 ul.style.display = 'none';
 return;
 }
 items.forEach(function (it) {
 var li = document.createElement('li');
 li.className = 'list-group-item list-group-item-action';
 var display = ((it.Nombre || '') + ' ' + (it.ApellidoPaterno || '') + ' ' + (it.ApellidoMaterno || '')).trim();

 if (!display) display = it.Email || it.email || it.UserName || '';
 li.textContent = display + (it.Email ? (' — ' + it.Email) : '');
 li.addEventListener('click', function () {
 var input = document.getElementById('buscarAlumno');
 if (input) input.value = it.Email || it.email || '';
 ul.innerHTML = '';
 ul.style.display = 'none';
 });
 ul.appendChild(li);
 });
 ul.style.display = 'block';
}

if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', setupAlumnoSearch);
else setupAlumnoSearch();

function renderAlumnosTable(alumnos) {
 var cont = document.getElementById('listaAlumnosAsignados');
 if (!cont) return;
 cont.innerHTML = '';
 if (!alumnos || alumnos.length ===0) {
 cont.innerHTML = '<p class="text-muted">No hay alumnos asignados a esta materia.</p>';
 return;
 }
 // This is a new comment added for clarity
 var table = document.createElement('table'); table.id = 'tablaAlumnos'; table.className = 'table table-striped';
 var thead = document.createElement('thead');
 // Removed Estatus column as requested
 thead.innerHTML = '<tr><th>Nombre</th><th>Apellidos</th><th>Email</th><th>Acciones</th></tr>';
 table.appendChild(thead);
 var tbody = document.createElement('tbody');
 alumnos.forEach(function (a) {
 var tr = document.createElement('tr');
 var nombre = a.Nombre || a.nombre || '';
 var ap1 = (a.ApellidoPaterno || a.apellidoPaterno || '');
 var ap2 = (a.ApellidoMaterno || a.apellidoMaterno || '');
 // sanitize placeholder values coming from DB
 if (nombre === 'N/A' || nombre === 'N/D') nombre = '';
 if (ap1 === 'N/A' || ap1 === 'N/D') ap1 = '';
 if (ap2 === 'N/A' || ap2 === 'N/D') ap2 = '';
 var ap = (ap1 + ' ' + ap2).trim();
 // try multiple possible fields for email
 var email = '';
 if (a.Email) email = a.Email;
 else if (a.email) email = a.email;
 else if (a.Correo) email = a.Correo;
 else if (a.correo) email = a.correo;
 else if (a.UserName) email = a.UserName;
 else if (a.userName) email = a.userName;
 else if (a.IdentityUser && (a.IdentityUser.Email || a.IdentityUser.email)) email = a.IdentityUser.Email || a.IdentityUser.email;
 // fallback: sometimes the alumno object is nested inside another object
 else if (a.Alumno && (a.Alumno.Email || a.Alumno.email || a.Alumno.Correo)) email = a.Alumno.Email || a.Alumno.email || a.Alumno.Correo || '';
 // Do not show Estatus column — only show basic info
 tr.innerHTML = '<td>' + (nombre || '') + '</td><td>' + ap + '</td><td>' + email + '</td>';
 var tdAcc = document.createElement('td');
 // action group: delete + estatus dropdown
 var grupoAcc = document.createElement('div'); grupoAcc.className = 'btn-group';

 // eliminar button
 if (a.AlumnoMateriaId || a.alumnoMateriaId) {
 var delBtn = document.createElement('button');
 delBtn.className = 'btn btn-sm btn-danger'; delBtn.textContent = 'Eliminar';
 delBtn.addEventListener('click',
 function ()
 {
 eliminardelgrupo(a.AlumnoMateriaId || a.alumnoMateriaId);
 });
 grupoAcc.appendChild(delBtn);
 } else {
 var delBtn = document.createElement('button');
 delBtn.className = 'btn btn-sm btn-danger'; delBtn.textContent = 'Eliminar';
 delBtn.addEventListener('click',
 function ()
 {
 eliminardelgrupo(a.AlumnoId || a.alumnoId || (a.Alumno && a.Alumno.AlumnoId));
 }
 );
 grupoAcc.appendChild(delBtn);
 }

 // removed estatus dropdown — only delete button remains
 tdAcc.appendChild(grupoAcc);
 tr.appendChild(tdAcc);
 tbody.appendChild(tr);
 });
 table.appendChild(tbody);
 cont.appendChild(table);
 // No global dropdown handlers needed since dropdown was removed

 // Initialize DataTable if available. Destroy existing instance first to avoid errors.
 try {
 if (window.jQuery && $.fn.dataTable) {
 if ($.fn.dataTable.isDataTable('#tablaAlumnos')) {
 try { $('#tablaAlumnos').DataTable().clear().destroy(); } catch (e) { /* ignore */ }
 }
 
 $('#tablaAlumnos').DataTable({
 pageLength:10,
 lengthMenu: [5,10,25,50],
 ordering: true,
 searching: true,
 responsive: true,
 autoWidth: false,
 language: {
 url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
 }
 });
 }
 } catch (e) {
 console.warn('DataTables init failed', e);
 }
}

async function cargarAlumnosAsignados(materiaOrAlumnos) {
 var cont = document.getElementById('listaAlumnosAsignados');
 if (!cont) return;
 try {
 if (Array.isArray(materiaOrAlumnos))
 {
 renderAlumnosTable(materiaOrAlumnos);
 return;
 }
 var materiaId = (typeof materiaOrAlumnos !== 'undefined' && materiaOrAlumnos) ? materiaOrAlumnos : resolveMateriaId();
 if (!materiaId) {
 cont.innerHTML = '<p class="text-muted">No hay materia seleccionada.</p>'; return;
 }
 var basePath = (window.appBasePath || '');
 // ensure fresh data (no cache) and include credentials
 var resp = await fetch('/Materias/ObtenerAlumnosPorMateria?materiaId=' + encodeURIComponent(materiaId), { credentials: 'same-origin', cache: 'no-store' });
 if (!resp.ok) {
 cont.innerHTML = '<p class="text-danger">Error al cargar alumnos.</p>';
 return;
 }
 var data = await resp.json();
 var alumnos = [];
 if (data) {
 if (Array.isArray(data)) alumnos = data; // backward compat
 else if (Array.isArray(data.alumnos)) alumnos = data.alumnos;
 }
 renderAlumnosTable(alumnos);
 } catch (e) {
 console.error('Error cargarAlumnosAsignados', e);
 }
}

async function eliminardelgrupo(enlaceId) {
 if (!enlaceId) return;
 if (!confirm('¿Eliminar alumno?')) return;
 try {
 var basePath = (window.appBasePath || '');
 var r = await fetch('/Materias/EliminarAlumnoDeMateria?idEnlace=' + encodeURIComponent(enlaceId), { method: 'DELETE' });
 if (!r.ok) throw new Error('No eliminado');
 alert('Alumno eliminado');
 if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(resolveMateriaId());
 } catch (e) {
 console.error(e);
 alert('Error al eliminar alumno');
 }
}

// Expose globally
window.cargarAlumnosAsignados = cargarAlumnosAsignados;
window.eliminardelgrupo = eliminardelgrupo;

// NOTE: Estatus change UI/endpoint was removed; no client-side function needed.

// Import handling: expose a top-level function so dynamic buttons/modals can open file selector
function createAndOpenFileInput(grupoId) {
 // Evitar abrir el diálogo más de una vez simultáneamente
 if (window._importDialogOpen) return;
 window._importDialogOpen = true;

 var input = document.getElementById('fileImportarAlumnos');
 if (input)
 {
 try {
 input.remove();
 } catch (e) { }
 }
 input = document.createElement('input');
 input.type = 'file';
 input.accept = '.xlsx,.xls';
 input.id = 'fileImportarAlumnos';
 input.style.display = 'none';
 document.body.appendChild(input);
 input.addEventListener('change', async function (ev) {
 try {
 var file = ev.target.files && ev.target.files[0];
 if (!file) return;

 var fd = new FormData();
 fd.append('file', file);

 if (grupoId) fd.append('GrupoId', grupoId);
 var mid = resolveMateriaId();
 if (mid) fd.append('MateriaId', mid);
 console.log('ImportarAlumnosExcel: enviando', file.name, 'MateriaId=', mid, 'GrupoId=', grupoId);

 var basePath = (window.appBasePath || '');
 var resp = await fetch('/Alumno/ImportarAlumnosExcel', { method: 'POST', body: fd, credentials: 'same-origin' });
 var json = await resp.json().catch(function(){return {};});

 if (!resp.ok) {
 alert(json.mensaje || 'Error importar');
 return;
 }

 // Mostrar resumen detallado de la importación (si la API lo retorna)
 try {
 var totalLeidos = json.TotalLeidos || json.Total ||0;
 var agregadosCount = (json.Agregados && Array.isArray(json.Agregados)) ? json.Agregados.length : (json.AgregadosCount ||0);
 var omitidosCount = (json.Omitidos && Array.isArray(json.Omitidos)) ? json.Omitidos.length : (json.OmitidosCount ||0);
 var noEncontradosCount = (json.NoEncontrados && Array.isArray(json.NoEncontrados)) ? json.NoEncontrados.length : (json.NoEncontradosCount ||0);
 var summary = `Total leídos: ${totalLeidos}\nAgregados: ${agregadosCount}\nOmitidos: ${omitidosCount}\nNo encontrados: ${noEncontradosCount}`;
 if (window.Swal && typeof Swal.fire === 'function') {
 Swal.fire('Importación completa', summary.replace(/\n/g, '<br/>'), 'success');
 } else {
 alert(summary.replace(/\n/g, '\n'));
 }
 } catch (e) {
 console.warn('No se pudo construir resumen de importación', e);
 if (window.Swal && typeof Swal.fire === 'function') Swal.fire('Importación completa', 'Importación finalizada', 'success');
 else alert('Importación completada');
 }

 // Si la API devuelve la lista de alumnos importados, renderizarlos directamente
 if (json && Array.isArray(json.Alumnos) && json.Alumnos.length >0) {
 if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(json.Alumnos);
 } else {
 if (typeof cargarAlumnosAsignados === 'function') cargarAlumnosAsignados(resolveMateriaId());
 }
 } catch (e) {
 console.error(e);
 alert('Error al subir archivo');
 } finally {
 // Limpiar y permitir futuras aperturas
 try {
 input.remove();
 } catch (e) { }
 window._importDialogOpen = false;
 }
 });
 setTimeout(function ()
 {
 try {
 input.click();
 } catch (e) {
 console.error(e);
 window._importDialogOpen = false;
 }
 },10);
}

// Also support older code that registers handler on DOMContentLoaded: keep listener but add delegation
document.addEventListener('DOMContentLoaded', function () {
 var btn = document.getElementById('btnImportarAlumnos');
 if (btn) btn.addEventListener('click', function (e) { e.preventDefault(); createAndOpenFileInput(); });

 // Delegated listener: if the button is added later (dynamically), catch clicks on it
 document.addEventListener('click', function (e) {
 try {
 var target = e.target;
 if (!target) return;
 var btnEl = target.closest && target.closest('#btnImportarAlumnos');
 if (btnEl) {
 e.preventDefault();
 createAndOpenFileInput();
 }
 } catch (ex) { /* swallow */ }
 });
});

