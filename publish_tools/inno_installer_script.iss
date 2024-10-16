; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define RESOURCES "installer_resources"
;#define MyAppPublisher "VanCorp"
;#define APPEXENAME "AuroraVisionLauncher.exe"
;#define APPURL "https://github.com/Pedrokostam/AuroraVisionLauncher"
;#define APPNAME "AuroraVisionLauncher"
;#define VERSION "0.0.0.0"
;#define BINDIR ""
;#define OUTDIR ""

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{2C508297-5294-4681-BB5E-D2B20F6FC4AB}
OutputDir={#OUTDIR}
AppName={#APPNAME}
AppVersion={#VERSION}
AppVerName={#APPNAME} {#VERSION}
;AppPublisher={#MyAppPublisher}
AppPublisherURL={#APPURL}
AppSupportURL={#APPURL}
AppUpdatesURL={#APPURL}
DefaultDirName={autopf}\{#APPNAME}
DefaultGroupName={#APPNAME}

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

AllowNoIcons=yes

;WizardImageFile={#RESOURCES}/images/panel.bmp
;WizardSmallImageFile={#RESOURCES}/images/icon.bmp

LicenseFile=..\LICENSE.txt
;InfoBeforeFile=C:\Users\Pedro\source\repos\AuroraVisionLauncher\LICENSE.txt
;InfoAfterFile=C:\Users\Pedro\source\repos\AuroraVisionLauncher\LICENSE.txt

; Uncomment the following line to run in non administrative install mode (install for current user only.)
PrivilegesRequiredOverridesAllowed=dialog
OutputBaseFilename={#APPNAME} {#VERSION} installer
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#BINDIR}\{#APPEXENAME}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BINDIR}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#APPNAME}"; Filename: "{app}\{#APPEXENAME}"
Name: "{group}\{cm:UninstallProgram,{#APPNAME}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#APPNAME}"; Filename: "{app}\{#APPEXENAME}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#APPEXENAME}"; Description: "{cm:LaunchProgram,{#StringChange(APPNAME, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeUninstall(): Boolean;
  var ErrorCode: Integer;
BEGIN
    result := True;
    IF ShellExec('open','tasklist.exe','/fi /im "ImageName eq {#APPEXENAME}"','',SW_HIDE,ewNoWait,ErrorCode) THEN
    BEGIN
        IF (MsgBox('The app will be closed before uninstalling. Continue?', mbConfirmation, MB_YESNO) = IDYES) THEN
        BEGIN
            ShellExec('open','taskkill.exe','/f /im "{#APPEXENAME}"','',SW_HIDE,ewNoWait,ErrorCode);
            Exit;
        END
        ELSE
        BEGIN
            MsgBox('Uninstallation interrupted.',mbError,MB_OK);
            result := False;
            Exit;
        END;
    END;
END;