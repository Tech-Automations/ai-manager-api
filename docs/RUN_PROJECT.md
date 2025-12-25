# How to Run the Project

## Step 1: Copy Runtime to System Location

Run this command in your terminal (you'll need to enter your password):

```bash
./setup-runtime.sh
```

**OR** run this single command:

```bash
sudo mkdir -p /usr/local/share/dotnet/shared/Microsoft.NETCore.App && sudo mkdir -p /usr/local/share/dotnet/shared/Microsoft.AspNetCore.App && sudo cp -r ~/.dotnet/shared/Microsoft.NETCore.App/8.0.22 /usr/local/share/dotnet/shared/Microsoft.NETCore.App/ && sudo cp -r ~/.dotnet/shared/Microsoft.AspNetCore.App/8.0.22 /usr/local/share/dotnet/shared/Microsoft.AspNetCore.App/
```

## Step 2: Verify Runtime is Installed

```bash
/usr/local/share/dotnet/dotnet --list-runtimes | grep "8.0"
```

Should show:
```
Microsoft.AspNetCore.App 8.0.22 [/usr/local/share/dotnet/shared/Microsoft.AspNetCore.App]
Microsoft.NETCore.App 8.0.22 [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]
```

## Step 3: Run the Project

```bash
cd src/AIProjectManager.API
dotnet run
```

## Step 4: Access the API

Once running, open your browser to:
- **Swagger UI:** https://localhost:5001/swagger
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001

## Test the API

1. **Register a new user:**
   - POST `/api/auth/register`
   - Body:
     ```json
     {
       "email": "admin@example.com",
       "password": "SecurePassword123!",
       "firstName": "John",
       "lastName": "Doe",
       "tenantName": "Example Company",
       "subdomain": "example-company"
     }
     ```

2. **Login:**
   - POST `/api/auth/login`
   - Body:
     ```json
     {
       "email": "admin@example.com",
       "password": "SecurePassword123!"
     }
     ```

3. **Use the token** in subsequent requests:
   - Header: `Authorization: Bearer {your-token-here}`

---

**Note:** The runtime is already installed in `~/.dotnet`. You just need to copy it to the system location so the system-wide dotnet command can find it.

