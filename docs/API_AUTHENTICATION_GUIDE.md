# API Authentication Guide

## Important: This API Uses JWT Authentication (Not Auth0)

This API uses **custom JWT (JSON Web Token) authentication**, not Auth0. The JWT configuration is located in `appsettings.json` under the `JwtSettings` section.

## JWT Configuration Location

**File:** `src/AIProjectManager.API/appsettings.json`

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "AIProjectManager",
    "Audience": "AIProjectManager",
    "ExpiryMinutes": "1440"
  }
}
```

**Configuration Details:**
- **SecretKey**: Used to sign and verify JWT tokens
- **Issuer**: Token issuer identifier
- **Audience**: Token audience identifier
- **ExpiryMinutes**: Token expiration time (1440 = 24 hours)

---

## How Authentication Works

1. **Register** a new user (creates tenant + user) → Get JWT token
2. **Login** with email/password → Get JWT token
3. **Use token** in `Authorization: Bearer {token}` header for protected endpoints

---

## Step-by-Step: Getting Your Token

### Step 1: Register a New User (First Time)

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "admin@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "tenantName": "My Company",
  "subdomain": "my-company"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "guid",
    "email": "admin@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Admin",
    "tenantId": "guid"
  }
}
```

**Save the `token` value** - you'll need it for authenticated requests.

### Step 2: Login (If Already Registered)

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "admin@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** Same as register - includes `token` and `user` objects.

---

## Using the Token in API Calls

Once you have a token, include it in the `Authorization` header for all protected endpoints.

### Format:
```
Authorization: Bearer {your-token-here}
```

---

## Examples: Calling the API

### Option 1: Using Swagger UI (Easiest)

1. **Start the API** (run in debug mode or `dotnet run`)
2. **Open Swagger:** Navigate to `https://localhost:5001/swagger`
3. **Register/Login:**
   - Click on `/api/auth/register` or `/api/auth/login`
   - Click "Try it out"
   - Enter your credentials
   - Click "Execute"
   - Copy the `token` from the response

4. **Authorize in Swagger:**
   - Click the **"Authorize"** button (top right, lock icon)
   - Paste your token in the `Value` field
   - Click "Authorize"
   - Click "Close"

5. **Now you can test protected endpoints:**
   - All endpoints with the lock icon are now accessible
   - Try `GET /api/auth/me` to verify
   - Try `GET /api/projects` to see your projects

---

### Option 2: Using cURL

#### Register (First Time):
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe",
    "tenantName": "My Company",
    "subdomain": "my-company"
  }'
```

**Save the token from the response.**

#### Login:
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "SecurePassword123!"
  }'
```

#### Call Protected Endpoint:
```bash
curl -X GET https://localhost:5001/api/projects \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json"
```

**Example with actual token:**
```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X GET https://localhost:5001/api/projects \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

---

### Option 3: Using Postman

1. **Register/Login:**
   - Method: `POST`
   - URL: `https://localhost:5001/api/auth/register` or `/api/auth/login`
   - Headers: `Content-Type: application/json`
   - Body (raw JSON): Your credentials
   - Send request
   - Copy the `token` from response

2. **Set Authorization:**
   - Go to **Authorization** tab
   - Type: `Bearer Token`
   - Token: Paste your token
   - Now all requests will include the token

3. **Test Protected Endpoints:**
   - Method: `GET`
   - URL: `https://localhost:5001/api/projects`
   - The Authorization header is automatically included

---

### Option 4: Using JavaScript/Fetch

```javascript
// Step 1: Login
const loginResponse = await fetch('https://localhost:5001/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    email: 'admin@example.com',
    password: 'SecurePassword123!'
  })
});

const { token } = await loginResponse.json();

// Step 2: Use token for protected endpoints
const projectsResponse = await fetch('https://localhost:5001/api/projects', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json',
  }
});

const projects = await projectsResponse.json();
console.log(projects);
```

---

## Protected vs Public Endpoints

### Public Endpoints (No Token Required):
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login

### Protected Endpoints (Token Required):
- `GET /api/auth/me` - Get current user
- `GET /api/projects` - Get all projects
- `POST /api/projects` - Create project
- `GET /api/projects/{id}` - Get project by ID
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project
- `GET /api/tasks` - Get all tasks
- `POST /api/tasks` - Create task
- `GET /api/tasks/{id}` - Get task by ID
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task
- `POST /api/aichat/chat` - AI chat

---

## Troubleshooting "Unauthorized" Errors

### Issue: Getting 401 Unauthorized

**Possible Causes:**

1. **Token Not Included:**
   - Make sure you're including `Authorization: Bearer {token}` header
   - Check spelling: "Bearer" not "bearer" or "BEARER"

2. **Token Expired:**
   - Tokens expire after 24 hours (1440 minutes)
   - Solution: Login again to get a new token

3. **Invalid Token:**
   - Make sure you copied the entire token (they're long)
   - Don't include quotes around the token
   - Make sure there's no extra whitespace

4. **Wrong Endpoint:**
   - Verify you're calling the correct URL
   - Check if the endpoint requires authentication

5. **Server Not Running:**
   - Make sure the API is running
   - Check if Swagger is accessible at `https://localhost:5001/swagger`

### Quick Test:

Try this to verify your token works:

```bash
# Replace YOUR_TOKEN with your actual token
curl -X GET https://localhost:5001/api/auth/me \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json"
```

If this works, your token is valid. If not, login again to get a new token.

---

## Token Structure

Your JWT token contains:
- **userId**: Your user ID
- **tenantId**: Your tenant ID (for multi-tenancy)
- **email**: Your email address
- **role**: Your role (Admin/User)
- **exp**: Expiration timestamp

You can decode your token at [jwt.io](https://jwt.io) to see its contents (but don't share your token publicly).

---

## Security Notes

1. **Token Storage:**
   - Store tokens securely (don't commit to git)
   - Use environment variables in production
   - Tokens expire after 24 hours

2. **HTTPS:**
   - Always use HTTPS in production
   - The API uses HTTPS in development (localhost:5001)

3. **Secret Key:**
   - Change the `SecretKey` in `appsettings.json` for production
   - Use a strong, randomly generated key (minimum 32 characters)
   - Store production secrets in secure vaults (Azure Key Vault, AWS Secrets Manager, etc.)

---

## Quick Reference

### Register User:
```bash
POST /api/auth/register
Body: { email, password, firstName, lastName, tenantName, subdomain }
Response: { token, user }
```

### Login:
```bash
POST /api/auth/login
Body: { email, password }
Response: { token, user }
```

### Use Token:
```
Header: Authorization: Bearer {token}
```

### Get Current User:
```bash
GET /api/auth/me
Header: Authorization: Bearer {token}
Response: { id, email, firstName, lastName, role, tenantId }
```

---

## Need Help?

If you're still getting unauthorized errors:
1. Verify the API is running
2. Test registration/login first
3. Copy the entire token (no quotes, no spaces)
4. Make sure you're using `Bearer` (capital B)
5. Check token expiration (login again if needed)

---

**Last Updated:** 2024

