# RansomGuard Backend Implementation Plan
## Expert-Guided Implementation Strategy

> **Code Style:** This guide uses **modern C# 12/13 syntax** (.NET 8+):
> - Collection expressions: `= []` instead of `= new List<>()`
> - Switch expressions for pattern matching
> - LINQ functional style where appropriate
> - `static` methods for utility functions
> - `is` patterns for null checks and ranges

## Context

User wants to implement the RansomGuard backend from the perspective of a .NET expert and cybersecurity expert. The backend is currently at ~15-20% completion with solid foundations:

**What's Ready:**
- Serilog structured logging (production-grade)
- xUnit test infrastructure with FluentAssertions + Moq
- FileValidator with path traversal protection
- All dependencies installed (EF Core, PeNet, SecurityCodeScan)
- Empty but properly structured directories (Controllers/, Services/, Data/, Models/)

**What Needs Building:**
- Database layer (DbContext, entities, migrations, repository)
- File upload endpoint with PE header validation
- PE analysis service (entropy, API detection, risk scoring)
- Analysis retrieval endpoints
- End-to-end integration

**User's Requirement:** Provide complete, detailed instructions for each step, but user will write the code themselves.

---

## Recommended Implementation Order (Expert Perspective)

Based on .NET best practices and cybersecurity principles, here's the optimal build sequence with **defense-in-depth** and **fail-fast** philosophy:

### 🎯 PRIORITY ORDER RATIONALE

**Why this order?**
1. **Database First** - Foundation for persistence, needed by all services
2. **Models/DTOs** - Data contracts, needed by controllers and services
3. **Security Layer** - PE header validation, GUID filenames (prevent attacks before processing)
4. **Upload Controller** - Accept files, validate, persist metadata
5. **PE Analysis Service** - Core business logic (entropy, API detection, scoring)
6. **Retrieval Endpoints** - Query results
7. **Integration** - Wire everything together with proper error handling
8. **Testing** - Comprehensive coverage including security edge cases

---

## 📋 STEP-BY-STEP IMPLEMENTATION GUIDE

### **MILESTONE 1: Database Foundation (4-6 hours)**

#### Step 1.1: Create Database Entity
**File:** `backend/RansomGuard.API/Data/Entities/AnalysisResultEntity.cs`

**What to implement:**
```csharp
namespace RansomGuard.API.Data.Entities
{
    public class AnalysisResultEntity
    {
        // Primary key
        public Guid Id { get; set; }

        // File metadata
        public string Filename { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty; // SHA256
        public long FileSize { get; set; }

        // Analysis results
        public DateTime Timestamp { get; set; }
        public int RiskScore { get; set; }
        public double Entropy { get; set; }
        public string SuspiciousAPIs { get; set; } = string.Empty; // JSON array
        public string Verdict { get; set; } = string.Empty; // Safe, Suspicious, Ransomware

        // PE file details
        public int SectionCount { get; set; }
        public int ImportCount { get; set; }
        public int ExportCount { get; set; }
    }
}
```

**Key considerations:**
- `Guid` primary key (matches CLAUDE.md spec)
- `FileHash` for deduplication and integrity
- `SuspiciousAPIs` as JSON string (EF Core doesn't natively support List<string> in SQLite)
- All properties have default values (avoid nullable warnings)

---

#### Step 1.2: Create DbContext
**File:** `backend/RansomGuard.API/Data/RansomGuardDbContext.cs`

**What to implement:**
```csharp
using Microsoft.EntityFrameworkCore;
using RansomGuard.API.Data.Entities;

namespace RansomGuard.API.Data
{
    public class RansomGuardDbContext : DbContext
    {
        public RansomGuardDbContext(DbContextOptions<RansomGuardDbContext> options)
            : base(options)
        {
        }

        public DbSet<AnalysisResultEntity> AnalysisResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AnalysisResultEntity
            modelBuilder.Entity<AnalysisResultEntity>(entity =>
            {
                entity.ToTable("AnalysisResults");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Filename)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.FileHash)
                    .IsRequired()
                    .HasMaxLength(64); // SHA256 hex string

                entity.Property(e => e.Verdict)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.SuspiciousAPIs)
                    .HasMaxLength(4000); // SQLite TEXT limit

                // Index for faster queries
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.FileHash);
            });
        }
    }
}
```

**Key considerations:**
- Constructor injection for `DbContextOptions`
- Explicit table/column configuration (avoid EF Core conventions)
- Indexes on `Timestamp` (for history queries) and `FileHash` (deduplication)
- String length limits prevent SQLite bloat

---

#### Step 1.3: Update appsettings.json
**File:** `backend/RansomGuard.API/appsettings.json`

**Add this configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ransomguard.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  },
  "AllowedHosts": "*",
  "RansomGuard": {
    "MaxUploadSizeMB": 10,
    "TempDirectory": "./uploads/temp",
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]
  }
}
```

**Key considerations:**
- SQLite database in project root (simple for demo)
- EF logging suppressed (avoid query spam)
- Custom `RansomGuard` section for app settings
- CORS origins for React dev servers (Vite uses 5173, CRA uses 3000)

---

#### Step 1.4: Register DbContext in Program.cs
**File:** `backend/RansomGuard.API/Program.cs`

**Add after `builder.Services.AddOpenApi();`:**
```csharp
// Add DbContext
builder.Services.AddDbContext<RansomGuardDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Also add using statement at top:**
```csharp
using Microsoft.EntityFrameworkCore;
using RansomGuard.API.Data;
```

