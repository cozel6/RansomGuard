# Demo Sample  RansomGuard Test File

## Overview

To validate that the RansomGuard detection pipeline works correctly, the project
includes a safe demo DLL that mimics the static characteristics of ransomware
without performing any malicious actions.

This file is intended exclusively for testing and demonstration purposes.

---

## What is a Demo Sample

A demo sample is a compiled binary that exhibits the same **static features**
that a Machine Learning model looks for in real ransomware, but contains
no destructive logic whatsoever.

This concept is well established in the cybersecurity industry. The most
well-known example is the **EICAR Test File**, a standard dummy file recognized
by every antivirus engine as a test case without being harmful.

RansomGuard's demo sample follows the same principle, adapted for PE-based
static analysis.

| Characteristic              | Description                                 | Present |
| --------------------------- | ------------------------------------------- | ------- |
| High Shannon entropy        | Simulated encrypted data section            | Yes     |
| Cryptographic API imports   | CryptEncrypt, BCryptEncrypt, CryptGenRandom | Yes     |
| Suspicious PE section names | Non-standard section naming                 | Yes     |
| Low export count            | Typical of packed/obfuscated binaries       | Yes     |
| No debug information        | Stripped debug symbols                      | Yes     |

---

## What the Demo DLL Does NOT Do

This file contains absolutely none of the following:

- No file encryption or modification
- No deletion of shadow copies or backups
- No network connections or C2 communication
- No registry modifications
- No process injection or privilege escalation
- No self-replication or propagation

The cryptographic API functions are imported but never called.
The binary compiles, loads, and exits immediately without side effects.

## Source Code

Both source files are in the `demo/` directory. Compiled binaries are also checked in so
the service can be tested without a C toolchain.

| File | Compiled binary | Expected verdict |
|------|----------------|-----------------|
| `ransomguard_demo.c` | `ransomguard_demo.dll` | Suspicious (~0.62 confidence) |
| `ransomguard_demo_high_risk.c` | `ransomguard_demo_high_risk.dll` | Ransomware (~0.80+ confidence) |

### ransomguard_demo.c

Standard suspicious sample: imports cryptographic and process APIs but never calls them.
Entropy pad uses pseudo-random bytes to raise Shannon entropy into the suspicious range.

```c
#include <windows.h>
#include <wincrypt.h>   /* CryptEncrypt, CryptGenRandom            */
#include <bcrypt.h>     /* BCryptEncrypt, BCryptGenRandom           */

/* Declared but NEVER called — linker records them in the PE import table */
extern BOOL    WINAPI CryptEncrypt(HCRYPTKEY, HCRYPTHASH, BOOL, DWORD, BYTE*, DWORD*, DWORD);
extern BOOL    WINAPI CryptGenRandom(HCRYPTPROV, DWORD, BYTE*);
extern NTSTATUS WINAPI BCryptEncrypt(BCRYPT_KEY_HANDLE, PUCHAR, ULONG, VOID*, PUCHAR, ULONG, PUCHAR, ULONG, ULONG*, ULONG);
extern NTSTATUS WINAPI BCryptGenRandom(BCRYPT_ALG_HANDLE, PUCHAR, ULONG, ULONG);
extern BOOL    WINAPI DeleteFileW(LPCWSTR);
extern BOOL    WINAPI MoveFileExW(LPCWSTR, LPCWSTR, DWORD);
extern LPVOID  WINAPI VirtualAlloc(LPVOID, SIZE_T, DWORD, DWORD);
extern HANDLE  WINAPI CreateRemoteThread(HANDLE, LPSECURITY_ATTRIBUTES, SIZE_T, LPTHREAD_START_ROUTINE, LPVOID, DWORD, LPDWORD);
extern HANDLE  WINAPI OpenProcess(DWORD, BOOL, DWORD);
extern LONG    WINAPI RegSetValueExW(HKEY, LPCWSTR, DWORD, DWORD, const BYTE*, DWORD);
extern BOOL    WINAPI CreateProcessW(LPCWSTR, LPWSTR, LPSECURITY_ATTRIBUTES, LPSECURITY_ATTRIBUTES, BOOL, DWORD, LPVOID, LPCWSTR, LPSTARTUPINFOW, LPPROCESS_INFORMATION);

/* 4 KB pseudo-random pad — raises Shannon entropy into suspicious range */
__attribute__((section(".rdata"))) static const unsigned char _entropy_pad[4096] = {
    0xe3, 0x7f, 0x2a, 0xb1, 0x9c, 0x54, 0xd8, 0x03, /* ... 4096 bytes total ... */
    [4095] = 0xff
};

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
    (void)hinstDLL; (void)lpvReserved;
    return TRUE;  /* nothing happens at runtime */
}
```

Compile with mingw-w64:

```bash
x86_64-w64-mingw32-gcc -shared -o demo/ransomguard_demo.dll demo/ransomguard_demo.c \
    -ladvapi32 -lkernel32 -lbcrypt -Wl,--subsystem,windows
```

---

### ransomguard_demo_high_risk.c

High-risk sample: uses `__attribute__((used))` to force all 11 suspicious APIs into the
import table (not just declared), and uses a perfectly uniform entropy pad
(0x00–0xFF repeated × 16) that gives Shannon entropy = 8.0 bits.

Expected heuristic score breakdown:
```
+40  suspicious API count > 5  (11 APIs)
+30  entropy > 7.0             (8.0 bits)
+10  export count < 5
─────────────────────────────
= 80  → Verdict: Ransomware
```

```c
#include <windows.h>
#include <wincrypt.h>
#include <bcrypt.h>

/* Force all 11 APIs into import table via a used function-pointer array */
static void * const _api_refs[] __attribute__((used, section(".rdata"))) = {
    (void *)CryptEncrypt,   (void *)CryptGenRandom,
    (void *)BCryptEncrypt,  (void *)BCryptGenRandom,
    (void *)DeleteFileW,    (void *)MoveFileExW,
    (void *)VirtualAlloc,   (void *)CreateRemoteThread,
    (void *)OpenProcess,    (void *)RegSetValueExW,
    (void *)CreateProcessW,
};

/* Uniform 0x00..0xFF × 16 = 4096 bytes, Shannon entropy = 8.0 bits */
__attribute__((section(".rdata"))) static const unsigned char _entropy_pad[4096] = {
    0x00, 0x01, 0x02, /* ... 0x00–0xFF repeated 16 times ... */ 0xfe, 0xff,
};

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
    (void)hinstDLL; (void)lpvReserved; (void)_entropy_pad; (void)_api_refs;
    return TRUE;
}
```

Compile with mingw-w64:

```bash
x86_64-w64-mingw32-gcc -shared -o demo/ransomguard_demo_high_risk.dll \
    demo/ransomguard_demo_high_risk.c -ladvapi32 -lkernel32 -lbcrypt
```

---

## How to Test

```bash
# Start ML service
cd ml-service && uvicorn app.main:app --reload --port 8000

# Test suspicious sample (~0.62 confidence)
curl -X POST http://localhost:8000/predict -F "file=@demo/ransomguard_demo.dll"

# Test high-risk sample (ransomware verdict)
curl -X POST http://localhost:8000/predict -F "file=@demo/ransomguard_demo_high_risk.dll"
```
