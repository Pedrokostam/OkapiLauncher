[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    $NoZip
)
$params =@(
     '-file' 
     "$PSScriptRoot\publish_tools\publish-release.ps1"
)
if($NoZip.IsPresent){
    $params+=@('-NoZip')
}
pwsh @params