---

#### Step 1.5: Run EF Core Migrations
**Commands to run:**
```bash
# From backend/RansomGuard.API directory
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Expected output:**
- Migration file created in `Migrations/` directory
- `ransomguard.db` file created in project root
- AnalysisResults table created with indexes

**Troubleshooting:**
- If `dotnet ef` not found: `dotnet tool install --global dotnet-ef`
- If migration fails: verify `Microsoft.EntityFrameworkCore.Design` package installed

---

#### Step 1.6: Create Repository
**File:** `backend/RansomGuard.API/Services/AnalysisRepository.cs`

**What to implement:**
```csharp
using Microsoft.EntityFrameworkCore;
using RansomGuard.API.Data;
using RansomGuard.API.Data.Entities;

namespace RansomGuard.API.Services
{
    public interface IAnalysisRepository
    {
        Task<Guid> SaveAnalysisAsync(AnalysisResultEntity entity);
        Task<AnalysisResultEntity?> GetAnalysisByIdAsync(Guid id);
        Task<List<AnalysisResultEntity>> GetRecentAnalysesAsync(int count);
    }

    public class AnalysisRepository : IAnalysisRepository
    {
        private readonly RansomGuardDbContext _context;
        private readonly ILogger<AnalysisRepository> _logger;

        public AnalysisRepository(RansomGuardDbContext context, ILogger<AnalysisRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Guid> SaveAnalysisAsync(AnalysisResultEntity entity)
        {
            _context.AnalysisResults.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Analysis saved: {Id}, Verdict: {Verdict}, Risk: {Risk}",
                entity.Id, entity.Verdict, entity.RiskScore);

            return entity.Id;
        }

