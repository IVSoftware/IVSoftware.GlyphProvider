param(
    [switch]$NoBuild,
    [switch]$SkipLaunch,
    [switch]$IncludeMauiWindows,
    [int]$ObservationSeconds = 10
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

$desktopProjects = @(
    @{
        Name = 'QuickStart.Wpf'
        Csproj = 'QuickStart.Wpf.Demo\QuickStart.Wpf.Demo.csproj'
        Exe = 'QuickStart.Wpf.Demo\bin\Debug\net10.0-windows\QuickStart.Wpf.Demo.exe'
        BuildArgs = @('--no-restore', '/p:UseSharedCompilation=false', '--verbosity', 'minimal')
    },
    @{
        Name = 'QuickStart.WinForms'
        Csproj = 'QuickStart.WinForms.Demo\QuickStart.WinForms.Demo.csproj'
        Exe = 'QuickStart.WinForms.Demo\bin\Debug\net10.0-windows\QuickStart.WinForms.Demo.exe'
        BuildArgs = @('--no-restore', '/p:UseSharedCompilation=false', '--verbosity', 'minimal')
    },
    @{
        Name = 'FontViewer.Wpf'
        Csproj = 'FontViewer.WPF.Demo\FontViewer.Wpf.Demo.csproj'
        Exe = 'FontViewer.WPF.Demo\bin\Debug\net10.0-windows\FontViewer.Wpf.Demo.exe'
        BuildArgs = @('--no-restore', '/p:UseSharedCompilation=false', '/p:SignAssembly=false', '--verbosity', 'minimal')
    },
    @{
        Name = 'FontViewer.WinForms'
        Csproj = 'FontViewer.WinForms.Demo\FontViewer.WinForms.Demo.csproj'
        Exe = 'FontViewer.WinForms.Demo\bin\Debug\net10.0-windows\FontViewer.WinForms.Demo.exe'
        BuildArgs = @('--no-restore', '/p:UseSharedCompilation=false', '--verbosity', 'minimal')
    }
)

if ($IncludeMauiWindows) {
    $desktopProjects += @{
        Name = 'QuickStart.Maui.Windows'
        Csproj = 'QuickStart.Maui.Demo\QuickStart.Maui.Demo.csproj'
        Exe = 'QuickStart.Maui.Demo\bin\Debug\net10.0-windows10.0.19041.0\win-x64\QuickStart.Maui.Demo.exe'
        BuildArgs = @('-f', 'net10.0-windows10.0.19041.0', '--no-restore', '/p:UseSharedCompilation=false', '--verbosity', 'minimal')
    }
    $desktopProjects += @{
        Name = 'FontViewer.Maui.Windows'
        Csproj = 'FontViewer.Maui.Demo\FontViewer.Maui.Demo.csproj'
        Exe = 'FontViewer.Maui.Demo\bin\Debug\net10.0-windows10.0.19041.0\win-x64\FontViewer.Maui.Demo.exe'
        BuildArgs = @('-f', 'net10.0-windows10.0.19041.0', '--no-restore', '/p:UseSharedCompilation=false', '--verbosity', 'minimal')
    }
}

function Invoke-Build {
    param(
        [Parameter(Mandatory)]
        [hashtable]$Project
    )

    Write-Host ""
    Write-Host "== Build: $($Project.Name) =="

    & dotnet build (Join-Path $root $Project.Csproj) @($Project.BuildArgs) | Out-Host
    if ($LASTEXITCODE -ne 0) {
        return [pscustomobject]@{
            Name = $Project.Name
            Phase = 'build'
            Status = 'failed'
            Detail = 'Build failed. See output above for the first actionable compiler/package error.'
        }
    }

    return [pscustomobject]@{
        Name = $Project.Name
        Phase = 'build'
        Status = 'passed'
        Detail = 'Build succeeded.'
    }
}

function Invoke-LaunchSmoke {
    param(
        [Parameter(Mandatory)]
        [hashtable]$Project
    )

    $exePath = Join-Path $root $Project.Exe
    if (-not (Test-Path $exePath)) {
        return [pscustomobject]@{
            Name = $Project.Name
            Phase = 'launch'
            Status = 'missing-exe'
            Detail = $exePath
        }
    }

    $proc = Start-Process -FilePath $exePath -PassThru
    Start-Sleep -Seconds $ObservationSeconds

    if ($proc.HasExited) {
        return [pscustomobject]@{
            Name = $Project.Name
            Phase = 'launch'
            Status = 'exited'
            Detail = "ExitCode=$($proc.ExitCode)"
        }
    }

    try {
        Stop-Process -Id $proc.Id -Force
    }
    catch {
        # Best-effort only.
    }

    return [pscustomobject]@{
        Name = $Project.Name
        Phase = 'launch'
        Status = 'running'
        Detail = "PID=$($proc.Id)"
    }
}

$results = New-Object System.Collections.Generic.List[object]

foreach ($project in $desktopProjects) {
    if (-not $NoBuild) {
        $buildResult = Invoke-Build -Project $project
        $results.Add($buildResult)
        if ($buildResult.Status -ne 'passed') {
            continue
        }
    }

    if (-not $SkipLaunch) {
        $results.Add((Invoke-LaunchSmoke -Project $project))
    }
}

Write-Host ""
Write-Host "== Smoke Summary =="
$results | Format-Table -AutoSize

$hasFailures = @($results | Where-Object { $_.Status -notin @('passed', 'running') }).Count -gt 0
if ($hasFailures) {
    Write-Host ""
    Write-Host "Smoke result: FAIL" -ForegroundColor Red
    Write-Host "Meaning: at least one demo did not build, did not produce an executable, or exited during the observation window."
    exit 1
}

Write-Host ""
Write-Host "Smoke result: PASS" -ForegroundColor Green
