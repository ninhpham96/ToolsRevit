
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "MF Quick Select"
#define MyAppCondition "2.1"
#define MyAppVersion 20221228
#define MyAppPublisher "MF TECNICA"
#define MyAppURL "https://mf-tools.info/"
#define AppGUID "{88C3E65F-BE4D-410A-8040-5F6DDAE31E28}"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{88C3E65F-BE4D-410A-8040-5F6DDAE31E28}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName=C:\ProgramData\Autodesk
DisableDirPage=yes
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
PrivilegesRequired=lowest
OutputBaseFilename={#MyAppName} Installer JP
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\icon.ico
DisableWelcomePage=no
WizardImageFile=banner.bmp
SetupIconFile=icon.ico
OutputDir="Setup\"
CloseApplications=force
LicenseFile=license.rtf

[Components]
Name: "Revit2020"; Description: "Add-in Revit 2020"; Types: full
Name: "Revit2021"; Description: "Add-in Revit 2021"; Types: full
Name: "Revit2022"; Description: "Add-in Revit 2022"; Types: full
Name: "Revit2023"; Description: "Add-in Revit 2023"; Types: full
Name: "Revit2024"; Description: "Add-in Revit 2024"; Types: full
; Name: "Resources"; Description: "Resources"; Types: full; Flags: fixed

[Messages]
WelcomeLabel2=このツールはあなたのコンピューターにインストールされます。続行する前に、他のすべてのアプリケーションを閉じて、アンチウイルスを無効にしておくことをお勧めします。

[Languages]
//Name: "english"; MessagesFile: "compiler:Default.isl"  
Name: "japanese"; MessagesFile: "compiler:Languages/Japanese.isl"

[Code]

// Import IsISPackageInstalled() function from UninsIS.dll at setup time
function DLLIsISPackageInstalled(AppId: string;
  Is64BitInstallMode, IsAdminInstallMode: DWORD): DWORD;
  external 'IsISPackageInstalled@files:UninsIS.dll stdcall setuponly';

// Import CompareISPackageVersion() function from UninsIS.dll at setup time
function DLLCompareISPackageVersion(AppId, InstallingVersion: string;
  Is64BitInstallMode, IsAdminInstallMode: DWORD): longint;
  external 'CompareISPackageVersion@files:UninsIS.dll stdcall setuponly';

// Import UninstallISPackage() function from UninsIS.dll at setup time
function DLLUninstallISPackage(AppId: string;
  Is64BitInstallMode, IsAdminInstallMode: DWORD): DWORD;
  external 'UninstallISPackage@files:UninsIS.dll stdcall setuponly';


// Wrapper for UninsIS.dll IsISPackageInstalled() function
// Returns true if package is detected as installed, or false otherwise
function IsISPackageInstalled(): boolean;
  begin
  result := DLLIsISPackageInstalled(
    '{#AppGUID}',                     // AppId
    DWORD(Is64BitInstallMode()),      // Is64BitInstallMode
    DWORD(IsAdminInstallMode())       // IsAdminInstallMode
  ) = 1;
  if result then
    Log('UninsIS.dll - Package detected as installed')
  else
    Log('UninsIS.dll - Package not detected as installed');
  end;

// Wrapper for UninsIS.dll CompareISPackageVersion() function
// Returns:
// < 0 if version we are installing is < installed version
// 0   if version we are installing is = installed version
// > 0 if version we are installing is > installed version
function CompareISPackageVersion(): longint;
  begin
  result := DLLCompareISPackageVersion(
    '{#AppGUID}',                        // AppId
    '{#MyAppVersion}',                   // InstallingVersion
    DWORD(Is64BitInstallMode()),         // Is64BitInstallMode
    DWORD(IsAdminInstallMode())          // IsAdminInstallMode
  );
  if result < 0 then
    Log('UninsIS.dll - This version {#MyAppVersion} older than installed version')
  else if result = 0 then
    Log('UninsIS.dll - This version {#MyAppVersion} same as installed version')
  else
    Log('UninsIS.dll - This version {#MyAppVersion} newer than installed version');
  end;

// Wrapper for UninsIS.dll UninstallISPackage() function
// Returns 0 for success, non-zero for failure
function UninstallISPackage(): DWORD;
  begin
  result := DLLUninstallISPackage(
    '{#AppGUID}',                   // AppId
    DWORD(Is64BitInstallMode()),    // Is64BitInstallMode
    DWORD(IsAdminInstallMode())     // IsAdminInstallMode
  );
  if result = 0 then
    Log('UninsIS.dll - Installed package uninstall completed successfully')
  else
    Log('UninsIS.dll - installed package uninstall did not complete successfully');
  end;


function PrepareToInstall(var NeedsRestart: boolean): string;
  begin
  result := '';
  // If package installed, uninstall it automatically if the version we are
  // installing does not match the installed version; If you want to
  // automatically uninstall only...
  // ...when downgrading: change <> to <
  // ...when upgrading:   change <> to >
  if IsISPackageInstalled() and (CompareISPackageVersion() <> 0) then
    UninstallISPackage();
  end;

[Files] 
Source: "UninsIS.dll"; Flags: dontcopy
Source: "Output\*"; DestDir: "C:\ProgramData\Autodesk\Revit\Addins\2020"; Components: Revit2020; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Output\*"; DestDir: "C:\ProgramData\Autodesk\Revit\Addins\2021"; Components: Revit2021; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Output\*"; DestDir: "C:\ProgramData\Autodesk\Revit\Addins\2022"; Components: Revit2022; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Output\*"; DestDir: "C:\ProgramData\Autodesk\Revit\Addins\2023"; Components: Revit2023; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Output\*"; DestDir: "C:\ProgramData\Autodesk\Revit\Addins\2024"; Components: Revit2024; Flags: ignoreversion recursesubdirs createallsubdirs

; NOTE: Don't use "Flags: ignoreversion" on any shared system files