        public async Task<AnalysisResultEntity?> GetAnalysisByIdAsync(Guid id)
        {
            return await _context.AnalysisResults
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<AnalysisResultEntity>> GetRecentAnalysesAsync(int count)
        {
            return await _context.AnalysisResults
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
```

**Key considerations:**
- Interface for testability (can mock in tests)
- `AsNoTracking()` for read queries (performance)
- Structured logging with context
- Async all the way (non-blocking I/O)

---

#### Step 1.7: Register Repository in Program.cs
**Add after DbContext registration:**
```csharp
// Add repositories
builder.Services.AddScoped<IAnalysisRepository, AnalysisRepository>();
```

**Add using:**
```csharp
using RansomGuard.API.Services;
```

---

### **MILESTONE 2: Models & DTOs (2 hours)**

#### Step 2.1: Create Verdict Enum
**File:** `backend/RansomGuard.API/Models/Verdict.cs`

```csharp
namespace RansomGuard.API.Models
{
    public enum Verdict
    {
        Safe,
        Suspicious,
        Ransomware
    }
}
```

---

#### Step 2.2: Create AnalysisResult DTO
**File:** `backend/RansomGuard.API/Models/AnalysisResult.cs`

```csharp
namespace RansomGuard.API.Models
{
    public class AnalysisResult
    {
        public Guid UploadId { get; set; }
        public string Filename { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int RiskScore { get; set; }
        public double Entropy { get; set; }
        public List<string> SuspiciousAPIs { get; set; } = []; // C# 12 collection expression
        public Verdict Verdict { get; set; }
        public string FileHash { get; set; } = string.Empty;
    }
}
```

---

#### Step 2.3: Create UploadResponse DTO
**File:** `backend/RansomGuard.API/Models/UploadResponse.cs`

```csharp
namespace RansomGuard.API.Models
{
    public class UploadResponse
    {
        public Guid UploadId { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public Verdict Verdict { get; set; }
    }
}
```

---

#### Step 2.4: Create ErrorResponse DTO
**File:** `backend/RansomGuard.API/Models/ErrorResponse.cs`

```csharp
namespace RansomGuard.API.Models
{
    public class ErrorResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
```

---

### **MILESTONE 3: Security Layer - PE Header Validation (3 hours)**

#### Step 3.1: Extend FileValidator with PE Magic Bytes
**File:** `backend/RansomGuard.API/Validators/FileValidator.cs`

**IMPORTANT:** FileValidator is now a **static class** with all static methods.

**Add this static method to existing FileValidator class:**
```csharp
public static async Task<bool> IsValidPEHeaderAsync(Stream fileStream)
{
    if (fileStream == null || fileStream.Length < 2)
        return false;

    // Save position to restore later
    var originalPosition = fileStream.Position;
    fileStream.Position = 0;

    try
    {
        // Read first 2 bytes (MZ signature)
        var buffer = new byte[2];
        var bytesRead = await fileStream.ReadAsync(buffer);

        if (bytesRead != 2)
            return false;

        // Check for MZ magic bytes (0x4D5A)
        return buffer[0] == 0x4D && buffer[1] == 0x5A;
    }
    finally
    {
        // Restore stream position
        fileStream.Position = originalPosition;
    }
}
```

**Key considerations:**
- **Static method** - No instance required, call via `FileValidator.IsValidPEHeaderAsync()`
- Async I/O (non-blocking)
- Stream position restoration (allows reuse)
- Validates MZ signature (0x4D5A = "MZ" in ASCII)
- OWASP A03 mitigation (validates file type beyond extension)

---

#### Step 3.2: Add PE Header Tests
**File:** `backend/RansomGuard.API.Tests/Unit/FileValidatorTests.cs`

**IMPORTANT:** Since FileValidator is static, call methods directly on the class (no `_validator` instance needed).

**Add new test class region:**
```csharp
#region PE Header Validation Tests

[Fact]
public async Task IsValidPEHeader_ValidMZSignature_ReturnsTrue()
{
    // Arrange
    var stream = new MemoryStream(new byte[] { 0x4D, 0x5A, 0x00, 0x00 }); // MZ header

    // Act - Call static method directly
    var result = await FileValidator.IsValidPEHeaderAsync(stream);

    // Assert
    result.Should().BeTrue();
    stream.Position.Should().Be(0); // Stream position restored
}

[Fact]
public async Task IsValidPEHeader_InvalidSignature_ReturnsFalse()
{
    // Arrange
    var stream = new MemoryStream(new byte[] { 0x50, 0x4B, 0x03, 0x04 }); // ZIP header (PK)

    // Act - Call static method directly
    var result = await FileValidator.IsValidPEHeaderAsync(stream);

    // Assert
    result.Should().BeFalse();
}

[Fact]
public async Task IsValidPEHeader_EmptyStream_ReturnsFalse()
{
    // Arrange
    var stream = new MemoryStream();

    // Act - Call static method directly
    var result = await FileValidator.IsValidPEHeaderAsync(stream);

    // Assert
    result.Should().BeFalse();
}

[Fact]
public async Task IsValidPEHeader_NullStream_ReturnsFalse()
{
    // Act - Call static method directly
    var result = await FileValidator.IsValidPEHeaderAsync(null!);

    // Assert
    result.Should().BeFalse();
}

#endregion
```

---

#### Step 3.3: Create FileUploadHelper (GUID generation + temp storage)
**File:** `backend/RansomGuard.API/Services/FileUploadHelper.cs`

```csharp
using System.Security.Cryptography;
using System.Text;

namespace RansomGuard.API.Services
{
    public interface IFileUploadHelper
    {
        Task<(string FilePath, string Hash)> SaveUploadedFileAsync(Stream fileStream, string originalFilename);
        void DeleteFile(string filePath);
        string GetTempDirectory();
    }

    public class FileUploadHelper : IFileUploadHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileUploadHelper> _logger;
        private readonly string _tempDirectory;

        public FileUploadHelper(IConfiguration configuration, ILogger<FileUploadHelper> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _tempDirectory = _configuration["RansomGuard:TempDirectory"] ?? "./uploads/temp";

            // Create temp directory if it doesn't exist
            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
                _logger.LogInformation("Created temp directory: {Path}", _tempDirectory);
            }
        }

        public string GetTempDirectory() => _tempDirectory;

        public async Task<(string FilePath, string Hash)> SaveUploadedFileAsync(Stream fileStream, string originalFilename)
        {
            // Generate GUID filename (NEVER trust user input)
            var extension = Path.GetExtension(originalFilename);
            var guid = Guid.NewGuid();
            var safeFilename = $"{guid}{extension}";
            var filePath = Path.Combine(_tempDirectory, safeFilename);

            // Save file to disk
            using (var fileStreamDisk = File.Create(filePath))
            {
                fileStream.Position = 0; // Reset stream
                await fileStream.CopyToAsync(fileStreamDisk);
            }

            // Calculate SHA256 hash
            var hash = await CalculateSHA256Async(filePath);

            _logger.LogInformation("File saved: {Filename} -> {SafeFilename}, Hash: {Hash}",
                originalFilename, safeFilename, hash);

            return (filePath, hash);
        }

        public void DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted temp file: {Path}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {Path}", filePath);
            }
        }

        private async Task<string> CalculateSHA256Async(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexStringLower(hashBytes); // .NET 5+ optimized method
        }
    }
}
```

**Key considerations:**
- GUID filenames prevent path traversal attacks
- SHA256 for file integrity verification
- Temp directory auto-creation
- Proper resource disposal (using statements)
- Comprehensive logging for security audit trail

---

#### Step 3.4: Register FileUploadHelper in Program.cs
**Add after repository registration:**
```csharp
// Add file upload helper
builder.Services.AddScoped<IFileUploadHelper, FileUploadHelper>();
```

---

### **MILESTONE 4: File Upload Controller (4 hours)**

#### Step 4.1: Update Program.cs to Enable Controllers
**Replace the current minimal configuration with:**

```csharp
// Add services
builder.Services.AddControllers(); // Add this line
builder.Services.AddOpenApi();
```

**And in the middleware section, replace the `/health` endpoint with:**
```csharp
// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map controllers
app.MapControllers();
```

---

#### Step 4.2: Create FileUploadController
**File:** `backend/RansomGuard.API/Controllers/FileUploadController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using RansomGuard.API.Models;
using RansomGuard.API.Services;
using RansomGuard.API.Validators;

namespace RansomGuard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadHelper _fileHelper;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(
            IFileUploadHelper fileHelper,
            ILogger<FileUploadController> logger)
        {
            _fileHelper = fileHelper;
            _logger = logger;
        }

        /// <summary>
        /// Upload a PE file (.exe or .dll) for ransomware analysis
        /// </summary>
        /// <param name="file">The PE file to analyze</param>
        /// <returns>Upload result with analysis ID</returns>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            _logger.LogInformation("Upload attempt: {Filename}, Size: {Size}", file?.FileName, file?.Length);

            // Validation: File present
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ErrorResponse
                {
                    Code = "FILE_REQUIRED",
                    Message = "No file uploaded"
                });
            }

