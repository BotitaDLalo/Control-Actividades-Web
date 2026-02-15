//document.addEventListener('DOMContentLoaded', function () {

//    const toggleBtn = document.getElementById('cambiarModo');
//    const root = document.documentElement;
//    const iconPath = document.getElementById('icono-path');
//    const THEME_KEY = 'theme';

//    if (!toggleBtn || !iconPath) {
//        console.error('Botón o icono no encontrado');
//        return;
//    }

//    const ICONO_SOL =
//        "M8 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8z" +
//        "M8 0a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-1 0v-1A.5.5 0 0 1 8 0z" +
//        "M8 13.5a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-1 0v-1a.5.5 0 0 1 .5-.5z" +
//        "M2.343 2.343a.5.5 0 0 1 .707 0l.708.708a.5.5 0 0 1-.708.708l-.707-.708a.5.5 0 0 1 0-.708z" +
//        "M11.95 11.95a.5.5 0 0 1 .707 0l.708.708a.5.5 0 1 1-.708.708l-.707-.707a.5.5 0 0 1 0-.708z" +
//        "M0 8a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 0 1h-1A.5.5 0 0 1 0 8z" +
//        "M13.5 8a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 0 1h-1a.5.5 0 0 1-.5-.5z" +
//        "M2.343 13.657a.5.5 0 0 1 0-.707l.708-.708a.5.5 0 1 1 .707.708l-.707.707a.5.5 0 0 1-.708 0z" +
//        "M11.95 4.05a.5.5 0 0 1 0-.707l.708-.708a.5.5 0 1 1 .707.708l-.707.707a.5.5 0 0 1-.708 0z";

//    const ICONO_LUNA =
//        "M6 0a6 6 0 1 0 6 6A5 5 0 0 1 6 0z";

//    const savedTheme = localStorage.getItem(THEME_KEY);

//    if (savedTheme === 'dark') {
//        root.setAttribute('data-theme', 'dark');
//        iconPath.setAttribute('d', ICONO_LUNA);
//    } else {
//        iconPath.setAttribute('d', ICONO_SOL);
//    }

//    toggleBtn.addEventListener('click', function () {
//        const isDark = root.getAttribute('data-theme') === 'dark';

//        if (isDark) {
//            root.removeAttribute('data-theme');
//            localStorage.setItem(THEME_KEY, 'light');
//            iconPath.setAttribute('d', ICONO_SOL);
//        } else {
//            root.setAttribute('data-theme', 'dark');
//            localStorage.setItem(THEME_KEY, 'dark');
//            iconPath.setAttribute('d', ICONO_LUNA);
//        }
//    });

//});
