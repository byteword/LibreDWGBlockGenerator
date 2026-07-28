#ifndef SourceRoot
  #error SourceRoot define is required.
#endif
#ifndef OutputDir
  #error OutputDir define is required.
#endif
#ifndef RepositoryRoot
  #error RepositoryRoot define is required.
#endif
#ifndef AppVersion
  #error AppVersion define is required.
#endif

#define AppName "LDBGen"
#define AppLongName "LibreDWG Block Generator"
#define AppExeName "libredwg-block-generator.exe"

[Setup]
AppId={{7A615B38-3C18-46D1-A9F1-B652545F5A74}
AppName={#AppName}
AppVerName={#AppLongName} {#AppVersion}
AppVersion={#AppVersion}
AppPublisher=LDBGen contributors
AppPublisherURL=https://github.com/byteword/LibreDWGBlockGenerator
AppSupportURL=https://github.com/byteword/LibreDWGBlockGenerator/issues
AppUpdatesURL=https://github.com/byteword/LibreDWGBlockGenerator/releases
DefaultDirName={localappdata}\Programs\LDBGen
DefaultGroupName=LDBGen
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#OutputDir}
OutputBaseFilename=LDBGen-{#AppVersion}-UserSetup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
UninstallDisplayIcon={app}\{#AppExeName}
VersionInfoVersion={#AppVersion}
VersionInfoCompany=LDBGen contributors
VersionInfoDescription={#AppLongName} user installer
VersionInfoProductName={#AppLongName}
VersionInfoProductVersion={#AppVersion}
LicenseFile={#RepositoryRoot}\LICENSE
SetupLogging=yes

[Files]
Source: "{#SourceRoot}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\App Paths\{#AppExeName}"; ValueType: string; ValueName: ""; ValueData: "{app}\{#AppExeName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\App Paths\{#AppExeName}"; ValueType: string; ValueName: "Path"; ValueData: "{app}"; Flags: uninsdeletevalue

[Icons]
Name: "{group}\LDBGen Command Prompt"; Filename: "{cmd}"; Parameters: "/K cd /d ""{app}"""; WorkingDir: "{app}"

[Run]
Filename: "{app}\{#AppExeName}"; Parameters: "--version"; Description: "Verify the installed LDBGen version"; Flags: postinstall skipifsilent runhidden waituntilterminated

