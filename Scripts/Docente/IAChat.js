(function(){
    function appendMessage(role, text){
        var chat = document.getElementById('chatBox');
        var wrapper = document.createElement('div');
        wrapper.style.margin = '6px 0';
        // Ensure text is a string to avoid runtime errors when null/objects are passed
        var safeText = (text === null || text === undefined) ? '' : String(text);
        // Preserve newlines in the displayed HTML
        var htmlText = escapeHtml(safeText).replace(/\n/g, '<br>');

        if(role === 'user'){
            wrapper.style.textAlign = 'right';
            wrapper.innerHTML = '<div style="display:inline-block;background:#007bff;color:#fff;padding:8px;border-radius:8px;max-width:80%">'+htmlText+'</div>';
        } else {
            wrapper.style.textAlign = 'left';
            // For assistant messages remove common markdown decorations so text looks conversational
            wrapper.innerHTML = '<div style="display:inline-block;background:#f1f1f1;color:#000;padding:8px;border-radius:8px;max-width:80%">'+htmlText+'</div>';
        }
        chat.appendChild(wrapper);
        chat.scrollTop = chat.scrollHeight;
    }

    // Remove common markdown formatting so assistant responses appear as plain conversational text
    function sanitizeMarkdown(raw) {
        if (!raw) return '';
        var s = String(raw);
        // Remove fenced code blocks ```...```
        s = s.replace(/```[\s\S]*?```/g, '');
        // Remove inline code `...`
        s = s.replace(/`([^`]*)`/g, '$1');
        // Remove bold **text** and __text__
        s = s.replace(/\*\*(.*?)\*\*/g, '$1');
        s = s.replace(/__(.*?)__/g, '$1');
        // Remove italic *text* and _text_ (only when closed)
        s = s.replace(/\*(.*?)\*/g, '$1');
        s = s.replace(/_(.*?)_/g, '$1');
        // Remove markdown headers like ### Title
        s = s.replace(/^\s*#{1,6}\s*/gm, '');
        // Convert simple markdown lists to plain lines
        s = s.replace(/^-\s+/gm, '• ');
        s = s.replace(/^\d+\.\s+/gm, function(m){ return m; });
        // Collapse multiple blank lines
        s = s.replace(/\n{3,}/g, '\n\n');
        return s.trim();
    }

    function escapeHtml(unsafe) {
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/\"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    async function sendPrompt(prompt){
        console.log('sendPrompt called', prompt);
        appendMessage('user', prompt);
        var payload = { model: null, text: prompt };
        try{
            // Build API URL using appBase if provided (handles apps hosted under a virtual directory)
            var basePath = (typeof window.appBase === 'string' && window.appBase.length) ? window.appBase : '';
            // Ensure no trailing slash on basePath
            if (basePath.endsWith('/')) basePath = basePath.slice(0, -1);
            var apiUrl = basePath + '/api/IA/GenerarContenido';

            var resp = await fetch(apiUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if(!resp.ok){
                var txt = await resp.text();
                // Try to extract a friendly message from JSON responses
                try{
                    var parsed = JSON.parse(txt);
                    var friendly = parsed && (parsed.mensaje || parsed.message || parsed.error || parsed.detalle) ? (parsed.mensaje || parsed.message || parsed.error || parsed.detalle) : txt;
                    appendMessage('assistant', 'Error: ' + friendly);
                } catch(e){
                    appendMessage('assistant', 'Error: ' + txt);
                }
                return;
            }
            var data = await resp.json();
            // Try to extract text depending on wrapper
            var generated = '';
            try{
                if(data.candidates && data.candidates[0] && data.candidates[0].content && data.candidates[0].content.parts){
                    generated = data.candidates[0].content.parts.map(p=>p.text).join('\n');
                } else if(data.candidates && data.candidates[0] && data.candidates[0].content){
                    generated = JSON.stringify(data.candidates[0].content);
                } else if(data.detalle){
                    generated = data.detalle;
                } else {
                    generated = JSON.stringify(data);
                }
            } catch(e){ generated = JSON.stringify(data); }

            // sanitize assistant output to remove markdown-like decorations
            var cleaned = sanitizeMarkdown(generated);
            appendMessage('assistant', cleaned);
        } catch(e){
            appendMessage('assistant', 'Error interno: ' + e.message);
        }
    }

    document.addEventListener('DOMContentLoaded', function(){
        var sendBtn = document.getElementById('sendBtn');
        var input = document.getElementById('promptInput');
        sendBtn.addEventListener('click', function(){
            var v = input.value.trim();
            if(!v) return;
            input.value = '';
            sendPrompt(v);
        });
        input.addEventListener('keydown', function(e){
            if(e.key === 'Enter' && !e.shiftKey){
                e.preventDefault();
                sendBtn.click();
            }
        });
    });
})();
