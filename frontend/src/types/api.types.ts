// src/types/api.types.ts

export enum Verdict {
  Safe = 'Safe',
  Suspicious = 'Suspicious',
  Ransomware = 'Ransomware',
}

export interface UploadResponse {
  uploadId: string
  message: string
  riskScore: number
  verdict: Verdict
  mlConfidence?: number
  mlModelVersion?: string
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
  mlConfidence?: number
  mlModelVersion?: string
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
