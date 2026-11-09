<#
.SYNOPSIS
Runs the clone-bucket Unity project in batch mode: compile, EditMode or PlayMode tests, or one static method.

.DESCRIPTION
Usage (any working directory):
  pwsh -File games\clone-bucket\unity\tools\unity-batch.ps1 -Mode compile
  pwsh -File games\clone-bucket\unity\tools\unity-batch.ps1 -Mode editmode -TimeoutMinutes 20
  pwsh -File games\clone-bucket\unity\tools\unity-batch.ps1 -Mode playmode
  pwsh -File games\clone-bucket\unity\tools\unity-batch.ps1 -Mode method -ExecuteMethod Some.Class.Method
  pwsh -File games\clone-bucket\unity\tools\unity-batch.ps1 -Mode method -ExecuteMethod Some.Class.Method -WithGraphics

Modes:
  compile   Unity -batchmode -nographics -quit (import and compile only).
  editmode  Unity -runTests -testPlatform EditMode, NUnit results XML written to -LogDir.
  playmode  Unity -runTests -testPlatform PlayMode, same results output.
  method    Unity -executeMethod <Class.Method> -quit.

-WithGraphics  Omits -nographics for any mode, so a graphics device exists and shaders
  really compile and cameras can render (needed for shader audits and PNG snapshots).
  Without it, every run stays -nographics as before. The project lock is unchanged.

Only ONE Unity process may use the project at a time. Unity locks
Temp\UnityLockfile and writes to Library\, so concurrent runs corrupt the cache.
This script takes an exclusive lock file (<LogDir>\unity.lock) before starting and
waits while Temp\UnityLockfile is held by another process (for example an open
Unity Editor). It retries every 15 seconds until -TimeoutMinutes runs out, then exits 3.
The lock is released in a finally block, even when Unity fails or the script is interrupted.

After Unity exits the script prints: the exit code, the log path, every distinct
"error CS" line (max 200), the count of "warning CS" lines, and for test modes the
passed, failed and total counts from the results XML plus the name and message of each failure.

Exit codes:
  Unity's own exit code (also the script's exit code when Unity finishes)
  124  Unity exceeded -TimeoutMinutes and was killed
  3    could not acquire the lock or the project before the lock timeout
  2    invalid arguments or missing project or editor
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('compile', 'editmode', 'playmode', 'method')]
    [string]$Mode,

    [string]$ExecuteMethod,

    [switch]$WithGraphics,

    [int]$TimeoutMinutes = 40,

    [string]$LogDir = '<local-temp>\unity-logs',

    [string]$EditorPath = 'C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Unity.exe'
)

$ErrorActionPreference = 'Stop'

if ($Mode -eq 'method' -and [string]::IsNullOrWhiteSpace($ExecuteMethod)) {
    Write-Error 'Mode "method" needs -ExecuteMethod <Class.Method>.'
    exit 2
}

$ProjectPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\clone-bucket'))
if (-not (Test-Path -LiteralPath (Join-Path $ProjectPath 'ProjectSettings\ProjectVersion.txt'))) {
    Write-Error "Unity project not found at $ProjectPath"
    exit 2
}
if (-not (Test-Path -LiteralPath $EditorPath)) {
    Write-Error "Unity editor not found at $EditorPath"
    exit 2
}

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$LogFile = Join-Path $LogDir "unity-$Mode-$stamp.log"
$ResultsFile = Join-Path $LogDir "unity-$Mode-$stamp.results.xml"
$LockFile = Join-Path $LogDir 'unity.lock'
$ProjectLockFile = Join-Path $ProjectPath 'Temp\UnityLockfile'

