# Read the Amnesia installation path from environment variable
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

# Navigate to the Streamnesia.Client project
pushd .\src\Streamnesia.Client

# Publish the application
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true -o ../../deployment/streamnesia/ --self-contained

# Return to the root directory
popd

# Clean up unnecessary files from deployment
pushd .\deployment\streamnesia
Remove-Item *.pdb -ErrorAction SilentlyContinue
Remove-Item *.Development.json
popd

# Create Streamnesia folder inside the Amnesia install if it doesn't exist
$streamnesiaPath = Join-Path $amnesiaPath "streamnesia"
if (-not (Test-Path $streamnesiaPath)) {
    New-Item -ItemType Directory -Path $streamnesiaPath | Out-Null
}

$amnesiaExePath = Join-Path $amnesiaPath "Amnesia.exe"
Copy-Item -Path $amnesiaExePath -Destination .\deployment\Amnesia.exe -Force

# Copy the deployment output to the Amnesia streamnesia folder
Copy-Item -Path .\deployment\streamnesia\* -Destination $streamnesiaPath -Recurse -Force

Write-Host ""
Write-Host "Deployment completed successfully."
Write-Host "Files copied to: $streamnesiaPath"
