param(
    [string]$States,
    [string]$Out,
    [string]$Only,
    [ValidateSet('', 'zero', 'iphone')][string]$Profile = '',
    [int]$TimeoutSec = 900,
    [string]$LogFile,
    [ValidateSet('Minimized', 'Normal', 'Hidden')][string]$WindowStyle = 'Minimized',
    [switch]$Isolated,
    [switch]$Batchmode
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '..\..\lib\roots.ps1')
$mazeRoot = Get-MazeRoot $PSScriptRoot
$parity = Join-Path $mazeRoot 'tools\parity'
$project = Join-Path $mazeRoot 'unity\maze-puzzle'
$exe = Join-Path $project 'Builds\Parity\OrbitalSalvageParity.exe'
if (-not (Test-Path $exe)) { throw "Player not built: run games\maze-puzzle\tools\parity\windows\unity\build.ps1 first" }

if (-not $States) {
    $default = Join-Path $parity 'states.json'
    $States = if (Test-Path $default) { $default } else { Join-Path $parity 'unity\smoke-states.json' }
}
if (-not $Out) { $Out = Join-Path $parity 'out\unity' }
if (-not $LogFile) { $LogFile = Join-Path $parity 'out\unity-logs\player.log' }

$States = (Resolve-Path $States).Path
New-Item -ItemType Directory -Force -Path $Out | Out-Null
$Out = (Resolve-Path $Out).Path
New-Item -ItemType Directory -Force -Path (Split-Path $LogFile) | Out-Null

function Invoke-Player([string[]]$Extra, [string]$Log, [string]$SummaryDir) {
    if (Test-Path $Log) { Remove-Item $Log -Force }
    $arguments = @()
    if ($Batchmode) { $arguments += '-batchmode' }
    $arguments += @('-screen-fullscreen', '0', '-screen-width', '430', '-screen-height', '932', '-logFile', "`"$Log`"")
    $arguments += @('--parity', "`"$States`"", "`"$Out`"")
    if ($SummaryDir) { $arguments += @('--summary', "`"$SummaryDir`"") }
    $arguments += $Extra

    $process = Start-Process -FilePath $exe -ArgumentList $arguments -WindowStyle $WindowStyle -PassThru
    if (-not $process.WaitForExit($TimeoutSec * 1000)) {
        $process.Kill()
        Write-Host "Timed out after $TimeoutSec s; killed player. Log: $Log"
        if (Test-Path $Log) { Get-Content $Log -Tail 40 | Write-Host }
        return 124
    }
    return $process.ExitCode
}

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$exitCode = 0

if (-not $Isolated) {
    $extra = @()
    if ($Only) { $extra += @('--only', $Only) }
    if ($Profile) { $extra += @('--profile', $Profile) }
    $exitCode = Invoke-Player $extra $LogFile $null
    if ($exitCode -ne 0) {
        $errorsFile = Join-Path $Out 'errors.json'
        if (Test-Path $errorsFile) { Get-Content $errorsFile | Write-Host }
        if (Test-Path $LogFile) { Get-Content $LogFile -Tail 40 | Write-Host }
    }
} else {
    $onlyIds = if ($Only) { $Only -split ',' } else { $null }
    $logDir = Split-Path $LogFile
    $summaryRoot = Join-Path $Out '.summary'
    if (Test-Path $summaryRoot) { Remove-Item $summaryRoot -Recurse -Force }
    $allTimings = @()
    $allErrors = @()

    foreach ($state in (Get-Content $States -Raw | ConvertFrom-Json)) {
        if ($onlyIds -and ($onlyIds -notcontains $state.id)) { continue }
        foreach ($p in $state.profiles) {
            if ($Profile -and $Profile -ne $p) { continue }
            $summary = Join-Path $summaryRoot "$($state.id)__$p"
            $log = Join-Path $logDir "player.$($state.id).$p.log"
            $code = Invoke-Player @('--only', $state.id, '--profile', $p) $log $summary
            $timingsFile = Join-Path $summary 'timings.json'
            if (Test-Path $timingsFile) { $allTimings += (Get-Content $timingsFile -Raw | ConvertFrom-Json).states }
            $errorsFile = Join-Path $summary 'errors.json'
            if (Test-Path $errorsFile) { $allErrors += (Get-Content $errorsFile -Raw | ConvertFrom-Json) }
            if ($code -ne 0) {
                Write-Host "FAILED $($state.id)/$p exit code $code (log: $log)"
                if ($exitCode -eq 0) { $exitCode = $code }
            }
        }
    }

    ([ordered]@{ totalSeconds = [math]::Round($sw.Elapsed.TotalSeconds, 3); states = $allTimings } | ConvertTo-Json -Depth 6) | Set-Content (Join-Path $Out 'timings.json')
    $errorsOut = Join-Path $Out 'errors.json'
    if ($allErrors.Count -gt 0) { $allErrors | ConvertTo-Json -Depth 6 | Set-Content $errorsOut } elseif (Test-Path $errorsOut) { Remove-Item $errorsOut -Force }
    if (Test-Path $summaryRoot) { Remove-Item $summaryRoot -Recurse -Force }
}

$sw.Stop()
Write-Host ("Player exit code {0}, {1:n1}s, out: {2}" -f $exitCode, $sw.Elapsed.TotalSeconds, $Out)
exit $exitCode
