// src/features/history/components/HistoryTable.tsx
import { Link } from 'react-router-dom'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { VerdictBadge } from '@/features/analysis/components/VerdictBadge'
import { formatTimestamp, getRiskScoreColor } from '@/utils/formatting'
import { AnalysisHistoryItem } from '@/types/api.types'
import { cn } from '@/utils/cn'

interface HistoryTableProps {
  data: AnalysisHistoryItem[]
}

export function HistoryTable({ data }: HistoryTableProps) {
  if (data.length === 0) {
    return (
      <Card>
        <CardContent className="p-12 text-center">
          <p className="text-terminal-muted">No analysis history found</p>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader><CardTitle>RESULTS ({data.length})</CardTitle></CardHeader>
      <CardContent>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-terminal-light">
                <th className="text-left p-4 text-terminal-muted font-medium">Filename</th>
                <th className="text-left p-4 text-terminal-muted font-medium">Verdict</th>
                <th className="text-left p-4 text-terminal-muted font-medium">Risk Score</th>
                <th className="text-left p-4 text-terminal-muted font-medium">Analyzed</th>
                <th className="text-left p-4 text-terminal-muted font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.map((item) => (
                <tr key={item.uploadId} className="border-b border-terminal-light/50 hover:bg-terminal-gray/30 transition-colors">
                  <td className="p-4"><code className="text-terminal-cyan">{item.filename}</code></td>
                  <td className="p-4"><VerdictBadge verdict={item.verdict} /></td>
                  <td className="p-4"><span className={cn('font-bold', getRiskScoreColor(item.riskScore))}>{item.riskScore}</span></td>
                  <td className="p-4 text-terminal-muted text-sm">{formatTimestamp(item.timestamp)}</td>
                  <td className="p-4">
                    <Button variant="outline" size="sm" asChild>
                      <Link to={`/result/${item.uploadId}`}>View Details</Link>
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </CardContent>
    </Card>
  )
}
