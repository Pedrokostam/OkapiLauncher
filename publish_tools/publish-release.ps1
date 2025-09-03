[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    $NoZip
)

$SolutionDir = Split-Path $PSScriptRoot -Parent
$ErrorActionPreference = 'Stop' # any error should stop the procedure

# VARIABLES
$outputInfoFile = Join-Path $PSScriptRoot 'output_info.json'
$PublishTools = Join-Path $SolutionDir 'publish_tools'
$innoCompilerPath = 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'
$AppName = 'Okapi Launcher'
$ProjectName = 'OkapiLauncher'
$ProjectDir = Join-Path $SolutionDir $ProjectName
$CsProjName = "$($ProjectName).csproj"
$AppExeName = "$($ProjectName).exe"
$ProjPath = Join-Path $ProjectDir $CsProjName
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
if (Test-Path $outputInfoFile) {
    Remove-Item -Force $outputInfoFile
}

# VERSION
[xml]$csproj = Get-Content $ProjPath
$version = $csproj.SelectSingleNode('/Project/PropertyGroup/Version').InnerText

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
