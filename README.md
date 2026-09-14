# Argent Setup UI

Minimal, customer-friendly Setup wizard prototype for Argent Job Scheduler / Queue Engine.

## Client goals reflected here

- Simple path for distracted admins — short copy, one job per screen, clear progress
- **No secondary .NET downloads** — Setup is self-contained; welcome screen states this explicitly
- Same functional fields as legacy Setup (license, node, install/remove, paths, service/SQL, customer, progress, done)
- Overloaded “Service Startup Info” split into **Folders** + **Service & SQL**
- ODBC “No” keeps the user in Setup (SQL unchecked) instead of exiting

## Web preview (shareable)

```bash
cd web
npm install
npm run dev
```

Open http://localhost:3000

## Deploy on Vercel

1. Import [RightTailCorp/Argent-Setup](https://github.com/RightTailCorp/Argent-Setup)
2. Set **Root Directory** to `web`
3. Framework: Next.js (auto) → Deploy

## WPF shell (.NET Framework 4.8)

Open `ArgentSetupUi.sln` in VS2022 — UI-only, no install engine.
