const instances = new WeakMap();

export function initialize(root, options) {
    dispose(root);

    if (!root) {
        return;
    }

    const handle = root.querySelector("[data-resizable-split-handle]");
    if (!handle) {
        return;
    }

    const minWidth = Number(options.minStartPaneWidth);
    const maxWidth = Number(options.maxStartPaneWidth);
    const defaultWidth = Number(options.defaultStartPaneWidth);
    const mediaQuery = window.matchMedia(options.desktopMediaQuery || "(min-width: 1280px)");
    let currentWidth = clamp(defaultWidth, minWidth, maxWidth);
    let startX = 0;
    let startWidth = currentWidth;
    let isDragging = false;
    let previousUserSelect = "";
    let previousCursor = "";

    const setWidth = (width) => {
        currentWidth = clamp(width, minWidth, maxWidth);
        root.style.setProperty("--resizable-split-start-width", `${currentWidth}px`);
        handle.setAttribute("aria-valuenow", Math.round(currentWidth).toString());
    };

    const stopDragging = () => {
        if (!isDragging) {
            return;
        }

        isDragging = false;
        root.classList.remove("is-resizing");
        document.body.style.userSelect = previousUserSelect;
        document.body.style.cursor = previousCursor;
        window.removeEventListener("pointermove", onPointerMove);
        window.removeEventListener("pointerup", stopDragging);
        window.removeEventListener("pointercancel", stopDragging);
    };

    const onPointerMove = (event) => {
        if (!isDragging) {
            return;
        }

        setWidth(startWidth + event.clientX - startX);
    };

    const onPointerDown = (event) => {
        if (!mediaQuery.matches || event.button !== 0) {
            return;
        }

        event.preventDefault();
        startX = event.clientX;
        startWidth = currentWidth;
        isDragging = true;
        previousUserSelect = document.body.style.userSelect;
        previousCursor = document.body.style.cursor;
        document.body.style.userSelect = "none";
        document.body.style.cursor = "col-resize";
        root.classList.add("is-resizing");
        window.addEventListener("pointermove", onPointerMove);
        window.addEventListener("pointerup", stopDragging);
        window.addEventListener("pointercancel", stopDragging);
    };

    const onKeyDown = (event) => {
        if (!mediaQuery.matches) {
            return;
        }

        if (event.key === "ArrowLeft") {
            event.preventDefault();
            setWidth(currentWidth - 16);
        } else if (event.key === "ArrowRight") {
            event.preventDefault();
            setWidth(currentWidth + 16);
        } else if (event.key === "Home") {
            event.preventDefault();
            setWidth(minWidth);
        } else if (event.key === "End") {
            event.preventDefault();
            setWidth(maxWidth);
        }
    };

    const onMediaChange = () => {
        if (!mediaQuery.matches) {
            stopDragging();
        }
    };

    setWidth(currentWidth);
    handle.addEventListener("pointerdown", onPointerDown);
    handle.addEventListener("keydown", onKeyDown);
    mediaQuery.addEventListener("change", onMediaChange);

    instances.set(root, () => {
        stopDragging();
        handle.removeEventListener("pointerdown", onPointerDown);
        handle.removeEventListener("keydown", onKeyDown);
        mediaQuery.removeEventListener("change", onMediaChange);
    });
}

export function dispose(root) {
    const cleanup = instances.get(root);

    if (!cleanup) {
        return;
    }

    cleanup();
    instances.delete(root);
}

function clamp(value, min, max) {
    return Math.min(Math.max(value, min), max);
}
