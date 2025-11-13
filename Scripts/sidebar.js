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

        // Adjust root font-size so UI scales inversely to page zoom (so sidebar doesn't cover content)
        try {
            // compute percent scale: when DPR >1 (zoom in) we reduce font-size, when DPR <1 (zoom out) we increase
            var scalePercent = Math.round((1 / dpr) * 100);
            // clamp to avoid extreme sizes
            var minPct = 75; // 75%
            var maxPct = 120; // 120%
            if (scalePercent < minPct) scalePercent = minPct;
            if (scalePercent > maxPct) scalePercent = maxPct;
            if (Math.abs(scalePercent - 100) <= 2) {
                // near default, remove inline override to keep browser default behavior
                document.documentElement.style.fontSize = '';
                delete document.documentElement.dataset.uiScale;
            } else {
                document.documentElement.style.fontSize = scalePercent + '%';
                document.documentElement.dataset.uiScale = scalePercent;
            }
        } catch (e) {
            // silent fail if any environment blocks changing root font size
            console.warn('Zoom scaling adjust failed', e);
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
        // find our section buttons and section containers matching the markup in _Sidebar.cshtml
        var toggles = Array.prototype.slice.call(document.querySelectorAll('.section-btn'));
        var sections = Array.prototype.slice.call(document.querySelectorAll('.nav-section'));

        if (!toggles.length || !sections.length) {
            console.warn('Sidebar: no section buttons or nav sections found');
            return;
        }

        function closeAllExcept(targetId) {
            sections.forEach(function (s) {
                var submenu = s.querySelector('.section-submenu');
                var btn = s.querySelector('.section-btn');
                if (!submenu || !btn) return;
                if (targetId && s.id === targetId) return;
                s.classList.remove('open');
                submenu.setAttribute('aria-hidden', 'true');
                btn.setAttribute('aria-expanded', 'false');
                try { s.dataset.manual = 'false'; } catch (e) { /* ignore */ }
            });
        }

        toggles.forEach(function (btn) {
            btn.addEventListener('click', function (ev) {
                ev.preventDefault();
                // find the parent nav-section for this button
                var parentSection = btn.closest('.nav-section');
                if (!parentSection) return;
                var submenu = parentSection.querySelector('.section-submenu');
                if (!submenu) return;

                var isOpen = parentSection.classList.contains('open');
                if (isOpen) {
                    // close
                    parentSection.classList.remove('open');
                    parentSection.querySelector('.section-btn').setAttribute('aria-expanded', 'false');
                    submenu.setAttribute('aria-hidden', 'true');
                    try { parentSection.dataset.manual = 'false'; } catch (e) { }
                } else {
                    // open this and close others
                    closeAllExcept(parentSection.id);
                    parentSection.classList.add('open');
                    parentSection.querySelector('.section-btn').setAttribute('aria-expanded', 'true');
                    submenu.setAttribute('aria-hidden', 'false');
                    try { parentSection.dataset.manual = 'true'; } catch (e) { }
                }
            });

            btn.addEventListener('keydown', function (e) { if (e.key === 'Enter' || e.key === ' ') { btn.click(); e.preventDefault(); } });
        });

        // hover behaviour for desktop: show submenu on hover if not manually opened
        if (!('ontouchstart' in window) && window.innerWidth > 768) {
            sections.forEach(function (sec) {
                var submenu = sec.querySelector('.section-submenu');
                var btn = sec.querySelector('.section-btn');
                if (!submenu || !btn) return;
                sec.addEventListener('mouseenter', function () {
                    if (sec.dataset.manual === 'true') return;
                    closeAllExcept(sec.id);
                    sec.classList.add('open');
                    submenu.setAttribute('aria-hidden', 'false');
                    btn.setAttribute('aria-expanded', 'true');
                });
                sec.addEventListener('mouseleave', function () {
                    if (sec.dataset.manual === 'true') return;
                    sec.classList.remove('open');
                    submenu.setAttribute('aria-hidden', 'true');
                    btn.setAttribute('aria-expanded', 'false');
                });
            });
        }

        // initial open state: restore last opened section or open first
        var last = null; try { last = localStorage.getItem('sidebar.lastSection'); } catch (e) { last = null; }
        var targetSection = null;
        if (last) targetSection = document.getElementById(last);
        if (!targetSection) {
            // pick first section's submenu
            var firstSection = sections[0];
            if (firstSection) targetSection = firstSection;
        }

        // close all, then open the chosen
        sections.forEach(function (s) { var sub = s.querySelector('.section-submenu'); if (sub) { sub.setAttribute('aria-hidden', 'true'); } s.classList.remove('open'); var b = s.querySelector('.section-btn'); if (b) b.setAttribute('aria-expanded', 'false'); try { s.dataset.manual = 'false'; } catch (e) {} });
        if (targetSection) {
            var parent = (targetSection.classList && targetSection.classList.contains('nav-section')) ? targetSection : targetSection.closest('.nav-section');
            if (parent) {
                parent.classList.add('open');
                var pb = parent.querySelector('.section-btn'); if (pb) pb.setAttribute('aria-expanded', 'true');
                var ps = parent.querySelector('.section-submenu'); if (ps) ps.setAttribute('aria-hidden', 'false');
                try { parent.dataset.manual = 'true'; } catch (e) {}
            }
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

})();})();