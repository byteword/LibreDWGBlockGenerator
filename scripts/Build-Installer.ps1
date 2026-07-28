[CmdletBinding()]
param(
    [string] $LibreDwgSourceDir = $env:LIBREDWG_SOURCE_DIR,
    [string] $LibreDwgBuildDir = $env:LIBREDWG_BINARY_DIR,
    [string] $CMakePath = "",
    [string] $InnoCompilerPath = "",
    [switch] $SkipInstallerCompile
)

$ErrorActionPreference = "Stop"
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..")).TrimEnd("\")
$artifactsRoot = Join-Path $repositoryRoot "artifacts"
$packageRoot = Join-Path $artifactsRoot "installer-package"
$payloadRoot = Join-Path $packageRoot "payload"
$nativeBuildRoot = Join-Path $artifactsRoot "installer-native-build"
$installerOutput = Join-Path $artifactsRoot "installer"

if ([string]::IsNullOrWhiteSpace($LibreDwgSourceDir)) {
    $LibreDwgSourceDir = Join-Path $repositoryRoot ".local/libredwg-0.13.4"
}
if ([string]::IsNullOrWhiteSpace($LibreDwgBuildDir)) {
    $LibreDwgBuildDir = Join-Path $LibreDwgSourceDir "build-msvc"
}
$LibreDwgSourceDir = [System.IO.Path]::GetFullPath($LibreDwgSourceDir)
$LibreDwgBuildDir = [System.IO.Path]::GetFullPath($LibreDwgBuildDir)

if (-not (Test-Path -LiteralPath (Join-Path $LibreDwgSourceDir "include/dwg.h"))) {
    throw "LibreDWG source was not found: $LibreDwgSourceDir"
}
if (-not (Test-Path -LiteralPath (Join-Path $LibreDwgBuildDir "Release/libredwg.lib")) -or
    -not (Test-Path -LiteralPath (Join-Path $LibreDwgBuildDir "Release/libredwg.dll"))) {
    throw "A Release LibreDWG build was not found: $LibreDwgBuildDir"
}

if ([string]::IsNullOrWhiteSpace($CMakePath)) {
    $cmakeCandidates = @(
        "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe",
        "C:\Program Files\CMake\bin\cmake.exe"
    )
    $CMakePath = $cmakeCandidates |
        Where-Object { Test-Path -LiteralPath $_ } |
        Select-Object -First 1
}
if ([string]::IsNullOrWhiteSpace($CMakePath) -or -not (Test-Path -LiteralPath $CMakePath)) {
    throw "CMake was not found. Pass -CMakePath."
}

$versionProps = [xml](Get-Content -LiteralPath (Join-Path $repositoryRoot "Directory.Build.props") -Raw)
$appVersion = [string]$versionProps.Project.PropertyGroup.FileVersion
if ($appVersion -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    throw "FileVersion must use MAJOR.MINOR.PATCH.BUILD: $appVersion"
}

