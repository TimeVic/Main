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

messaging.getToken({ vapidKey: 'BIyPXPr213LclQcFJgg3IfVOyLFDfBS8GV4f3LrHNEmckpWVMPq6lcUpcEByLacUReItPkO4eiw-uhdC2IFQ0lg' })
    .then((currentToken) => {
        console.info('GCM Token ', currentToken);
    }).catch((err) => {
        console.log('An error occurred while retrieving token. ', err);
    });
messaging.onMessage((payload) => {
    console.log('Message received: ', payload);
    new Notification(payload.notification.title, payload.notification);
});
console.log(messaging)
