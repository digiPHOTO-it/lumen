; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyApp1Name     "Lumen-Self-Service"
#define MyApp2Name     "Lumen-Slide-Show"
#define MyAppVersion   "3.2.rc3"
#define MyAppPublisher "digiPHOTO.it"
#define MyAppURL       "http://www.digiphoto.it/lumen-self-service"
#define MyApp1ExeName   "Digiphoto.Lumen.SelfService.MobileUI.exe"
#define MyApp2ExeName   "Digiphoto.Lumen.SelfService.SlideShow.exe"
#define dirPLat        "bin\Release"



[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{467985CF-D520-40C6-9275-396F81E4A19C}
AppName={#MyApp1Name}
AppVersion={#MyAppVersion}
;AppVerName={#MyApp1Name} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\digiPHOTO\Lumen.SelfService
DefaultGroupName=digiPHOTO
DisableProgramGroupPage=yes
LicenseFile=..\Digiphoto.Lumen.UI\{#dirPlat}\License_it.txt
OutputDir=C:\rilasci\Lumen
OutputBaseFilename=setup-{#MyApp1Name}-{#MyAppVersion}
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
Source: "..\Digiphoto.Lumen.SelfService.SlideShow\{#dirPlat}\Digiphoto.Lumen.SelfService.SlideShow.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.SelfService.SlideShow\{#dirPlat}\Digiphoto.Lumen.SelfService.SlideShow.exe.config"; DestDir: "{app}"; Flags: uninsneveruninstall onlyifdoesntexist

; --- dipendenze ---                                                            
Source: "..\Digiphoto.Lumen.PresentationFramework\{#dirPlat}\Digiphoto.Lumen.PresentationFramework.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\{#dirPlat}\Digiphoto.Lumen.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\{#dirPlat}\Digiphoto.Lumen.Core.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\{#dirPlat}\log4net.dll"; DestDir: "{app}"; Flags: ignoreversion


[Icons]
Name: "{group}\{#MyApp1Name}"; Filename: "{app}\{#MyApp1ExeName}"
Name: "{commondesktop}\{#MyApp1Name}"; Filename: "{app}\{#MyApp1ExeName}"; Tasks: desktopicon
Name: "{commondesktop}\{#MyApp2Name}"; Filename: "{app}\{#MyApp2ExeName}"; Tasks: desktopicon
