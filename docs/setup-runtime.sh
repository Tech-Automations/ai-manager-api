#!/bin/bash

# Script to copy .NET 8 runtime to system location
# Run this script and enter your password when prompted

echo "Copying .NET 8 runtime to system location..."
echo "You will be prompted for your password."

sudo mkdir -p /usr/local/share/dotnet/shared/Microsoft.NETCore.App
sudo mkdir -p /usr/local/share/dotnet/shared/Microsoft.AspNetCore.App
sudo cp -r ~/.dotnet/shared/Microsoft.NETCore.App/8.0.22 /usr/local/share/dotnet/shared/Microsoft.NETCore.App/
sudo cp -r ~/.dotnet/shared/Microsoft.AspNetCore.App/8.0.22 /usr/local/share/dotnet/shared/Microsoft.AspNetCore.App/

echo ""
echo "✅ Runtime copied successfully!"
echo ""
echo "Verifying installation..."
/usr/local/share/dotnet/dotnet --list-runtimes | grep "8.0"
echo ""
echo "Now you can run the project with:"
echo "  cd src/AIProjectManager.API"
echo "  dotnet run"

