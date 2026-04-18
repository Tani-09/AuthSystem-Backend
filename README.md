# 🔐 Auth System API

A complete backend authentication system built using ASP.NET Core following Clean Architecture principles.

---

##  Features

*  JWT Authentication (Access Token)
*  Refresh Token Flow
*  Role-Based Authorization (RBAC)
*  Email Verification (SMTP - Gmail)
*  Forgot & Reset Password
*  Redis Integration (Token Storage & Validation)
*  Logout with Token Blacklisting

---

##  Tech Stack

* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* Redis (Docker)
* SMTP (Gmail)

---

##  Architecture

Clean Architecture with separation of concerns:

* API Layer → Controllers
* Application Layer → Business Logic
* Infrastructure Layer → External Services (DB, Redis, Email)

---

##  Authentication Flow

1. User registers → Email verification link sent
2. User verifies email → Account activated
3. User logs in → Access + Refresh token generated
4. Access token expires → Refresh API used
5. Logout → Token blacklisted via Redis

---

##  Screenshots

(Add screenshots in /docs/screenshots)

---

##  How to Run

1. Clone the repository
2. Configure `appsettings.json`
3. Run the API
4. Test using Swagger

---

##  Key Highlights

* Used Redis for fast token validation instead of DB
* Implemented secure logout using token blacklisting
* Followed scalable Clean Architecture

---

##  Future Enhancements

* Azure Deployment
* SendGrid Integration
* Background Job Processing

---
