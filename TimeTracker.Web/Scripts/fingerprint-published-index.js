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
    { url: "/img/logo/black/clock-64.png", label: "img/logo/black/clock-64.png" },
    { url: "css/app.min.css", label: "css/app.min.css" },
    { url: "TimeTracker.Web.styles.css", label: "TimeTracker.Web.styles.css" },
    { url: "_content/LumexUI/js/LumexUI.js", label: "_content/LumexUI/js/LumexUI.js" },
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
