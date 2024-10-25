[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    $NoZip
)
$params =@(
    '-noprofile'
     '-file'
     "$PSScriptRoot\publish_tools\publish-release.ps1"
)
if($NoZip.IsPresent){
    $params+=@('-NoZip')
}
pwsh @params