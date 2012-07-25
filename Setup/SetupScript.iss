; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Lumen"
#define MyAppVersion "0.6.1"
#define MyAppPublisher "digiPHOTO.it"
#define MyAppURL "http://www.digiphoto.it/Lumen"
#define MyAppExeName "Digiphoto.Lumen.UI.exe"
#define MyConfExeName "Digiphoto.Lumen.GestoreConfigurazione.UI.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{8C3705BC-9065-4225-BB8F-9B9860357D88}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\digiPHOTO.Lumen
DefaultGroupName=digiPHOTO
DisableProgramGroupPage=yes
LicenseFile=..\Digiphoto.Lumen.UI\bin\Release\License_it.txt
OutputDir=C:\rilasci\Lumen
OutputBaseFilename=setup-{#MyAppName}-{#MyAppVersion}
Compression=lzma
SolidCompression=yes

[Languages]
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; --- Model ---
Source: "..\Digiphoto.Lumen.Model\bin\Release\Digiphoto.Lumen.Model.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Model\bin\Release\Digiphoto.Lumen.Model.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Model\bin\Release\EntityFramework.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Model\bin\Release\dbVuoto.sdf"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Model\bin\Release\dbVuoto.sqlite"; DestDir: "{app}"; Flags: ignoreversion
; --- Core ---
Source: "..\Digiphoto.Lumen.Core\bin\Release\Digiphoto.Lumen.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\bin\Release\Digiphoto.Lumen.Core.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\bin\Release\log4net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\bin\Release\MemBus.dll"; DestDir: "{app}"; Flags: ignoreversion
; --- Imaging ---                                                            
Source: "..\Digiphoto.Lumen.Imaging.Wic\bin\Release\Digiphoto.Lumen.Imaging.Wic.dll"; DestDir: "{app}"; Flags: ignoreversion
; --- Lumen ---
Source: "..\Digiphoto.Lumen.UI\bin\Release\Digiphoto.Lumen.UI.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.UI\bin\Release\Digiphoto.Lumen.UI.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.UI\bin\Release\Hardcodet.Wpf.TaskbarNotification.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.UI\bin\Release\System.Windows.Interactivity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.UI\bin\Release\WPFToolkit.Extended.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.UI\bin\Release\Reports\*"; DestDir: "{app}\Reports"; Flags: ignoreversion 
; --- Configuratore ---
Source: "..\Digiphoto.Lumen.GestoreConfigurazione.UI\bin\Release\Digiphoto.Lumen.GestoreConfigurazione.UI.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.GestoreConfigurazione.UI\bin\Release\Digiphoto.Lumen.GestoreConfigurazione.UI.exe.config"; DestDir: "{app}"; Flags: ignoreversion

; --- Driver sql x86 ---
Source: "..\packages\System.Data.SQLite.1.0.81.0\lib\net40\System.Data.SQLite.dll"; DestDir: "{app}";  Check: "not IsWin64"; Flags: ignoreversion
Source: "..\packages\System.Data.SQLite.1.0.81.0\lib\net40\System.Data.SQLite.Linq.dll"; DestDir: "{app}"; Check: "not IsWin64"; Flags: ignoreversion
; --- Driver sql x64 ---
Source: "..\packages\System.Data.SQLite.x64.1.0.81.0\lib\net40\System.Data.SQLite.dll"; DestDir: "{app}";  Check: "IsWin64"; Flags: ignoreversion
Source: "..\packages\System.Data.SQLite.x64.1.0.81.0\lib\net40\System.Data.SQLite.Linq.dll"; DestDir: "{app}"; Check: "IsWin64"; Flags: ignoreversion


; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{#MyAppName} Configurazione"; Filename: "{app}\{#MyConfExeName}"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyConfExeName}"; Description: "lancia il gestore della configurazione"; Flags: nowait postinstall skipifsilent runascurrentuser

