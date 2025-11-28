importScripts("https://www.gstatic.com/firebasejs/9.6.11/firebase-app-compat.js");
importScripts("https://www.gstatic.com/firebasejs/9.6.11/firebase-messaging-compat.js");

const firebaseConfig = {
    apiKey: "AIzaSyAU0lEbkXSDObxI6vpGkGY1ZcsJwMku_ns",
    authDomain: "push-notification-9bc5f.firebaseapp.com",
    projectId: "push-notification-9bc5f",
    messagingSenderId: "700503342964",
    appId: "1:700503342964:web:6469fdcd5fbe2b160c3621"
};

firebase.initializeApp(firebaseConfig);
const messaging = firebase.messaging();

messaging.onBackgroundMessage((payload) => {
    console.log("Mensaje recibido en background:", payload);

    self.registration.showNotification(
        payload.notification.title,{
            body: payload.notification.body,
            icon: "/firebase-logo.png"}
    );
});