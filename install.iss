; Inno Setup Script for FitnessMarvel

; Build the project before creating the setup
#define BuildOutput Exec("cmd.exe", "/C msbuild ""D:\EUSmartAttendanceMiddlewareX\smart-attendance-middleware.csproj"" /t:Rebuild /p:Configuration=Debug;Platform=x86 /v:minimal", "", 1, 1)

#define MyAppName "FitnessMarvel"
#define MyAppVersion "3.2.4"
#define MyAppExeName "FitnessMarvel.exe"

; Set to Debug and x86 based on your Visual Studio settings
#define Configuration "Debug"
#define Platform "x86"

[Setup]
AppId={{01a88b72-f31b-43cd-bd82-719cecc1634b}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName=C:\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename=FitnessMarvel_v3.2.4
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "Regocx25-01.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall
Source: "bin\{#Platform}\{#Configuration}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#Platform}\{#Configuration}\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#Platform}\{#Configuration}\*.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\{#Platform}\{#Configuration}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb,*.vshost.*,Logs,FACE,offlinefaces,ServerLogs,token.json,crash.log,.firewall_rule_created,GLog*,StaffGLog*"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"; ValueType: string; ValueName: "{app}\{#MyAppExeName}"; ValueData: "RUNASADMIN"; Flags: uninsdeletevalue

[Run]
Filename: "{tmp}\Regocx25-01.exe"; Parameters: "/S"; Flags: waitprogress runascurrentuser; StatusMsg: "Installing fingerprint reader driver..."; Check: not RegKeyExists(HKLM, 'SOFTWARE\Classes\ZKFPEngX.ZKFPEngX')
; SQL Server Express — dynamically find latest package ID, install if missing or upgrade if present
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""$id = (winget search 'SQL Server Express' --source winget --accept-source-agreements 2>&1 | Select-String 'Microsoft\.SQLServer\.\d+\.Express' | Select-Object -First 1).Matches[0].Value; if ($id) { winget install --id $id --silent --accept-package-agreements --accept-source-agreements }"""; Flags: waitprogress runhidden; StatusMsg: "Installing SQL Server Express (latest)..."; Check: not IsSqlServerInstalled()
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""$id = (winget search 'SQL Server Express' --source winget --accept-source-agreements 2>&1 | Select-String 'Microsoft\.SQLServer\.\d+\.Express' | Select-Object -First 1).Matches[0].Value; if ($id) { winget upgrade --id $id --silent --accept-package-agreements --accept-source-agreements }"""; Flags: waitprogress runhidden; StatusMsg: "Checking for SQL Server Express updates..."; Check: IsSqlServerInstalled()
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent runasoriginaluser

[Code]
function IsSqlServerInstalled(): Boolean;
var
  names: String;
begin
  // Check if any SQL Server instance is registered
  Result := RegQueryStringValue(HKLM,
    'SOFTWARE\Microsoft\Microsoft SQL Server',
    'InstalledInstances', names) and (names <> '');
  if not Result then
    Result := RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL');
end;
