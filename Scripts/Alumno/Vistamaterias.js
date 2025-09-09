function cambiarVista(vista) {
    // Remover clase "active" de todas las pestañas y vistas
    document.querySelectorAll('.materia-tab-unique').forEach(tab => tab.classList.remove('active'));
    document.querySelectorAll('.materia-view-unique').forEach(view => view.classList.remove('active'));

    // Agregar clase "active" a la pestaña y vista seleccionada
    document.querySelector(`[onclick="cambiarVista('${vista}')"]`).classList.add('active');
    document.getElementById(vista).classList.add('active');
}
