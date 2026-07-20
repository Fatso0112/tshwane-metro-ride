# Tshwane Metro Ride API

Backend API for the **Tshwane Metro Ride** passenger transport system.

The backend currently supports passenger registration and stores passenger information securely in a MariaDB database. Additional features such as login, JWT authentication, bus-card management, wallet top-ups, tickets, routes, and trips will be added as development continues.

## Technology Stack

- C#
- ASP.NET Core Web API
- .NET 8
- Entity Framework Core 8
- Pomelo Entity Framework Core MySQL provider
- MariaDB through XAMPP
- Swagger / OpenAPI
- Git and GitHub

## Current Features

- ASP.NET Core Web API setup
- MariaDB database connection
- Entity Framework Core migrations
- Passenger database model
- Passenger registration endpoint
- Password hashing
- Duplicate email validation
- Swagger API documentation

## Project Structure

```text
tshwane-metro-ride/
├── TshwaneMetroRide.sln
├── README.md
└── TshwaneMetroRide.Api/
    ├── Controllers/
    │   └── AuthController.cs
    ├── Data/
    │   └── ApplicationDbContext.cs
    ├── DTOs/
    │   └── Auth/
    │       └── RegisterPassengerRequest.cs
    ├── Migrations/
    ├── Models/
    │   └── Passenger.cs
    ├── Properties/
    ├── Program.cs
    ├── appsettings.json
    └── TshwaneMetroRide.Api.csproj
```

## Prerequisites

Install the following before running the backend:

1. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. [XAMPP](https://www.apachefriends.org/)
3. Git
4. Visual Studio Code or Visual Studio
5. The Entity Framework Core CLI tool

Confirm that .NET is installed:

```powershell
dotnet --version
```

The project currently uses .NET SDK 8.

Install the Entity Framework Core command-line tool:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.22
```

If it is already installed, update it:

```powershell
dotnet tool update --global dotnet-ef --version 8.0.22
```

Confirm that it works:

```powershell
dotnet ef --version
```

If PowerShell cannot find `dotnet ef`, close and reopen the terminal. You can also temporarily add the tools folder to the current terminal:

```powershell
$env:Path += ";$env:USERPROFILE\.dotnet\tools"
```

## Clone the Repository

Clone the `dev` branch:

```powershell
git clone -b dev https://github.com/Fatso0112/tshwane-metro-ride.git
```

Move into the project folder:

```powershell
cd tshwane-metro-ride
```

Replace `YOUR-GITHUB-USERNAME` with the repository owner's GitHub username.

## Database Setup with XAMPP

### 1. Start MariaDB

Open the XAMPP Control Panel and start:

```text
MySQL
```

XAMPP labels the service as MySQL, but it uses MariaDB.

Apache is not required to run the API. Start Apache only when you want to use phpMyAdmin.

### 2. Open MariaDB

From PowerShell:

```powershell
& "C:\xampp\mysql\bin\mysql.exe" -u root
```

If the root account has a password:

```powershell
& "C:\xampp\mysql\bin\mysql.exe" -u root -p
```

### 3. Create the database

At the MariaDB prompt, run:

```sql
CREATE DATABASE tshwane_metro_ride_db
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
```

Confirm that it exists:

```sql
SHOW DATABASES;
```

Exit MariaDB:

```sql
EXIT;
```

## Configure the Database Connection

Open:

```text
TshwaneMetroRide.Api/appsettings.json
```

For the default local XAMPP root account with no password, use:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=tshwane_metro_ride_db;User=root;Password=;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

When your MariaDB user has a password, update the connection string:

```json
"DefaultConnection": "Server=localhost;Port=3306;Database=tshwane_metro_ride_db;User=root;Password=YOUR_PASSWORD;"
```

Do not commit real passwords, production credentials, or secrets to GitHub.

For team development, each developer should configure the connection string for their own machine.

## Install Project Dependencies

Move into the API project:

```powershell
cd TshwaneMetroRide.Api
```

Restore the NuGet packages:

```powershell
dotnet restore
```

Build the project:

```powershell
dotnet build
```

## Apply the Database Migrations

Make sure MySQL is running in XAMPP, then run:

```powershell
dotnet ef database update
```

This creates the required database tables, including:

```text
passengers
__efmigrationshistory
```

To verify the tables using MariaDB:

```powershell
& "C:\xampp\mysql\bin\mysql.exe" -u root
```

Then run:

```sql
USE tshwane_metro_ride_db;
SHOW TABLES;
```

## Run the Backend

From the `TshwaneMetroRide.Api` folder:

```powershell
dotnet run
```

The terminal will display the local API address, for example:

```text
http://localhost:5229
```

The exact port may differ on another developer's machine.

## Open Swagger

After starting the API, open:

```text
http://localhost:5229/swagger
```

Replace `5229` with the port shown in the terminal.

Swagger can be used to view and test the available endpoints.

## Passenger Registration

### Endpoint

```http
POST /api/auth/register
```

### Example request

```json
{
  "fullName": "Test Passenger",
  "email": "passenger@example.com",
  "phoneNumber": "0712345678",
  "password": "Password123!"
}
```

### Successful response

A successful registration returns:

```text
201 Created
```

The password is hashed before it is stored. Plain-text passwords are never saved in the database.

Attempting to register with an email address that already exists returns:

```text
409 Conflict
```

## Confirm Registered Passengers

Connect to MariaDB:

```powershell
& "C:\xampp\mysql\bin\mysql.exe" -u root
```

Run:

```sql
USE tshwane_metro_ride_db;

SELECT
    Id,
    FullName,
    Email,
    PhoneNumber,
    CreatedAt
FROM passengers;
```

Do not include `PasswordHash` in normal queries or API responses.

## Development Branch

The shared development branch is:

```text
dev
```

Before starting work:

```powershell
git checkout dev
git pull origin dev
```

For a new feature, create a feature branch from `dev`:

```powershell
git checkout dev
git pull origin dev
git checkout -b feature/feature-name
```

After completing the work:

```powershell
git add .
git commit -m "Describe the completed change"
git push -u origin feature/feature-name
```

Create a pull request from the feature branch into `dev`.

Avoid pushing unfinished or untested code directly to `dev`.

## Common Problems

### API executable is locked

If the build says `TshwaneMetroRide.Api.exe` is being used by another process, an older API instance is still running.

Stop it with:

```powershell
Get-Process TshwaneMetroRide.Api -ErrorAction SilentlyContinue |
    Stop-Process -Force
```

Then run:

```powershell
dotnet clean
dotnet build
dotnet run
```

### `dotnet ef` is not recognised

Close and reopen PowerShell or Visual Studio Code.

Alternatively:

```powershell
$env:Path += ";$env:USERPROFILE\.dotnet\tools"
```

Then:

```powershell
dotnet ef --version
```

### Database connection fails

Check that:

- MySQL is running in XAMPP.
- MariaDB is using port `3306`.
- `tshwane_metro_ride_db` exists.
- The username and password in `appsettings.json` are correct.
- No other MySQL or MariaDB service is conflicting with XAMPP.

### phpMyAdmin does not open

Start both Apache and MySQL in XAMPP, then open:

```text
http://localhost/phpmyadmin/
```

phpMyAdmin is optional. The backend can run without Apache or phpMyAdmin.

## Planned Features

- Passenger login
- JWT authentication
- Passenger profile management
- Bus-card linking
- Wallet balance
- Card top-ups
- Wallet transaction history
- Ticket purchases
- Routes and stops
- Bus and trip management
- Passenger trip history
- Administrative functionality

## Security Notes

- Never store plain-text passwords.
- Never return password hashes from API endpoints.
- Never commit real database passwords or JWT secrets.
- Use environment variables or .NET user secrets for sensitive values.
- Validate all incoming API requests.
- Protect private endpoints with JWT authentication.

## Team Notes

Always ensure that:

1. XAMPP MySQL is running before starting the API.
2. The latest migrations have been applied.
3. You are working from the latest `dev` branch.
4. The project builds successfully before creating a pull request.
5. New endpoints are tested through Swagger before they are merged.
