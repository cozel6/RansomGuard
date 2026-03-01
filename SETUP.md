# RansomGuard - Setup and Dependencies Installation

**Compatible with:** Windows, macOS, and Linux

## Prerequisites

- **.NET 10 SDK** (version 10.0.103 or newer)

```bash
dotnet --version
# Should output: 10.0.103 or higher
```

## Navigate to Project Directory

```bash
cd backend/RansomGuard.API
```

## Install NuGet Packages

All packages are already configured in `RansomGuard.API.csproj` for .NET 10:

### Entity Framework Core with SQLite

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.2
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.2
```

### Logging (Serilog)

```bash
dotnet add package Serilog.AspNetCore --version 10.0.0
dotnet add package Serilog.Sinks.File --version 7.0.0
```

### PeNet - PE File Reader

```bash
dotnet add package PeNet --version 4.0.3
```

**Note:** `Microsoft.AspNetCore.Cors` and `Serilog.Sinks.Console` are no longer needed as they are included in the framework or are redundant with other packages.

## Restore and Build

```bash
dotnet restore
dotnet build
```

## Run Application

```bash
dotnet run
```

## Verify Installation

```bash
dotnet list package
```
