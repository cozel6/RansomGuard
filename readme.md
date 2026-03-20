## About

**RansomGuard** is a web platform that uses **Machine Learning** to detect ransomware in executable files. The system performs a static analysis of an uploaded `.exe` or `.dll` file (without executing it) and returns a real-time prediction:

- **SAFE** — the file is likely benign
- **RANSOMWARE** — the file exhibits malicious characteristics

This project was developed as part of a research paper on **"Ransomware Evolution and Defense Mechanisms"**.

## Project Status

| Component         | Status            | Progress |
| ----------------- | ----------------- | -------- |
| Backend API       | 🟢 Complete       | 96%      |
| Static Analysis   | 🟢 Complete       | 100%     |
| Database          | 🟢 Complete       | 100%     |
| Code Quality      | 🟢 Complete       | 100%     |
| Unit Tests        | 🟢 Complete       | 32 tests |
| Integration Tests | 🟡 Partial        | 2 tests  |
| ML Service        | ⚪ Deferred       | 0%       |
| Frontend          | ⚪ Deferred       | 0%       |
| Research Docs     | 🔴 Not Started    | 0%       |

**Current Status:** Backend production-ready with comprehensive code quality tools
**Last Updated:** 2026-03-20

## Documentation

### For Developers

- **[TODO.md](TODO.md)** - Step-by-step development checklist
- **[SETUP.md](SETUP.md)** - Backend setup instructions
- **[TESTING.md](TESTING.md)** - Testing guide and troubleshooting
- **[CONFIGURATION_SUMMARY.md](CONFIGURATION_SUMMARY.md)** - Code quality tools configuration
- **[IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md)** - Comprehensive implementation guide

### Technical Specifications

- **[/docs/README.md](docs/README.md)** - Documentation structure guide
- **[/docs/architecture/](docs/architecture/)** - Technical architecture docs _(planned)_

## Quick Start

### Prerequisites

- .NET 10.0 SDK
- Visual Studio Code or Visual Studio 2022

### Setup

```bash
# Clone repository
git clone <repo-url>
cd RansomGuard

# Install dependencies
cd backend/RansomGuard.API
dotnet restore

# Run backend
dotnet watch
```

Visit: **http://localhost:5087/swagger**

### Running Tests

```bash
cd backend/RansomGuard.API.Tests
dotnet test
```

## Getting Started

### Backend Setup

To set up and run the backend API, follow the instructions in [SETUP.md](SETUP.md).

## Security

- Files are never executed on the server
- Analysis is exclusively static
- Files are deleted from the server after analysis
- Maximum upload size: 10MB

## Academic Context

This project is part of the research paper:

> **"Ransomware Evolution and Defense Mechanisms"**
> Cybersecurity — ML-based malware detection

**Main sources:**

- ENISA Threat Landscape for Ransomware Attacks (2022)
- CISA #StopRansomware Guide (2023)
- Razaulla et al., "The Age of Ransomware", IEEE Access (2023)
- Sgandurra et al., "Automated Dynamic Analysis of Ransomware", arXiv (2016)
