$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot 'RoboCopyManager\RoboCopyManager.csproj'
$publishDir = Join-Path $repoRoot 'artifacts\installer-publish'
$outputDir = Join-Path $repoRoot 'artifacts'
$appExe = Join-Path $publishDir 'RoboCopyManager.exe'
$installerExe = Join-Path $outputDir 'RoboCopyManager-Setup.exe'
$installerSource = Join-Path $PSScriptRoot 'RoboCopyManagerInstaller.cs'
$manifest = Join-Path $PSScriptRoot 'RoboCopyManager.Setup.manifest'
$icon = Join-Path $repoRoot 'RoboCopyManager\RoboCopyManager.ico'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'

if (-not (Test-Path $csc)) {
    throw "C# compiler not found: $csc"
}

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

dotnet publish $project --configuration Release --runtime win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    --output $publishDir
if ($LASTEXITCODE -ne 0) {
    throw 'Publishing the application failed.'
}

if (-not (Test-Path $appExe)) {
    throw "Published app executable not found: $appExe"
}

& $csc /nologo /target:winexe "/out:$installerExe" "/win32manifest:$manifest" "/win32icon:$icon" `
    /reference:System.Windows.Forms.dll /reference:System.Drawing.dll `
    "/resource:$appExe,RoboCopyManager.exe" "/resource:$icon,InstallerIcon.ico" `
    $installerSource
if ($LASTEXITCODE -ne 0) {
    throw 'Compiling the setup wizard failed.'
}

Write-Output "Setup created: $installerExe"
