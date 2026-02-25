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

---
