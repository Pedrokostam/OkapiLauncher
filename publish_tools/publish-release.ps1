Push-Location $PSScriptRoot
try {
    $ErrorActionPreference='Stop'

    $innoCompilerPath = 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'
    $AppName = 'AuroraVisionLauncher'
    $ProjPath = '../AuroraVisionLauncher/AuroraVisionLauncher.csproj'

    if (-not (Test-Path $innoCompilerPath)) {
        Write-Error 'Inno Setup is not installed'
    }
    $BaseOutputDir = '../Release'
    if (Test-Path $BaseOutputDir) {
        Remove-Item -Recurse -Force $BaseOutputDir
    }

    $Binaries = Join-Path $BaseOutputDir $AppName

    [xml]$csproj = Get-Content $ProjPath
    $version = $csproj.SelectSingleNode('/Project/PropertyGroup/Version').InnerText

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
    $compressParams = @{
        LiteralPath      = $Binaries
        DestinationPath  = $ZipPath
        CompressionLevel = 'Optimal'
    }
    Compress-Archive @compressParams

    # ---   INNO   ---
    $innoParams = @(
        "/DVERSION=$version"
        "/DAPPNAME=$AppName"
        "/DBINDIR=$Binaries"
        "/DOUTDIR=$BaseOutputDir"
        'inno_installer_scripts.iss'
    )
    . $innoCompilerPath @innoParams

} finally {
    Pop-Location
}