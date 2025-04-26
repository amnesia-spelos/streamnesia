# Build the project first
& ".\deploy.ps1"

# Paths
$sevenZip = "7z.exe" # Assumes 7z.exe is in PATH, otherwise provide full path
$deploymentDir = ".\deployment"

# Get the latest git commit hash (short)
$gitHash = (git rev-parse --short HEAD).Trim()
if ([string]::IsNullOrEmpty($gitHash)) {
    Write-Host "Failed to retrieve git commit hash."
    exit 1
}

# Compose output archive path
$outputArchive = "$env:USERPROFILE\Desktop\streamnesia_release_$gitHash.7z"

# Get Amnesia location from environment variable
$amnesiaPath = $env:AMNESIA_LOCATION
if ([string]::IsNullOrEmpty($amnesiaPath)) {
    Write-Host ""
    Write-Host "Environment variable 'AMNESIA_LOCATION' not found."
    Write-Host "Please set it to your Amnesia: The Dark Descent installation directory."
    Write-Host "Example:"
    Write-Host '$env:AMNESIA_LOCATION = "C:\Program Files (x86)\Steam\steamapps\common\Amnesia The Dark Descent"'
    Write-Host ""
    exit 1
}

# Verify Amnesia.exe exists
$amnesiaExePath = Join-Path $amnesiaPath "Amnesia.exe"
if (-not (Test-Path $amnesiaExePath)) {
    Write-Host "Amnesia.exe not found at $amnesiaExePath"
    exit 1
}

# Prepare temporary packaging structure
$tempPackageDir = ".\temp_package"
if (Test-Path $tempPackageDir) {
    Remove-Item $tempPackageDir -Recurse -Force
}
New-Item -ItemType Directory -Path "$tempPackageDir\streamnesia" | Out-Null

# Copy deployment files into temp_package/streamnesia
Copy-Item "$deploymentDir\*" "$tempPackageDir\streamnesia\" -Recurse -Force

# Copy Amnesia.exe into temp_package root
Copy-Item $amnesiaExePath "$tempPackageDir\Amnesia.exe" -Force

# Create the 7z archive
if (Test-Path $outputArchive) {
    Remove-Item $outputArchive
}

Push-Location $tempPackageDir
& "$sevenZip" a "$outputArchive" * | Out-Null
Pop-Location

# Clean up temp folder
Remove-Item $tempPackageDir -Recurse -Force

Write-Host ""
Write-Host "Deployment package created successfully: $outputArchive"
