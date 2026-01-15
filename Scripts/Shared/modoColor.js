const toggleBtn = document.getElementById('cambiarModo');

toggleBtn.addEventListener('click', () => {
    const root = document.documentElement;

    if (root.getAttribute('data-theme') === 'dark') {
        root.removeAttribute('data-theme');
    } else {
        root.setAttribute('data-theme', 'dark');
    }
});