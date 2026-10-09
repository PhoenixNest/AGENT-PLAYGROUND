param(
    [int]$Runs = 5,
    [int]$TimeoutSec = 170,
    [ValidateSet('before', 'after')][string]$Phase = 'before',
    [string]$Only = '',
    [string]$ReportDate = '2026-10-03'
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '..\..\lib\roots.ps1')
$mazeRoot = Get-MazeRoot $PSScriptRoot
$parity = Join-Path $mazeRoot 'tools\parity'
$project = Join-Path $mazeRoot 'unity\maze-puzzle'
$exe = Join-Path $project 'Builds\Parity\OrbitalSalvageParity.exe'
if (-not (Test-Path $exe)) { throw "Player not built: run games\maze-puzzle\tools\parity\windows\unity\build.ps1 first" }

$logDir = Join-Path $parity 'out\unity-logs'
$phaseRoot = Join-Path $parity ("out\perf-" + $Phase)
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
New-Item -ItemType Directory -Force -Path $phaseRoot | Out-Null

$scenarios = @('home', 'live', 'fps-home', 'fps-game')
$apis = @('d3d12', 'd3d11')
if ($Only) {
    $wanted = $Only -split ','
    $scenarios = @($scenarios | Where-Object { $wanted -contains $_ })
    $apiFilter = @($wanted | Where-Object { $_ -eq 'd3d11' -or $_ -eq 'd3d12' })
    if ($apiFilter.Count -gt 0) { $apis = $apiFilter }
}

function Get-GpuUtil {
    $cmd = Get-Command nvidia-smi -ErrorAction SilentlyContinue
    if (-not $cmd) { return $null }
    $lines = & nvidia-smi --query-gpu=utilization.gpu --format=csv,noheader,nounits 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $lines) { return $null }
    $max = $null
    foreach ($line in @($lines)) {
        $t = "$line".Trim()
        if ($t -match '^\d+$') {
            $n = [int]$t
            if ($null -eq $max -or $n -gt $max) { $max = $n }
        }
    }
    return $max
}

function Wait-QuietGpu {
    $deadline = (Get-Date).AddSeconds(90)
    $last = Get-GpuUtil
    if ($null -eq $last) {
        return @{ util = $null; quiet = $false; note = 'nvidia-smi unavailable' }
    }
    while ($last -ge 15 -and (Get-Date) -lt $deadline) {
        Start-Sleep -Seconds 2
        $last = Get-GpuUtil
        if ($null -eq $last) { break }
    }
    $quiet = ($null -ne $last -and $last -lt 15)
    $note = ''
    if (-not $quiet -and $null -ne $last) { $note = ("GPU still {0}% after wait" -f $last) }
    return @{ util = $last; quiet = $quiet; note = $note }
}

function Get-Median([double[]]$values) {
    $s = @($values | Sort-Object)
    $n = $s.Count
    if ($n -eq 0) { return 0 }
    if (($n % 2) -eq 1) { return [double]$s[[int]($n / 2)] }
    return ([double]$s[($n / 2) - 1] + [double]$s[$n / 2]) / 2.0
}

function Format-Triple([double[]]$values) {
    if ($null -eq $values -or $values.Count -eq 0) { return 'n/a' }
    $min = ($values | Measure-Object -Minimum).Minimum
    $max = ($values | Measure-Object -Maximum).Maximum
    $med = Get-Median $values
    return ('{0:0.00} / {1:0.00} / {2:0.00}' -f $min, $med, $max)
}

function Invoke-PerfPlayer([string]$scenario, [string]$api, [string]$outDir, [string]$log) {
    if (Test-Path $log) { Remove-Item $log -Force }
    $argList = New-Object System.Collections.Generic.List[string]
    if ($api -eq 'd3d11') { [void]$argList.Add('-force-d3d11') }
    [void]$argList.Add('-screen-fullscreen')
    [void]$argList.Add('0')
    [void]$argList.Add('-screen-width')
    [void]$argList.Add('1290')
    [void]$argList.Add('-screen-height')
    [void]$argList.Add('2796')
    [void]$argList.Add('-logFile')
    [void]$argList.Add($log)
    [void]$argList.Add('-parity-perf')
    [void]$argList.Add($scenario)
    [void]$argList.Add($outDir)
    $proc = Start-Process -FilePath $exe -ArgumentList $argList.ToArray() -PassThru -WindowStyle Minimized
    if (-not $proc.WaitForExit($TimeoutSec * 1000)) {
        try { $proc.Kill() } catch { }
        Write-Host ("TIMEOUT pid {0} {1} {2}" -f $proc.Id, $api, $scenario)
        return 124
    }
    return $proc.ExitCode
}

