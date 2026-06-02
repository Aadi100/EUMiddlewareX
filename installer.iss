; Inno Setup Script for Smart Attendance Middleware
#define MyAppName "Smart Attendance Middleware"
#define MyAppVersion "1.0.0"
#define MyAppExeName "smart-attendance-middleware.exe"

; Change this to "Release" when you are ready for production
#define Configuration "Debug" 

[Setup]
AppId={{01a88b72-f31b-43cd-bd82-719cecc1634b}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename=SmartAttendanceSetup_Debug
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\{#Configuration}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#Configuration}\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#Configuration}\*.config"; DestDir: "{app}"; Flags: ignoreversion
; FIXED: Changed "Exclude" to "Excludes"
Source: "bin\{#Configuration}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb,*.vshost.*"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent