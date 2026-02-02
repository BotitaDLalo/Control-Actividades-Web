(function (global) {
    // Simple HTTP service wrapper using fetch
    // Exposes httpService with methods: get, post, put, del

    function buildQueryString(params) {
        if (!params) return '';
        var esc = encodeURIComponent;
        var query = Object.keys(params)
            .filter(function (k) { return params[k] !== undefined && params[k] !== null; })
            .map(function (k) { return esc(k) + '=' + esc(params[k]); })
            .join('&');
        return query ? ('?' + query) : '';
    }

    async function request(method, url, options) {
        options = options || {};
        var params = options.params || null;
        var body = options.body;
        var headers = options.headers || {};

        var finalUrl = url + buildQueryString(params);

        var fetchOptions = {
            method: method,
            headers: headers,
            credentials: 'same-origin'
        };

        if (body !== undefined && body !== null) {
            // If body is FormData, let it be; otherwise JSON stringify
            if (body instanceof FormData) {
                fetchOptions.body = body;
                // Let browser set content-type for FormData
            } else {
                if (!fetchOptions.headers['Content-Type'] && !fetchOptions.headers['content-type']) {
                    fetchOptions.headers['Content-Type'] = 'application/json; charset=utf-8';
                }
                fetchOptions.body = typeof body === 'string' ? body : JSON.stringify(body);
            }
        }

        var resp = await fetch(finalUrl, fetchOptions);

        var contentType = resp.headers.get('content-type') || '';

        if (!resp.ok) {
            var err = new Error('HTTP error ' + resp.status);
            err.status = resp.status;
            // try to parse body for error details
            try {
                if (contentType.indexOf('application/json') !== -1) {
                    err.body = await resp.json();
                } else {
                    err.body = await resp.text();
                }
            } catch (e) {
                // ignore
            }
            throw err;
        }

        if (contentType.indexOf('application/json') !== -1) {
            return resp.json();
        }

        return resp.text();
    }

    var httpService = {
        get: function (url, params, headers) {
            return request('GET', url, { params: params, headers: headers });
        },
        post: function (url, body, params, headers) {
            return request('POST', url, { body: body, params: params, headers: headers });
        },
        put: function (url, body, params, headers) {
            return request('PUT', url, { body: body, params: params, headers: headers });
        },
        del: function (url, params, headers) {
            return request('DELETE', url, { params: params, headers: headers });
        }
    };

    global.httpService = global.httpService || httpService;
})(window);
