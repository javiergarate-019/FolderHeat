param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "src\FolderHeat.App\FolderHeat.App.csproj"
$artifactsPath = Join-Path $repoRoot "artifacts"
$publishPath = Join-Path $artifactsPath "publish\FolderHeat-$Runtime"
$zipPath = Join-Path $artifactsPath "FolderHeat-portable-$Runtime.zip"

New-Item -ItemType Directory -Force -Path $artifactsPath | Out-Null

if (Test-Path $publishPath) {
    Remove-Item -LiteralPath $publishPath -Recurse -Force
}

$publishArgs = @(
    "publish",
    $projectPath,
    "--configuration",
    $Configuration,
    "--runtime",
    $Runtime,
    "--self-contained",
    "true",
    "--output",
    $publishPath,
    "-p:PublishSingleFile=false",
    "-p:PublishReadyToRun=false",
    "-p:GeneratePortableZip=false"
)

if ($SkipBuild) {
    $publishArgs += "--no-build"
}

dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

if (Test-Path $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $publishPath "*") -DestinationPath $zipPath -Force
Write-Host "Portable ZIP: $zipPath"
