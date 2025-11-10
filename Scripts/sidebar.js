// Manejo del sidebar: toggle, navegación por íconos, y accordion de secciones
(function () {
    var sidebar = document.getElementById('appSidebar');
    var toggle = document.getElementById('sidebarToggle');
    var toggleTop = document.getElementById('sidebarToggleTop');
    var content = document.getElementById('sidebarContent');
    var icons = document.querySelectorAll('.sidebar-icons .icon-item');
    var titles = document.querySelectorAll('.menu-title');

    if (!sidebar) return;

    // Cargar estado (colapsado o no)
    var collapsed = localStorage.getItem('sidebar.collapsed') === 'true';
    if (collapsed) sidebar.classList.add('collapsed');

    function setCollapsed(c) {
        if (c) sidebar.classList.add('collapsed'); else sidebar.classList.remove('collapsed');
        localStorage.setItem('sidebar.collapsed', !!c);
        // ajustar main margin si existe
        var main = document.querySelector('.app-main');
        if (main) main.classList.toggle('sidebar-collapsed', c);
    }

    toggle && toggle.addEventListener('click', function () { setCollapsed(!sidebar.classList.contains('collapsed')); });
    if (toggleTop) toggleTop.addEventListener('click', function () { setCollapsed(!sidebar.classList.contains('collapsed')); });

    // click en iconos: abrir la sección correspondiente
    icons.forEach(function (it) {
        it.addEventListener('click', function () {
            icons.forEach(function(i){ i.classList.remove('active'); });
            it.classList.add('active');
            var target = it.getAttribute('data-target');
            // mostrar solo la sección seleccionada
            var sections = document.querySelectorAll('.menu-section');
            sections.forEach(function (s) { s.classList.remove('open'); s.style.display = 'none'; s.querySelectorAll('.menu-title')[0].setAttribute('aria-expanded','false'); });
            var el = document.getElementById(target);
            if (el) { el.style.display = 'block'; el.classList.add('open'); el.querySelectorAll('.menu-title')[0].setAttribute('aria-expanded','true'); }
            // expandir si está colapsado
            if (sidebar.classList.contains('collapsed')) setCollapsed(false);
        });
        it.addEventListener('keydown', function(e){ if(e.key === 'Enter' || e.key === ' ') { it.click(); e.preventDefault(); }});
    });

    // accordion titles: toggle open class
    titles.forEach(function (t) {
        t.addEventListener('click', function () {
            var parent = t.parentElement;
            var isOpen = parent.classList.contains('open');
            parent.classList.toggle('open');
            t.setAttribute('aria-expanded', parent.classList.contains('open'));
        });
        t.addEventListener('keydown', function(e){ if(e.key === 'Enter' || e.key === ' ') { t.click(); e.preventDefault(); }});
    });

    // Inicial: mostrar última sección seleccionada o primera
    document.addEventListener('DOMContentLoaded', function () {
        var last = localStorage.getItem('sidebar.lastSection') || null;
        var targetEl = null;
        if (last) targetEl = document.getElementById(last);
        if (!targetEl) targetEl = document.querySelector('.menu-section');
        if (targetEl) {
            document.querySelectorAll('.menu-section').forEach(function (s) { s.style.display = 'none'; s.classList.remove('open'); s.querySelectorAll('.menu-title')[0].setAttribute('aria-expanded','false'); });
            targetEl.style.display = 'block'; targetEl.classList.add('open'); targetEl.querySelectorAll('.menu-title')[0].setAttribute('aria-expanded','true');
            var iconFor = document.querySelector('.sidebar-icons .icon-item[data-target="'+targetEl.id+'"]');
            if (iconFor) { document.querySelectorAll('.sidebar-icons .icon-item').forEach(function(i){i.classList.remove('active');}); iconFor.classList.add('active'); }
        }

        // guardar cuando se cambia la sección (observador simple)
        document.querySelectorAll('.menu-section').forEach(function(s){ s.addEventListener('transitionend', function(){ if (s.classList.contains('open')) localStorage.setItem('sidebar.lastSection', s.id); }); });
    });
})();