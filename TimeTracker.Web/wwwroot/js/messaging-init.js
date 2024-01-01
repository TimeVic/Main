// Import the functions you need from the SDKs you need
import { initializeApp } from "https://www.gstatic.com/firebasejs/9.6.7/firebase-app.js";
import { getAnalytics } from "https://www.gstatic.com/firebasejs/9.6.7/firebase-analytics.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/9.6.7/firebase-messaging.js";
// TODO: Add SDKs for Firebase products that you want to use
// https://firebase.google.com/docs/web/setup#available-libraries

Notification.requestPermission().then(async (permission) => {
    if (permission === 'granted') {
        console.info('Notifications is allowed');
    } else {
        console.info('Notifications is not allowed');
    }
});

// Your web app's Firebase configuration
// For Firebase JS SDK v7.20.0 and later, measurementId is optional
const firebaseConfig = {
    apiKey: "AIzaSyB31CZJYQgLM0vgPdsQTVuqOMFynf433os",
    authDomain: "timevic-374620.firebaseapp.com",
    projectId: "timevic-374620",
    storageBucket: "timevic-374620.appspot.com",
    messagingSenderId: "200936816986",
    appId: "1:200936816986:web:4bd65df47211471c96b363",
    measurementId: "G-SMS5FSN017"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
console.log(getMessaging)
const analytics = getAnalytics(app);
const messaging = getMessaging(app);

export function getGcmToken() {
    return new Promise((resolve) => {
        getToken(messaging, { vapidKey: 'BIyPXPr213LclQcFJgg3IfVOyLFDfBS8GV4f3LrHNEmckpWVMPq6lcUpcEByLacUReItPkO4eiw-uhdC2IFQ0lg' })
            .then((currentToken) => {
                resolve(currentToken);
            }).catch((err) => {
                console.log('An error occurred while retrieving token. ', err);
            });    
    });
}

await getGcmToken()
    .then(token => {
        console.info('Received token: ' + token)
    });

onMessage(messaging, function(payload) {
    console.log('Message received. ', payload);
    new Notification(payload.notification.title, payload.notification);
});
