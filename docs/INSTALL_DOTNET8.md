# Install .NET 8 SDK

## Current Issue

The project requires .NET 8 SDK, but your system currently has .NET 6 SDK installed.

**Current Version:** .NET 6.0.408  
**Required Version:** .NET 8.x

## Installation Options

### Option 1: Install via Homebrew (Recommended for macOS)

```bash
brew install --cask dotnet-sdk
```

This will install the latest .NET SDK (likely version 10). You can also install .NET 8 specifically:

```bash
brew install --cask dotnet-sdk8-0-100
```

**Note:** This will require your admin password.

### Option 2: Download from Microsoft (Alternative)

1. Visit: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download the macOS installer (.pkg file)
3. Run the installer
4. Follow the installation wizard

### Option 3: Install via Script (Fastest)

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
```

Then add to your PATH:
```bash
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.zshrc
echo 'export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools' >> ~/.zshrc
```

## Verify Installation

After installation, verify .NET 8 is available:

```bash
dotnet --version
# Should show: 8.x.x
```

Also check available SDKs:

```bash
dotnet --list-sdks
# Should show .NET 8 SDK in the list
```

## After Installation

Once .NET 8 SDK is installed:

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Build the project:**
   ```bash
   dotnet build
   ```

3. **Run the project:**
   ```bash
   cd src/AIProjectManager.API
   dotnet run
   ```

## Quick Install Command

If you have admin access, run this command:

```bash
brew install --cask dotnet-sdk
```

Then verify:
```bash
dotnet --version
```

---

**Note:** Installing .NET SDK requires administrator privileges. You'll be prompted for your password during installation.

