# Argent Setup UI

Minimal Setup wizard prototype for Argent Job Scheduler / Queue Engine.

## Client goals

- Simple path for distracted admins
- **No secondary .NET downloads** — Setup is self-contained
- Same functional fields as legacy Setup
- Service Startup Info split into **Folders** + **Service & SQL**
- ODBC “No” keeps the user in Setup (SQL unchecked)

## Local

```bash
npm install
npm run dev
```

Open http://localhost:3000

## Deploy on Vercel

Import this repo — Root Directory stays **`.`** (repo root). Framework: Next.js.

## WPF shell

`ArgentSetupUi.sln` — .NET Framework 4.8 UI-only prototype (not used by Vercel).
