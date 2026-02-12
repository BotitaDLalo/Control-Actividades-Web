// Small shared helpers used by multiple views
window.CommonHelpers = window.CommonHelpers || {};
(function(ns){
    ns.escapeHtml = function (s) {
        if (!s) return '';
        return String(s).replace(/[&<>\"'`]/g,
            function (m) {
                return ({
                    '&': '&amp;',
                    '<': '&lt;',
                    '>': '&gt;',
                    '"': '&quot;',
                    "'": '&#39;',
                    '`': '&#96;'
                })[m];
            });
    };

    ns.parseServerDate = function(dateVal){
        if (!dateVal) return null;
        if (dateVal instanceof Date) return dateVal;
        var s = String(dateVal).trim();
        var msMatch = s.match(/\/Date\((-?\d+)(?:[-+][0-9]+)?\)\/?/);
        if (msMatch) {
            var ms = parseInt(msMatch[1], 10);
            if (!isNaN(ms)) return new Date(ms);
        }

        if (/^\d+$/.test(s)) return new Date(parseInt(s,10));

        var d = new Date(s);

        if (!isNaN(d.getTime())) return d;

        var d2 = new Date(s.replace(' ', 'T'));

        if (!isNaN(d2.getTime())) return d2;

        var parsed = Date.parse(s);

        if (!isNaN(parsed)) return new Date(parsed);

        return null;
    };

    ns.formatDateToLocale = function (dateVal) {
        var d = ns.parseServerDate(dateVal);
        if (!d) return (dateVal ? String(dateVal) : 'No disponible');
        try {
            return d.toLocaleString('es-ES');
        }
        catch (e) {
            return d.toString();
        }
    };

    ns.copyText = function(text){
        if (!text) return false;
        if (navigator.clipboard) {
            navigator.clipboard.writeText(text).catch(function () {
                /* ignore */
            }); return true;
        }

        var ta = document.createElement('textarea');
        ta.value = text;
        document.body.appendChild(ta);
        ta.select();
        try {
            document.execCommand('copy');
        } catch (e) { } ta.remove();
        return true;
    };

})(window.CommonHelpers);
