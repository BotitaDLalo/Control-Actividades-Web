import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.11/firebase-app.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/9.6.11/firebase-messaging.js";

const firebaseConfig = {
    apiKey: "AIzaSyAU0lEbkXSDObxI6vpGkGY1ZcsJwMku_ns",
    authDomain: "push-notification-9bc5f.firebaseapp.com",
    projectId: "push-notification-9bc5f",
    messagingSenderId: "700503342964",
    appId: "1:700503342964:web:6469fdcd5fbe2b160c3621"
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
                vapidKey: "BLzbaIQqIsTJ1vti9QsOh_qintrrCFYSnWcUpREM0QscujnnT-X61W51S-awt0534HKFSyYdPgUU8xNCMhvIHDg",
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

    //Mostrar notificación
    new Notification(payload.notification.title, {
        body: payload.notification.body
    });

});