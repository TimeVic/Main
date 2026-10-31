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
    element.setAttribute('href', fileName)
};

window.setDocumentLanguage = function (language) {
    document.documentElement.lang = language || "en";
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

window.getTextAreaState = function (editor) {
    return {
        value: editor.value,
        selectionStart: editor.selectionStart,
        selectionEnd: editor.selectionEnd
    };
};

window.setTextAreaSelection = function (editor, selectionStart, selectionEnd) {
    editor.focus();
    editor.setSelectionRange(selectionStart, selectionEnd);
};

window.popupPortal = {
    showPopover: function (element) {
        if (!element || typeof element.showPopover !== "function" || element.matches(":popover-open")) {
            return;
        }

        element.showPopover();
    },

    getPanelStyle: function (anchor, panelWidth, estimatedPanelHeight, offset, viewportPadding) {
        if (!anchor) {
            return "";
        }

        const rect = anchor.getBoundingClientRect();
        const width = Math.min(panelWidth, window.innerWidth - viewportPadding * 2);
        const left = Math.min(
            Math.max(rect.right - width, viewportPadding),
            window.innerWidth - width - viewportPadding
        );

        const spaceBelow = window.innerHeight - rect.bottom - viewportPadding;
        const isOpenAbove = spaceBelow < estimatedPanelHeight && rect.top > spaceBelow;
        const top = isOpenAbove
            ? Math.max(rect.top - estimatedPanelHeight - offset, viewportPadding)
            : Math.min(rect.bottom + offset, window.innerHeight - viewportPadding);

        return `left:${left}px;top:${top}px;width:${width}px;max-width:calc(100vw - ${viewportPadding * 2}px);`;
    }
};

window.openInNewTab = function (url) {
    window.open(url, "_blank");
}


window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        return false;
    }
};

async function convertBlobToPng(blob) {
    if (blob.type === "image/png") {
        return blob;
    }

    return new Promise((resolve, reject) => {
        const objectUrl = URL.createObjectURL(blob);
        const img = new Image();
        img.crossOrigin = "anonymous";

        img.onload = () => {
            try {
                let width = img.naturalWidth || img.width;
                let height = img.naturalHeight || img.height;

                // Handle SVGs or images without explicit dimensions
                if (!width || !height) {
                    width = 512;
                    height = 512;
                }

                const canvas = document.createElement("canvas");
                canvas.width = width;
                canvas.height = height;

                const ctx = canvas.getContext("2d");
                if (!ctx) {
                    URL.revokeObjectURL(objectUrl);
                    reject(new Error("Unable to create canvas 2d context"));
                    return;
                }

                ctx.drawImage(img, 0, 0, width, height);
                canvas.toBlob((pngBlob) => {
                    URL.revokeObjectURL(objectUrl);
                    if (pngBlob) {
                        resolve(pngBlob);
                    } else {
                        reject(new Error("Failed to convert image to PNG blob"));
                    }
                }, "image/png");
            } catch (err) {
                URL.revokeObjectURL(objectUrl);
                reject(err);
            }
        };

        img.onerror = () => {
            URL.revokeObjectURL(objectUrl);
            reject(new Error("Failed to load image for clipboard conversion"));
        };

        img.src = objectUrl;
    });
}

