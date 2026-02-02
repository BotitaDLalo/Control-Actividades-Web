(function (global) {
    // materiasService wraps calls to /api/Materias and centralizes view param
    // Usage: materiasService.getMaterias({ view: 'WEB' }).then(...)

    var baseUrl = '/api/Materias';

    function ensureViewParam(opts) {
        opts = opts || {};
        opts.params = opts.params || {};
        if (!opts.params.view) {
            // default to WEB
            opts.params.view = 'WEB';
        }
        return opts;
    }

    var materiasService = {
        getMaterias: function (opts) {
            opts = ensureViewParam(opts);
            return window.httpService.get(baseUrl + '/ObtenerMaterias', opts.params, opts.headers);
        },
        getMateria: function (id, opts) {
            opts = ensureViewParam(opts);
            opts.params.id = id;
            return window.httpService.get(baseUrl + '/ObtenerMateriaUnica', opts.params, opts.headers);
        },
        crearMateria: function (materiaDto, opts) {
            opts = ensureViewParam(opts);
            return window.httpService.post(baseUrl + '/CrearMateriaSinGrupo', materiaDto, opts.params, opts.headers);
        },
        updateMateria: function (updateDto, opts) {
            opts = ensureViewParam(opts);
            return window.httpService.put(baseUrl + '/UpdateSubject', updateDto, opts.params, opts.headers);
        },
        deleteMateria: function (id, opts) {
            opts = ensureViewParam(opts);
            return window.httpService.del(baseUrl + '/DeleteSubject/' + encodeURIComponent(id), opts.params, opts.headers);
        }
    };

    global.materiasService = global.materiasService || materiasService;
})(window);
