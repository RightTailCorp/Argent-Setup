const { contextBridge, webFrame } = require("electron");
const setupLog = require("./setupLog");

contextBridge.exposeInMainWorld("argentDesktop", {
  logEvent(message) {
    setupLog.logEvent(message);
  },
  logLabel(label, value) {
    setupLog.logLabel(label, value);
  },
  logFilePath() {
    return setupLog.logFilePath();
  },
  adjustZoom(delta) {
    const next = Math.min(3, Math.max(0.5, webFrame.getZoomFactor() + delta));
    webFrame.setZoomFactor(Number(next.toFixed(2)));
    return next;
  },
  setZoom(factor) {
    const next = Math.min(3, Math.max(0.5, Number(factor) || 1));
    webFrame.setZoomFactor(Number(next.toFixed(2)));
    return next;
  },
  getZoom() {
    return webFrame.getZoomFactor();
  },
});
