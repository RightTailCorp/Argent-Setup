# Argent Setup UI

Windows Setup wizard for Argent Job Scheduler / Queue Engine (desktop `.exe` package).

## Client goals

- Simple path for distracted admins
- **No secondary .NET downloads** — Setup is self-contained
- Same functional fields as legacy Setup
- Events logged to a text file in the same style as legacy `SETUP_LOG.TXT` / `SAMPLE.TXT`

## Desktop package

Deliverable: **`ArgentSetup-Exe.zip`**

```bash
npm install
npm run build
Remove-Item -Recurse -Force desktop\web -ErrorAction SilentlyContinue
Copy-Item -Recurse out desktop\web
cd desktop
npm install
npm run pack
```

Then open `ArgentSetup-Exe`, keep only the win32-x64 app folder contents, remove unused Chromium license/locale clutter, and zip.

Double-click `ArgentSetup.exe` inside the unzipped folder (keep all files together).

## Setup log

Path (per machine):

`%LOCALAPPDATA%\Argent\Setup\LOGS\SETUP_LOG.TXT`

Example: `C:\Users\<user>\AppData\Local\Argent\Setup\LOGS\SETUP_LOG.TXT`

Format matches legacy Argent timestamps / machine / user / process ids. See `docs\SAMPLE.TXT` and `docs\CLIENT-MESSAGE-LOGGING.txt`.

## Local web preview (optional)

```bash
npm install
npm run dev
```

Open http://localhost:3000

## WPF shell (optional)

`ArgentSetupUi.sln` — .NET Framework 4.8 UI prototype with the same log format.
