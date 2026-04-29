# Codex Smoke

This is the root manual smoke entry point for `D:\Codex`.

Use it when you want one habitual command instead of remembering repo-specific smoke scripts.

## Everyday

From `D:\Codex`:

```powershell
.\smoke-codex.ps1
```

Include the MAUI Windows head where supported:

```powershell
.\smoke-codex.ps1 -IncludeMauiWindows
```

Dry-run orchestration without building or launching:

```powershell
.\smoke-codex.ps1 -NoBuild -SkipLaunch
```

List currently registered repo smoke targets:

```powershell
.\smoke-codex.ps1 -List
```

Run one repo only:

```powershell
.\smoke-codex.ps1 -Repo GlyphProvider
```

## Mental Model

- `smoke-codex.ps1` lives at the root and orchestrates repo-level smoke scripts.
- Each repo keeps ownership of its own narrow smoke logic.
- The root script gives you one stable weekly habit whether or not you use scheduler support.