            // Validation: File size - Call static method
            if (!FileValidator.IsValidSize(file.Length))
            {
                _logger.LogWarning("File too large: {Size} bytes", file.Length);
                return BadRequest(new ErrorResponse
                {
                    Code = "FILE_TOO_LARGE",
                    Message = "File exceeds 10MB limit"
                });
            }

            // Validation: Extension - Call static method
            if (!FileValidator.IsValidExtension(file.FileName))
            {
                _logger.LogWarning("Invalid extension: {Filename}", file.FileName);
                return BadRequest(new ErrorResponse
                {
                    Code = "INVALID_FILE_TYPE",
                    Message = "Only .exe and .dll files are allowed"
                });
            }

            // Validation: Path traversal - Call static method
            if (FileValidator.ContainsPathTraversal(file.FileName))
            {
                _logger.LogWarning("Path traversal attempt: {Filename}", file.FileName);
                return BadRequest(new ErrorResponse
                {
                    Code = "INVALID_FILENAME",
                    Message = "Filename contains invalid characters"
                });
            }

            // Validation: PE header (magic bytes) - Call static method
            using var stream = file.OpenReadStream();
            if (!await FileValidator.IsValidPEHeaderAsync(stream))
            {
                _logger.LogWarning("Invalid PE header: {Filename}", file.FileName);
                return BadRequest(new ErrorResponse
                {
                    Code = "INVALID_PE_HEADER",
                    Message = "File is not a valid PE executable"
                });
            }

            // Save file with GUID filename
            var (filePath, hash) = await _fileHelper.SaveUploadedFileAsync(stream, file.FileName);

            // TODO: Call PE Analysis Service (Milestone 5)
            // TODO: Save to database (Milestone 5)
            // For now, return placeholder response

            var uploadId = Guid.NewGuid();
            _logger.LogInformation("Upload successful: {UploadId}, Hash: {Hash}", uploadId, hash);

            return Ok(new UploadResponse
            {
                UploadId = uploadId,
                Message = "File uploaded successfully. Analysis pending.",
                RiskScore = 0,
                Verdict = Verdict.Safe
            });
        }
    }
}
```

**Key security features:**
1. **Defense in depth** - Multiple validation layers
2. **Fail-fast** - Reject invalid files early
3. **Logging** - Security audit trail
4. **GUID filenames** - Prevent path traversal
5. **PE header check** - Prevent non-PE files
6. **Structured error responses** - Client-friendly

---

#### Step 4.3: FileValidator DI Registration (NOT NEEDED)
**IMPORTANT:** Since FileValidator is a **static class**, it does NOT need to be registered in DI.

**Simply call methods directly:**
```csharp
// In any controller or service
FileValidator.IsValidExtension(filename);
FileValidator.IsValidSize(fileSize);
await FileValidator.IsValidPEHeaderAsync(stream);
```

**No constructor injection needed** - static utility class pattern.

---

### **MILESTONE 5: PE Analysis Service (6-8 hours)**

This is the most complex component. Implementation details:

#### Step 5.1: Create PEAnalysisService Interface
**File:** `backend/RansomGuard.API/Services/IPEAnalysisService.cs`

```csharp
using RansomGuard.API.Models;

