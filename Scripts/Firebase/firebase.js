import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.11/firebase-app.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/9.6.11/firebase-messaging.js";

const firebaseConfig = {
    apiKey: "AIzaSyBAZnCbjlXGteAcLSZRV2soNVA3L4fHNpA",
    authDomain: "apptokens-dc835.firebaseapp.com",
    projectId: "apptokens-dc835",
    messagingSenderId: "309526870092",
    appId: "1:309526870092:web:551aef71e9dfb58c21dce9"
};

const app = initializeApp(firebaseConfig);

export const messaging = getMessaging(app);

export async function registrarTokenFcm() {
        try {
            console.log("Solicitando permisos...");

            const permission = await Notification.requestPermission();

            if (permission !== "granted") {
                console.warn("Permisos no concedidos:", permission);
                return;
            }

            const reg = await navigator.serviceWorker.register('/firebase-messaging-sw.js');

            console.log("Consiguiendo token...");
            const token = await getToken(messaging, {
                vapidKey: "BEFhmn2Q7-Bz2ORaOl2WPtJr3KHehzLXMctKFNbf1ohL0A8H55RvDTYvyS37whGFUPkNJA2fPP63p18QmTuYc8U",
                serviceWorkerRegistration: reg
            });

            if (!token) {
                console.log("No se pudo obtener el token FCM." + err); 
                return;
            } 

            console.log("El token está listo: " + token + ". Vamos a registrarlo");
            $.ajax({
                url: '/api/Notificaciones/RegistrarToken',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ Token: token }),
                success: function (r) {
                    console.log("Respuesta servidor:", r);
                },
                error: function (err) {
                    console.error("Error server:", err.responseText);
                }
            });
            
        } catch (err) {
            console.error("Error obteniendo token:", err);
        }
}

// Escuchar mensajes en primer plano
onMessage( messaging, (payload) => {
    console.log("Mensaje recibido en foreground:", payload);

    new Notification(payload.notification.title, {
        body: payload.notification.body
    });
});