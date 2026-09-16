; MpyjVPN Installer Script
#define MyAppName "MpyjVPN"
#define MyAppVersion "1.0.0-beta.6"
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

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch MpyjVPN"; Flags: nowait postinstall skipifsilent

[Code]
var
  DownloadPage: TDownloadWizardPage;

function OnDownloadProgress(const Url, FileName: String; const Progress, ProgressMax: Int64): Boolean;
begin
  if Progress = ProgressMax then
    Log(Format('Successfully downloaded file to {tmp}: %s', [FileName]));
  Result := True;
end;

procedure InitializeWizard;
begin
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), @OnDownloadProgress);
end;

function IsWarpInstalled(): Boolean;
begin
  Result := FileExists(ExpandConstant('{commonpf}\Cloudflare\Cloudflare WARP\warp-cli.exe')) or
            FileExists(ExpandConstant('{commonpf32}\Cloudflare\Cloudflare WARP\warp-cli.exe'));
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if (CurPageID = wpReady) and (not IsWarpInstalled()) then
  begin
    DownloadPage.Clear;
    DownloadPage.Add('https://1111-releases.cloudflareclient.com/win/latest', 'Cloudflare_WARP_Release-x64.msi', '');
    DownloadPage.Show;
    try
      try
        DownloadPage.Download;
      except
        MsgBox('Failed to download Cloudflare WARP. Please install it manually from https://1.1.1.1/ and run this installer again.', mbError, MB_OK);
        Result := False;
      end;
    finally
      DownloadPage.Hide;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    if not IsWarpInstalled() then
    begin
      if FileExists(ExpandConstant('{tmp}\Cloudflare_WARP_Release-x64.msi')) then
      begin
        if not Exec('msiexec.exe', '/i "' + ExpandConstant('{tmp}\Cloudflare_WARP_Release-x64.msi') + '" /qn /norestart', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
        begin
          Log('WARP installation failed with code: ' + IntToStr(ResultCode));
        end
        else
        begin
          Log('WARP installed successfully');
        end;
      end;
    end;
  end;
end;