[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    $NoZip
)

$SolutionDir = Split-Path $PSScriptRoot -Parent                        # ./OkapiLauncher
$ErrorActionPreference = 'Stop'                                        # any error should stop the procedure

# VARIABLES
$PublishTools = Join-Path $SolutionDir 'publish_tools'                 # ./OkapiLauncher/publish_tools
$outputInfoFile = Join-Path $PublishTools 'output_info.json'           # ./OkapiLauncher/publish_tools/output_info.json
$innoCompilerPath = 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'     # {INNO}
$AppName = 'Okapi Launcher'                                            # Okapi Launcher
$ProjectName = 'OkapiLauncher'                                         # OkapiLauncher
$ProjectDir = Join-Path $SolutionDir $ProjectName                      # ./OkapiLauncher/OkapiLauncher
$CsProjName = "$($ProjectName).csproj"                                 # ./OkapiLauncher/OkapiLauncher.csproj
$AppExeName = "$($ProjectName).exe"                                    # ./OkapiLauncher/OkapiLauncher.exe
$ProjPath = Join-Path $ProjectDir $CsProjName                          # ./OkapiLauncher/OkapiLauncher/OkapiLauncher.csproj
$BaseOutputDir = Join-Path $SolutionDir 'Release'                      # ./OkapiLauncher/Release
$Binaries = Join-Path $BaseOutputDir $AppName                          # ./OkapiLauncher/Release/Okapi Lancher
$ExePath = Join-Path $Binaries $AppExeName                             # ./OkapiLauncher/Release/Okapi Lancher/OkapiLauncher.exe
$InnoScriptPath = Join-Path $PublishTools 'inno_installer_script.iss'  # 

## TEST PATHS
if (-not (Test-Path $innoCompilerPath)) {
    Write-Error 'Inno Setup is not installed'
}
if (Test-Path $BaseOutputDir) {
    Remove-Item -Recurse -Force $BaseOutputDir
}
if (Test-Path $outputInfoFile) {
    Remove-Item -Force $outputInfoFile
}

# GUID
$assemblyInfoPath = Join-Path $ProjectDir Properties AssemblyInfo.cs
$matchedGuid = Get-Item $assemblyInfoPath | Select-String -Pattern '^\s*\[assembly: Guid\("(?<guid>[\w-]+)"'
if ($matchedGuid) {
    $GUID = $matchedGuid.Matches[0].Groups['guid']
} else {
    Write-Error 'Could not find GUID of the app'
}

# REPO
$appSettingsPath = Join-Path $ProjectDir appsettings.json 
$json = Get-Content $appSettingsPath | ConvertFrom-Json
$repoUrl = $json.AppConfig.githubLink
    
# ---   DOTNET   ---
Write-Host ""
Write-Host "================== PERFORMING BUILD =================="
Write-Host ""
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

# VERSION
$exeItem = Get-Item $ExePath
$version = $exeItem.VersionInfo.FileVersion # Reads version from built exe. Assumes GitVersion is run as task during build.

# VARIABLES CONT'D
$ZipPath = Join-Path $BaseOutputDir "$($AppName)_$($Version).zip"

# ---   COMPRESS   ---
Write-Host ""
Write-Host "================== COMPRESSING =================="
Write-Host ""
if (-not $NoZip.IsPresent) {
    $compressParams = @{
        LiteralPath      = $Binaries
        DestinationPath  = $ZipPath
        CompressionLevel = 'Optimal'
    }
    Compress-Archive @compressParams
}

# ---   INNO   ---
Write-Host ""
Write-Host "================== CREATING INSTALLER =================="
Write-Host ""
$retries = 5
while ($true) {
    # repeat creation until there is no error
    $innoParams = @(
        "/DVERSION=$version"
        "/DAPPNAME=$AppName"
        "/DAPPEXENAME=$AppExeName"
        "/DBINDIR=$Binaries"
        "/DOUTDIR=$BaseOutputDir"
        "/DAPPURL=$repoUrl"
        "/DGUID=$Guid"
        $InnoScriptPath
    )
    $innoLog = ''
    . $innoCompilerPath @innoParams | Tee-Object -Variable innoLog
    $innoInstallerPath = $innoLog | Select-Object -Last 1 | Get-Item | Select-Object -ExpandProperty FullName
    if ($LASTEXITCODE -eq 0) {
        break
    } else {
        $retries--
        if ($retries -le 0) {
            Write-Error 'Could not compile installer'
        }
        Start-Sleep -Seconds 3
    }
}
@{
    ZippedBinaries = if ($NoZip.IsPresent) { $null } else { $ZipPath }
    Installer      = $innoInstallerPath
} | ConvertTo-Json > $outputInfoFile
