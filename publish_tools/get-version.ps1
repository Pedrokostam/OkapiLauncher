$SolutionDir = Split-Path $PSScriptRoot -Parent
$ErrorActionPreference = 'Stop' # any error should stop the procedure

# VARIABLES
$AppName = 'Okapi Launcher'
$ProjectName = 'OkapiLauncher'
$ProjectDir = Join-Path $SolutionDir $ProjectName
$CsProjName = "$($ProjectName).csproj"
$AppExeName = "$($ProjectName).exe"
$ProjPath = Join-Path $ProjectDir $CsProjName
$BaseOutputDir = Join-Path $SolutionDir 'Release'
$Binaries = Join-Path $BaseOutputDir $AppName
$ExePath = Join-Path $Binaries "$ProjectName.exe"

# VERSION
$exeItem = Get-Item $ExePath
$version = $exeItem.VersionInfo.FileVersionRaw # Reads version from built exe. Assumes GitVersion is run as task during build.
"$($version.Major).$($version.Minor).$($version.Build)"
