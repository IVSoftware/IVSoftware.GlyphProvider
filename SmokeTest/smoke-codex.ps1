param(
    [string[]]$Repo,
    [switch]$List,
    [switch]$NoBuild,
    [switch]$SkipLaunch,
    [switch]$IncludeMauiWindows
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

$targets = @(
    [pscustomobject]@{
        Name = 'GlyphProvider'
        Script = 'IVSoftware\IVSoftware.GlyphProvider\smoke-demos.ps1'
        Description = 'Desktop and optional MAUI Windows smoke for IVSoftware.GlyphProvider.'
    }
)

if ($List) {
    $targets |
        Select-Object Name, Description, @{ Name = 'Script'; Expression = { Join-Path $root $_.Script } } |
        Format-Table -AutoSize
    return
}

$selectedTargets =
    if ($Repo -and $Repo.Count -gt 0) {
        $targets | Where-Object { $Repo -contains $_.Name }
    }
    else {
        $targets
    }

if (@($selectedTargets).Count -eq 0) {
    Write-Host "No smoke targets matched the requested repo filter." -ForegroundColor Yellow
    Write-Host "Tip: run .\smoke-codex.ps1 -List"
    exit 1
}

$results = New-Object System.Collections.Generic.List[object]

foreach ($target in $selectedTargets) {
    $scriptPath = Join-Path $root $target.Script
    if (-not (Test-Path $scriptPath)) {
        $results.Add([pscustomobject]@{
            Name = $target.Name
            Status = 'missing-script'
            Detail = $scriptPath
        })
        continue
    }

    Write-Host ""
    Write-Host "== Repo Smoke: $($target.Name) ==" -ForegroundColor Cyan
    Write-Host $scriptPath

    $childArgs = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $scriptPath)
    if ($NoBuild) { $childArgs += '-NoBuild' }
    if ($SkipLaunch) { $childArgs += '-SkipLaunch' }
    if ($IncludeMauiWindows) { $childArgs += '-IncludeMauiWindows' }

    & powershell @childArgs
    $exitCode = $LASTEXITCODE

    $results.Add([pscustomobject]@{
        Name = $target.Name
        Status = if ($exitCode -eq 0) { 'passed' } else { 'failed' }
        Detail = "ExitCode=$exitCode"
    })
}

Write-Host ""
Write-Host "== Codex Smoke Summary ==" -ForegroundColor Cyan
$results | Format-Table -AutoSize

$hasFailures = @($results | Where-Object { $_.Status -ne 'passed' }).Count -gt 0
if ($hasFailures) {
    Write-Host ""
    Write-Host "Codex smoke result: FAIL" -ForegroundColor Red
    Write-Host "Meaning: at least one repo-level smoke script failed or could not be found."
    exit 1
}

Write-Host ""
Write-Host "Codex smoke result: PASS" -ForegroundColor Green
