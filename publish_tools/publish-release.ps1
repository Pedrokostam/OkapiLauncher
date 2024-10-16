[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    $NoZip
)

$SolutionDir = Split-Path $PSScriptRoot -Parent
$ErrorActionPreference = 'Stop' # any error should stop the procedure

# VARIABLES
$PublishTools = Join-Path $SolutionDir 'publish_tools'
$innoCompilerPath = 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'
$AppName = 'AuroraVisionLauncher'
$ProjPath = Join-Path $SolutionDir 'AuroraVisionLauncher' 'AuroraVisionLauncher.csproj'
$BaseOutputDir = Join-Path $SolutionDir 'Release'
$Binaries = Join-Path $BaseOutputDir $AppName
$InnoScriptPath = Join-Path $PublishTools 'inno_installer_script.iss'

## TEST PATHS
if (-not (Test-Path $innoCompilerPath)) {
    Write-Error 'Inno Setup is not installed'
}
if (Test-Path $BaseOutputDir) {
    Remove-Item -Recurse -Force $BaseOutputDir
}

# VERSION
[xml]$csproj = Get-Content $ProjPath
$version = $csproj.SelectSingleNode('/Project/PropertyGroup/Version').InnerText

# VARIABLES CONT'D
$ZipPath = Join-Path $BaseOutputDir "$($AppName)_$($Version).zip"
    
# ---   DOTNET   ---
$dotnetParams = @(
    'publish'
    $ProjPath
    '-o'
    $Binaries
    '--arch'
    'x64'
    '--configuration'
    'Release'
    '--no-self-contained'
    '/p:WarningLevel=0'
)
dotnet @dotnetParams

# ---   COMPRESS   ---
if (-not $NoZip.IsPresent) {
    $compressParams = @{
        LiteralPath      = $Binaries
        DestinationPath  = $ZipPath
        CompressionLevel = 'Optimal'
    }
    Compress-Archive @compressParams
}
# ---   INNO   ---
while ($true) {
    # repeat creation until there is no error
    $innoParams = @(
        "/DVERSION=$version"
        "/DAPPNAME=$AppName"
        "/DBINDIR=$Binaries"
        "/DOUTDIR=$BaseOutputDir"
        $InnoScriptPath
    )
    . $innoCompilerPath @innoParams /V10
    if ($LASTEXITCODE -eq 0) {
        break
    }else{
        start-sleep -Seconds 1
    }
}
