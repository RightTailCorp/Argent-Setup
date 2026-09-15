declare global {
  interface Window {
    argentDesktop?: {
      logEvent: (message: string) => void;
      logLabel: (label: string, value: string) => void;
    };
  }
}

export function setupLog(message: string) {
  if (typeof window === "undefined") return;
  window.argentDesktop?.logEvent?.(message);
}

export function setupLogLabel(label: string, value: string) {
  if (typeof window === "undefined") return;
  if (window.argentDesktop?.logLabel) {
    window.argentDesktop.logLabel(label, value);
    return;
  }
  setupLog(`${label}: ${value}`);
}
