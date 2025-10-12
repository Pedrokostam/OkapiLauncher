[CmdletBinding()]
param (
     [Parameter(Mandatory)]
    [string]
    $Version,
    [Parameter()]
    [switch]
    $NoZip
)
$params =@(
    '-noprofile'
     '-file'
     "$PSScriptRoot\publish_tools\publish-release.ps1 -Version $Version"
)
if($NoZip.IsPresent){
    $params+=@('-NoZip')
}
pwsh @params