[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $KeyPhrase,
    [Parameter(Mandatory)]
    [string]
    $RegistryAppName,
    [Parameter(Mandatory)]
    [string]
    $MainAppExecutablePath,
    [Parameter(Mandatory)]
    [string]
    $ProtoAssociations   
)
try {

    function Get-Hash([string]$value) {
        $byteArray = [System.Text.Encoding]::UTF8.GetBytes($value)
        $md5 = [System.Security.Cryptography.MD5]::Create()
        $hash = $md5.ComputeHash($byteArray)
        $hashString = -join ($hash | ForEach-Object { '{0:x2}' -f $_ })
        $hashString
    }

    $passwordGood = (Get-Hash $RegistryAppName) -eq $KeyPhrase
    if (-not $passwordGood) {
        # Write-Error 'Do not call this script manually!' -ErrorAction Stop
    }


    class RegistryAssocation {
        [string]$IconPath
        [string]$Extension
    
        RegistryAssocation($jsonObject) {
            $this.Extension = $jsonObject.Extension
            $this.IconPath = $jsonObject.IconPath
        }
    }
    Set-Clipboard $ProtoAssociations
    $jsons = $ProtoAssociations | ConvertFrom-Json
    $Associations = foreach ($json in $jsons) {
        [RegistryAssocation]::new($json)
    }
    $VerbosePreference='Continue'
    # $WhatIfPreference = $true
    # exit
    # Write-Error 'Do not call this script manually!' -ErrorAction Stop

    if ($PSVersionTable.PSVersion.Major -lt 7) {
        function concat {
            [CmdletBinding()]
            param (
                [Parameter()]
                [string]
                $Path,
                [Parameter()]
                [string]
                $ChildPath,
                [Parameter(ValueFromRemainingArguments)]
                [string[]]
                $Additionals
            )
            $restParts = @($ChildPath) + $Additionals
            $joiningPath = $Path
            foreach ($subPart in $restParts) {
                $joiningPath = Join-Path -Path $joiningPath -ChildPath $subPart
            }
            $joiningPath
        }
    } else {
        function concat {
            [CmdletBinding()]
            param (
                [Parameter()]
                [string]
                $Path,
                [Parameter()]
                [string]
                $ChildPath,
                [Parameter(ValueFromRemainingArguments)]
                [string[]]
                $Additionals
            )
            Join-Path -Path $Path -ChildPath $ChildPath -AdditionalChildPath $Additionals
        }
    }

    function Remove-KeyRobust([string]$path) {
        if (Test-Path $path) {
            $netPath = (Split-Path $path -NoQualifier).Trim('\')
            try {
                Remove-Item $path -Recurse -Verbose -ErrorAction Stop
            } catch {
                $key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($netPath, $true)
                $subkeyNames = $key.GetSubKeyNames()
                foreach ($subkeyName in $subkeyNames) {
                    Write-Host "Deleting $subkeyName"
                    $key.DeleteSubKey($subkeyName, $false)
                }
                $key.Close()
                Remove-Item $path -Verbose
            }
        }
    }

    function Remove-ExplorerAssociation {
        [CmdletBinding()]
        param (
            [Parameter()]
            [RegistryAssocation]
            $Association
        )
        $path = concat HKCU: Software Microsoft Windows CurrentVersion Explorer FileExts $Association.Extension
        Remove-KeyRobust $path
    }

    function Set-AppShellKey {
        [CmdletBinding()]
        param (
            [Parameter()]
            [RegistryAssocation]
            $Association
        )
        $registryName = $RegistryAppName + $Association.Extension
        $registryKeyPath = concat HKCU: Software Classes $registryName
        Remove-KeyRobust $registryKeyPath -Verbose

        $defaultIconRegistryPath = concat $registryKeyPath DefaultIcon
        $iconPath = "$($Association.IconPath),0"
        New-Item $defaultIconRegistryPath -Force -Value $iconPath -Verbose
    
        $command = "`"$MainAppExecutablePath`" `"%1`""
        $commandOpenSubkeyPath = concat $registryKeyPath shell open command
        New-Item $commandOpenSubkeyPath -Force -Value $command -Verbose
    }

    function Set-Association {
        [CmdletBinding()]
        param (
            [Parameter()]
            [RegistryAssocation]
            $Association
        )
        $registryName = $RegistryAppName + $Association.Extension
        $keyPath = concat HKCU: Software Classes $Association.Extension
        Remove-KeyRobust $keyPath
        New-Item $keyPath -Force -Value $registryName -Verbose
    }
    foreach ($association in $Associations) {
        Remove-ExplorerAssociation $association
        Set-AppShellKey $association
        Set-Association $association
    }
} catch {
    $error
}