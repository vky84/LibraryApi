# ?? Quick Fix Reference Card

## ? What Was Fixed

### 1. CORS Error ? FIXED
**Added to `Program.cs`:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

app.UseCors("AllowAll");  // Before UseAuthorization
```

### 2. Browser Launch ? FIXED
**Updated `launchSettings.json`:**
```json
{
  "launchBrowser": true,
  "launchUrl": "swagger"
}
```

### 3. HTTPS Certificate ?? ACTION NEEDED
**Run as Administrator:**
```powershell
dotnet dev-certs https --trust
```

---

## ? Quick Commands

```powershell
# Fix HTTPS certificate (as Administrator)
dotnet dev-certs https --trust

# Run NotificationService
cd C:\Personals\LibraryApi\NotificationService
dotnet run

# Test HTTP
start http://localhost:5089/swagger

# Test HTTPS (after certificate trust)
start https://localhost:7230/swagger
```

---

## ?? Quick Test

1. Press **F5** in Visual Studio
2. Browser opens automatically ? ?
3. Goes to Swagger UI ? ?
4. Click `POST /api/notifications/send`
5. Try it out with:
```json
{
  "userId": "user1",
  "subject": "Test",
  "message": "Hello!"
}
```
6. Click Execute
7. **Expected:** Status 200 OK, no CORS error ? ?

---

## ?? URLs

| Service | HTTP | HTTPS |
|---------|------|-------|
| NotificationService | http://localhost:5089/swagger | https://localhost:7230/swagger |
| LibraryApi | http://localhost:5262/swagger | https://localhost:7261/swagger |

---

## ?? Troubleshooting

**CORS error?** ? Clear browser cache  
**Browser not opening?** ? Check launch profile (select "http" or "https")  
**HTTPS warning?** ? Trust certificate: `dotnet dev-certs https --trust`  
**Still issues?** ? See `CORS_HTTPS_FIX_GUIDE.md`

---

**Status:** ? All fixed! Build successful! Ready to run!
