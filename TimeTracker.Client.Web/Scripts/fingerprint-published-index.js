const fs = require("fs");
const zlib = require("zlib");

const [manifestPath, indexPath, sourcePath] = process.argv.slice(2);

if (!manifestPath || !indexPath) {
    console.error("Usage: node fingerprint-published-index.js <manifestPath> <indexPath>");
    process.exit(1);
}

const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
const html = fs.readFileSync(resolveHtmlSourcePath(sourcePath, indexPath), "utf8");
const fingerprints = new Map();

for (const endpoint of manifest.Endpoints || []) {
    const properties = new Map((endpoint.EndpointProperties || []).map(property => [property.Name, property.Value]));
    const label = properties.get("label");
    const fingerprint = properties.get("fingerprint");

    if (!label || !fingerprint || label.endsWith(".br") || label.endsWith(".gz")) {
        continue;
    }

    fingerprints.set(label, fingerprint);
}

const assets = [
    { url: "/favicon.ico", label: "favicon.ico" },
    { url: "/favicon.svg", label: "favicon.svg" },
    { url: "/apple-touch-icon.png", label: "apple-touch-icon.png" },
    { url: "/android-chrome-192x192.png", label: "android-chrome-192x192.png" },
    { url: "/android-chrome-512x512.png", label: "android-chrome-512x512.png" },
    { url: "/site.webmanifest", label: "site.webmanifest" },
    { url: "css/app.min.css", label: "css/app.min.css" },
    { url: "vendor/github-markdown/github-markdown.min.css", label: "vendor/github-markdown/github-markdown.min.css" },
    { url: "vendor/github-markdown/timevic-markdown-theme.css", label: "vendor/github-markdown/timevic-markdown-theme.css" },
    { url: "TimeTracker.Client.Web.styles.css", label: "TimeTracker.Client.Web.styles.css" },
];

let updatedHtml = html;

for (const asset of assets) {
    const fingerprint = fingerprints.get(asset.label);

    if (!fingerprint) {
        console.warn(`Fingerprint was not found for ${asset.label}`);
        continue;
    }

    updatedHtml = updatedHtml.split(asset.url).join(`${asset.url}?v=${fingerprint}`);
}

fs.writeFileSync(indexPath, updatedHtml);
fs.writeFileSync(`${indexPath}.gz`, zlib.gzipSync(updatedHtml));
fs.writeFileSync(
    `${indexPath}.br`,
    zlib.brotliCompressSync(updatedHtml, {
        params: {
            [zlib.constants.BROTLI_PARAM_QUALITY]: zlib.constants.BROTLI_MAX_QUALITY,
        },
    })
);

function resolveHtmlSourcePath(sourcePath, fallbackPath) {
    if (!sourcePath || !fs.existsSync(sourcePath)) {
        return fallbackPath;
    }

    const stats = fs.statSync(sourcePath);

    if (!stats.isDirectory()) {
        return sourcePath;
    }

    const htmlFiles = fs
        .readdirSync(sourcePath)
        .filter(fileName => fileName.endsWith(".html"))
        .sort();

    if (htmlFiles.length === 0) {
        return fallbackPath;
    }

    return `${sourcePath}/${htmlFiles[0]}`;
}