function Read-DrawFile([string]$path) {
    $map = @{}
    foreach ($line in (Get-Content $path)) {
        $parts = $line.Split(' ', 2)
        if ($parts.Length -eq 2) { $map[$parts[0]] = $parts[1].Trim() }
    }
    return $map
}

$records = @()
foreach ($scenario in $scenarios) {
    foreach ($api in $apis) {
        Write-Host ("=== {0} {1} x{2} ===" -f $Phase, $scenario, $api)
        $avgs = @()
        $p95s = @()
        $gpus = @()
        $notes = @()
        $cache = ''
        $device = ''
        $extra = @()
        for ($i = 1; $i -le $Runs; $i++) {
            $quiet = Wait-QuietGpu
            $gpuText = 'n/a'
            if ($null -ne $quiet.util) { $gpuText = [string]$quiet.util }
            $gpus += $gpuText
            if ($quiet.note) { $notes += ("run{0}: {1}" -f $i, $quiet.note) }
            $runDir = Join-Path $phaseRoot ("{0}-{1}-run{2}" -f $scenario, $api, $i)
            if (Test-Path $runDir) { Remove-Item $runDir -Recurse -Force }
            New-Item -ItemType Directory -Force -Path $runDir | Out-Null
            $log = Join-Path $logDir ("perf-{0}-{1}-{2}-run{3}.log" -f $Phase, $scenario, $api, $i)
            $code = Invoke-PerfPlayer $scenario $api $runDir $log
            if ($code -ne 0) {
                $notes += ("run{0}: exit {1}" -f $i, $code)
                Write-Host ("  run {0} exit {1} gpu {2}%" -f $i, $code, $gpuText)
                continue
            }
            $deviceFile = Join-Path $runDir 'device.txt'
            if (Test-Path $deviceFile) { $device = (Get-Content $deviceFile -TotalCount 1) }
            if ($scenario -eq 'home') {
                $drawPath = Join-Path $runDir 'draw-ms.txt'
                if (-not (Test-Path $drawPath)) { $notes += ("run{0}: missing draw-ms" -f $i); continue }
                $draw = Read-DrawFile $drawPath
                $avgs += [double]$draw['avg']
                $p95s += [double]$draw['p95']
                $cache = $draw['cache']
                $extra += ("gc {0}" -f $draw['gcBytes'])
            } elseif ($scenario -eq 'live') {
                $statsPath = Join-Path $runDir 'stats.json'
                if (-not (Test-Path $statsPath)) { $notes += ("run{0}: missing stats" -f $i); continue }
                $stats = Get-Content $statsPath -Raw | ConvertFrom-Json
                $avgs += [double]$stats.full.avg
                $p95s += [double]$stats.full.p95
                $extra += ("nofence {0:0.00} fence {1:0.00}" -f [double]$stats.full.noFenceAvg, [double]$stats.full.fenceAvg)
            } else {
                $fpsPath = Join-Path $runDir 'fps.json'
                if (-not (Test-Path $fpsPath)) { $notes += ("run{0}: missing fps" -f $i); continue }
                $fps = Get-Content $fpsPath -Raw | ConvertFrom-Json
                $avgs += [double]$fps.avgFps
                $p95s += [double]$fps.frameP95Ms
            }
            Write-Host ("  run {0} gpu {1}% device {2}" -f $i, $gpuText, $device)
        }
        $records += [pscustomobject]@{
            scenario = $scenario
            api = $api
            runs = $Runs
            device = $device
            cache = $cache
            gpuAtStart = ($gpus -join ',')
            avgMinMedMax = (Format-Triple ([double[]]$avgs))
            p95MinMedMax = (Format-Triple ([double[]]$p95s))
            avgs = $avgs
            p95s = $p95s
            notes = ($notes -join '; ')
            extra = ($extra -join '; ')
        }
    }
}

$summaryPath = Join-Path $phaseRoot 'summary.json'
$records | ConvertTo-Json -Depth 6 | Set-Content -Path $summaryPath -Encoding utf8
Write-Host ("Wrote {0}" -f $summaryPath)

function Read-Phase([string]$name) {
    $path = Join-Path $parity ("out\perf-" + $name + "\summary.json")
    if (-not (Test-Path $path)) { return $null }
    return @(Get-Content $path -Raw | ConvertFrom-Json)
}

function Row-For($rows, $scenario, $api) {
    foreach ($r in @($rows)) {
        if ($r.scenario -eq $scenario -and $r.api -eq $api) { return $r }
    }
    return $null
}

