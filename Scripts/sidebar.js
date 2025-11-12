// Manejo del sidebar: navegación por íconos, y submenus desplegables
(function () {
    'use strict';

    // small fetch with timeout utility and send credentials so auth cookie is included
    function fetchWithTimeout(url, options, timeout = 8000) {
        options = options || {};
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

    async function populateCursos() {
        var list = document.getElementById('cursos-list');
        if (!list) return;
        list.innerHTML = '<li class="loading">Cargando...</li>';
        try {
            var alumnoId = window.alumnoIdGlobal || null;
            var docenteId = null;
            if (window.docenteIdGlobal) docenteId = window.docenteIdGlobal;
            var divDoc = document.getElementById('docente-datos');
            if (!docenteId && divDoc && divDoc.dataset && divDoc.dataset.docenteid) docenteId = divDoc.dataset.docenteid;

            if (alumnoId) {
                var resp = await fetchWithTimeout('/Alumno/ObtenerClases?alumnoId=' + encodeURIComponent(alumnoId), { method: 'GET' }, 8000);
                if (!resp.ok) { list.innerHTML = '<li>Error al cargar (' + resp.status + ')</li>'; return; }
                var text = await resp.text();
                var clases = JSON.parse(text || '[]');
                renderClasesList(clases, list);
                return;
            }

            if (docenteId) {
                var mvcUrl = '/Materias/ObtenerMateriasSinGrupo?docenteId=' + encodeURIComponent(docenteId);
                var resp2 = await fetchWithTimeout(mvcUrl, { method: 'GET' }, 8000);
                if (!resp2.ok) { list.innerHTML = '<li>Error al cargar (' + resp2.status + ')</li>'; return; }
                var text2 = await resp2.text();
                var materias2 = JSON.parse(text2 || '[]');
                var mapped = materias2.map(function (m) {
                    return { MateriaId: m.MateriaId || m.materiaId || m.Id || m.id, NombreMateria: m.NombreMateria || m.nombreMateria || m.Nombre || m.nombre };
                });
                renderMateriasForDocente(mapped, list);
                return;
            }

            list.innerHTML = '<li>No identificado</li>';
        } catch (e) {
            console.error('populateCursos error', e);
            if (list) list.innerHTML = '<li>Error</li>';
        }
    }

    function renderClasesList(clases, list) {
        list.innerHTML = '';
        if (!clases || clases.length === 0) { list.innerHTML = '<li>No tienes cursos</li>'; return; }
        clases.forEach(function (c) {
            var li = document.createElement('li');
            var name = c.Nombre || c.nombre || c.NombreGrupo || c.nombreGrupo || '';
            var id = c.Id || c.id || c.GrupoId || c.grupoId || null;
            if (c.esGrupo || c.esGrupo === true) {
                li.innerHTML = '<a href="#" class="group-link">' + escapeHtml(name) + '</a>';
                var sub = document.createElement('ul'); sub.className = 'group-materias'; sub.style.display = 'none';
                var items = c.Materias || c.materias || [];
                if (items && items.length) {
                    items.forEach(function (m) {
                        var mli = document.createElement('li');
                        var mName = m.Nombre || m.NombreMateria || m.nombre || m.nombreMateria || '';
                        var mId = m.Id || m.id || m.MateriaId || m.materiaId || null;
                        mli.innerHTML = '<a href="/Alumno/Clase?tipo=materia&id=' + encodeURIComponent(mId) + '">' + escapeHtml(mName) + '</a>';
                        sub.appendChild(mli);
                    });
                } else { var placeholder = document.createElement('li'); placeholder.textContent = 'Sin materias'; sub.appendChild(placeholder); }
                li.appendChild(sub);
                li.querySelector('.group-link').addEventListener('click', function (ev) { ev.preventDefault(); sub.style.display = (sub.style.display === 'none' || !sub.style.display) ? 'block' : 'none'; });
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
        if (!materias || materias.length === 0) { list.innerHTML = '<li>No hay materias</li>'; return; }
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

    // Toggle collapsed sidebar (icon-only) and mobile overlay
    function toggleSidebarCollapsed() {
        var sidebar = document.getElementById('appSidebar');
        if (!sidebar) return;
        sidebar.classList.toggle('collapsed');
    }

    function openSidebarMobile() {
        var sidebar = document.getElementById('appSidebar');
        var backdrop = document.getElementById('sidebar-backdrop');
        if (!sidebar) return;
        sidebar.classList.add('mobile-open');
        if (backdrop) backdrop.classList.add('show');
    }

    function closeSidebarMobile() {
        var sidebar = document.getElementById('appSidebar');
        var backdrop = document.getElementById('sidebar-backdrop');
        if (!sidebar) return;
        sidebar.classList.remove('mobile-open');
        if (backdrop) backdrop.classList.remove('show');
    }

    // Automatically adjust sidebar when zoom or small viewport detected
    var zoomCheckTimeout;
    function checkZoomAndAdjust() {
        var sidebar = document.getElementById('appSidebar');
        if (!sidebar) return;
        var dpr = window.devicePixelRatio || 1;
        var vw = Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
        // More aggressive collapse: if DPR indicates zoom >= ~140% or viewport width small => collapse
        var shouldCollapse = (dpr >= 1.4) || (vw < 1200);
        if (shouldCollapse) {
            sidebar.classList.add('collapsed');
            // also ensure main content margin is reduced by applying collapsed class effect
            document.documentElement.classList.add('sidebar-collapsed-active');
        } else {
            sidebar.classList.remove('collapsed');
            document.documentElement.classList.remove('sidebar-collapsed-active');
        }
    }

    function scheduleZoomCheck(delay) {
        clearTimeout(zoomCheckTimeout);
        zoomCheckTimeout = setTimeout(checkZoomAndAdjust, delay || 150);
    }

    // Listen for common events that change zoom / viewport
    window.addEventListener('resize', function () { scheduleZoomCheck(150); });
    window.addEventListener('orientationchange', function () { scheduleZoomCheck(200); });
    // detect ctrl + wheel zoom
    window.addEventListener('wheel', function (e) { if (e.ctrlKey) scheduleZoomCheck(300); }, { passive: true });
    // detect key combinations that may change zoom (Ctrl + '+/-/0')
    window.addEventListener('keydown', function (e) { if ((e.ctrlKey || e.metaKey) && (e.key === '+' || e.key === '-' || e.key === '0')) scheduleZoomCheck(300); });

    // MAIN UI behaviour
    function initSidebar() {
        var icons = Array.prototype.slice.call(document.querySelectorAll('.sidebar-icons .icon-item'));
        var sections = Array.prototype.slice.call(document.querySelectorAll('.menu-section'));

        if (!icons.length || !sections.length) {
            console.warn('Sidebar: no icons or sections found');
            return;
        }

        function closeAllExcept(id) {
            sections.forEach(function (s) {
                if (s.id === id) return;
                s.classList.remove('open');
                s.style.display = 'none';
                s.dataset.manual = 'false';
            });
        }

        icons.forEach(function (it) {
            it.addEventListener('click', function () {
                icons.forEach(function (i) { i.classList.remove('active'); });
                it.classList.add('active');
                var targetId = it.getAttribute('data-target');
                var el = document.getElementById(targetId);
                if (!el) return;
                var isOpen = el.classList.contains('open');
                if (isOpen) {
                    el.classList.remove('open'); el.style.display = 'none'; el.dataset.manual = 'false';
                } else {
                    closeAllExcept(targetId);
                    el.style.display = 'block';
                    window.requestAnimationFrame(function () { el.classList.add('open'); });
                    el.dataset.manual = 'true';
                    if (targetId === 'menu-cursos') populateCursos();
                }
            });

            it.addEventListener('keydown', function (e) { if (e.key === 'Enter' || e.key === ' ') { it.click(); e.preventDefault(); } });
        });

        // hover behaviour for desktop
        if (!('ontouchstart' in window) && window.innerWidth > 768) {
            sections.forEach(function (sec) {
                sec.addEventListener('mouseenter', function () {
                    if (sec.dataset.manual === 'true') return;
                    closeAllExcept(sec.id);
                    sec.style.display = 'block';
                    window.requestAnimationFrame(function () { sec.classList.add('open'); });
                });
                sec.addEventListener('mouseleave', function () {
                    if (sec.dataset.manual === 'true') return;
                    sec.classList.remove('open'); sec.style.display = 'none';
                });
            });
        }

        // initial open
        var last = null; try { last = localStorage.getItem('sidebar.lastSection'); } catch (e) { last = null; }
        var targetEl = null; if (last) targetEl = document.getElementById(last);
        if (!targetEl) targetEl = sections[0];
        sections.forEach(function (s) { s.style.display = 'none'; s.classList.remove('open'); s.dataset.manual = 'false'; });
        if (targetEl) { targetEl.style.display = 'block'; targetEl.classList.add('open'); targetEl.dataset.manual = 'true';
            var iconFor = document.querySelector('.sidebar-icons .icon-item[data-target="' + (targetEl ? targetEl.id : '') + '"]');
            if (iconFor) { document.querySelectorAll('.sidebar-icons .icon-item').forEach(function (i) { i.classList.remove('active'); }); iconFor.classList.add('active'); }
        }

        // wire collapse toggle if exists
        var toggleBtn = document.getElementById('sidebarToggle');
        if (toggleBtn) toggleBtn.addEventListener('click', function () { toggleSidebarCollapsed(); });

        // create mobile backdrop and wire open/close
        var backdrop = document.createElement('div'); backdrop.id = 'sidebar-backdrop'; document.body.appendChild(backdrop);
        backdrop.addEventListener('click', function () { closeSidebarMobile(); });

        // open mobile via adding class to body button (if user has a hamburger elsewhere)
        var mobileOpenBtn = document.querySelector('[data-toggle="sidebar-mobile"]');
        if (mobileOpenBtn) mobileOpenBtn.addEventListener('click', function (e) { e.preventDefault(); openSidebarMobile(); });

        // perform initial zoom/viewport check
        checkZoomAndAdjust();
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', initSidebar); else initSidebar();
    window.populateCursos = populateCursos;

})();