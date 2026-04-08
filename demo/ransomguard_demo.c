/*
 * ransomguard_demo.c
 *
 * RansomGuard Demo DLL  safe test artifact
 *
 * Purpose : exhibit the static PE characteristics that the EMBER2024 model
 *           uses as malware signals, without performing any harmful action.
 *
 * What this file does at runtime : loads and immediately returns DLL_PROCESS_ATTACH.
 * Nothing else happens.
 *
 * Compile (MinGW on Windows or cross-compile with mingw-w64 on macOS/Linux):
 *   x86_64-w64-mingw32-gcc -shared -o ransomguard_demo.dll ransomguard_demo.c \
 *       -ladvapi32 -lkernel32
 */

#include <windows.h>
#include <wincrypt.h> /* CryptEncrypt, CryptGenRandom                    */
#include <bcrypt.h>   /* BCryptEncrypt, BCryptGenRandom                  */
                      /* -----------------------------------------------------------------------
                       * Import declarations
                       *
                       * Every function below is declared but NEVER called.
                       * The linker will still record them in the PE import table, which is what
                       * static-analysis tools (and EMBER) inspect.
                       * ----------------------------------------------------------------------- */

/* --- Cryptography (high-signal ransomware indicators) --- */
/*
 * CryptEncrypt  encrypts data using a symmetric key (legacy WinCrypt API).
 *   hKey       : handle to the encryption key (obtained via CryptGenKey)
 *   hHash      : optional handle to a hash object (for combined hash+encrypt); NULL if unused
 *   Final      : TRUE if this is the last block of data to encrypt
 *   dwFlags    : modifier flags (usually 0)
 *   pbData     : pointer to the buffer containing plaintext; encrypted data is written back here
 *   pdwDataLen : on input  length of plaintext; on output  length of ciphertext
 *   dwBufLen   : total size of the pbData buffer (must be large enough for the ciphertext)
 * Ransomware uses this to encrypt victim files in-place.
 */
extern BOOL WINAPI CryptEncrypt(
    HCRYPTKEY hKey, HCRYPTHASH hHash, BOOL Final, DWORD dwFlags,
    BYTE *pbData, DWORD *pdwDataLen, DWORD dwBufLen);

/*
 * CryptGenRandom  fills a buffer with cryptographically random bytes (legacy WinCrypt API).
 *   hProv   : handle to a CSP (Cryptographic Service Provider), opened via CryptAcquireContext
 *   dwLen   : number of random bytes to generate
 *   pbBuffer: pointer to the output buffer that receives the random bytes
 * Ransomware uses this to generate random encryption keys that the victim cannot predict.
 */
extern BOOL WINAPI CryptGenRandom(
    HCRYPTPROV hProv, DWORD dwLen, BYTE *pbBuffer);

/*
 * BCryptEncrypt  encrypts data using a symmetric key (modern CNG API).
 *   hKey        : handle to the key object (created via BCryptGenerateSymmetricKey)
 *   pbInput     : pointer to the plaintext buffer
 *   cbInput     : size of the plaintext in bytes
 *   pPaddingInfo: pointer to padding scheme info (e.g. BCRYPT_PKCS1_PADDING_INFO for RSA);
 *                 NULL for block ciphers like AES-CBC
 *   pbIV        : pointer to the initialisation vector buffer; NULL if not applicable
 *   cbIV        : size of the IV in bytes
 *   pbOutput    : pointer to the output buffer that receives the ciphertext
 *   cbOutput    : size of the output buffer
 *   pcbResult   : receives the number of bytes written to pbOutput
 *   dwFlags     : e.g. BCRYPT_BLOCK_PADDING to pad the last block automatically
 * Preferred by modern ransomware over the legacy CryptEncrypt.
 */
extern NTSTATUS WINAPI BCryptEncrypt(
    BCRYPT_KEY_HANDLE hKey, PUCHAR pbInput, ULONG cbInput,
    VOID *pPaddingInfo, PUCHAR pbIV, ULONG cbIV,
    PUCHAR pbOutput, ULONG cbOutput, ULONG *pcbResult, ULONG dwFlags);
/*
 * BCryptGenRandom  fills a buffer with cryptographically random bytes (modern CNG API).
 *   hAlgorithm : handle to a CNG algorithm provider opened with BCRYPT_RNG_ALGORITHM;
 *                can be NULL when BCRYPT_USE_SYSTEM_PREFERRED_RNG flag is set
 *   pbBuffer   : pointer to the output buffer that receives the random bytes
 *   cbBuffer   : number of random bytes to generate
 *   dwFlags    : e.g. BCRYPT_USE_SYSTEM_PREFERRED_RNG to use the OS default RNG
 * Ransomware uses this to generate unpredictable AES session keys.
 */
extern NTSTATUS WINAPI BCryptGenRandom(
    BCRYPT_ALG_HANDLE hAlgoritm,
    PUCHAR pbBuffer,
    ULONG cbBuffer,
    ULONG dwFlags);

/*
 * DeleteFileW  deletes a file from the filesystem (Unicode version).
 * lpFileName : full path to the file to delete (wide-char string)
 * Ransomware deletes original plaintext files after encrypting them,
 * or deletes shadow copies / backups to prevent recovery.
 */

extern BOOL WINAPI DeleteFileW(LPCWSTR lpFileName);
/*
 * MoveFileExW  moves or renames a file, with optional flags (Unicode version).
 *   lpExistingFileName : path to the source file
 *   lpNewFileName      : path to the destination (NULL to delete the file)
 *   dwFlags            : e.g. MOVEFILE_REPLACE_EXISTING, MOVEFILE_DELAY_UNTIL_REBOOT
 * Ransomware uses this to rename encrypted files (e.g. "document.docx" → "document.docx.locked").
 */
extern BOOL WINAPI MoveFileExW(LPCWSTR lpExistingFileName, LPCWSTR lpNewFileName, DWORD dwFlags);

/* --- Process / memory (injection indicators) --- */
/*
 * VirtualAlloc  reserves or commits a region of memory in the calling process's address space.
 *   lpAddress       : desired starting address (NULL lets the OS choose)
 *   dwSize          : size of the region in bytes
 *   flAllocationType: MEM_COMMIT | MEM_RESERVE  reserve and commit pages
 *   flProtect       : e.g. PAGE_EXECUTE_READWRITE  makes the region executable
 * Ransomware allocates executable memory to unpack or inject shellcode at runtime.
 */
extern LPVOID WINAPI VirtualAlloc(LPVOID lpAddress, SIZE_T dwSize, DWORD FileAllocationType, DWORD flProtect);

/*
 * CreateRemoteThread  creates a thread in the address space of another process.
 *   hProcess              : handle to the target process (from OpenProcess)
 *   lpThreadAttributes    : security attributes for the new thread (NULL = default)
 *   dwStackSize           : initial stack size; 0 = use default
 *   lpStartAddress        : pointer to the function the thread will execute in the remote process
 *   lpParameter           : argument passed to that function
 *   dwCreationFlags       : 0 = start immediately; CREATE_SUSPENDED = start paused
 *   lpThreadId            : receives the thread ID of the new thread
 * Classic process-injection technique: writes shellcode into a remote process then
 * creates a thread there to execute it.
 */
extern HANDLE WINAPI CreateRemoteThread(
    HANDLE hProcess, LPSECURITY_ATTRIBUTES lpThreadAttributes,
    SIZE_T dwStackSize, LPTHREAD_START_ROUTINE lpStartAddress,
    LPVOID lpParameter, DWORD dwCreationFlags, LPDWORD lpThreadId);

/*
 * OpenProcess  opens an existing process and returns a handle to it.
 *   dwDesiredAccess : requested access rights, e.g. PROCESS_ALL_ACCESS
 *   bInheritHandle  : TRUE if child processes should inherit this handle
 *   dwProcessId     : PID of the target process
 * Required before VirtualAllocEx / WriteProcessMemory / CreateRemoteThread
 * can be used to inject into another process.
 */
extern HANDLE WINAPI OpenProcess(
    DWORD dwDesiredAccess, BOOL bInheritHandle, DWORD dwProcessId);

/* --- Registry (persistence indicators) --- */

