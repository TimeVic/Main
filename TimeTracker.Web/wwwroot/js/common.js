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

window.isTextSelected = function () {
    return window.getSelection().toString().length > 0;
};

window.scrollHelper = {

    scrollToBottom: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) {
            el.scrollTop = el.scrollHeight;
        }
    },

    onScrollTopReached: function (elementId, dotNetObjRef) {
        const el = document.getElementById(elementId);
        if (!el) return;

        el.addEventListener("scroll", () => {
            if (el.scrollTop === 0) {
                dotNetObjRef.invokeMethodAsync("OnScrollTopReached");
            }
        });
    },

    hasScroll: function (elementId) {
        const el = document.getElementById(elementId);
        if (!el) return false;

        return el.scrollHeight > el.clientHeight;
    }
};

window.popupHelper = {
    portalAppend: function (element) {
        document.getElementById("tv-portal-root").appendChild(element);
    },

    portalRemove: function (element) {
        if (element && element.remove) element.remove();
    },

    addOutsideClick: function (dotnetRef, popupId) {
        function handler(e) {
            const popup = document.getElementById(popupId);
            if (!popup) return;

            if (!popup.contains(e.target)) {
                dotnetRef.invokeMethodAsync("Hide");
                document.removeEventListener("click", handler);
            }
        }

        setTimeout(() => document.addEventListener("click", handler), 10);
    },

    addEscapeClose: function (dotnetRef) {
        function handler(e) {
            if (e.key === "Escape") {
                dotnetRef.invokeMethodAsync("Hide");
                document.removeEventListener("keydown", handler);
            }
        }

        document.addEventListener("keydown", handler);
    },

    getRect: function (element) {
        return element.getBoundingClientRect();
    }
};
