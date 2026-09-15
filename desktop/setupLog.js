const fs = require("fs");
const path = require("path");
const os = require("os");

const MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

let sequence = 0;
let headerWritten = false;

function logDir() {
  const base = process.env.LOCALAPPDATA || path.join(os.homedir(), "AppData", "Local");
  return path.join(base, "Argent", "Setup", "LOGS");
}

function logFilePath() {
  return path.join(logDir(), "SETUP_LOG.TXT");
}

function formatTimestamp(d) {
  const pad2 = (n) => String(n).padStart(2, "0");
  const pad3 = (n) => String(n).padStart(3, "0");
  return `${pad2(d.getDate())} ${MONTHS[d.getMonth()]} ${d.getFullYear()} ${pad2(d.getHours())}:${pad2(
    d.getMinutes()
  )}:${pad2(d.getSeconds())}.${pad3(d.getMilliseconds())}`;
}

function formatLine(message) {
  const d = new Date();
  const machine = os.hostname();
  const user = (os.userInfo().username || "").toUpperCase();
  const pid = process.pid;
  const tid = 0;
  sequence += 1;
  return `${formatTimestamp(d)} ${machine} ${user} (${String(pid).padStart(5, "0")}-${String(tid).padStart(
    5,
    "0"
  )}) ${String(sequence).padStart(5, "0")} ${message ?? ""}`;
}

function ensureHeader() {
  const file = logFilePath();
  if (headerWritten || fs.existsSync(file)) {
    headerWritten = true;
    return;
  }
  fs.mkdirSync(logDir(), { recursive: true });
  const header = `

Argent Job Scheduler 10.0-2401-64W-A Setup Log File
Copyright (c) 2024 Argent Software

This product is protected by one or more of the following U.S. Patents:

6483813; 511167; 511346; 530335; 543551; 553142; 553143; 553144;
 553626; 553627; 553628; 553629; 553630; 553631; 553635; 553636;
 553637; 562835; 562836; 562837.

Please contact Support at Support@Argent.com or help.Argent.com.
`;
  fs.appendFileSync(file, header, "utf8");
  headerWritten = true;
}

function logLabel(label, value) {
  const padded = `${label}:`.padEnd(20, " ");
  logEvent(`${padded}${value ?? ""}`);
}

function logEvent(message) {
  fs.mkdirSync(logDir(), { recursive: true });
  ensureHeader();
  fs.appendFileSync(logFilePath(), `${formatLine(message)}\n`, "utf8");
}

function logSessionStart() {
  ensureHeader();
  logEvent("");
  logLabel("Machine", os.hostname());
  logEvent("");
  logEvent("");
  logLabel("User", os.userInfo().username || "");
  logEvent("");
  logLabel("Product", "Argent Job Scheduler");
  logEvent("");
  logLabel("Version", "10.0-2401-A");
  logEvent("");
  logLabel("Program", process.arch.includes("64") ? "64bit" : "32bit");
  logEvent("");
  logEvent("Setup UI session started");
}

module.exports = { logEvent, logLabel, logSessionStart, logFilePath };
