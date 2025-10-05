# Build the solution
dotnet build VPExtensionManager.sln -c Release

# Check if build was successful
if ($LASTEXITCODE -eq 0) {
    # Path definitions
    $releaseDir = "VPExtensionManager\bin\Release\net6.0-windows10.0.19041.0"
    $zipPath = "VPExtensionManager\bin\VPExtensionManager.zip"

    # Remove existing zip file if it exists
    if (Test-Path $zipPath) {
        Remove-Item $zipPath
    }

    # Change to release directory and create zip file
    Push-Location $releaseDir
    & 7z a -tzip "..\..\VPExtensionManager.zip" "*"
    Pop-Location

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Build and zip completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Failed to create zip file!" -ForegroundColor Red
    }
} else {
    Write-Host "Build failed!" -ForegroundColor Red
}