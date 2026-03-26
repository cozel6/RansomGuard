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
      <CardHeader>
        <CardTitle>ANALYSIS RESULT</CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="flex items-center justify-between">
          <span className="text-terminal-muted terminal-label">verdict:</span>
          <VerdictBadge verdict={result.verdict} />
        </div>

        <div className="flex justify-center py-4">
          <RiskScoreMeter score={result.riskScore} />
        </div>

        <div className="space-y-3 border-t border-terminal-light pt-4">
          <div className="flex justify-between items-center">
            <span className="text-terminal-muted terminal-label">filename:</span>
            <code className="text-terminal-cyan">{result.filename}</code>
          </div>
          <div className="flex justify-between items-center">
            <span className="text-terminal-muted terminal-label">entropy:</span>
            <span className="text-terminal-green">{result.entropy.toFixed(2)}</span>
          </div>
          <div className="flex justify-between items-center">
            <span className="text-terminal-muted terminal-label">sha256:</span>
            <code className="text-terminal-cyan text-xs" title={result.fileHash}>
              {result.fileHash.substring(0, 14)}...
            </code>
          </div>
          <div className="flex justify-between items-center">
            <span className="text-terminal-muted terminal-label">analyzed:</span>
            <span className="text-terminal-green">{formatTimestamp(result.timestamp)}</span>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
