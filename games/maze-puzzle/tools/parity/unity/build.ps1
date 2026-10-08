param(
    [string]$UnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe',
    [string]$LogFile
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$project = Join-Path $repoRoot 'games\maze-puzzle\unity\maze-puzzle'
$outDir = Join-Path $repoRoot 'tools\parity\out\unity-logs'
if (-not $LogFile) { $LogFile = Join-Path $outDir 'build.log' }
New-Item -ItemType Directory -Force -Path (Split-Path $LogFile) | Out-Null

if (-not (Test-Path $UnityExe)) { throw "Unity editor not found: $UnityExe" }

$exe = Join-Path $project 'Builds\Parity\OrbitalSalvageParity.exe'
if (Test-Path $exe) { Remove-Item $exe -Force }

$arguments = @(
    '-batchmode', '-quit',
    '-projectPath', "`"$project`"",
    '-buildTarget', 'Win64',
    '-executeMethod', 'OrbitalSalvage.Editor.ParityBuild.BuildPlayer',
    '-logFile', "`"$LogFile`""
)

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$process = Start-Process -FilePath $UnityExe -ArgumentList $arguments -Wait -PassThru
$sw.Stop()

Write-Host ("Unity exit code {0}, {1:n1}s, log: {2}" -f $process.ExitCode, $sw.Elapsed.TotalSeconds, $LogFile)

if ($process.ExitCode -ne 0 -or -not (Test-Path $exe)) {
    Get-Content $LogFile -Tail 40 | Write-Host
    exit 1
}

Write-Host "Built $exe"
exit 0