namespace RansomGuard.API.Services
{
    public interface IPEAnalysisService
    {
        Task<AnalysisResult> AnalyzeFileAsync(string filePath, string originalFilename, string fileHash);
    }
}
```

---

#### Step 5.2: Create PEAnalysisService Implementation
**File:** `backend/RansomGuard.API/Services/PEAnalysisService.cs`

**CRITICAL: This is the core ransomware detection logic**

```csharp
using PeNet;
using RansomGuard.API.Models;
using System.Security.Cryptography;

namespace RansomGuard.API.Services
{
    public class PEAnalysisService : IPEAnalysisService
    {
        private readonly ILogger<PEAnalysisService> _logger;

        // Suspicious Windows APIs commonly used by ransomware
        private static readonly string[] SuspiciousAPIs =
        [
            "CryptEncrypt", "CryptDecrypt", "CryptAcquireContext",
            "BCryptEncrypt", "BCryptDecrypt", "BCryptGenRandom",
            "DeleteFile", "DeleteFileW", "DeleteFileA",
            "CreateProcess", "CreateProcessW", "CreateProcessA",
            "RegSetValue", "RegSetValueEx", "RegSetValueExW",
            "WriteFile", "WriteFileEx",
            "VirtualAlloc", "VirtualAllocEx",
            "CreateRemoteThread", "OpenProcess"
        ];

        public PEAnalysisService(ILogger<PEAnalysisService> logger)
        {
            _logger = logger;
        }

        public async Task<AnalysisResult> AnalyzeFileAsync(string filePath, string originalFilename, string fileHash)
        {
            _logger.LogInformation("Starting analysis: {Filename}", originalFilename);

            var peFile = new PeFile(filePath);

            // Extract features
            var entropy = CalculateEntropy(filePath);
            var sectionCount = peFile.ImageSectionHeaders?.Length ?? 0;
            var importCount = peFile.ImportedFunctions?.Length ?? 0;
            var exportCount = peFile.ExportedFunctions?.Length ?? 0;

            // Detect suspicious APIs
            var detectedAPIs = DetectSuspiciousAPIs(peFile);

            // Calculate risk score (CLAUDE.md algorithm)
            var riskScore = CalculateRiskScore(entropy, detectedAPIs.Count, sectionCount, exportCount);

            // Determine verdict
            var verdict = DetermineVerdict(riskScore);

            _logger.LogInformation(
                "Analysis complete: {Filename}, Risk: {Risk}, Verdict: {Verdict}, Entropy: {Entropy:F2}",
                originalFilename, riskScore, verdict, entropy);

            return new AnalysisResult
            {
                UploadId = Guid.NewGuid(),
                Filename = originalFilename,
                Timestamp = DateTime.UtcNow,
                RiskScore = riskScore,
                Entropy = entropy,
                SuspiciousAPIs = detectedAPIs,
                Verdict = verdict,
                FileHash = fileHash
            };
        }

        private static double CalculateEntropy(string filePath)
        {
            var fileBytes = File.ReadAllBytes(filePath);
            var frequencies = new int[256];

            // Count byte frequencies
            foreach (var b in fileBytes)
            {
                frequencies[b]++;
            }

            // Calculate Shannon entropy using LINQ
            var fileLength = fileBytes.Length;
            return frequencies
                .Where(freq => freq > 0)
                .Select(freq => (double)freq / fileLength)
                .Sum(probability => -probability * Math.Log2(probability));
        }

        private static List<string> DetectSuspiciousAPIs(PeFile peFile)
        {
            if (peFile.ImportedFunctions == null)
            {
                return []; // C# 12 collection expression
            }

            // LINQ functional style - more concise and idiomatic
            return peFile.ImportedFunctions
                .Select(import => import.Name)
                .Where(functionName => !string.IsNullOrEmpty(functionName) &&
                    SuspiciousAPIs.Any(api => functionName.Contains(api, StringComparison.OrdinalIgnoreCase)))
                .Distinct()
                .ToList();

            return detected.Distinct().ToList();
        }

