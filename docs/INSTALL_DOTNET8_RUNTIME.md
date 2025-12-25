# Install .NET 8 Runtime

## Current Issue

The project builds successfully, but requires .NET 8.0 runtime to run. Your system has:
- ✅ .NET 6.0.16 runtime
- ✅ .NET 7.0.5 runtime  
- ✅ .NET 10.0.1 runtime
- ❌ .NET 8.0 runtime (MISSING)

## Quick Fix: Download and Install .NET 8 Runtime

### Option 1: Direct Download (Recommended)

1. Visit: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download: **ASP.NET Core Runtime 8.0.x - macOS ARM64** (.pkg installer)
3. Run the installer
4. Restart terminal/IDE

### Option 2: Install via Homebrew

```bash
# Check if available
brew search dotnet

# If available, install runtime
brew install --cask dotnet-runtime
```

### Option 3: Install via Script

```bash
curl -sSL https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime aspnetcore
```

Then add to PATH:
```bash
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.zshrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.zshrc
```

## Verify Installation

After installation:

```bash
dotnet --list-runtimes
```

Should show:
```
Microsoft.AspNetCore.App 8.0.x [/usr/local/share/dotnet/shared/Microsoft.AspNetCore.App]
Microsoft.NETCore.App 8.0.x [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]
```

## After Installation

Once .NET 8 runtime is installed, run the project:

```bash
cd src/AIProjectManager.API
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: https://localhost:5001/swagger

---

## Alternative: Use Docker (If Runtime Installation Fails)

If you have Docker installed, you can run the application in a container instead:

```bash
# Build Docker image (if Dockerfile exists)
docker build -t ai-project-manager .

# Run container
docker run -p 5000:5000 -p 5001:5001 ai-project-manager
```

---

**Quick Link:** https://dotnet.microsoft.com/download/dotnet/8.0

