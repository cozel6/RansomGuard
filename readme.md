# RansomGuard

> Static PE file analysis platform for ransomware detection using Machine Learning

**RansomGuard** is a web platform that performs **static analysis** of uploaded `.exe` and `.dll` files  without executing them  and returns a real-time verdict powered by a trained ML model:

| Verdict | Meaning |
|---------|---------|
| **SAFE** | File shows no indicators of ransomware behavior |
| **SUSPICIOUS** | File has some concerning characteristics; manual review recommended |
| **RANSOMWARE** | File exhibits strong ransomware indicators; treat as malicious |

This project was developed as part of the research paper **"Ransomware Evolution and Defense Mechanisms"**.

---

## Project Status

| Component                | Status         | Progress |
| ------------------------ | -------------- | -------- |
| Backend API              | Complete       | 100%     |
| Static Analysis Engine   | Complete       | 100%     |
| Database                 | Complete       | 100%     |
| Unit Tests               | Complete       | 47 tests |
| Integration Tests        | Complete       | 16 tests |
| Frontend                 | Complete       | 100%     |
| ML Service               | Complete       | 100%     |
| Backend ↔ ML Integration | Complete       | 100%     |
| Research Docs            | Not Started    | 0%       |
| Docker Deployment        | Future         | 0%       |

**Last Updated:** 2026-05-26

---

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│                     Browser (React)                       │
│   Upload .exe/.dll  →  Results page  →  History page     │
└─────────────────────────┬────────────────────────────────┘
                          │ HTTP REST
