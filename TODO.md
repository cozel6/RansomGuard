# RansomGuard - Development TODO

**Last updated:** 2026-03-18

## 📋 How to Use This Document

- `[ ]` checkbox = task not started
- `[x]` checkbox = task completed
- Tasks are organized by priority phases
- Each task links to relevant documentation

---

## Phase 1: Project Setup & Infrastructure

### 1.1 Testing Infrastructure

- [x] Create RansomGuard.API.Tests project
  - [x] Add xUnit, xUnit.runner.visualstudio, Microsoft.NET.Test.Sdk
  - [x] Add Moq for mocking
  - [x] Add FluentAssertions for readable assertions
  - [x] Add project reference to RansomGuard.API
  - [x] Create sample test to verify setup
- [x] Add test project to RansomGuard.sln
- [x] Verify `dotnet test` runs successfully

### 1.2 Code Quality & Static Analysis

- [ ] Create `.editorconfig` with C# formatting rules
  - [ ] Define indentation (4 spaces)
  - [ ] Set naming conventions (PascalCase for classes, camelCase for locals)
  - [ ] Configure line length limits
  - [ ] Set severity levels (warning vs error)
- [ ] Create `Directory.Build.props` for solution-wide analyzer configuration
  - [ ] Enable TreatWarningsAsErrors for Release
  - [ ] Enable EnforceCodeStyleInBuild
  - [ ] Configure AnalysisLevel=latest
- [x] Add analyzer packages to RansomGuard.API
  - [x] SonarAnalyzer.CSharp
  - [x] SecurityCodeScan.VS2019
- [ ] Verify `dotnet build` shows analyzer suggestions
- [ ] Fix any existing analyzer warnings

### 1.3 Documentation Structure

- [x] Create `/docs` directory structure
  - [x] `/docs/research/` - Academic research docs (Romanian)
  - [x] `/docs/architecture/` - Technical architecture docs (English)
  - [x] `/docs/api/` - API documentation (auto-generated from Swagger)
- [x] Create `/docs/README.md` with documentation guide
- [ ] Create research documentation templates
  - [ ] `/docs/research/01-introduction.md`
  - [ ] `/docs/research/02-literature-review.md`
  - [ ] `/docs/research/03-anatomy-attacks.md`
  - [ ] `/docs/research/04-defense-mechanisms.md`
  - [ ] `/docs/research/05-implementation-notes.md`

---

## Phase 2: Backend Core API

### 2.1 File Upload Endpoint

- [x] Create file validation utilities (`Validators/FileValidator.cs`)
  - [x] Validate file size (max 10MB)
  - [x] Validate file extension (.exe, .dll whitelist)
  - [x] Path traversal detection
  - [x] PE header validation (MZ signature 0x4D5A)
  - [x] Made FileValidator static class for consistency
- [x] Add unit tests for validation logic
  - [x] Test file size validation
  - [x] Test extension validation
  - [x] Test path traversal attempts (../../etc/passwd)
  - [x] Test PE header validation (valid MZ, invalid signatures, null/empty streams)
- [x] Create `Controllers/FileUploadController.cs`
  - [x] POST /api/upload endpoint
  - [x] Accept IFormFile (multipart/form-data)
  - [x] Call FileValidator.IsValidPEHeaderAsync()
  - [x] Call FileUploadHelper.SaveUploadedFileAsync() with GUID filename
  - [x] Store in isolated temp directory
  - [x] Return upload ID (GUID) with UploadResponse
  - [x] Log upload events with structured logging
  - [x] All validations implemented (size, extension, path traversal, PE header)
- [x] Create `Models/UploadResponse.cs` DTO
- [x] Create `Models/ErrorResponse.cs` DTO
- [x] Create `Models/Verdict.cs` enum
- [x] Create `Services/FileUploadHelper.cs` and `IFileUploadHelper` interface
  - [x] SaveUploadedFileAsync with GUID filename generation
  - [x] SHA256 hash calculation with Convert.ToHexStringLower()
  - [x] DeleteFile cleanup method
  - [x] Temp directory auto-creation
  - [x] Comprehensive logging
- [ ] Test null byte injection (file.exe\0.txt)
- [ ] Add integration test for upload endpoint
- [x] Document endpoint in XML comments (for Swagger)

### 2.2 PE Analysis Service

