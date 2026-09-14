; MpyjVPN Installer Script
#define MyAppName "MpyjVPN"
#define MyAppVersion "1.0.0-beta.5"
#define MyAppPublisher "Mpyj"
#define MyAppExeName "MpyjVPN.Avalonia.exe"

[Setup]
AppId={{B8F3A2C1-9D4E-4F7A-A1B2-C3D4E5F60718}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=..\dist
OutputBaseFilename=MpyjVPN-Setup-{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
SetupIconFile=icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "..\dist\publish-normal\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "Cloudflare_WARP_Release-x64.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch MpyjVPN"; Flags: nowait postinstall skipifsilent

[Code]
function IsWarpInstalled(): Boolean;
begin
  Result := FileExists(ExpandConstant('{commonpf}\Cloudflare\Cloudflare WARP\warp-cli.exe')) or
            FileExists(ExpandConstant('{commonpf32}\Cloudflare\Cloudflare WARP\warp-cli.exe'));
end;

procedure InstallWarp();
var
  ResultCode: Integer;
begin
  if IsWarpInstalled() then
  begin
    exit;
  end;

  if MsgBox('MpyjVPN requires Cloudflare WARP to work properly.' + #13#10 + #13#10 +
            'Do you want to install it now?' + #13#10 + #13#10 +
            'This is required for the VPN to connect.', mbConfirmation, MB_YESNO) = idYes then
  begin
    if not Exec(ExpandConstant('{tmp}\Cloudflare_WARP_Release-x64.msi'), '/qn /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
    begin
      MsgBox('WARP installation failed with code: ' + IntToStr(ResultCode) + #13#10 + #13#10 +
             'Please install Cloudflare WARP manually from https://1.1.1.1/ and try again.', mbError, MB_OK);
    end
    else
    begin
      MsgBox('Cloudflare WARP installed successfully!', mbInformation, MB_OK);
    end;
  end
  else
  begin
    MsgBox('You can install Cloudflare WARP later from https://1.1.1.1/' + #13#10 + #13#10 +
           'Without it, MpyjVPN may not be able to connect.', mbInformation, MB_OK);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    InstallWarp();
  end;
end;