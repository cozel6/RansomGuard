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

export function getRiskScoreColor(verdict: Verdict): string {
  switch (verdict) {
    case Verdict.Safe: return 'text-terminal-green'
    case Verdict.Suspicious: return 'text-terminal-yellow'
    case Verdict.Ransomware: return 'text-terminal-red'
    default: return 'text-terminal-muted'
  }
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
