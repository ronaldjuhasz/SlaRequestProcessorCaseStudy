param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Message
)

$ErrorActionPreference = "Stop"

try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch {}

function Step($text) {
    Write-Host ""
    Write-Host $text
}

try {
    $repoRoot = (git rev-parse --show-toplevel 2>$null).Trim()
    if (-not $repoRoot) { throw "Not inside a git repository." }
    Set-Location $repoRoot

    Step "dotnet clean"
    dotnet clean

    Step "dotnet test"
    dotnet test
    if ($LASTEXITCODE -ne 0) { throw "dotnet test failed" }

    Step "git add -A"
    git add -A

    Step "git status (after add)"
    git status --porcelain

    Step "git commit"
    git commit -m $Message
    if ($LASTEXITCODE -ne 0) { throw "git commit failed (nothing to commit?)" }

    Step "git push"
    git push
    if ($LASTEXITCODE -ne 0) { throw "git push failed" }

    Step "Done."
}
catch {
    Write-Host ""
    Write-Host "Aborted: $($_.Exception.Message)"
    exit 1
}
