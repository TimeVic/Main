window.clickOnElement = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.click()
    }
};

window.saveAsFile = function(filename, bytesBase64) {
    const link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + bytesBase64;
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
};

window.getReCaptchaToken = function (siteKey) {
    return new Promise((resolve, reject) => {
        grecaptcha.ready(function () {
            grecaptcha.execute(siteKey, { action: 'submit' }).then(function (token) {
                resolve(token);
            });
        });
    });
};

window.setFavicon = function (isColor) {
    const element = document.querySelectorAll('[rel=icon]')[0];
    const dir = isColor ? 'color' : 'black';
    element.setAttribute('href', `/img/logo/${dir}/clock-256.png`)
};
