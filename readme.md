## About

**RansomGuard** is a web platform that uses **Machine Learning** to detect ransomware in executable files. The system performs a static analysis of an uploaded `.exe` or `.dll` file (without executing it) and returns a real-time prediction:

- **SAFE** — the file is likely benign
- **RANSOMWARE** — the file exhibits malicious characteristics

This project was developed as part of a research paper on **"Ransomware Evolution and Defense Mechanisms"**.

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
