document.addEventListener("DOMContentLoaded", function () {
    const chatIcon = document.getElementById("chatIcon");
    const iaChatModal = document.getElementById("iaChatModal");
    const closeChat = document.getElementById("closeChat");
    const sendMessage = document.getElementById("sendMessage");
    const userInput = document.getElementById("userInput");
    const chatContent = document.getElementById("chatContent");
    const chatModal = document.querySelector(".chat-modal");


    // Función para alternar el modal
    chatIcon.addEventListener("click", function (event) {
        event.preventDefault();
        chatModal.classList.toggle("active");
    });

    chatIcon.addEventListener("click", function (e) {
        e.preventDefault();
        iaChatModal.classList.add("active");
    });

    closeChat.addEventListener("click", function () {
        iaChatModal.classList.remove("active");
    });

    sendMessage.addEventListener("click", function () {
        sendUserMessage();
    });

    userInput.addEventListener("keypress", function (e) {
        if (e.key === "Enter") {
            sendUserMessage();
        }
    });

    // Cierra el modal al hacer clic fuera de él
    document.addEventListener("click", function (event) {
        if (!chatModal.contains(event.target) && !chatIcon.contains(event.target)) {
            chatModal.classList.remove("active");
        }
    });


    // Proxy requests to server-side endpoint to avoid exposing API keys in client
    async function sendMessageToGemini(message) {
        try {
            const response = await fetch('/api/IA/GenerarContenido', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    model: 'gemini-2.5-flash',
                    contents: [ { parts: [ { text: message } ] } ]
                })
            });

            const data = await response.json().catch(() => null);
            if (!response.ok) {
                console.error('Error desde servidor al solicitar Gemini:', response.status, data);
                return null;
            }

            // The server returns the raw response from Google; try to extract text
            if (data && data.candidates && data.candidates.length > 0) {
                return data.candidates[0].content.parts[0].text;
            }

            // Fallback: try common shapes
            if (data && data.output) return data.output;
            if (typeof data === 'string') return data;

            console.error('Respuesta inesperada de GenerarContenido:', data);
            return null;
        } catch (error) {
            console.error('Error al comunicar con el endpoint server /api/IA/GenerarContenido:', error);
            return null;
        }
    }


    function sendUserMessage() {
        const message = userInput.value;
        if (message.trim() === "") return;

        const userMessage = document.createElement("div");
        userMessage.classList.add("user-message");
        userMessage.textContent = message;
        chatContent.appendChild(userMessage);

        sendMessageToGemini(message).then(response => {
            if (response) {
                const botMessage = document.createElement("div");
                botMessage.classList.add("bot-message");
                botMessage.textContent = response;
                chatContent.appendChild(botMessage);
            } else {
                mostrarError();
            }

            chatContent.scrollTop = chatContent.scrollHeight;
        });

        userInput.value = "";
    }

    function mostrarError() {
        const botMessage = document.createElement("div");
        botMessage.classList.add("bot-message");
        botMessage.textContent = "Ocurrió un error al obtener la respuesta. Intenta de nuevo.";
        chatContent.appendChild(botMessage);
    }
});
