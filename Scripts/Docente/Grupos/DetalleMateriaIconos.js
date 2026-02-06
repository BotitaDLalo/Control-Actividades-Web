var div = document.getElementById("docente-datos");
var docenteIdGlobal = div && div.dataset ? div.dataset.docenteid : null;

function copiarCodigoAcceso() {
    try {
        const codigoElemento = document.getElementById("codigoAcceso");
        if (!codigoElemento) return;
        const codigo = (codigoElemento.innerText || codigoElemento.textContent || "").trim();
        if (!codigo) return;

        // Prefer navigator.clipboard when available (secure, async)
        if (navigator.clipboard && typeof navigator.clipboard.writeText === 'function') {
            navigator.clipboard.writeText(codigo).then(() => {
                _feedbackCopySuccess();
            }).catch(() => {
                // fallback
                _fallbackCopy(codigo);
            });
        } else {
            _fallbackCopy(codigo);
        }
    } catch (e) {
        console.error('Error copiando código de acceso', e);
    }
}

function _fallbackCopy(text) {
    try {
        const inputTemp = document.createElement('input');
        inputTemp.style.position = 'fixed';
        inputTemp.style.left = '-10000px';
        inputTemp.value = text;
        document.body.appendChild(inputTemp);
        inputTemp.select();
        inputTemp.setSelectionRange(0,99999);
        document.execCommand('copy');
        document.body.removeChild(inputTemp);
        _feedbackCopySuccess();
    } catch (e) {
        console.error('Fallback copy failed', e);
    }
}

function _feedbackCopySuccess() {
    // Try to find an icon inside the header/button to toggle; if not present, flash the button text
    var icono = document.querySelector('.copiar-icono') || document.querySelector('#codigoAcceso + .copiar-icono') || null;
    var btn = null;
    // find any nearby button that calls copiarCodigoAcceso
    try {
        btn = Array.from(document.querySelectorAll('button')).find(b => (b.getAttribute && b.getAttribute('onclick') || '').indexOf('copiarCodigoAcceso') !== -1) || null;
    } catch (e) { btn = null; }

    if (icono) {
        try {
            icono.classList.remove('fa-copy');
            icono.classList.add('fa-check');
            setTimeout(function () {
                icono.classList.remove('fa-check');
                icono.classList.add('fa-copy');
            },1600);
            return;
        } catch (e) { /* continue to button fallback */ }
    }

    if (btn) {
        var orig = btn.innerHTML;
        try {
            btn.innerHTML = '<span style="display:inline-block; color:green; font-weight:600;">Copiado ✓</span>';
            setTimeout(function () { btn.innerHTML = orig; },1600);
            return;
        } catch (e) { /* noop */ }
    }

    // last resort: tiny toast using alert (rare)
    try { console.info('Código copiado al portapapeles'); } catch (e) { }
}
