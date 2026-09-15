const { app, BrowserWindow } = require("electron");
const setupLog = require("./setupLog");
const http = require("http");
const fs = require("fs");
const path = require("path");

const ROOT = path.join(__dirname, "web");

// Prevent trackpad/Ctrl pinch from shrinking the UI into the corner
app.commandLine.appendSwitch("disable-pinch");

const MIME = {
  ".html": "text/html; charset=utf-8",
  ".js": "application/javascript; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".json": "application/json",
  ".png": "image/png",
  ".jpg": "image/jpeg",
  ".jpeg": "image/jpeg",
  ".svg": "image/svg+xml",
  ".ico": "image/x-icon",
  ".woff": "font/woff",
  ".woff2": "font/woff2",
  ".txt": "text/plain; charset=utf-8",
};

function contentType(filePath) {
  return MIME[path.extname(filePath).toLowerCase()] || "application/octet-stream";
}

function startServer() {
  return new Promise((resolve, reject) => {
    const server = http.createServer((req, res) => {
      try {
        let urlPath = decodeURIComponent((req.url || "/").split("?")[0]);
        if (urlPath === "/") urlPath = "/index.html";
        let filePath = path.join(ROOT, urlPath.replace(/^\//, "").replace(/\//g, path.sep));
        if (fs.existsSync(filePath) && fs.statSync(filePath).isDirectory()) {
          filePath = path.join(filePath, "index.html");
        }
        if (!fs.existsSync(filePath) || !fs.statSync(filePath).isFile()) {
          filePath = path.join(ROOT, "index.html");
        }
        const data = fs.readFileSync(filePath);
        res.writeHead(200, {
          "Content-Type": contentType(filePath),
          "Cache-Control": "no-store",
        });
        res.end(data);
      } catch (err) {
        res.writeHead(500);
        res.end(String(err));
      }
    });
    server.on("error", reject);
    // Bind to an ephemeral port so we never reuse a stale previous Setup process.
    server.listen(0, "127.0.0.1", () => {
      const addr = server.address();
      resolve({ server, port: addr && addr.port });
    });
  });
}

async function createWindow() {
  setupLog.logSessionStart();
  const { port } = await startServer();
  const win = new BrowserWindow({
    width: 1100,
    height: 720,
    minWidth: 900,
    minHeight: 600,
    title: "Argent Job Scheduler Setup",
    backgroundColor: "#FFFFFF",
    autoHideMenuBar: true,
    resizable: true,
    maximizable: true,
    show: false,
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
      nodeIntegration: false,
      contextIsolation: true,
      zoomFactor: 1,
    },
  });
  win.setMenuBarVisibility(false);

  const lockZoom = async () => {
    try {
      await win.webContents.setVisualZoomLevelLimits(1, 1);
    } catch (_) {}
    win.webContents.setZoomFactor(1);
    win.webContents.setZoomLevel(0);
    await win.webContents.insertCSS(`
      html, body { width:100% !important; height:100% !important; margin:0 !important; overflow:hidden !important; }
      body > div, body > div > div { width:100% !important; height:100% !important; min-height:100% !important; }
    `);
  };

  win.once("ready-to-show", async () => {
    await lockZoom();
    win.show();
  });
  win.webContents.on("did-finish-load", () => {
    lockZoom();
  });

  // Ctrl+/- / 0 only (no wheel/pinch — those were shrinking the UI)
  win.webContents.on("before-input-event", (event, input) => {
    if (!(input.control || input.meta) || input.type !== "keyDown") return;
    if (input.key === "=" || input.key === "+") {
      win.webContents.setZoomFactor(Math.min(2, win.webContents.getZoomFactor() + 0.1));
      event.preventDefault();
    } else if (input.key === "-") {
      win.webContents.setZoomFactor(Math.max(0.8, win.webContents.getZoomFactor() - 0.1));
      event.preventDefault();
    } else if (input.key === "0") {
      win.webContents.setZoomFactor(1);
      win.webContents.setZoomLevel(0);
      event.preventDefault();
    }
  });

  await win.loadURL(`http://127.0.0.1:${port}/`);
}

app.whenReady().then(createWindow);
app.on("window-all-closed", () => app.quit());