- [x] Add `PeNet` NuGet package to RansomGuard.API
- [x] Create `Services/PEAnalysisService.cs` and `IPEAnalysisService` interface
  - [x] Implement `AnalyzeFileAsync(string filePath)`
  - [x] Calculate Shannon entropy with LINQ optimization
  - [x] Extract PE sections
  - [x] Extract import table (API calls)
  - [x] Extract export table
  - [x] Detect suspicious APIs with Contains() for better matching
  - [x] Generate risk score (0-100) with C# 12 switch expressions
  - [x] Modernized with C# 12/13 features (collection expressions, pattern matching)
- [x] Create `Models/AnalysisResult.cs` DTO
  - [x] UploadId (Guid)
  - [x] Filename (string)
  - [x] Timestamp (DateTime)
  - [x] RiskScore (int)
  - [x] Entropy (double)
  - [x] SuspiciousAPIs (List<string>)
  - [x] Verdict (Verdict enum)
  - [x] FileHash (SHA256 string)
- [x] Register service in `Program.cs` DI container
- [ ] Add unit tests for analysis logic
  - [ ] Test entropy calculation
  - [ ] Test suspicious API detection
  - [ ] Test risk score algorithm
- [ ] Add integration test with demo DLL file

### 2.3 Database & Persistence

- [x] Add Entity Framework Core packages
  - [x] Microsoft.EntityFrameworkCore
  - [x] Microsoft.EntityFrameworkCore.Sqlite
  - [x] Microsoft.EntityFrameworkCore.Design
- [x] Create `Data/RansomGuardDbContext.cs`
  - [x] DbSet<AnalysisResultEntity> AnalysisResults
  - [x] Configure entity mappings with fluent API
  - [x] Add indexes for Timestamp and FileHash
  - [x] Configure SQLite connection string in appsettings.json
- [x] Create `Data/Entities/AnalysisResultEntity.cs`
  - [x] Map from AnalysisResult DTO
  - [x] Added proper constraints (max length, required fields)
- [x] Run EF Core migrations
  - [x] `dotnet ef migrations add InitialCreate`
  - [x] `dotnet ef database update`
- [x] Create `Services/AnalysisRepository.cs`
  - [x] `SaveAnalysisAsync(AnalysisResultEntity entity)`
  - [x] `GetAnalysisByIdAsync(Guid id)`
  - [x] `GetRecentAnalysesAsync(int count)`
  - [x] Added IAnalysisRepository interface
  - [x] Added logging for save operations
- [x] Register repository in DI container (Program.cs)
- [ ] Add unit tests for repository
- [ ] Add integration test for database operations

### 2.4 Result Retrieval Endpoints

- [x] Create `Controllers/AnalysisController.cs`
  - [x] GET /Analysis/{id:guid} - Retrieve single result
  - [x] GET /Analysis/history?count=10 - Recent analyses
- [x] Add error handling (404 if not found)
- [ ] Add response caching headers
- [x] Document endpoints in XML comments
- [ ] Add integration tests for retrieval endpoints

### 2.5 Swagger UI Configuration

- [x] Install Swashbuckle.AspNetCore package
- [x] Configure AddSwaggerGen() in Program.cs
- [x] Configure UseSwagger() and UseSwaggerUI() middleware
- [x] Add app.MapControllers() for controller routing
- [x] Add automatic redirect from root (/) to /swagger
- [x] Remove OpenAPI in favor of classic Swagger UI
- [ ] Configure Swagger XML documentation output
- [ ] Add API versioning support

### 2.6 End-to-End Workflow Integration

- [ ] Update `FileUploadController` to call `PEAnalysisService`
- [ ] Chain: Upload → Validate → Analyze → Persist → Return Result
- [x] Add async/await throughout the pipeline
- [ ] Add timeout handling for long-running analysis
- [x] Add comprehensive logging (Serilog)
- [ ] Delete temp files after analysis
- [ ] Add end-to-end integration test
  - [ ] Upload file → Analyze → Retrieve result → Verify correctness

---

## Phase 3: Testing & Quality Assurance

### 3.1 Unit Test Coverage

- [ ] Achieve >80% code coverage for Services layer
- [x] Add coverage reporting with Coverlet
  - [x] `dotnet add package coverlet.collector`
  - [ ] Run: `dotnet test /p:CollectCoverage=true`