if (Test-Path -LiteralPath $packageRoot) {
    $resolvedPackage = [System.IO.Path]::GetFullPath($packageRoot)
    $resolvedArtifacts = [System.IO.Path]::GetFullPath($artifactsRoot).TrimEnd("\") + "\"
    if (-not $resolvedPackage.StartsWith($resolvedArtifacts, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Unsafe package cleanup path: $resolvedPackage"
    }
    Remove-Item -LiteralPath $resolvedPackage -Recurse -Force
}
New-Item -ItemType Directory -Path $payloadRoot -Force | Out-Null
New-Item -ItemType Directory -Path $nativeBuildRoot -Force | Out-Null
New-Item -ItemType Directory -Path $installerOutput -Force | Out-Null

$cliProject = Join-Path $repositoryRoot "src/LibreDWG.BlockGenerator.Cli/LibreDWG.BlockGenerator.Cli.csproj"
& dotnet restore $cliProject -r win-x64 --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw "CLI win-x64 restore failed." }
& dotnet publish $cliProject `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --no-restore `
    --output $payloadRoot `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false `
    -p:PublishReadyToRun=false
if ($LASTEXITCODE -ne 0) { throw "CLI publish failed." }

& $CMakePath `
    "-S$(Join-Path $repositoryRoot 'native')" `
    "-B$nativeBuildRoot" `
    -A x64 `
    "-DLIBREDWG_SOURCE_DIR=$($LibreDwgSourceDir.Replace('\', '/'))" `
    "-DLIBREDWG_BINARY_DIR=$($LibreDwgBuildDir.Replace('\', '/'))"
if ($LASTEXITCODE -ne 0) { throw "Native adapter CMake configure failed." }
& $CMakePath --build $nativeBuildRoot --config Release --parallel 4
if ($LASTEXITCODE -ne 0) { throw "Native adapter build failed." }

$nativeOutput = Join-Path $nativeBuildRoot "Release"
foreach ($fileName in @("lbg_native.dll", "libredwg.dll")) {
    $source = Join-Path $nativeOutput $fileName
    if (-not (Test-Path -LiteralPath $source)) {
        throw "Native payload file was not generated: $source"
    }
    Copy-Item -LiteralPath $source -Destination $payloadRoot -Force
}
Copy-Item -LiteralPath (Join-Path $repositoryRoot "LICENSE") -Destination $payloadRoot -Force

$payloadFiles = Get-ChildItem -LiteralPath $payloadRoot -File -Recurse |
    Sort-Object FullName |
    ForEach-Object {
        [ordered]@{
            relativePath = [System.IO.Path]::GetRelativePath($payloadRoot, $_.FullName).Replace("\", "/")
            length = $_.Length
            sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        }
    }
[ordered]@{
    schemaVersion = 1
    product = "LDBGen"
    version = $appVersion
    runtimeIdentifier = "win-x64"
    files = @($payloadFiles)
} | ConvertTo-Json -Depth 5 |
    Set-Content -LiteralPath (Join-Path $packageRoot "payload-manifest.json") -Encoding utf8NoBOM

$publishedExe = Get-Item -LiteralPath (Join-Path $payloadRoot "libredwg-block-generator.exe")
if ($publishedExe.VersionInfo.FileVersion -ne $appVersion -or
    $publishedExe.VersionInfo.ProductVersion -ne $appVersion) {
    throw "Published executable version does not match $appVersion."
}

if ($SkipInstallerCompile) {
    [pscustomobject]@{
        Payload = $payloadRoot
        Manifest = Join-Path $packageRoot "payload-manifest.json"
        Version = $appVersion
        Files = $payloadFiles.Count
    } | Format-List
    exit 0
}

if ([string]::IsNullOrWhiteSpace($InnoCompilerPath)) {
    $innoCandidates = @(
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe"
    )
    $InnoCompilerPath = $innoCandidates |
        Where-Object { Test-Path -LiteralPath $_ } |
        Select-Object -First 1
}
if ([string]::IsNullOrWhiteSpace($InnoCompilerPath) -or
    -not (Test-Path -LiteralPath $InnoCompilerPath)) {
    throw "Inno Setup 6 was not found. Pass -InnoCompilerPath or use -SkipInstallerCompile."
}

& $InnoCompilerPath `
    /Qp `
    "/DSourceRoot=$payloadRoot" `
    "/DOutputDir=$installerOutput" `
    "/DRepositoryRoot=$repositoryRoot" `
    "/DAppVersion=$appVersion" `
    (Join-Path $repositoryRoot "installer/LDBGen.iss")
if ($LASTEXITCODE -ne 0) { throw "Inno Setup compile failed." }

$setupPath = Join-Path $installerOutput "LDBGen-$appVersion-UserSetup.exe"
if (-not (Test-Path -LiteralPath $setupPath)) {
    throw "Installer was not generated: $setupPath"
}
$setup = Get-Item -LiteralPath $setupPath
[pscustomobject]@{
    Installer = $setup.FullName
    Version = $appVersion
    Size = $setup.Length
    Sha256 = (Get-FileHash -LiteralPath $setup.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
} | Format-List
