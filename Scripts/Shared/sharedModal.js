(function(global){
    // Reusable modal utility. Injects a single modal into body and exposes showSharedModal
    const MODAL_ID = 'sharedGenericModal';

    function ensureModalExists(){
        if (document.getElementById(MODAL_ID)) return;
        const html = `
        <div class="modal fade" id="${MODAL_ID}" tabindex="-1" aria-hidden="true">
          <div class="modal-dialog modal-lg">
            <div class="modal-content">
              <div class="modal-header"><h5 class="modal-title" id="${MODAL_ID}-title"></h5><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Cerrar"></button></div>
              <div class="modal-body" id="${MODAL_ID}-body"></div>
              <div class="modal-footer" id="${MODAL_ID}-footer"><button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button></div>
            </div>
          </div>
        </div>`;
        const div = document.createElement('div'); div.innerHTML = html;
        document.body.appendChild(div.firstElementChild);
    }

    function showSharedModal(options){
        // options: { title, bodyHtml, footerHtml, size }
        ensureModalExists();
        const titleEl = document.getElementById(MODAL_ID + '-title');
        const bodyEl = document.getElementById(MODAL_ID + '-body');
        const footerEl = document.getElementById(MODAL_ID + '-footer');
        titleEl.innerHTML = options.title || '';
        if (typeof options.bodyHtml === 'string') bodyEl.innerHTML = options.bodyHtml;
        else if (options.bodyHtml instanceof HTMLElement) { bodyEl.innerHTML = ''; bodyEl.appendChild(options.bodyHtml); }
        else bodyEl.innerHTML = '';
        if (options.footerHtml) footerEl.innerHTML = options.footerHtml; else footerEl.innerHTML = '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>';

        const modalEl = document.getElementById(MODAL_ID);
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
        return modal;
    }

    global.sharedModal = global.sharedModal || { show: showSharedModal };
})(window);
