document.addEventListener('DOMContentLoaded', function () {
    const btnCambiar = document.getElementById('btnCambiarCodigo');
    if (!btnCambiar) return;

    btnCambiar.addEventListener('click', async function () {
        try {
            // Validate materiaIdGlobal
            if (typeof materiaIdGlobal === 'undefined' || materiaIdGlobal === null || materiaIdGlobal === '0' || materiaIdGlobal === 0) {
                Swal.fire({ icon: 'error', title: 'Materia no identificada', text: 'No se pudo identificar la materia actual.' });
                return;
            }

            const confirm = await Swal.fire({
                title: '\u00BFEst\u00E1 seguro?',
                text: 'Se generar\u00E1 autom\u00E1ticamente un nuevo c\u00F3digo de clase. \u00BFDesea continuar?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'S\u00ED, generar c\u00F3digo',
                cancelButtonText: 'No, cancelar'
            });

            if (!confirm.isConfirmed) return;

            const resp = await fetch(`/Materias/CambiarCodigoAuto?materiaId=${encodeURIComponent(materiaIdGlobal)}`, { method: 'POST' });

            if (!resp.ok) {
                const text = await resp.text().catch(() => '');
                throw new Error(text || 'No se pudo cambiar el c\u00F3digo.');
            }

            const data = await resp.json().catch(() => null);
            const nuevo = (data && (data.CodigoAcceso || data.codigoAcceso)) || '';

            if (nuevo) {
                const el = document.getElementById('codigoAcceso');
                if (el) el.innerText = nuevo;
            }

            Swal.fire({ icon: 'success', title: 'C\u00F3digo actualizado', text: nuevo, showConfirmButton: false, timer: 1400 });
        } catch (err) {
            console.error('Error al cambiar código de materia:', err);
            Swal.fire({ icon: 'error', title: 'Error', text: err.message || 'No se pudo cambiar el c\u00F3digo' });
        }
    });
});
