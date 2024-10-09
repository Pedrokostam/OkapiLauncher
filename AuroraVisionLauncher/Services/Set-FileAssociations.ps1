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
trap{
    Write-Host -ForegroundColor Red "Encountered error during script!"
    Write-Host 
    $logLocation = Join-Path (Split-Path $MainAppExecutablePath) "Log_Associations.log"
    $error[0] | Out-File -FilePath $logLocation -Encoding utf8
    $error[0] 
    Write-Host 
    Write-Host -ForegroundColor Red "Error copied to $logLocation."
    Read-Host 'Press Enter to leave, hopefully informing the developer about the error'
    break
}
function Get-Hash([string]$value) {
    $byteArray = [System.Text.Encoding]::UTF8.GetBytes($value)
    $md5 = [System.Security.Cryptography.MD5]::Create()
    $hash = $md5.ComputeHash($byteArray)
    $hashString = -join ($hash | ForEach-Object { '{0:x2}' -f $_ })
    $hashString
}

$passwordGood = (Get-Hash $RegistryAppName) -eq $KeyPhrase
if (-not $passwordGood) {
    Write-Error 'Do not call this script manually!' -ErrorAction Stop
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

function Format-NewItem{
    [CmdletBinding()]
    param (
        [Parameter(ValueFromPipeline)]
        $item,
        [Parameter()]
        $base
    )
    process {
        if ($base) {
            $Base = $base.replace('HKCU:', 'HKEY_CURRENT_USER')
        } else {
            $base = ' '
        }
        $name = $item.Name
        $prop = $item.property
        if ($prop -eq '(default)') {
            $prop = ''
        }
        $propVal = $item.GetValue($prop)
        $subkeyname = ($name.replace($base, ''))
        if ($prop -eq '') {
            if ($subkeyname) {
                Write-Host -foreground yellow 'Subkey: ' -NoNewline
                Write-Host ($name.replace($base, ''))
            }
            Write-Host -foreground green 'Value: ' -NoNewline
            Write-Host "$propVal"
        } else {
            if ($subkeyname) {
                Write-Host -foreground yellow 'Subkey: ' -NoNewline
                Write-Host ($name.replace($base, ''))
            }
            Write-Host -foreground green "$prop`: " -NoNewline
            Write-Host "$propVal"
        }
    }
}

function Remove-KeyRobust([string]$path) {
    if (Test-Path $path) {
        $netPath = (Split-Path $path -NoQualifier).Trim('\')
        try {
            Remove-Item $path -Recurse -ErrorAction Stop
        } catch {
            $key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($netPath, $true)
            $subkeyNames = $key.GetSubKeyNames()
            foreach ($subkeyName in $subkeyNames) {
                Write-Host "Deleting $subkeyName"
                $key.DeleteSubKey($subkeyName, $false)
            }
            $key.Close()
            Remove-Item $path 
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
    Remove-KeyRobust $registryKeyPath 

    Write-Host 'Created key: ' -NoNewline -ForegroundColor Magenta
    Write-Host $registryKeyPath

    $defaultIconRegistryPath = concat $registryKeyPath DefaultIcon
    $iconPath = "$($Association.IconPath),0"
    New-Item $defaultIconRegistryPath -Force -Value $iconPath | Format-NewItem -base $registryKeyPath
    
    $command = "`"$MainAppExecutablePath`" `"%1`""
    $commandOpenSubkeyPath = concat $registryKeyPath shell open command
    New-Item $commandOpenSubkeyPath -Force -Value $command | Format-NewItem -base $registryKeyPath
}

function Set-Association {
    [CmdletBinding()]
    param (
        [Parameter()]
        [RegistryAssocation]
        $Association
    )
    $registryName = $RegistryAppName + $Association.Extension
    $registryKeyPath = concat HKCU: Software Classes $Association.Extension
    Remove-KeyRobust $registryKeyPath
    Write-Host 'Created key: ' -NoNewline -ForegroundColor Magenta
    Write-Host $registryKeyPath
    New-Item $registryKeyPath -Force -Value $registryName | Format-NewItem -base $registryKeyPath
}
Write-Host "----  Removing previous associations  ----`n" -ForegroundColor Cyan
foreach ($association in $Associations) {
    Write-Host "$($association.Extension)" -NoNewline
    $null = Remove-ExplorerAssociation $association
    Write-Host ' - Gone'
}
Write-Host "`n--------  Setting app shell keys ---------`n" -foreground Cyan
foreach ($association in $Associations) {
    Set-AppShellKey $association
}
Write-Host "`n---------  Setting associations  ---------`n" -ForegroundColor Cyan
foreach ($association in $Associations) {
    Set-Association $association
}
