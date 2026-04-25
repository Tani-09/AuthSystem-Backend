# 🔐 AuthSystem — Production-Style Authentication API

A complete backend authentication system built using **ASP.NET Core Web API**, deployed on **Microsoft Azure**, following **Clean Architecture** principles.

---

## 🎥 Demo

> Video walkthrough available — covers full API flow + Azure infrastructure proof.

---

## 🧱 Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core Web API |
| ORM | Entity Framework Core |
| Database | Azure SQL Database |
| Cache | Azure Redis Cache |
| Secret Management | Azure Key Vault |
| Hosting | Azure App Service |
| Email | SMTP (Gmail) |

---

## 🏗️ Architecture

Clean Architecture with clear separation of concerns:
AuthSystem.API            → Controllers, Middleware
AuthSystem.Application    → Interfaces, DTOs, Business Logic
AuthSystem.Domain         → Entities
AuthSystem.Infrastructure → DB, Redis, Email, Token Services

---

## 🔑 Features Implemented

### 1. User Registration
- Email, Password, First Name, Last Name
- Password hashed using ASP.NET Core Identity PasswordHasher
- Stored in Azure SQL Database

### 2. Email Verification
- Secure random token via RandomNumberGenerator
- Token stored in DB with 24 hour expiry
- Verification link sent via SMTP
- Login blocked until email is verified

### 3. Login with Password Validation
- Password hash verified using PasswordHasher
- Email verification status checked
- Returns JWT Access Token + Refresh Token

### 4. JWT Authentication
- Signed using symmetric key
- Claims include UserId and Email
- Validated on every request via middleware

### 5. Refresh Token Rotation
- New refresh token issued on every refresh
- Old token immediately invalidated in Redis and DB
- Prevents replay attacks and token theft

### 6. Logout with Redis Blacklisting
- Access token stored in Redis on logout
- Key format: `blacklist:{token}`
- Every request checks Redis — blacklisted tokens rejected instantly

### 7. Rate Limiting
- Max 5 login/register attempts per minute
- Built using ASP.NET Core Rate Limiting middleware
- Protects against brute force attacks

### 8. Global Exception Middleware
- Catches all unhandled exceptions
- Returns proper HTTP status codes (401, 403, 404, 400)
- Clean JSON error response instead of generic 500

### 9. Role-Based Authorization
- Roles stored in DB — UserRoles table
- Roles embedded in JWT claims on login
- Admin-only endpoints protected via [Authorize(Roles = "Admin")]

### 10. Forgot & Reset Password
- Secure token generated and stored in DB with 1 hour expiry
- Reset link sent via email
- Token validated and password rehashed on reset

### 11. Azure Key Vault
- All secrets stored in Azure Key Vault
- App Service accesses secrets via Managed Identity (RBAC)
- No secrets hardcoded or stored in environment variables
- Secrets: JWT Key, Redis Connection String, SMTP credentials

---

## ☁️ Azure Infrastructure

| Service | Purpose |
|---|---|
| Azure App Service | API Hosting |
| Azure SQL Database | User, Role, RefreshToken storage |
| Azure Redis Cache | Token blacklisting + Refresh token storage |
| Azure Key Vault | Secret management via Managed Identity |

---

## 🔒 Security Practices

- No plain text passwords — hashed using ASP.NET Identity
- Email must be verified before login
- JWT tokens have short expiry
- Refresh token rotation on every use
- Redis-based token revocation on logout
- Rate limiting on sensitive endpoints
- All secrets in Azure Key Vault — zero hardcoded credentials

---

## 📁 Project Structure
AuthSystem.API/
├── Controllers/
│   └── AuthController.cs
├── Middleware/
│   └── ExceptionMiddleware.cs
├── Program.cs
AuthSystem.Application/
├── DTOs/
├── Services/
│   └── IUserService.cs
├── Responses/
AuthSystem.Domain/
├── Entities/
│   ├── User.cs
│   ├── RefreshToken.cs
│   └── Role.cs
AuthSystem.Infrastructure/
├── Data/
│   └── AuthDbContext.cs
├── Services/
│   ├── UserService.cs
│   ├── TokenService.cs
│   ├── EmailService.cs
│   └── RedisService.cs
├── Security/
│   └── PasswordHasherService.cs

---

## 🚀 How to Run Locally

1. Clone the repository
```bash
git clone https://github.com/Tani-09/AuthSystem-Backend.git
```

2. Configure `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-sql-connection-string"
  },
  "Jwt": { "Key": "your-jwt-key" },
  "Redis": { "ConnectionString": "your-redis-connection" },
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Email": "your-email",
    "Password": "your-app-password"
  },
  "App": { "BaseUrl": "https://localhost:7134" }
}
```

3. Run migrations:
```bash
dotnet ef database update
```

4. Run the API and open Swagger:
https://localhost:7134/swagger

---

## 👩‍💻 Author

**Taniya Sawlani**  
Backend Developer | ASP.NET Core | Azure  
[GitHub](https://github.com/Tani-09)
