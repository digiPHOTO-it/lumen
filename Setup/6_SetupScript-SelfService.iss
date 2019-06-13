; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName      "Lumen-Self-Service"
#define MyAppVersion   "3.2.rc1"
#define MyAppPublisher "digiPHOTO.it"
#define MyAppURL       "http://www.digiphoto.it/lumen-self-service"
#define MyAppExeName   "Digiphoto.Lumen.SelfService.MobileUI.exe"
#define dirPLat        "bin\Release"



[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{467985CF-D520-40C6-9275-396F81E4A19C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\digiPHOTO\Lumen.SelfService
DefaultGroupName=digiPHOTO
DisableProgramGroupPage=yes
LicenseFile=..\Digiphoto.Lumen.UI\{#dirPlat}\License_it.txt
OutputDir=C:\rilasci\Lumen
OutputBaseFilename=setup-{#MyAppName}-{#MyAppVersion}
Compression=lzma
SolidCompression=yes


[Languages]
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; --- mobile UI
Source: "..\Digiphoto.Lumen.SelfService.MobileUI\{#dirPlat}\Digiphoto.Lumen.SelfService.MobileUI.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.SelfService.MobileUI\{#dirPlat}\Digiphoto.Lumen.SelfService.MobileUI.exe.config"; DestDir: "{app}"; Flags: uninsneveruninstall onlyifdoesntexist

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon


