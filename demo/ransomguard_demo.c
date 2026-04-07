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
    ULONG dwFlags)

    /* --- File operations (deletion / overwrite indicators) --- */
    /*
     * DeleteFileW  deletes a file from the filesystem (Unicode version).
     *   lpFileName : full path to the file to delete (wide-char string)
     * Ransomware deletes original plaintext files after encrypting them,
     * or deletes shadow copies / backups to prevent recovery.
     */
