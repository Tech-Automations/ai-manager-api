# .NET 8 Runtime Installation Command

## ✅ Runtime Already Installed!

The .NET 8 runtime (8.0.22) has been successfully installed to: `~/.dotnet`

## To Make It Available System-Wide

Run this command (will require your password):

```bash
sudo mkdir -p /usr/local/share/dotnet/shared/Microsoft.NETCore.App && \
sudo mkdir -p /usr/local/share/dotnet/shared/Microsoft.AspNetCore.App && \
sudo cp -r ~/.dotnet/shared/Microsoft.NETCore.App/8.0.22 /usr/local/share/dotnet/shared/Microsoft.NETCore.App/ && \
sudo cp -r ~/.dotnet/shared/Microsoft.AspNetCore.App/8.0.22 /usr/local/share/dotnet/shared/Microsoft.AspNetCore.App/
```

## Alternative: Run Using User-Installed Runtime

Or, use the dotnet from your home directory:

```bash
export PATH=$HOME/.dotnet:$PATH
export DOTNET_ROOT=$HOME/.dotnet
cd src/AIProjectManager.API
$HOME/.dotnet/dotnet run
```

## Verify Installation

After copying, verify:

```bash
/usr/local/share/dotnet/dotnet --list-runtimes | grep "8.0"
```

Should show:
```
Microsoft.AspNetCore.App 8.0.22 [/usr/local/share/dotnet/shared/Microsoft.AspNetCore.App]
Microsoft.NETCore.App 8.0.22 [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]
```

## Then Run Project

```bash
cd src/AIProjectManager.API
dotnet run
```

---

**Quick Copy-Paste Command:**
```bash
sudo mkdir -p /usr/local/share/dotnet/shared/Microsoft.NETCore.App && sudo mkdir -p /usr/local/share/dotnet/shared/Microsoft.AspNetCore.App && sudo cp -r ~/.dotnet/shared/Microsoft.NETCore.App/8.0.22 /usr/local/share/dotnet/shared/Microsoft.NETCore.App/ && sudo cp -r ~/.dotnet/shared/Microsoft.AspNetCore.App/8.0.22 /usr/local/share/dotnet/shared/Microsoft.AspNetCore.App/
```