- [ ] Review coverage report and add missing tests

### 3.2 Security Testing

- [ ] Test all OWASP Top 10 mitigations
  - [ ] Injection attacks (SQL, path traversal, null byte)
  - [ ] File upload vulnerabilities
  - [ ] Insecure deserialization
  - [ ] Logging failures
- [ ] Run SecurityCodeScan and fix warnings
- [ ] Perform manual security review

### 3.3 Performance Testing

- [ ] Test file upload with 10MB file
- [ ] Test analysis performance (should complete <5 seconds)
- [ ] Test concurrent uploads (stress test)
- [ ] Optimize any bottlenecks

---

## Phase 4: Research Documentation

### 4.1 Introduction (Chapter 1)

- [ ] Write `/docs/research/01-introduction.md`
  - [ ] Ransomware threat landscape
  - [ ] Global statistics (costs, attack frequency)
  - [ ] Thesis motivation (why ransomware detection matters)
  - [ ] RansomGuard project objectives
- [ ] Gather statistics from:
  - [ ] ENISA Threat Landscape 2024
  - [ ] Verizon DBIR 2024
  - [ ] Cybersecurity Ventures reports

### 4.2 Literature Review (Chapter 2)

- [ ] Write `/docs/research/02-literature-review.md`
  - [ ] Ransomware definition
  - [ ] Classification (crypto-ransomware vs locker-ransomware)
  - [ ] Encryption algorithms used (AES, RSA)
- [ ] Cite academic papers:
  - [ ] Razaulla et al., "The Age of Ransomware", IEEE Access 2023
  - [ ] Sgandurra et al., "Automated Dynamic Analysis", arXiv 2016
  - [ ] Kharraz et al., "Cutting the Gordian Knot", DIMVA 2015

### 4.3 Anatomy of Attacks (Chapters 3 & 5)

- [ ] Write `/docs/research/03-anatomy-attacks.md`
  - [ ] Kill chain (initial access, lateral movement, exfiltration, encryption)
  - [ ] Case studies: WannaCry, NotPetya, Colonial Pipeline, REvil
  - [ ] Ransomware-as-a-Service (RaaS)
  - [ ] Attack automation (cryptoworms, network scanning)
- [ ] Include diagrams (Mermaid or PlantUML)
- [ ] Reference MITRE ATT&CK framework

### 4.4 Defense Mechanisms (Chapter 6)

- [ ] Write `/docs/research/04-defense-mechanisms.md`
  - [ ] Prevention (backup, segmentation, Zero Trust)
  - [ ] Detection (EDR, SIEM, ML-based)
  - [ ] Response (incident response, recovery)
  - [ ] Role of AI/ML in detection
- [ ] Compare static vs dynamic analysis
- [ ] Discuss ML approaches (Random Forest, XGBoost, Neural Networks)

### 4.5 Implementation Notes (Chapter 7)

- [ ] Write `/docs/research/05-implementation-notes.md`
  - [ ] Why .NET for backend? (performance, type safety, ecosystem)
  - [ ] Why Python for ML? (scikit-learn, PyTorch, rich ecosystem)
  - [ ] Why SQLite? (lightweight, embedded, sufficient for demo)
  - [ ] Clean Architecture rationale
  - [ ] Security design decisions
- [ ] Document architectural trade-offs

---

## Phase 5: Frontend Development (Deferred)

### 5.1 React Project Setup

- [ ] Create `/frontend` directory
- [ ] Initialize React app with Vite or Create React App
- [ ] Configure TypeScript
- [ ] Add TailwindCSS or Material-UI
- [ ] Add Axios for HTTP requests

### 5.2 File Upload UI

- [ ] Create FileUpload component
  - [ ] Drag & drop zone
  - [ ] File selection button
  - [ ] File validation (client-side)
  - [ ] Upload progress indicator
- [ ] Add error handling and user feedback

### 5.3 Results Display

- [ ] Create AnalysisResult component
  - [ ] Display verdict (SAFE / RANSOMWARE)
  - [ ] Show risk score with visual indicator (progress bar, color-coded)
  - [ ] Show entropy value
  - [ ] List suspicious APIs detected
  - [ ] Show timestamp
- [ ] Add result details expansion

### 5.4 History Dashboard

