# Demo Sample — RansomGuard Test File

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

TBD
