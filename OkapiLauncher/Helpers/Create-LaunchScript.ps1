<#
.SYNOPSIS
Creates a batch file that runs Okapi Launcher from an arbitrary location, preserving any parameters passed to it.

.DESCRIPTION
This script locates the executable of Okapi Launcher and generates a batch file
that launches it with any forwarded arguments. The batch file is written to a specified
destination with a default or custom script name.

.PARAMETER Destination
The path to the directory where the batch file will be created.

.PARAMETER ScriptName
The name of the batch file. Defaults to 'Okapi'. The batch file will always have a .bat extension.

.EXAMPLE
.\Create-LaunchScript.ps1 -Destination 'C:\Tools'

Creates Okapi.bat in C:\Tools.

C:\Tools\Okapi.bat <path to avproj>

Launches Okapi and makes it load the given .avproj.

.EXAMPLE
.\Create-LaunchScript.ps1 -Destination 'C:\Tools' -ScriptName 'RunApp'

Creates RunApp.bat in C:\Tools.

.NOTES
The batch file accepts all arguments and forwards them to the underlying executable.
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $Destination,
    [Parameter()]
    [string]
    $ScriptName = 'Okapi'
)
$scriptPath = Join-Path -Path $Destination -ChildPath $ScriptName
# Find executable
$exe = Get-ChildItem $PSScriptRoot/.. -Filter '*.exe' | Select-Object -First 1
if (-not $exe) {
    Write-Error "No executable found in the parent directory." -ErrorAction Stop
}

$scriptPath = [System.IO.Path]::ChangeExtension($scriptPath, '.bat')
$script = @"
@echo off

rem Check if Okapi exists in the location
if not exist "$($exe.FullName)" (
echo Could not find the Okapi executable. Please run Create-LaunchScript.ps1 again.
exit /b 1
)

rem Execute normally, without waiting
start "" "$($exe.FullName)" %*
exit /b
"@
Set-Content -LiteralPath $scriptPath -Value $script -Force -Encoding utf8
Write-Host "Created a batch script $scriptName at $scriptPath - call it from the command line to run the executable (accepts parameters)"