window.copyImageToClipboard = async (url) => {
    try {
        if (!navigator.clipboard?.write || typeof ClipboardItem === "undefined") {
            console.warn("Clipboard API or ClipboardItem is not supported");
            return false;
        }

        const fetchAndConvert = async () => {
            let response = await fetch(url, { credentials: "include" });

            // If session expired during idle, refresh via check-is-logged-in and retry
            if (response.status === 401) {
                try {
                    const parsedUrl = new URL(url, window.location.href);
                    const refreshUrl = `${parsedUrl.origin}/user/check-is-logged-in`;
                    await fetch(refreshUrl, { credentials: "include" });
                    response = await fetch(url, { credentials: "include" });
                } catch (_) {}
            }

            if (!response.ok) {
                throw new Error(`Failed to fetch image: ${response.status} ${response.statusText}`);
            }

            const blob = await response.blob();
            return await convertBlobToPng(blob);
        };

        // Call navigator.clipboard.write with a Promise synchronously inside user activation
        try {
            const pngPromise = fetchAndConvert();
            const item = new ClipboardItem({ "image/png": pngPromise });
            await navigator.clipboard.write([item]);
            return true;
        } catch (promiseErr) {
            // Fallback for older browsers that do not support Promise inside ClipboardItem
            if (promiseErr instanceof TypeError) {
                const pngBlob = await fetchAndConvert();
                const item = new ClipboardItem({ "image/png": pngBlob });
                await navigator.clipboard.write([item]);
                return true;
            }
            console.error("Failed to write to clipboard:", promiseErr);
            return false;
        }
    } catch (err) {
        console.error("copyImageToClipboard error:", err);
        return false;
    }
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

window.attachmentInput = (() => {
    const instances = new Map();
    let nextId = 0;

    const hasFiles = (event) => {
        return Array.from(event.dataTransfer?.types || []).includes("Files");
    };

    const setInputFiles = (input, files) => {
        if (!input || !files.length) {
            return;
        }

        const transfer = new DataTransfer();
        files.forEach((file) => transfer.items.add(file));
        input.files = transfer.files;
        input.dispatchEvent(new Event("change", { bubbles: true }));
    };

    const getClipboardFiles = (clipboardData) => {
        const items = Array.from(clipboardData?.items || []);
        return items
            .filter((item) => item.kind === "file")
            .map((item) => {
                const file = item.getAsFile();
                return file;
            })
            .filter(Boolean);
    };

    const attach = (root, input, dotNetRef) => {
        if (!root || !input || !dotNetRef) {
            return null;
        }

        const existingInstance = Array.from(instances.values())
            .find((instance) => instance.root === root && instance.input === input);
        if (existingInstance) {
            detach(existingInstance.id);
        }

        const id = `attachment-input-${++nextId}`;
        let dragDepth = 0;

        const setDragActive = (isActive) => {
            dotNetRef.invokeMethodAsync("SetAttachmentDragActive", isActive);
        };

        const onDragEnter = (event) => {
            if (!hasFiles(event)) {
                return;
            }

            event.preventDefault();
            dragDepth += 1;
            setDragActive(true);
        };

        const onDragOver = (event) => {
            if (!hasFiles(event)) {
                return;
            }

            event.preventDefault();
            event.dataTransfer.dropEffect = "copy";
        };

        const onDragLeave = (event) => {
            if (!hasFiles(event)) {
                return;
            }

            event.preventDefault();
            dragDepth = Math.max(0, dragDepth - 1);
            if (dragDepth === 0) {
                setDragActive(false);
            }
        };

        const onDrop = (event) => {
            if (!hasFiles(event)) {
                return;
            }

            event.preventDefault();
            dragDepth = 0;
            setDragActive(false);
            setInputFiles(input, Array.from(event.dataTransfer.files || []));
        };

        const onPaste = (event) => {
            const activeElement = document.activeElement;
            if (activeElement && activeElement !== document.body && !root.contains(activeElement)) {
                return;
            }

            const files = getClipboardFiles(event.clipboardData);
            if (!files.length) {
                return;
            }

            event.preventDefault();
            setInputFiles(input, files);
        };

        root.addEventListener("dragenter", onDragEnter);
        root.addEventListener("dragover", onDragOver);
        root.addEventListener("dragleave", onDragLeave);
        root.addEventListener("drop", onDrop);
        document.addEventListener("paste", onPaste, true);

        instances.set(id, {
            id,
            root,
            input,
            onDragEnter,
            onDragOver,
            onDragLeave,
            onDrop,
            onPaste
        });

        return id;
    };

    const detach = (id) => {
        const instance = instances.get(id);
        if (!instance) {
            return;
        }

        instance.root.removeEventListener("dragenter", instance.onDragEnter);
        instance.root.removeEventListener("dragover", instance.onDragOver);
        instance.root.removeEventListener("dragleave", instance.onDragLeave);
        instance.root.removeEventListener("drop", instance.onDrop);
        document.removeEventListener("paste", instance.onPaste, true);
        instances.delete(id);
    };

    return {
        attach,
        detach
    };
})();

window.getTimeCaret = function(el) {
    if (!el) return 0;
    return el.selectionStart || 0;
};

window.setTimeCaret = function(el, pos) {
    if (!el) return;
    try {
        el.setSelectionRange(pos, pos);
    } catch (e) {}
};