$before = Read-Phase 'before'
$after = Read-Phase 'after'
$reportDir = Join-Path $parity 'reports'
New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
$reportPath = Join-Path $reportDir ("perf-" + $ReportDate + ".md")

$labels = @(
    @{ scenario = 'home'; api = 'd3d12'; metric = 'Home Draw avg ms (min/med/max)'; p95 = 'Home Draw p95 ms' },
    @{ scenario = 'home'; api = 'd3d11'; metric = 'Home Draw avg ms (min/med/max)'; p95 = 'Home Draw p95 ms' },
    @{ scenario = 'live'; api = 'd3d12'; metric = 'Live frame+fence avg ms'; p95 = 'Live frame+fence p95 ms' },
    @{ scenario = 'live'; api = 'd3d11'; metric = 'Live frame+fence avg ms'; p95 = 'Live frame+fence p95 ms' },
    @{ scenario = 'fps-home'; api = 'd3d12'; metric = 'Home presented FPS'; p95 = 'Home frame p95 ms' },
    @{ scenario = 'fps-home'; api = 'd3d11'; metric = 'Home presented FPS'; p95 = 'Home frame p95 ms' },
    @{ scenario = 'fps-game'; api = 'd3d12'; metric = 'Game presented FPS'; p95 = 'Game frame p95 ms' },
    @{ scenario = 'fps-game'; api = 'd3d11'; metric = 'Game presented FPS'; p95 = 'Game frame p95 ms' }
)

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("# Parity performance " + $ReportDate)
[void]$sb.AppendLine("")
[void]$sb.AppendLine("Windowed player 1290x2796. Each cell is min / median / max over $Runs runs. GPU utilisation is sampled with nvidia-smi before each run (wait up to 90s for under 15%). Home Draw is HomeChrome.Draw over 20 frames at 0, 16, ... 304 ms. Live frame is update + render + composite, scale 1.0, 600 scripted steps; avg includes the GPU fence, and the notes carry the no-fence CPU submit time. Presented FPS is a 10s window with vSync off and targetFrameRate -1.")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("| Scenario | API | Before avg | Before p95 | After avg | After p95 | GPU% at start (after) |")
[void]$sb.AppendLine("| --- | --- | --- | --- | --- | --- | --- |")
foreach ($lab in $labels) {
    $b = Row-For $before $lab.scenario $lab.api
    $a = Row-For $after $lab.scenario $lab.api
    $bAvg = if ($b) { $b.avgMinMedMax } else { '' }
    $bP = if ($b) { $b.p95MinMedMax } else { '' }
    $aAvg = if ($a) { $a.avgMinMedMax } else { '' }
    $aP = if ($a) { $a.p95MinMedMax } else { '' }
    $gpu = if ($a) { $a.gpuAtStart } elseif ($b) { $b.gpuAtStart } else { '' }
    [void]$sb.AppendLine(("| {0} | {1} | {2} | {3} | {4} | {5} | {6} |" -f $lab.metric, $lab.api, $bAvg, $bP, $aAvg, $aP, $gpu))
}
[void]$sb.AppendLine("")
[void]$sb.AppendLine("## Notes")
[void]$sb.AppendLine("")
foreach ($name in @('before', 'after')) {
    $rows = Read-Phase $name
    if (-not $rows) { continue }
    [void]$sb.AppendLine("### $name")
    [void]$sb.AppendLine("")
    foreach ($r in @($rows)) {
        $line = "- $($r.scenario) $($r.api): device $($r.device); gpu $($r.gpuAtStart)"
        if ($r.cache) { $line += "; cache $($r.cache)" }
        if ($r.notes) { $line += "; $($r.notes)" }
        if ($r.extra) { $line += "; $($r.extra)" }
        [void]$sb.AppendLine($line)
    }
    [void]$sb.AppendLine("")
}
[void]$sb.AppendLine("## D3D11 opt-in")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("The parity capture player (tools/parity/windows/unity/build.ps1, ParityBuild) does not set a graphics API, so Unity 6 keeps the default (D3D12 on this machine) and stills stay on that path. A separate player for play mode is tools/parity/windows/unity/build-play.ps1 (PlayBuild), which forces Direct3D11 only for that build and writes games/maze-puzzle/output/maze-puzzle-windows/OrbitalSalvage.exe. It restores ProjectSettings afterwards, including the original graphics API list.")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("Editor Play mode uses the graphics API of the Editor process. Start the Editor with -force-d3d11 to play on D3D11. The running API is SystemInfo.graphicsDeviceType.")
[void]$sb.AppendLine("")
Set-Content -Path $reportPath -Value $sb.ToString() -Encoding utf8
Write-Host ("Wrote {0}" -f $reportPath)
exit 0