        private static int CalculateRiskScore(double entropy, int suspiciousAPICount, int sectionCount, int exportCount)
        {
            int score = 0;

            // High entropy = encrypted/packed (CLAUDE.md: +30)
            if (entropy > 7.0)
                score += 30;
            else if (entropy > 6.5)
                score += 15;

            // Many suspicious APIs (CLAUDE.md: +40)
            score += suspiciousAPICount switch
            {
                > 5 => 40,
                > 2 => 20,
                > 0 => 10,
                _ => 0
            };

            // Unusual section count (CLAUDE.md: +20)
            if (sectionCount is > 8 or < 2)
                score += 20;

            // Low export count = likely executable, not library (CLAUDE.md: +10)
            if (exportCount < 5)
                score += 10;

            return Math.Min(score, 100); // Cap at 100
        }

        private static Verdict DetermineVerdict(int riskScore) => riskScore switch
        {
            >= 70 => Verdict.Ransomware,
            >= 35 => Verdict.Suspicious,
            _ => Verdict.Safe
        };
    }
}
```

**Key algorithms:**
1. **Shannon entropy** - Detects encrypted/packed files (ransomware often encrypts its payload)
2. **API detection** - Looks for crypto, file manipulation, process injection APIs
3. **Risk scoring** - Weighted heuristic from CLAUDE.md specifications
4. **Verdict thresholds** - Safe (<35), Suspicious (35-69), Ransomware (≥70)

---

#### Step 5.3: Register PEAnalysisService in Program.cs
**Add:**
```csharp
// Add PE analysis service
builder.Services.AddScoped<IPEAnalysisService, PEAnalysisService>();
```

---

#### Step 5.4: Integrate PE Analysis into FileUploadController

**Update the `UploadFile` method in FileUploadController** (replace TODO section):

**Add to constructor:**
```csharp
private readonly IPEAnalysisService _analysisService;
private readonly IAnalysisRepository _repository;

public FileUploadController(
    FileValidator validator,
    IFileUploadHelper fileHelper,
    IPEAnalysisService analysisService,
    IAnalysisRepository repository,
    ILogger<FileUploadController> logger)
{
    _validator = validator;
    _fileHelper = fileHelper;
    _analysisService = analysisService;
    _repository = repository;
    _logger = logger;
}
```

**Replace the TODO section with:**
```csharp
// Perform PE analysis
var analysisResult = await _analysisService.AnalyzeFileAsync(filePath, file.FileName, hash);

// Save to database
var entity = new Data.Entities.AnalysisResultEntity
{
    Id = analysisResult.UploadId,
    Filename = analysisResult.Filename,
    FileHash = analysisResult.FileHash,
    FileSize = file.Length,
    Timestamp = analysisResult.Timestamp,
    RiskScore = analysisResult.RiskScore,
    Entropy = analysisResult.Entropy,
    SuspiciousAPIs = System.Text.Json.JsonSerializer.Serialize(analysisResult.SuspiciousAPIs),
    Verdict = analysisResult.Verdict.ToString(),
    SectionCount = 0, // TODO: extract from PeFile
    ImportCount = 0,
    ExportCount = 0
};

await _repository.SaveAnalysisAsync(entity);

// Delete temp file (security best practice)
_fileHelper.DeleteFile(filePath);

_logger.LogInformation("Analysis complete: {UploadId}, Verdict: {Verdict}",
    analysisResult.UploadId, analysisResult.Verdict);

return Ok(new UploadResponse
{
    UploadId = analysisResult.UploadId,
    Message = $"Analysis complete: {analysisResult.Verdict}",
    RiskScore = analysisResult.RiskScore,
    Verdict = analysisResult.Verdict
});
```

**Add using at top:**
```csharp
using System.Text.Json;
```

---

### **MILESTONE 6: Analysis Retrieval Endpoints (2 hours)**

#### Step 6.1: Create AnalysisController
**File:** `backend/RansomGuard.API/Controllers/AnalysisController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using RansomGuard.API.Models;
using RansomGuard.API.Services;
using System.Text.Json;

