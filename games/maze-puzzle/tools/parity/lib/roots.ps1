function Get-MazeRoot([string]$Start) {
    $dir = (Resolve-Path -LiteralPath $Start).Path
    for ($i = 0; $i -lt 14; $i++) {
        $marker = Join-Path $dir 'tools\parity\package.json'
        $web = Join-Path $dir 'web\index.html'
        if ((Test-Path -LiteralPath $marker) -and (Test-Path -LiteralPath $web)) {
            return $dir
        }
        $parent = Split-Path -Parent $dir
        if (-not $parent -or $parent -eq $dir) { break }
        $dir = $parent
    }
    throw "games/maze-puzzle marker (tools/parity + web) not found from $Start"
}
