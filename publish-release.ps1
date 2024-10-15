Push-Location $PSScriptRoot
try {
    $BaseOutputDir = 'Release'
    if (Test-Path $BaseOutputDir) {
        Remove-Item -Recurse -Force $BaseOutputDir
    }
    $AppName = 'AuroraVisionLauncher'
    $Binaries = Join-Path $BaseOutputDir $AppName
    $ProjPath = 'AuroraVisionLauncher/AuroraVisionLauncher.csproj'
    [xml]$csproj = Get-Content $ProjPath
    $version = $csproj.SelectSingleNode('/Project/PropertyGroup/Version').InnerText
    $ZipPath = Join-Path $BaseOutputDir "$($AppName)_$($Version).zip"
    dotnet publish $ProjPath -o $Binaries --arch 'x64' --configuration Release --no-self-contained
    Compress-Archive $Binaries $ZipPath -CompressionLevel Optimal
    Invoke-Item $BaseOutputDir
} finally {
    Pop-Location
}