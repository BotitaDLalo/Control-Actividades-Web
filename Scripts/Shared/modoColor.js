const toggleBtn = document.getElementById('cambiarModo');
const icono = document.getElementById('iconoModo');
const root = document.documentElement;
const THEME_KEY = 'theme';

/*Íconos*/
const ICONO_CLARO = '/Content/Iconos/icono-modo-claro.svg';
const ICONO_OSCURO = '/Content/Iconos/icono-modo-oscuro.svg';

/*Cuando la página carga*/
const savedTheme = localStorage.getItem(THEME_KEY);

if (savedTheme === 'dark') {
    root.setAttribute('data-theme', 'dark');
    if (icono) icono.src = ICONO_OSCURO;
} else {
    if (icono) icono.src = ICONO_CLARO;
}

if (toggleBtn) {
    toggleBtn.addEventListener('click', () => {
        const isDark = root.getAttribute('data-theme') === 'dark';

        if (isDark) {
            root.removeAttribute('data-theme');
            localStorage.setItem(THEME_KEY, 'light');
            if (icono) icono.src = ICONO_CLARO;
        } else {
            root.setAttribute('data-theme', 'dark');
            localStorage.setItem(THEME_KEY, 'dark');
            if (icono) icono.src = ICONO_OSCURO;
        }
    });
} else {
    // elemento no presente en esta vista: no-op
}