importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-app-compat.js')
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging-compat.js')

// Your web app's Firebase configuration
// For Firebase JS SDK v7.20.0 and later, measurementId is optional
let firebaseConfig = {
    apiKey: "AIzaSyB31CZJYQgLM0vgPdsQTVuqOMFynf433os",
    authDomain: "timevic-374620.firebaseapp.com",
    projectId: "timevic-374620",
    storageBucket: "timevic-374620.appspot.com",
    messagingSenderId: "200936816986",
    appId: "1:200936816986:web:80222ba1991f5b5196b363",
    measurementId: "G-KZKP55D0BE"
};

let app = firebase.initializeApp(firebaseConfig);
let messaging = firebase.messaging();
