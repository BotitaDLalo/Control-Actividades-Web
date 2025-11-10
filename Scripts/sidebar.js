// Manejo del sidebar: navegación por íconos, y mini-menús (submenus) con animación suave
(function () {
    var sidebar = document.getElementById('appSidebar');
    var icons = document.querySelectorAll('.sidebar-icons .icon-item');
    var titles = document.querySelectorAll('.menu-title');

    if (!sidebar) return;

    function hideSection(section) {
        if (!section) return;
        section.classList.remove('open');
        var submenu = section.querySelector('.menu-list.submenu');
        var onEnd = function (e) {
            if (e.target !== submenu) return;
            section.style.display = 'none';
            submenu.removeEventListener('transitionend', onEnd);
        };
        if (submenu) {
            submenu.addEventListener('transitionend', onEnd);
        } else {
            section.style.display = 'none';
        }
    }

    function showSection(section) {
        if (!section) return;
        section.style.display = 'block';
        var submenu = section.querySelector('.menu-list.submenu');
        if (submenu) {
            window.requestAnimationFrame(function () {
                section.classList.add('open');
            });
        } else {
            section.classList.add('open');
        }
    }

    // small fetch with timeout utility and send credentials so auth cookie is included
    function fetchWithTimeout(url, options, timeout = 8000) {
        options = options || {};
        // ensure credentials sent for same-origin auth
        if (!options.credentials) options.credentials = 'same-origin';
        return new Promise(function (resolve, reject) {
            var didTimeOut = false;
            var timer = setTimeout(function () {
                didTimeOut = true;
                reject(new Error('timeout'));
            }, timeout);

            fetch(url, options).then(function (res) {
                if (!didTimeOut) {
                    clearTimeout(timer);
                    resolve(res);
                }
            }).catch(function (err) {
                if (didTimeOut) return;
                clearTimeout(timer);
                reject(err);
            });
        });
    }

    // Fetch and populate cursos for alumno or materias for docente
    async function populateCursos() {
        var list = document.getElementById('cursos-list');
        if (!list) return;
        list.innerHTML = '<li class="loading">Cargando...</li>';
        try {
            // detect alumnoId
            var alumnoId = window.alumnoIdGlobal || null;

            // detect docenteId robustly
            var docenteId = null;
            if (window.docenteIdGlobal) docenteId = window.docenteIdGlobal;
            var divDoc = document.getElementById('docente-datos');
            if (!docenteId && divDoc && divDoc.dataset && divDoc.dataset.docenteid) docenteId = divDoc.dataset.docenteid;

            console.log('populateCursos: alumnoId=', alumnoId, 'docenteId=', docenteId);

            if (alumnoId) {
                var resp = await fetchWithTimeout('/Alumno/ObtenerClases?alumnoId=' + encodeURIComponent(alumnoId), { method: 'GET' }, 8000);
                if (!resp.ok) { list.innerHTML = '<li>Error al cargar (' + resp.status + ')</li>'; return; }
                // try parse JSON safely
                var text = await resp.text();
                try {
                    var clases = JSON.parse(text);
                } catch (e) {
                    console.error('Respuesta no JSON para ObtenerClases:', text.slice(0,200));
                    list.innerHTML = '<li>Error al procesar datos</li>';
                    return;
                }
                renderClasesList(clases, list);
                return;
            }

            if (docenteId) {
                // use MVC endpoint directly to avoid API routing issues
                try {
                    var mvcUrl = '/Materias/ObtenerMateriasSinGrupo?docenteId=' + encodeURIComponent(docenteId);
                    var resp2 = await fetchWithTimeout(mvcUrl, { method: 'GET' }, 8000);
                    if (!resp2.ok) { list.innerHTML = '<li>Error al cargar (' + resp2.status + ')</li>'; return; }
                    var text2 = await resp2.text();
                    var materias2;
                    try {
                        materias2 = JSON.parse(text2);
                    } catch (e) {
                        console.error('Respuesta no JSON para ObtenerMateriasSinGrupo:', text2.slice(0,200));
                        list.innerHTML = '<li>Error al procesar datos</li>';
                        return;
                    }
                    // map to shape
                    var mapped = materias2.map(function (m) {
                        return { MateriaId: m.MateriaId || m.materiaId || m.MateriaId, NombreMateria: m.NombreMateria || m.nombreMateria || m.Nombre || m.nombre };
                    });
                    renderMateriasForDocente(mapped, list);
                    return;
                } catch (e) {
                    console.error('Error fetching materias MVC endpoint', e);
                    list.innerHTML = '<li>Error al cargar materias</li>';
                    return;
                }
            }

            list.innerHTML = '<li>No identificado</li>';
        } catch (e) {
            console.error('populateCursos error', e);
            if (list) list.innerHTML = '<li>Error</li>';
        }
    }

    function renderClasesList(clases, list) {
        list.innerHTML = '';
        if (!clases || clases.length === 0) {
            list.innerHTML = '<li>No tienes cursos</li>';
            return;
        }
        clases.forEach(function (c) {
            var li = document.createElement('li');
            var name = c.Nombre || c.nombre || c.NombreGrupo || c.nombreGrupo || '';
            var id = c.Id || c.id || c.GrupoId || c.grupoId || null;
            if (c.esGrupo || c.esGrupo === true) {
                li.innerHTML = '<a href="#" class="group-link">' + escapeHtml(name) + '</a>';
                var sub = document.createElement('ul');
                sub.className = 'group-materias';
                sub.style.display = 'none';
                var items = c.Materias || c.materias || [];
                if (items && items.length) {
                    items.forEach(function (m) {
                        var mli = document.createElement('li');
                        var mName = m.Nombre || m.NombreMateria || m.nombre || m.nombreMateria || '';
                        var mId = m.Id || m.id || m.MateriaId || m.materiaId || null;
                        mli.innerHTML = '<a href="/Alumno/Clase?tipo=materia&id=' + encodeURIComponent(mId) + '">' + escapeHtml(mName) + '</a>';
                        sub.appendChild(mli);
                    });
                } else {
                    var placeholder = document.createElement('li');
                    placeholder.textContent = 'Sin materias';
                    sub.appendChild(placeholder);
                }
                li.appendChild(sub);
                li.querySelector('.group-link').addEventListener('click', function (ev) {
                    ev.preventDefault();
                    sub.style.display = (sub.style.display === 'none' || !sub.style.display) ? 'block' : 'none';
                });
            } else {
                var mid = c.Id || c.id || c.MateriaId || c.materiaId || null;
                var mname = c.Nombre || c.NombreMateria || c.nombre || c.nombreMateria || '';
                li.innerHTML = '<a href="/Alumno/Clase?tipo=materia&id=' + encodeURIComponent(mid) + '">' + escapeHtml(mname) + '</a>';
            }
            list.appendChild(li);
        });
    }

    function renderMateriasForDocente(materias, list) {
        list.innerHTML = '';
        if (!materias || materias.length === 0) {
            list.innerHTML = '<li>No hay materias</li>';
            return;
        }
        materias.forEach(function (m) {
            var li = document.createElement('li');
            var name = m.NombreMateria || m.nombreMateria || m.Nombre || m.nombre || '';
            var id = m.MateriaId || m.materiaId || m.Id || m.id || null;
            li.innerHTML = '<a href="/Docente/Materia?materiaId=' + encodeURIComponent(id) + '">' + escapeHtml(name) + '</a>';
            list.appendChild(li);
        });
    }

    function escapeHtml(text) {
        if (text === null || text === undefined) return '';
        return String(text).replace(/[&"'<>]/g, function (a) { return { '&': '&amp;', '"': '&quot;', "'": '&#39;', '<': '&lt;', '>': '&gt;' }[a]; });
    }

    // icon clicks: marcar activo y toggle de la sección correspondiente
    icons.forEach(function (it) {
        it.addEventListener('click', function () {
            icons.forEach(function(i){ i.classList.remove('active'); });
            it.classList.add('active');
            var targetId = it.getAttribute('data-target');
            var sections = document.querySelectorAll('.menu-section');
            sections.forEach(function (s) {
                if (s.id !== targetId) {
                    if (s.classList.contains('open')) hideSection(s);
                }
            });
            var el = document.getElementById(targetId);
            if (!el) return;
            if (el.classList.contains('open')) {
                hideSection(el);
            } else {
                showSection(el);
                try { localStorage.setItem('sidebar.lastSection', targetId); } catch (e) {}
                if (targetId === 'menu-cursos') populateCursos();
            }
        });
        it.addEventListener('keydown', function(e){ if(e.key === 'Enter' || e.key === ' ') { it.click(); e.preventDefault(); }});
    });

    // titles click: toggle submenu open/close of its parent section
    titles.forEach(function (t) {
        t.addEventListener('click', function () {
            var parent = t.parentElement;
            var submenu = parent.querySelector('.menu-list.submenu');
            if (!submenu) return;
            if (parent.classList.contains('open')) {
                hideSection(parent);
            } else {
                document.querySelectorAll('.menu-section.open').forEach(function (s) { if (s !== parent) hideSection(s); });
                showSection(parent);
                if (parent.id === 'menu-cursos') populateCursos();
            }
        });
        t.addEventListener('keydown', function(e){ if(e.key === 'Enter' || e.key === ' ') { t.click(); e.preventDefault(); }});
    });

    // Inicial: muestra la última sección seleccionada o la primera
    document.addEventListener('DOMContentLoaded', function () {
        var last = null;
        try { last = localStorage.getItem('sidebar.lastSection'); } catch (e) { last = null; }
        var targetEl = null;
        if (last) targetEl = document.getElementById(last);
        if (!targetEl) targetEl = document.querySelector('.menu-section');
        document.querySelectorAll('.menu-section').forEach(function (s) { s.style.display = 'none'; s.classList.remove('open'); });
        if (targetEl) showSection(targetEl);
        var iconFor = document.querySelector('.sidebar-icons .icon-item[data-target="'+(targetEl?targetEl.id:'')+'"]');
        if (iconFor) { document.querySelectorAll('.sidebar-icons .icon-item').forEach(function(i){i.classList.remove('active');}); iconFor.classList.add('active'); }
    });
})();