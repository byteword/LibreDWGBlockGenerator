param(
    [Parameter(Mandatory = $true)]
    [string] $NativeLibrary,

    [string] $Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$nativePath = [System.IO.Path]::GetFullPath($NativeLibrary)
if (-not (Test-Path -LiteralPath $nativePath -PathType Leaf)) {
    throw "Native backend not found: $nativePath"
}

$artifactDirectory = Join-Path $repositoryRoot "artifacts/native-verification"
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
$outputPath = Join-Path $artifactDirectory "static-r2000-rectangle.dwg"
if (Test-Path -LiteralPath $outputPath) {
    Remove-Item -LiteralPath $outputPath -Force
}

$env:LIBREDWG_BLOCK_GENERATOR_NATIVE = $nativePath
dotnet run `
    --project (Join-Path $repositoryRoot "src/LibreDWG.BlockGenerator.Cli") `
    --configuration $Configuration `
    --no-build `
    -- generate `
    --input (Join-Path $repositoryRoot "examples/static-r2000-rectangle.json") `
    --output $outputPath
if ($LASTEXITCODE -ne 0) {
    throw "Static R2000 generation failed with exit code $LASTEXITCODE."
}

$file = Get-Item -LiteralPath $outputPath
if ($file.Length -le 0) {
    throw "Generated DWG is empty."
}

[pscustomobject]@{
    output = $file.FullName
    bytes = $file.Length
    sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
} | ConvertTo-Json -Compress

