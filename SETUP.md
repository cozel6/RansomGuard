# RansomGuard - Setup and Dependencies Installation

**Compatible with:** Windows, macOS, and Linux

## Prerequisites

```bash
dotnet --version
```

## Navigate to Project Directory

```bash
cd backend/RansomGuard.API
```

## Install NuGet Packages

### Entity Framework Core with SQLite

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Logging (Serilog)

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

### PeNet - PE File Reader

```bash
dotnet add package PeNet
```

### CORS Support

```bash
dotnet add package Microsoft.AspNetCore.Cors
```

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
