## About

**RansomGuard** is a web platform that uses **Machine Learning** to detect ransomware in executable files. The system performs a static analysis of an uploaded `.exe` or `.dll` file (without executing it) and returns a real-time prediction:

- **SAFE**  the file is likely benign
- **RANSOMWARE**  the file exhibits malicious characteristics

This project was developed as part of a research paper on **"Ransomware Evolution and Defense Mechanisms"**.

## Project Status

| Component                | Status         | Progress |
| ------------------------ | -------------- | -------- |
| Backend API              | 🟢 Complete    | ~95%     |
| Static Analysis          | 🟢 Complete    | 100%     |
| Database                 | 🟢 Complete    | 100%     |
| Unit Tests               | 🟢 Complete    | 32 tests |
| Integration Tests        | 🟡 Partial     | 2 tests  |
| Frontend                 | 🟢 Complete    | 100%     |
| ML Service               | 🟢 Complete    | 100%     |
| Backend ↔ ML Integration | 🔴 Not Started | 0%       |
| Research Docs            | 🔴 Not Started | 0%       |
| Docker Deployment        | ⚪ Future      | 0%       |

**Current Status:** Backend, Frontend și ML Service complete. Next steps Backend ↔.NET ↔ ML Service.
**Last Updated:** 2026-04-08

## Documentation

### For Developers

- **[TODO.md](TODO.md)** - Step-by-step development checklist
- **[SETUP.md](SETUP.md)** - Backend setup instructions
- **[TESTING.md](TESTING.md)** - Testing guide and troubleshooting

### Technical Specifications

- **[/docs/README.md](docs/README.md)** - Documentation structure guide
- **[/docs/architecture/](docs/architecture/)** - Technical architecture docs _(planned)_

## Quick Start

### Prerequisites

- .NET 10.0 SDK
- Node.js 18+
- Python 3.11+
- Homebrew (macOS)  necesar pentru `libomp` (LightGBM)

### Setup

```bash
# Clone repository
git clone <repo-url>
cd RansomGuard

# Backend
cd backend/RansomGuard.API
dotnet restore
dotnet watch
```

Visit: **http://localhost:5087/swagger**

```bash
# Frontend
cd frontend
npm install
npm run dev
```

Visit: **http://localhost:5173**

```bash
# ML Service
cd ml-service

# macOS: instalează libomp (necesar pentru LightGBM)
brew install libomp

# Setup virtual environment
python3.11 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
pip install git+https://github.com/FutureComputing4AI/EMBER2024.git
pip install signify==0.7.1

# Descarcă modelul EMBER2024 (~3.7MB)
python scripts/download_model.py

# Pornește serviciul
uvicorn app.main:app --reload --port 8000
```

Visit: **http://localhost:8000/health**

### Running Tests

```bash
cd backend/RansomGuard.API.Tests
dotnet test
```

### Test ML Service

```bash
curl -X POST http://localhost:8000/predict \
  -F "file=@demo/ransomguard_demo.dll"
# Răspuns așteptat: {"prediction":"suspicious","confidence":0.616,...}
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
> Cybersecurity  ML-based malware detection

**Main sources:**

- ENISA Threat Landscape for Ransomware Attacks (2022)
- CISA #StopRansomware Guide (2023)
- Razaulla et al., "The Age of Ransomware", IEEE Access (2023)
- Sgandurra et al., "Automated Dynamic Analysis of Ransomware", arXiv (2016)
