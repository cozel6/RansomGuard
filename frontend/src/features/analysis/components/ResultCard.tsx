// src/features/analysis/components/ResultCard.tsx
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { AnalysisResult } from '@/types/api.types'
import { RiskScoreMeter } from './RiskScoreMeter'
import { VerdictBadge } from './VerdictBadge'
import { formatTimestamp } from '@/utils/formatting'

interface ResultCardProps {
  result: AnalysisResult
}

export function ResultCard({ result }: ResultCardProps) {
  return (
    <Card>
      <CardHeader><CardTitle>Analysis Result</CardTitle></CardHeader>
      <CardContent className="space-y-6">
        <div className="flex items-center justify-between">
          <span className="text-terminal-muted">Verdict</span>
          <VerdictBadge verdict={result.verdict} />
        </div>

        <div className="flex justify-center py-4">
          <RiskScoreMeter score={result.riskScore} />
        </div>

        <div className="space-y-3 border-t border-terminal-light pt-4">
          <div className="flex justify-between">
            <span className="text-terminal-muted">Filename</span>
            <code className="text-terminal-cyan">{result.filename}</code>
          </div>
          <div className="flex justify-between">
            <span className="text-terminal-muted">Entropy</span>
            <span className="text-terminal-green">{result.entropy.toFixed(2)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-terminal-muted">File Hash (SHA256)</span>
            <code className="text-terminal-cyan text-xs truncate max-w-xs" title={result.fileHash}>
              {result.fileHash.substring(0, 16)}...
            </code>
          </div>
          <div className="flex justify-between">
            <span className="text-terminal-muted">Analyzed</span>
            <span className="text-terminal-green">{formatTimestamp(result.timestamp)}</span>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
