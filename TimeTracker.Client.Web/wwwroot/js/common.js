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

// Lumex modals close on clicks outside the dialog rect. Popovers can visually extend outside that rect,
// so suppress only the modal close call while keeping the click event available for Blazor item selection.
window.lumexModalPopoverGuard = (() => {
    const suppressedDialogs = new WeakSet();
    const originalClose = HTMLDialogElement.prototype.close;

    HTMLDialogElement.prototype.close = function (...args) {
        if (suppressedDialogs.has(this)) {
            return;
        }

        return originalClose.apply(this, args);
    };

    const suppressModalClose = (event) => {
        const popover = event.target.closest("dialog [data-popover]");

        if (!popover) {
            return;
        }

        const dialog = popover.closest("dialog");

        if (!dialog) {
            return;
        }

        const rect = dialog.getBoundingClientRect();
        const isOutsideDialog =
            event.clientY < rect.top ||
            event.clientY > rect.bottom ||
            event.clientX < rect.left ||
            event.clientX > rect.right;

        if (isOutsideDialog) {
            suppressedDialogs.add(dialog);

            setTimeout(() => {
                suppressedDialogs.delete(dialog);
            });
        }
    };

    document.addEventListener("click", suppressModalClose, true);
})();


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

    const getImageExtension = (mimeType) => {
        switch (mimeType) {
            case "image/jpeg":
                return "jpg";
            case "image/png":
                return "png";
            case "image/gif":
                return "gif";
            case "image/bmp":
                return "bmp";
            case "image/webp":
                return "webp";
            default:
                return "png";
        }
    };

    const getClipboardImageFiles = (clipboardData) => {
        const items = Array.from(clipboardData?.items || []);
        return items
            .filter((item) => item.kind === "file" && item.type.startsWith("image/"))
            .map((item, index) => {
                const file = item.getAsFile();
                if (!file) {
                    return null;
                }

                const extension = getImageExtension(file.type);
                return new File(
                    [file],
                    file.name || `clipboard-image-${Date.now()}-${index + 1}.${extension}`,
                    { type: file.type }
                );
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

            const imageFiles = getClipboardImageFiles(event.clipboardData);
            if (!imageFiles.length) {
                return;
            }

            event.preventDefault();
            setInputFiles(input, imageFiles);
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