/*
 * RegSetValueExW  writes data to a registry value (Unicode version).
 *   hKey        : handle to an open registry key (e.g. HKEY_CURRENT_USER\...\Run)
 *   lpValueName : name of the value to set
 *   Reserved    : must be 0
 *   dwType      : data type, e.g. REG_SZ (string) or REG_BINARY
 *   lpData      : pointer to the data to store
 *   cbData      : size of lpData in bytes
 * Ransomware writes its own executable path to the Run key so it survives reboots
 * and can re-encrypt files that were created after the initial infection.
 */
extern LONG WINAPI RegSetValueExW(
    HKEY hKey, LPCWSTR lpValueName, DWORD Reserved,
    DWORD dwType, const BYTE *lpData, DWORD cbData);

/* --- Process creation --- */

/*
 * CreateProcessW  creates a new process and its primary thread (Unicode version).
 *   lpApplicationName    : path to the executable (can be NULL if encoded in lpCommandLine)
 *   lpCommandLine        : command line string (e.g. L"cmd.exe /c vssadmin delete shadows /all")
 *   lpProcessAttributes  : security attributes for the process object (NULL = default)
 *   lpThreadAttributes   : security attributes for the main thread (NULL = default)
 *   bInheritHandles      : TRUE = child inherits open handles from parent
 *   dwCreationFlags      : e.g. CREATE_NO_WINDOW to run silently
 *   lpEnvironment        : environment block for the new process (NULL = inherit parent's)
 *   lpCurrentDirectory   : working directory for the new process (NULL = inherit parent's)
 *   lpStartupInfo        : startup configuration (window size, stdio handles, etc.)
 *   lpProcessInformation : receives PID, TID, and handles for the new process/thread
 * Ransomware uses this to silently run commands such as deleting Volume Shadow Copies
 * (vssadmin) or disabling Windows Backup, making recovery impossible.
 */

extern BOOL WINAPI CreateProcessW(
    LPCWSTR lpApplicationName, LPWSTR lpCommandLine,
    LPSECURITY_ATTRIBUTES lpProcessAttributes,
    LPSECURITY_ATTRIBUTES lpThreadAttributes,
    BOOL bInheritHandles, DWORD dwCreationFlags,
    LPVOID lpEnvironment, LPCWSTR lpCurrentDirectory,
    LPSTARTUPINFOW lpStartupInfo,
    LPPROCESS_INFORMATION lpProcessInformation);
/* -----------------------------------------------------------------------
 * High-entropy data section
 *
 * A 4 KB block of pseudo-random bytes raises the Shannon entropy of the
 * binary  another feature EMBER scores highly for ransomware.
 * The values are hard-coded constants; nothing is generated at runtime.
 * ----------------------------------------------------------------------- */
__attribute__((section(".rdata"))) static const unsigned char _entropy_pad[4096] = {
    /* 4096 bytes generated offline with: python3 -c "
     *   import os, textwrap
     *   data = os.urandom(4096)
     *   print(', '.join(f'0x{b:02x}' for b in data))
     * "
     * Paste the output here. A representative sample is shown below.
     */
    0xe3, 0x7f, 0x2a, 0xb1, 0x9c, 0x54, 0xd8, 0x03, 0xf6, 0x41, 0xaa, 0x27, 0x6e, 0xbc, 0x5d, 0x90,
    0x1a, 0x73, 0xce, 0x88, 0x3b, 0xf9, 0x07, 0x62, 0x4f, 0xd5, 0x19, 0x8e, 0xa0, 0x35, 0x7c, 0xeb,
    0x56, 0x0d, 0x94, 0xc2, 0x6b, 0xfe, 0x21, 0x47, 0x83, 0xb0, 0xda, 0x1f, 0x70, 0xac, 0x39, 0x65,
    /* ... repeat to fill 4096 bytes ... */
    [4095] = 0xff /* C99 designated initialiser  remaining bytes are 0x00 */
};

// DLL entry point  the only code that actually runs
BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
    (void)hinstDLL;
    (void)lpvReserved;

    switch (fdwReason)
    {
    case DLL_PROCESS_ATTACH:
        /* Intentionally empty  no harmful action */
        break;
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}