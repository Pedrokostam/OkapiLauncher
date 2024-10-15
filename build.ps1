Push-Location $PSScriptRoot
try {
    $ProjPath = "AuroraVisionLauncher/AuroraVisionLauncher.csproj"
    $PubFilePath = "AuroraVisionLauncher/AuroraVisionLauncher/Properties/PublishProfiles/FolderProfile.pubxml"
    dotnet publish $ProjPath -c Release  "/p:PublishProfile=$PubFilePath"
}finally{

}
Pop-Location