// src/types/api.types.ts

export enum Verdict {
  Unknown = 0,
  Safe = 1,
  Suspicious = 2,
  Ransomware = 3,
}

export interface UploadResponse {
  uploadId: string
  message: string
}

export interface AnalysisResult {
  uploadId: string
  filename: string
  timestamp: string
  riskScore: number
  entropy: number
  suspiciousAPIs: string[]
  verdict: Verdict
  fileHash: string
}

export interface ErrorResponse {
  error: {
    code: string
    message: string
    timestamp: string
  }
}

export interface HistoryParams {
  count?: number
  verdictFilter?: Verdict | null
}

export type AnalysisHistoryItem = Pick<
  AnalysisResult,
  'uploadId' | 'filename' | 'verdict' | 'timestamp' | 'riskScore'
>
