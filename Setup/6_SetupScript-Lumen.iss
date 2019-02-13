; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName       "Lumen"
#define MyAppVersion    "3.0-beta2"
#define MyAppPublisher  "digiPHOTO.it"
#define MyAppURL        "http://www.digiphoto.it/Lumen"
#define MyAppExeName    "Digiphoto.Lumen.UI.exe"
#define MyConfExeName   "Digiphoto.Lumen.GestoreConfigurazione.UI.exe"
#define SSHostExeName   "Digiphoto.Lumen.SelfService.HostConsole.exe" 
#define OnRideUIExeName "Digiphoto.Lumen.OnRide.UI.exe" 
#define dirPLat         "bin\Release"



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
DefaultDirName={pf}\digiPHOTO\Lumen
DefaultGroupName=digiPHOTO
DisableProgramGroupPage=yes
LicenseFile=..\Digiphoto.Lumen.UI\{#dirPlat}\License_it.txt
OutputDir=C:\rilasci\Lumen
OutputBaseFilename=setup-{#MyAppName}-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
UsePreviousAppDir=no


[Languages]
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "DisableAuoRun"; Description: "Disabilitare Auto-Run sui drive rimovibili"; 

[Dirs]
Name: {commonappdata}\{#MyAppPublisher}\{#MyAppName}

[Files]
; --- Model ---
Source: "..\Digiphoto.Lumen.Model\{#dirPlat}\Digiphoto.Lumen.Model.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Model\{#dirPlat}\Digiphoto.Lumen.Model.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Model\{#dirPlat}\EntityFramework.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Model\{#dirPlat}\dbVuoto.sdf"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Model\{#dirPlat}\dbVuoto.sqlite"; DestDir: "{app}"; Flags: ignoreversion
; --- Core ---
Source: "..\Digiphoto.Lumen.Core\{#dirPlat}\Digiphoto.Lumen.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\{#dirPlat}\Digiphoto.Lumen.Core.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\{#dirPlat}\log4net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\{#dirPlat}\MemBus.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\{#dirPlat}\ExifLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\{#dirPlat}\skgl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.Core\{#dirPlat}\Resources\*"; DestDir: "{app}\Resources"; Flags: recursesubdirs
; --- Imaging ---                                                            
Source: "..\Digiphoto.Lumen.Imaging.Wic\{#dirPlat}\Digiphoto.Lumen.Imaging.Wic.dll"; DestDir: "{app}"; Flags: ignoreversion
; --- Presentation Framework ---                                                            
Source: "..\Digiphoto.Lumen.PresentationFramework\{#dirPlat}\Digiphoto.Lumen.PresentationFramework.dll"; DestDir: "{app}"; Flags: ignoreversion
; --- Lumen ---
Source: "..\Digiphoto.Lumen.UI\{#dirPlat}\Digiphoto.Lumen.UI.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.UI\{#dirPlat}\Digiphoto.Lumen.UI.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.UI\{#dirPlat}\Hardcodet.Wpf.TaskbarNotification.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.UI\{#dirPlat}\System.Windows.Interactivity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.UI\{#dirPlat}\Reports\*"; DestDir: "{app}\Reports"; Flags: ignoreversion 
; --- Configuratore ---
Source: "..\Digiphoto.Lumen.GestoreConfigurazione.UI\lib\WPFToolkit.Extended.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.GestoreConfigurazione.UI\{#dirPlat}\Digiphoto.Lumen.GestoreConfigurazione.UI.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.GestoreConfigurazione.UI\{#dirPlat}\Digiphoto.Lumen.GestoreConfigurazione.UI.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.GestoreConfigurazione.UI\{#dirPlat}\Xceed*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.GestoreConfigurazione.UI\{#dirPlat}\EntityFramework.SqlServer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.GestoreConfigurazione.UI\{#dirPlat}\Images\*"; DestDir: "{app}\Images"; Flags: recursesubdirs
; --- Self Service
Source: "..\Digiphoto.Lumen.SelfService.HostConsole\{#dirPlat}\Digiphoto.Lumen.SelfService.HostConsole.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.SelfService.HostConsole\{#dirPlat}\Digiphoto.Lumen.SelfService.HostConsole.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.SelfService\{#dirPlat}\Digiphoto.Lumen.SelfService.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.GestoreConfigurazione.UI\Images\Operator1.jpg"; DestDir: "{%PUBLIC|C:\Users\Public}\Pictures\Lumen\Fotografi"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.GestoreConfigurazione.UI\Images\Lumen-selfservice-logo.png"; DestDir: "{%PUBLIC|C:\Users\Public}\Pictures\Lumen\Loghi"; Flags: ignoreversion
; --- OnRide
Source: "..\Digiphoto.Lumen.OnRide.UI\{#dirPlat}\Digiphoto.Lumen.OnRide.UI.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.OnRide.UI\{#dirPlat}\Digiphoto.Lumen.OnRide.UI.exe.config"; DestDir: "{app}"; Flags: ignoreversion
; --- altro varie
Source: "..\Digiphoto.Lumen.UI\{#dirPlat}\zxing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Digiphoto.Lumen.UI\{#dirPlat}\zxing.presentation.dll"; DestDir: "{app}"; Flags: ignoreversion


; --- Driver sql ---
; SqLite
Source: "..\packages\System.Data.SQLite.Core.1.0.99.0\build\net46\x86\SQLite.Interop.dll"; DestDir: "{app}"; Check: "not IsWin64"; Flags: ignoreversion
Source: "..\packages\System.Data.SQLite.Core.1.0.99.0\build\net46\x64\SQLite.Interop.dll"; DestDir: "{app}"; Check: "IsWin64"; Flags: ignoreversion
Source: "..\packages\System.Data.SQLite.Core.1.0.99.0\lib\net46\System.Data.SQLite.dll"; DestDir: "{app}";  Flags: ignoreversion
Source: "..\packages\System.Data.SQLite.EF6.1.0.99.0\lib\net46\System.Data.SQLite.EF6.dll"; DestDir: "{app}";  Flags: ignoreversion
Source: "..\packages\System.Data.SQLite.Linq.1.0.99.0\lib\net46\System.Data.SQLite.Linq.dll"; DestDir: "{app}";  Flags: ignoreversion
; MySql
Source: "..\packages\MySql.Data.6.9.9\lib\net45\MySql.Data.dll"; DestDir: "{app}";  Flags: ignoreversion
Source: "..\packages\MySql.Data.Entity.6.9.9\lib\net45\MySql.Data.Entity.EF6.dll"; DestDir: "{app}";  Flags: ignoreversion

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{#MyAppName} Configurazione"; Filename: "{app}\{#MyConfExeName}"
Name: "{group}\{#MyAppName} Self Service Host"; Filename: "{app}\{#SSHostExeName}"
Name: "{group}\{#MyAppName} OnRide manager"; Filename: "{app}\{#OnRideUIExeName}"
;
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyConfExeName}"; Description: "lancia il gestore della configurazione"; Flags: nowait postinstall skipifsilent runascurrentuser

[Registry]
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\policies\Explorer"; ValueName: NoDriveTypeAutorun; ValueType: dword; ValueData: 255; Flags: uninsdeletekey; Tasks: DisableAuoRun;
