param(
  [string]$Unity = "C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe",
  [string]$Filter = "FrameRateHarness"
)
$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Resolve-Path (Join-Path $here "..\..\..")
$project = Join-Path $root "games\maze-puzzle\unity\maze-puzzle"
$log = Join-Path $env:TEMP "physics-framerate-unity.log"
$results = Join-Path $env:TEMP "physics-framerate-results.xml"
if (Test-Path $log) { Remove-Item $log -Force }
$argList = @(
  "-batchmode",
  "-projectPath", $project,
  "-runTests",
  "-testPlatform", "EditMode",
  "-testFilter", $Filter,
  "-testResults", $results,
  "-logFile", $log
)
Write-Host "project $project"
Write-Host "log $log"
$p = Start-Process -FilePath $Unity -ArgumentList $argList -PassThru
Set-Content -Path (Join-Path $env:TEMP "physics-framerate-unity.pid") -Value $p.Id
Write-Host "unity pid $($p.Id)"
$p.WaitForExit()
Write-Host "unity exit $($p.ExitCode)"
exit $p.ExitCode
