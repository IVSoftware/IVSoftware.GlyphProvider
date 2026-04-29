# Demo Smoke

This repo now has a lightweight smoke harness at [smoke-demos.ps1](./smoke-demos.ps1).

## What It Is

This is not a full test framework. It is a cheap, repeatable “are the demos still alive?” pass.

It helps catch:

- demo drift
- packaging drift
- startup regressions
- environment weirdness that only shows up when a UI head is launched

## What It Does

By default it:

- builds the desktop demo heads
- launches each one
- waits 10 seconds by default
- reports whether the process stayed alive or exited immediately

Optionally it can include the Windows heads of the MAUI demos.

## Why This Exists

The library may stay healthy for a long stretch while the demos quietly drift.

This harness gives you a low-cost way to ask:

- did today’s change break a demo?
- or did the machine / Visual Studio / MAUI environment drift again?

## How To Run

From the repo root:

```powershell
.\smoke-demos.ps1
```

Include the Windows MAUI heads too:

```powershell
.\smoke-demos.ps1 -IncludeMauiWindows
```

That flag currently adds:

- `QuickStart.Maui.Windows`
- `FontViewer.Maui.Windows`

Skip the build and just check already-built executables:

```powershell
.\smoke-demos.ps1 -NoBuild
```

Build only, do not launch:

```powershell
.\smoke-demos.ps1 -SkipLaunch
```

Use a longer observation window:

```powershell
.\smoke-demos.ps1 -ObservationSeconds 10
```

## How To Think About “Scheduler”

Right now this is a manually runnable smoke harness.

Later, you can schedule it through:

- Windows Task Scheduler
- a Codex automation / reminder flow
- CI

The important idea is simple:

- run the demos a little more often
- make failures cheap to notice
- keep demo trouble from piling up until “one fine day”
