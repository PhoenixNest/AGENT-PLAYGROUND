param(
  [Parameter(Mandatory = $true)][string]$Streams,
  [Parameter(Mandatory = $true)][string]$Out,
  [string]$Filter = "",
  [int]$Perf = 0,
  [string[]]$Set = @(),
  [string[]]$Sweep = @(),
  [string]$Unity = "C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe"
)
$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $here "..\lib\roots.ps1")
$mazeRoot = Get-MazeRoot $here
$project = Join-Path $mazeRoot "unity\maze-puzzle"
$Streams = (Resolve-Path $Streams).Path
New-Item -ItemType Directory -Force -Path $Out | Out-Null
$Out = (Resolve-Path $Out).Path
$log = Join-Path $Out "unity.log"
$argList = @("-batchmode", "-projectPath", "`"$project`"", "-executeMethod", "OrbitalSalvage.Render.Editor.CanvasConformance.Run",
  "-streams", "`"$Streams`"", "-out", "`"$Out`"", "-logFile", "`"$log`"")
if ($Filter) { $argList += @("-filter", $Filter) }
foreach ($s in $Set) { $argList += @("-set", $s) }
foreach ($s in ($Sweep -join ";").Split(";", [StringSplitOptions]::RemoveEmptyEntries)) { $argList += @("-sweep", $s) }
if ($Perf -gt 0) { $argList += @("-perf", "$Perf") }
$p = Start-Process -FilePath $Unity -ArgumentList $argList -Wait -PassThru
git -C $mazeRoot checkout -- "games/maze-puzzle/unity/maze-puzzle/Assets/Resources/Fonts" 2>$null
Write-Host "unity exit code $($p.ExitCode); log: $log"
exit $p.ExitCode
