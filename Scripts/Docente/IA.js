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


    const geminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key=AIzaSyAoXeYbMKSLVxFsAFgHF9rJOppK8Xz2txg";


    async function sendMessageToGemini(message) {
        try {
            const response = await fetch(geminiApiUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    contents: [
                        {
                            parts: [
                                {
                                    text: message
                                }
                            ]
                        }
                    ]
                })
            });

            if (response.ok) {
                const data = await response.json();
                console.log("Respuesta completa de la API:", data);

                // Ahora accedemos al texto correcto en la respuesta
                if (data.candidates && data.candidates.length > 0) {
                    const botResponse = data.candidates[0].content.parts[0].text; // Accediendo al texto
                    return botResponse;  // Retornamos el texto del modelo
                } else {
                    console.error("No se encontraron candidatos en la respuesta");
                    return null;
                }
            } else {
                const errorData = await response.json();
                console.error("Error en la respuesta de Gemini:", response.status, response.statusText, errorData);
                alert(`Error en la solicitud: ${errorData.error.message}`);
                return null;
            }
        } catch (error) {
            console.error("Error al comunicarse con la API de Gemini:", error);
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
