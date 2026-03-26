// src/utils/formatting.ts
import { format } from 'date-fns'
import { Verdict } from '@/types/api.types'

export function formatTimestamp(timestamp: string): string {
  return format(new Date(timestamp), 'MMM d, yyyy HH:mm:ss')
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

export function getRiskScoreColor(score: number): string {
  if (score < 30) return 'text-terminal-green'
  if (score < 70) return 'text-terminal-yellow'
  return 'text-terminal-red'
}

export function getVerdictLabel(verdict: Verdict): string {
  switch (verdict) {
    case Verdict.Safe: return 'SAFE'
    case Verdict.Suspicious: return 'SUSPICIOUS'
    case Verdict.Ransomware: return 'RANSOMWARE'
    default: return 'UNKNOWN'
  }
}

export function getVerdictVariant(verdict: Verdict): 'safe' | 'suspicious' | 'ransomware' | 'secondary' {
  switch (verdict) {
    case Verdict.Safe: return 'safe'
    case Verdict.Suspicious: return 'suspicious'
    case Verdict.Ransomware: return 'ransomware'
    default: return 'secondary'
  }
}