┌─────────────────────────▼────────────────────────────────┐
│              Backend API  (ASP.NET Core 10)               │
│                                                           │
│  1. Validate: size, extension, PE magic bytes (MZ)       │
│  2. Static Heuristic Engine (C# / PeNet)                 │
│     • Shannon entropy calculation                         │
│     • PE section count anomaly detection                  │
│     • 21 suspicious Windows API detection                 │
│     • Risk score 0–100 → fallback verdict                 │
│  3. Call ML service → override verdict if available      │
│  4. Persist to SQLite                                     │
│  5. Delete temp file                                      │
└─────────────────────────┬────────────────────────────────┘
                          │ HTTP multipart/form-data
┌─────────────────────────▼────────────────────────────────┐
│              ML Service  (Python + FastAPI)               │
│                                                           │
│  • Feature extraction: thrember (EMBER2024 extractor)    │
│  • Model: LightGBM trained on EMBER2024 dataset          │
│  • Returns: prediction + confidence score (0.0–1.0)      │
└──────────────────────────────────────────────────────────┘
```

---

## Machine Learning Model

This section explains the machine learning approach in detail  the type of problem, the algorithm, the dataset, and how a prediction is produced.

### 1. Type of Classification Problem

RansomGuard solves a **binary classification** problem at the ML level:

> *Given a PE file, what is the probability that it is malicious?*

The model outputs a single continuous score **`p ∈ [0.0, 1.0]`**, where `0` means "certainly benign" and `1` means "certainly malicious". This score is then mapped to three human-readable verdicts using fixed thresholds:

```
raw_score >= 0.80  →  RANSOMWARE   (high confidence malicious)
raw_score >= 0.40  →  SUSPICIOUS   (ambiguous / review recommended)
raw_score  < 0.40  →  SAFE         (likely benign)
```

The three-class output is **not** produced directly by the model  it is a post-processing decision layer on top of a probabilistic binary classifier.

---

### 2. Algorithm: LightGBM (Gradient Boosting)

The model is a **LightGBM Booster**  a state-of-the-art gradient boosting decision tree framework developed by Microsoft Research.

#### What is Gradient Boosting?

Gradient Boosting builds an **ensemble of decision trees** sequentially, where each new tree corrects the residual errors of the previous ones:

```
Final prediction = Tree₁(x) + Tree₂(x) + Tree₃(x) + ... + Treeₙ(x)
```

Each tree is a "weak learner" (shallow decision tree), but their combination produces a strong, accurate classifier.

#### Why LightGBM specifically?

| Property | Details |
|----------|---------|
| **Growth strategy** | Leaf-wise (best-first) instead of level-wise  finds splits with the largest loss reduction first |
| **Speed** | Uses histogram-based split finding → dramatically faster than XGBoost on large feature sets |
| **Accuracy** | Consistently top performance on tabular/structured data in security research |
| **Feature handling** | Native support for high-dimensional sparse features (e.g. import hash vectors) |
| **Memory efficiency** | Gradient-based One-Side Sampling (GOSS)  keeps hard examples, samples easy ones |

#### Comparison with other algorithms

| Algorithm | Type | Strength | Weakness vs LightGBM |
|-----------|------|----------|----------------------|
| **Random Forest** | Bagging (parallel trees) | Robust, simple | Slower, less accurate on imbalanced data |
| **XGBoost** | Boosting (level-wise) | Well-tuned, accurate | Slower training on large feature sets |
| **LightGBM** | Boosting (leaf-wise) | Fast, accurate, memory-efficient | Can overfit on small datasets |
| **Neural Network (MLP)** | Deep learning | Flexible representation | Needs much more data, less interpretable |
| **SVM** | Kernel-based | Good on small data | Doesn't scale to 2000+ features |

LightGBM is the standard choice for malware classification on EMBER-style feature sets in academic and industry research.

---

### 3. Dataset: EMBER2024

The model is trained on **EMBER2024**  an open benchmark dataset for PE malware detection published at **SIGKDD 2025** by FutureComputing4AI (Sophos AI).

| Property | Value |
|----------|-------|
| **Full name** | Elastic Malware Benchmark for Empowering Researchers 2024 |
| **Published** | SIGKDD 2025 |
| **Authors** | FutureComputing4AI / Sophos AI |
| **Content** | PE files labeled as benign or malicious |
| **Feature dimensions** | ~2381 features per file |
| **Model source** | Hugging Face: `joyce8/EMBER2024-benchmark-models` |
| **Model file** | `EMBER2024_PE.model` (~3.7 MB, LightGBM `.txt` format) |
| **Extractor** | `thrember` Python library (official EMBER2024 extractor) |

EMBER2024 is the successor to the original EMBER 2018 dataset (Anderson & Roth, Elastic). It covers modern malware families and is periodically updated to reflect the current threat landscape.

---

### 4. Feature Extraction Pipeline

When a file arrives at the ML service, the `thrember.PEFeatureExtractor` processes the raw bytes and produces a **fixed-length feature vector** of ~2381 dimensions. No file path is needed  extraction works directly on bytes in memory.

```python
extractor = thrember.PEFeatureExtractor()
X = np.array(extractor.feature_vector(file_bytes)).reshape(1, -1)
raw_score = model.predict(X)[0]
```

The feature vector is composed of multiple groups:

#### Feature Groups

| Group | Features | Description |
|-------|----------|-------------|
| **Byte Histogram** | 256 values | Normalized frequency of each byte value (0x00–0xFF) |
| **Byte Entropy Histogram** | 256 values | Entropy of bytes in local sliding windows  captures packing/encryption |
| **String Features** | ~100 values | Statistics on printable strings: count, mean length, URLs, registry paths, MZ headers found inside |
| **General Info** | ~10 values | File size, virtual size, has debug info, has relocations, has resources |
| **PE Header** | ~50 values | Machine type, timestamp, characteristics flags, linker version, sizeof headers/code/data |
| **Section Features** | ~255 values (5 sections × 51 features) | For each section: hashed name, raw/virtual size, entropy, characteristics flags |
| **Import Features** | ~1280 values | Hashed library names + top-k function name hashes (sparse encoding) |
| **Export Features** | ~128 values | Hashed export function names |
| **Datadirectory** | ~30 values | Presence and sizes of PE data directories (imports, exports, resources, TLS, etc.) |

#### Why these features catch ransomware

- **High byte entropy** → file is encrypted or packed (common in ransomware to evade AV)
- **Cryptography API imports** (`CryptEncrypt`, `BCryptGenRandom`) → encryption behavior
- **Shadow copy deletion APIs** (`DeleteFile`, `CreateProcess` + `vssadmin`) → anti-recovery behavior
- **Anomalous section count/names** → custom packers, obfuscation
- **Low export count + high import count** → typical executable (not library) pattern

---

### 5. Two-Layer Analysis Architecture

RansomGuard uses a **defense-in-depth** approach with two independent analysis layers:

```
File Upload
    │
    ▼
┌─────────────────────────────────────┐
│  Layer 1: Static Heuristic Engine   │  ← Always runs (C# / .NET)
│                                     │
│  • Shannon entropy (threshold 6.5+) │
│  • PE section anomaly detection     │
│  • 21 suspicious Win32 API checks   │
│  • Risk score 0–100                 │
│  • Verdict: Safe / Suspicious /     │
│    Ransomware (fallback)            │
└──────────────────┬──────────────────┘
                   │
                   ▼  (if ML service available)
┌─────────────────────────────────────┐
│  Layer 2: ML Model (EMBER2024)      │  ← Overrides Layer 1 verdict
│                                     │
│  • thrember feature extraction      │
│  • LightGBM inference (~2381 feat.) │
│  • Probability score 0.0–1.0        │
│  • Verdict + confidence returned    │
└─────────────────────────────────────┘
                   │
                   ▼
           Final Verdict + Confidence
```

If the ML service is offline or fails, Layer 1 provides the verdict automatically  ensuring the system is always functional.

---

### 6. Static Heuristic Engine Details

The `PEAnalysisService` (C#) calculates a **risk score** from 0 to 100 using four indicators:

#### Shannon Entropy

Measures the randomness of the file's bytes. High entropy indicates encryption or compression  both common in ransomware (to hide payload or encrypted victim files):

```
H(X) = -∑ p(x) × log₂(p(x))
```

| Entropy Range | Score Added | Interpretation |
|--------------|-------------|----------------|
| > 7.0 bits   | +30         | Very likely packed/encrypted |
| > 6.5 bits   | +15         | Possibly packed |
| ≤ 6.5 bits   | +0          | Normal range |

#### Suspicious API Detection

The engine checks the PE Import Table for 21 Windows APIs commonly abused by ransomware:

```
Encryption:    CryptEncrypt, CryptDecrypt, CryptAcquireContext,
               BCryptEncrypt, BCryptDecrypt, BCryptGenRandom
File ops:      DeleteFile, DeleteFileW, DeleteFileA,
               WriteFile, WriteFileEx
Process:       CreateProcess, CreateProcessW, CreateProcessA
Registry:      RegSetValue, RegSetValueEx, RegSetValueExW
Memory/Inject: VirtualAlloc, VirtualAllocEx,
               CreateRemoteThread, OpenProcess
```

| APIs Detected | Score Added |
|--------------|-------------|
| > 5          | +40         |
| > 2          | +20         |
| > 0          | +10         |
| 0            | +0          |

#### Section Count Anomaly

```
section_count > 8 OR < 2  →  +20
```

Most legitimate executables have 3–6 sections. Extreme values suggest custom packers or malformed binaries.

#### Export Count

```
export_count < 5  →  +10
```

Executables (not libraries) typically export few or no functions. This is a weak signal used to differentiate `.exe` behavior.

#### Verdict Mapping

```
risk_score >= 70  →  Ransomware
risk_score >= 35  →  Suspicious
risk_score  < 35  →  Safe
```

---

### 7. Model Performance Context

The EMBER2024 benchmark model achieves the following metrics on the EMBER2024 test set (reported by the dataset authors):

| Metric | Value |
|--------|-------|
| **AUC-ROC** | ~0.99 |
| **TPR @ 1% FPR** | ~96% |
| **Model size** | ~3.7 MB |

*Note: These metrics reflect general PE malware detection. Ransomware-specific precision depends on the composition of malicious samples in the training set.*

---

## Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Backend API | ASP.NET Core | 10.0 |
| PE Parsing | PeNet | Latest |
| Database | SQLite + EF Core | Latest |
| Frontend | React + TypeScript + Tailwind | 18.3 / 5.6 |
| ML Framework | LightGBM | 4.5.0 |
| ML API | Python + FastAPI | 3.11+ / 0.115 |
| Feature Extraction | thrember (EMBER2024) | Latest |
| Model Registry | Hugging Face Hub |  |

---

## Quick Start

### Prerequisites

- .NET 10.0 SDK
- Node.js 18+
- Python 3.11+
- Homebrew (macOS)  required for `libomp` (LightGBM dependency)

### Backend

```bash
cd backend/RansomGuard.API
dotnet restore
dotnet watch
```

Swagger UI: **http://localhost:5087/swagger**

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Visit: **http://localhost:5173**

### ML Service

```bash
cd ml-service

# macOS: install libomp (required by LightGBM)
brew install libomp

# Create virtual environment
python3.11 -m venv venv
source venv/bin/activate

# Install dependencies
pip install -r requirements.txt
pip install git+https://github.com/FutureComputing4AI/EMBER2024.git
pip install signify==0.7.1

# Download EMBER2024 model (~3.7 MB from Hugging Face)
python scripts/download_model.py

# Start service
uvicorn app.main:app --reload --port 8000
```

Health check: **http://localhost:8000/health**

### Test the ML endpoint

```bash
curl -X POST http://localhost:8000/predict \
  -F "file=@demo/ransomguard_demo.dll"
# Expected: {"prediction":"suspicious","confidence":0.616,...}
```

### Run Backend Tests

```bash
cd backend/RansomGuard.API.Tests
dotnet test
```

---

## API Reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/upload` | Upload `.exe`/`.dll` for analysis |
| `GET` | `/api/analysis/{id}` | Retrieve analysis result by ID |
| `GET` | `/api/analysis/history` | List recent analyses (optional `?count=N&verdict=X`) |
| `GET` | `/swagger` | OpenAPI documentation |

### Upload Response

```json
{
  "uploadId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Analysis complete: Ransomware",
  "riskScore": 85,
  "verdict": "Ransomware",
  "mlConfidence": 0.943,
  "mlModelVersion": "ember2024-1.0"
}
```

### Error Codes

| Code | Meaning |
|------|---------|
| `FILE_TOO_LARGE` | File exceeds 10 MB |
| `INVALID_FILE_TYPE` | Extension is not `.exe` or `.dll` |
| `INVALID_PE_HEADER` | File does not start with MZ magic bytes |
| `ANALYSIS_FAILED` | Internal error during analysis |
| `NOT_FOUND` | Analysis ID does not exist |

---

## Security Model

Files are **never executed** on the server. The entire analysis is static.

| Threat | Mitigation |
|--------|-----------|
| Path traversal | GUID rename on upload; path traversal character rejection |
| Large file DoS | 10 MB hard limit |
| Malicious execution | Static analysis only; file deleted after analysis |
| Null byte injection | Filename sanitization |
| SQL injection | EF Core parameterized queries |
| Malformed PE | PeNet exception caught; request rejected with 400 |

---

## Academic Context

This project is part of the research paper:

> **"Ransomware Evolution and Defense Mechanisms"**
> Cybersecurity  ML-based malware detection

**Key references:**

- Anderson & Roth, "EMBER: An Open Dataset for Training Static PE Malware Machine Learning Models", arXiv 2018
- Joyce et al., "EMBER2024: A Large-Scale Benchmark for Static PE Malware Detection", SIGKDD 2025
- Razaulla et al., "The Age of Ransomware: A Survey on the Evolution, Taxonomy, and Research Directions", IEEE Access 2023
- Sgandurra et al., "Automated Dynamic Analysis of Ransomware: Benefits, Limitations and Use for Detection", arXiv 2016
- Ke et al., "LightGBM: A Highly Efficient Gradient Boosting Decision Tree", NeurIPS 2017
- ENISA Threat Landscape for Ransomware Attacks (2022, 2023)
- CISA #StopRansomware Guide (2023)

---

## Documentation

| Document | Purpose |
|----------|---------|
| [TODO.md](TODO.md) | Step-by-step development checklist |
| [SETUP.md](SETUP.md) | Backend setup instructions |
| [TESTING.md](TESTING.md) | Testing guide |
| [/docs/research/](docs/research/) | Academic research documentation |

---

## Service Ports

| Service | URL |
|---------|-----|
| Backend HTTP | http://localhost:5087 |
| Backend HTTPS | https://localhost:7179 |
| Swagger | http://localhost:5087/swagger |
| Frontend (dev) | http://localhost:5173 |
| ML Service | http://localhost:8000 |
