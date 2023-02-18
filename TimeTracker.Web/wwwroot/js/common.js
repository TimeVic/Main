window.clickOnElement = function (element) {
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

window.setFavicon = function (fileName) {
    const element = document.querySelectorAll('[rel=icon]')[0];
    element.setAttribute('href', `/img/logo/${fileName}`)
};

window.openFile = function(data) {
    var link = this.document.createElement('a');
    link.download = data.fileName;
    link.href = data.url;
    link.target ="_blank";
    this.document.body.appendChild(link);
    link.click();
    this.document.body.removeChild(link);
}
