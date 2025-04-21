& ".\deploy.ps1"

# Paths
$sevenZip = "7z.exe"
$sourceDir = "C:\Program Files (x86)\Steam\steamapps\common\Amnesia The Dark Descent"
$archivePath = "$env:USERPROFILE\Desktop\streamnesia_release.7z"
$streamnesia = Join-Path $sourceDir "streamnesia"
$amnesiaExe = Join-Path $sourceDir "Amnesia.exe"

Push-Location $sourceDir

# Create archive (excluding bot-config.json from streamnesia)
& "$sevenZip" a "$archivePath" `
    "Amnesia.exe" `
    "streamnesia" `
    "-xr!streamnesia\bot-config.json"

Pop-Location