- [ ] Create AnalysisHistory component
  - [ ] Table of recent analyses
  - [ ] Pagination
  - [ ] Click to view details
- [ ] Add filtering and sorting

### 5.5 Frontend-Backend Integration

- [ ] Configure CORS in backend (already done)
- [ ] Test API calls from React app
- [ ] Add loading states
- [ ] Add error boundaries

---

## Phase 6: ML Integration (Deferred)

### 6.1 Dataset Preparation

- [ ] Research Kaggle datasets
  - [ ] Microsoft Malware Classification (BIG2015)
  - [ ] Ransomware Detection Dataset
  - [ ] CIC-MalMem-2022
- [ ] Download and explore selected dataset
- [ ] Document dataset characteristics in `/docs/research/06-ml-implementation.md`

### 6.2 Feature Engineering

- [ ] Define feature vector
  - [ ] File entropy
  - [ ] PE section count and names
  - [ ] Import table size and suspicious API count
  - [ ] Export table size
  - [ ] File size
  - [ ] Timestamp anomalies
- [ ] Implement feature extraction pipeline
- [ ] Document feature engineering decisions

### 6.3 Model Training

- [ ] Create `/ml` directory
- [ ] Setup Python virtual environment
- [ ] Install dependencies (scikit-learn, pandas, numpy)
- [ ] Train Random Forest model (baseline)
- [ ] Train XGBoost model (optimized)
- [ ] Evaluate models (Accuracy, Precision, Recall, F1)
- [ ] Document evaluation metrics in research docs

### 6.4 Python ML Service

- [ ] Create FastAPI service
  - [ ] POST /predict endpoint
  - [ ] Load trained model
  - [ ] Accept feature vector JSON
  - [ ] Return prediction and confidence score
- [ ] Add model versioning
- [ ] Add logging

### 6.5 .NET ↔ Python Integration

- [ ] Add HttpClient in `Services/MLServiceClient.cs`
- [ ] Call Python service from PE Analysis workflow
- [ ] Add timeout and retry logic
- [ ] Add fallback to heuristic-only analysis if ML service unavailable

---

## Phase 7: Deployment & CI/CD (Future)

### 7.1 Docker Containerization

- [ ] Create Dockerfile for backend
- [ ] Create Dockerfile for ML service
- [ ] Create Dockerfile for frontend
- [ ] Create docker-compose.yml

### 7.2 GitHub Actions CI/CD

- [ ] Create `.github/workflows/backend-ci.yml`
  - [ ] Run tests on every PR
  - [ ] Run static analyzers
  - [ ] Build artifacts
- [ ] Create `.github/workflows/frontend-ci.yml`
- [ ] Add deployment workflow (optional)

### 7.3 Demo Deployment

- [ ] Deploy to Azure App Service / AWS / DigitalOcean
- [ ] Configure environment variables
- [ ] Setup monitoring and logging
- [ ] Document deployment process

---

## Phase 8: Demo DLL Implementation

### 8.1 Create Demo Ransomware Sample

- [ ] Create C# project for demo DLL
- [ ] Import cryptographic APIs (without calling them)
  - [ ] CryptEncrypt
  - [ ] BCryptEncrypt
  - [ ] CryptGenRandom
- [ ] Add high entropy data section
- [ ] Use non-standard PE section names
- [ ] Strip debug symbols
- [ ] Compile as DLL
- [ ] Verify it triggers RansomGuard detection
- [ ] Document in `/demo/readme.md`
- [ ] Add source code to `/demo/src/`

---

## Verification Checklist (Before Completion)

- [ ] All unit tests pass (`dotnet test`)
- [ ] All integration tests pass
- [ ] No analyzer warnings (`dotnet build`)
- [ ] Code formatted (`dotnet format`)
- [ ] All research documents completed
- [ ] API documentation up-to-date (Swagger)
- [ ] README.md updated with project status
- [ ] SETUP.md reflects current dependencies
- [ ] Demo video/screenshots created (optional)
- [ ] Deployment guide documented

---

## Quick Commands Reference

```bash
# Build
dotnet build

# Run with hot reload
dotnet watch

# Run tests
dotnet test

# Format code
dotnet format

# Add package
dotnet add package <PackageName>
```

---

**Notes:**

- All code changes should follow established patterns
- Research documentation should be in Romanian for thesis integration