# Opens a file with no sharing. Returns the stream, or $null if another process holds it.
function Open-Exclusive([string]$Path) {
    try {
        return [IO.File]::Open($Path, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    }
    catch [IO.IOException] {
        return $null
    }
}

# Script lock: an open handle with FileShare.None and DeleteOnClose. The OS releases it when
# the process exits, even after a crash, and the file is deleted on close, so no stale lock remains.
function Open-ScriptLock([string]$Path) {
    try {
        return [IO.FileStream]::new($Path, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite,
            [IO.FileShare]::None, 1, [IO.FileOptions]::DeleteOnClose)
    }
    catch [IO.IOException] {
        return $null
    }
}

# Waits for both the script lock and the Unity project lock, up to the timeout.
$lockDeadline = (Get-Date).AddMinutes($TimeoutMinutes)
$lockStream = $null
while ($true) {
    $lockStream = Open-ScriptLock $LockFile
    if ($lockStream) {
        $projectBusy = $false
        if (Test-Path -LiteralPath $ProjectLockFile) {
            $probe = Open-Exclusive $ProjectLockFile
            if ($probe) { $probe.Dispose() } else { $projectBusy = $true }
        }
        if (-not $projectBusy) { break }
        $lockStream.Dispose()
        $lockStream = $null
        Write-Host "Project is in use (Temp\UnityLockfile held). Waiting 15 s..."
    }
    else {
        Write-Host "Another unity-batch run holds $LockFile. Waiting 15 s..."
    }
    if ((Get-Date) -ge $lockDeadline) {
        Write-Error "Gave up after $TimeoutMinutes minutes waiting for the Unity project or lock."
        exit 3
    }
    Start-Sleep -Seconds 15
}

$exitCode = 0
$started = Get-Date
try {
    $unityArgs = @('-batchmode')
    if (-not $WithGraphics) { $unityArgs += '-nographics' }
    $unityArgs += @('-projectPath', $ProjectPath, '-logFile', $LogFile)
    switch ($Mode) {
        'compile' { $unityArgs += '-quit' }
        'editmode' { $unityArgs += @('-runTests', '-testPlatform', 'EditMode', '-testResults', $ResultsFile) }
        'playmode' { $unityArgs += @('-runTests', '-testPlatform', 'PlayMode', '-testResults', $ResultsFile) }
        'method' { $unityArgs += @('-executeMethod', $ExecuteMethod, '-quit') }
    }
    # Start-Process does not quote array items, so quote any that contain spaces.
    $argString = ($unityArgs | ForEach-Object { if ($_ -match '\s') { '"' + $_ + '"' } else { $_ } }) -join ' '

    Write-Host "Starting Unity ($Mode) on $ProjectPath"
    Write-Host "Log: $LogFile"
    $proc = Start-Process -FilePath $EditorPath -ArgumentList $argString -PassThru -WindowStyle Hidden

    $finished = $proc.WaitForExit($TimeoutMinutes * 60 * 1000)
    if ($finished) {
        $exitCode = $proc.ExitCode
    }
    else {
        Write-Warning "Unity did not exit within $TimeoutMinutes minutes. Killing it."
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
        $exitCode = 124
    }
}
finally {
    if ($lockStream) { $lockStream.Dispose() }
}

$duration = (Get-Date) - $started
Write-Host ''
Write-Host ('=== unity-batch {0} finished ===' -f $Mode)
Write-Host "Exit code : $exitCode"
Write-Host ('Duration  : {0:hh\:mm\:ss}' -f $duration)
Write-Host "Log file  : $LogFile"

if (Test-Path -LiteralPath $LogFile) {
    $logLines = Get-Content -LiteralPath $LogFile -Encoding UTF8

    $errorRx = [regex]'error (CS\d{4}):\s*(.*)$'
    $errorLines = $logLines | Where-Object { $_ -match 'error CS\d{4}' } | Select-Object -Unique
    Write-Host ''
    Write-Host ("Distinct 'error CS' lines: {0}" -f @($errorLines).Count)
    $shown = 0
    foreach ($line in $errorLines) {
        if ($shown -ge 200) { Write-Host '  ... truncated at 200 lines, see the log'; break }
        $m = $errorRx.Match($line)
        $where = ''
        if ($line -match '^(?<file>.+?)\((?<line>\d+),(?<col>\d+)\): error CS') {
            $where = '{0}:{1}' -f $Matches['file'], $Matches['line']
        }
        if ($m.Success) {
            Write-Host ('  [{0}] {1}  {2}' -f $m.Groups[1].Value, $where, $m.Groups[2].Value.Trim())
        }
        else {
            Write-Host "  $line"
        }
        $shown++
    }

    $warningCount = @($logLines | Where-Object { $_ -match 'warning CS\d{4}' }).Count
    Write-Host ''
    Write-Host "warning CS lines (total): $warningCount"
}
else {
    Write-Warning "Log file was not created: $LogFile"
}

if ($Mode -in @('editmode', 'playmode')) {
    Write-Host ''
    if (Test-Path -LiteralPath $ResultsFile) {
        [xml]$results = Get-Content -LiteralPath $ResultsFile -Raw
        $run = $results.'test-run'
        Write-Host ("Tests: passed {0}, failed {1}, total {2} (skipped {3}, inconclusive {4})" -f `
            $run.passed, $run.failed, $run.total, $run.skipped, $run.inconclusive)
        $failedCases = @($results.SelectNodes('//test-case[@result="Failed"]'))
        foreach ($tc in $failedCases) {
            $msg = ''
            if ($tc.failure -and $tc.failure.message) { $msg = $tc.failure.message.'#cdata-section' + $tc.failure.message.InnerText }
            Write-Host ("  FAILED {0}" -f $tc.fullname)
            Write-Host ("    {0}" -f ($msg -replace '\s+', ' ').Trim())
        }
    }
    else {
        Write-Warning "No results XML at $ResultsFile (Unity may have failed before running tests; check the log)."
    }
}

exit $exitCode
