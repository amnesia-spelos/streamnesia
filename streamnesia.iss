; Streamnesia Installer Script - Full Professional Version

[Setup]
AppName=Streamnesia
AppVersion=3.2.0
AppPublisher=Spelos
DefaultDirName={code:GetCustomInstallDir}
DefaultGroupName=Streamnesia
OutputDir=output
OutputBaseFilename=StreamnesiaInstaller
Compression=lzma
SolidCompression=yes
LicenseFile=LICENSE
AppPublisherURL=https://github.com/amnesia-spelos/streamnesia
AppSupportURL=https://github.com/amnesia-spelos/streamnesia
AppUpdatesURL=https://github.com/amnesia-spelos/streamnesia
WizardImageFile=installer/wizard.bmp
WizardSmallImageFile=installer/smalllogo.bmp
SetupIconFile=src/Streamnesia.Client/Lux.ico
DisableProgramGroupPage=yes
DisableWelcomePage=no
DisableFinishedPage=no
DirExistsWarning=no
UsePreviousAppDir=no
DisableDirPage=no
AppendDefaultDirName=no

[Files]
Source: "deployment\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs overwritereadonly

[Icons]
Name: "{group}\Streamnesia"; Filename: "{app}\Amnesia.exe"

[Code]
var
  WarningPage: TOutputMsgWizardPage;

function FindAmnesiaInstallPath(): String;
var
  SteamPath: String;
  Installed: Cardinal;
begin
  Result := ''; // Default to blank

  // Check if Amnesia is registered as installed
  if RegQueryDWordValue(HKEY_CURRENT_USER, 'Software\Valve\Steam\Apps\57300', 'Installed', Installed) then
  begin
    if Installed = 1 then
    begin
      // Find Steam installation path
      if RegQueryStringValue(HKEY_CURRENT_USER, 'Software\Valve\Steam', 'SteamPath', SteamPath) then
      begin
        // Expected install path
        Result := SteamPath + '\steamapps\common\Amnesia The Dark Descent';
      end;
    end;
  end;

  // Fallback default path
  if (Result = '') or (not DirExists(Result)) then
  begin
    if DirExists('C:\Program Files (x86)\Steam\steamapps\common\Amnesia The Dark Descent') then
      Result := 'C:\Program Files (x86)\Steam\steamapps\common\Amnesia The Dark Descent';
  end;
end;

function GetCustomInstallDir(Default: string): string;
var
  Path: string;
begin
  Path := FindAmnesiaInstallPath();
  if Path <> '' then
    Result := Path
  else
    Result := Default;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  TargetFile, BackupFile, SelectedDir: string;
begin
  Result := True;

  SelectedDir := WizardForm.DirEdit.Text;
  TargetFile := AddBackslash(SelectedDir) + 'Amnesia.exe';

  // Check selected directory
  if CurPageID = wpSelectDir then
  begin
    if not FileExists(TargetFile) then
    begin
      MsgBox('The selected folder does not contain Amnesia.exe.'#13#10'Please select your Amnesia installation directory.', mbError, MB_OK);
      Result := False;
      exit;
    end;
  end;

  // After custom warning page
  if CurPageID = WarningPage.ID then
  begin
    BackupFile := AddBackslash(SelectedDir) + 'Amnesia_backup_before_streamnesia.exe';

    if not FileExists(BackupFile) then
    begin
      try
        FileCopy(TargetFile, BackupFile, False);
      except
        MsgBox('Failed to create backup of Amnesia.exe.'#13#10'Installation cannot continue.', mbError, MB_OK);
        Result := False;
        exit;
      end;
    end;
  end;
end;

procedure InitializeWizard;
begin
  // Insert custom warning page after directory selection
  WarningPage := CreateOutputMsgPage(wpSelectDir, 'Warning: Overwriting Amnesia.exe',
    'Streamnesia will replace the existing Amnesia.exe file.',
    'A backup named "Amnesia_backup_before_streamnesia.exe" will be created automatically before replacement.'#13#10#13#10 +
    'If you want to revert to the original Amnesia, rename the backup to Amnesia.exe after uninstalling Streamnesia.');
end;
