importScripts("https://www.gstatic.com/firebasejs/9.6.11/firebase-app-compat.js");
importScripts("https://www.gstatic.com/firebasejs/9.6.11/firebase-messaging-compat.js");

const firebaseConfig = {
    apiKey: "AIzaSyBAZnCbjlXGteAcLSZRV2soNVA3L4fHNpA",
    authDomain: "apptokens-dc835.firebaseapp.com",
    projectId: "apptokens-dc835",
    messagingSenderId: "309526870092",
    appId: "1:309526870092:web:551aef71e9dfb58c21dce9"
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