namespace RansomGuard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisRepository _repository;
        private readonly ILogger<AnalysisController> _logger;

        public AnalysisController(IAnalysisRepository repository, ILogger<AnalysisController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve analysis result by ID
        /// </summary>
        /// <param name="id">Analysis ID (GUID)</param>
        /// <returns>Analysis result</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AnalysisResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAnalysis(Guid id)
        {
            var entity = await _repository.GetAnalysisByIdAsync(id);

            if (entity == null)
            {
                _logger.LogWarning("Analysis not found: {Id}", id);
                return NotFound(new ErrorResponse
                {
                    Code = "NOT_FOUND",
                    Message = "Analysis result not found"
                });
            }

            var result = new AnalysisResult
            {
                UploadId = entity.Id,
                Filename = entity.Filename,
                Timestamp = entity.Timestamp,
                RiskScore = entity.RiskScore,
                Entropy = entity.Entropy,
                SuspiciousAPIs = JsonSerializer.Deserialize<List<string>>(entity.SuspiciousAPIs) ?? new(),
                Verdict = Enum.Parse<Verdict>(entity.Verdict),
                FileHash = entity.FileHash
            };

            return Ok(result);
        }

        /// <summary>
        /// Get recent analysis history
        /// </summary>
        /// <param name="count">Number of results (default 10, max 100)</param>
        /// <returns>List of recent analyses</returns>
        [HttpGet("history")]
        [ProducesResponseType(typeof(List<AnalysisResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistory([FromQuery] int count = 10)
        {
            // Cap at 100 to prevent abuse
            count = Math.Min(count, 100);

            var entities = await _repository.GetRecentAnalysesAsync(count);

            var results = entities.Select(e => new AnalysisResult
            {
                UploadId = e.Id,
                Filename = e.Filename,
                Timestamp = e.Timestamp,
                RiskScore = e.RiskScore,
                Entropy = e.Entropy,
                SuspiciousAPIs = JsonSerializer.Deserialize<List<string>>(e.SuspiciousAPIs) ?? new(),
                Verdict = Enum.Parse<Verdict>(e.Verdict),
                FileHash = e.FileHash
            }).ToList();

            return Ok(results);
        }
    }
}
```

---

### **MILESTONE 7: Testing & Validation (4 hours)**

#### Step 7.1: Integration Test for Upload Flow
**File:** `backend/RansomGuard.API.Tests/Integration/FileUploadIntegrationTests.cs`

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace RansomGuard.API.Tests.Integration
{
    public class FileUploadIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public FileUploadIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UploadFile_ValidPEFile_ReturnsSuccess()
        {
            // Arrange - Create minimal PE file (MZ header)
            var peBytes = CreateMinimalPEFile();
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(peBytes);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            content.Add(fileContent, "file", "test.exe");

            // Act
            var response = await _client.PostAsync("/api/fileupload/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UploadFile_InvalidExtension_ReturnsBadRequest()
        {
            // Arrange
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(new byte[] { 0x4D, 0x5A });
            content.Add(fileContent, "file", "test.txt");

            // Act
            var response = await _client.PostAsync("/api/fileupload/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private byte[] CreateMinimalPEFile()
        {
            // Minimal PE file with MZ header
            var bytes = new byte[512];
            bytes[0] = 0x4D; // M
            bytes[1] = 0x5A; // Z
            return bytes;
        }
    }
}
```

---

#### Step 7.2: Unit Tests for PEAnalysisService

**Create comprehensive tests for entropy calculation, API detection, and risk scoring.**

**File:** `backend/RansomGuard.API.Tests/Unit/PEAnalysisServiceTests.cs`

```csharp
// TODO: Add tests for:
// - CalculateEntropy with known entropy files
// - DetectSuspiciousAPIs with mock PeFile
// - CalculateRiskScore with various scenarios
// - DetermineVerdict boundary testing (34, 35, 69, 70)
```

---

### **MILESTONE 8: Security Hardening (2 hours)**

#### Step 8.1: Add Global Error Handling Middleware
**File:** `backend/RansomGuard.API/Middleware/ErrorHandlingMiddleware.cs`

```csharp
using RansomGuard.API.Models;
using System.Net;
using System.Text.Json;

namespace RansomGuard.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new ErrorResponse
            {
                Code = "INTERNAL_ERROR",
                Message = "An error occurred processing your request"
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}
```

**Register in Program.cs:**
```csharp
// Add error handling (BEFORE UseHttpsRedirection)
app.UseMiddleware<ErrorHandlingMiddleware>();
```

---

#### Step 8.2: Add CORS Configuration
**In Program.cs, add after `AddControllers()`:**

```csharp
// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("RansomGuard:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

**Add middleware (BEFORE UseHttpsRedirection):**
```csharp
app.UseCors();
```

---

### **MILESTONE 9: Final Testing (2 hours)**

#### Step 9.1: Run All Tests
```bash
dotnet test --logger "console;verbosity=detailed"
```

#### Step 9.2: Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

#### Step 9.3: Security Analysis
```bash
dotnet build
# Review SecurityCodeScan and SonarAnalyzer warnings
```

#### Step 9.4: Manual Testing
```bash
# Start API
dotnet run

# Test with curl
curl -X POST http://localhost:5087/api/fileupload/upload \
  -F "file=@test.exe"

# Retrieve result
curl http://localhost:5087/api/analysis/{guid}

# Get history
curl http://localhost:5087/api/analysis/history?count=5
```

---

## 🔐 SECURITY CHECKLIST (OWASP Top 10)

Before deploying:

- [x] **A01: Broken Access Control** - No auth for demo (acceptable), rate limiting recommended
- [x] **A02: Cryptographic Failures** - SHA256 file hashing, HTTPS enforced
- [x] **A03: Injection** - Path traversal prevention, extension whitelist, PE header validation
- [x] **A04: Insecure Design** - GUID filenames, isolated temp directory, file deletion
- [x] **A05: Security Misconfiguration** - Serilog, HTTPS, CORS configured, error handling
- [x] **A06: Vulnerable Components** - Latest .NET 10, dependency scanning recommended
- [x] **A07: Authentication Failures** - N/A for demo
- [x] **A08: Software Integrity** - File hashing, no code execution
- [x] **A09: Logging Failures** - Comprehensive structured logging
- [x] **A10: SSRF** - N/A (no external requests)

---

## 📊 VERIFICATION STEPS

After implementation:

1. **Build succeeds**: `dotnet build` (0 errors)
2. **All tests pass**: `dotnet test` (green)
3. **No analyzer warnings**: Check build output
4. **Code coverage >80%**: Run coverlet
5. **API runs**: `dotnet run` → Swagger at http://localhost:5087/swagger
6. **Upload works**: Test with real .exe file
7. **Database persists**: Check `ransomguard.db` has records
8. **Logs work**: Check `logs/` directory
9. **Temp files deleted**: Check `uploads/temp/` is empty after analysis
10. **Error handling**: Test with invalid inputs

---

## 🎯 CRITICAL SUCCESS FACTORS

**From .NET Expert Perspective:**
1. **Dependency Injection** - Everything registered, testable, follows SOLID
2. **Async/Await** - Non-blocking I/O throughout
3. **Error Handling** - Global middleware + endpoint-specific
4. **Logging** - Structured, contextual, actionable
5. **Testability** - Interfaces for all services, repository pattern

**From Cybersecurity Expert Perspective:**
1. **Defense in Depth** - Multiple validation layers
2. **Fail-Fast** - Reject attacks early
3. **Principle of Least Privilege** - GUID filenames, isolated directories
4. **Audit Trail** - Comprehensive logging
5. **Input Validation** - Whitelist approach (not blacklist)
6. **File Integrity** - SHA256 hashing
7. **No Execution** - Static analysis only (never run uploaded files)

---

## 📁 CRITICAL FILES SUMMARY

| File | Purpose | Lines (approx) |
|------|---------|---------------|
| `Data/RansomGuardDbContext.cs` | EF Core database context | 50 |
| `Data/Entities/AnalysisResultEntity.cs` | Database entity | 30 |
| `Models/AnalysisResult.cs` | DTO for API responses | 20 |
| `Models/UploadResponse.cs` | Upload endpoint response | 15 |
| `Models/Verdict.cs` | Enum (Safe/Suspicious/Ransomware) | 10 |
| `Services/AnalysisRepository.cs` | Database operations | 60 |
| `Services/FileUploadHelper.cs` | File storage + hashing | 80 |
| `Services/PEAnalysisService.cs` | **CORE LOGIC** - Ransomware detection | 150 |
| `Validators/FileValidator.cs` | Security validations | 50 (extended) |
| `Controllers/FileUploadController.cs` | Upload API endpoint | 120 |
| `Controllers/AnalysisController.cs` | Retrieval endpoints | 80 |
| `Middleware/ErrorHandlingMiddleware.cs` | Global error handler | 40 |
| `Program.cs` | DI registration + middleware | 80 (updated) |
| `appsettings.json` | Configuration | 20 |

**Total: ~800 lines of production code**

---

## 🚀 IMPLEMENTATION TIMELINE

**Realistic estimates for solo development:**

- Milestone 1 (Database): 4-6 hours
- Milestone 2 (Models): 2 hours
- Milestone 3 (Security): 3 hours
- Milestone 4 (Upload Controller): 4 hours
- Milestone 5 (PE Analysis): **6-8 hours** (most complex)
- Milestone 6 (Retrieval): 2 hours
- Milestone 7 (Testing): 4 hours
- Milestone 8 (Security Hardening): 2 hours
- Milestone 9 (Final Testing): 2 hours

**Total: 29-37 hours (~1 week full-time)**

---

## 💡 EXPERT TIPS

1. **Test after each milestone** - Don't wait until the end
2. **Use Swagger** - Test endpoints interactively
3. **Watch logs** - Serilog output helps debug
4. **Git commits** - Commit after each working milestone
5. **SecurityCodeScan** - Fix warnings immediately
6. **PeNet docs** - Read PeNet documentation for advanced features
7. **Sample PE files** - Use Windows system DLLs for testing (C:\Windows\System32\*.dll)

---

This plan provides complete implementation guidance while letting you write all the code yourself. Each step includes security rationale and best practices from both .NET and cybersecurity expert perspectives.
