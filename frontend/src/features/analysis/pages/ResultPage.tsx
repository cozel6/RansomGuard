// src/features/analysis/pages/ResultPage.tsx
import { useParams, Link } from 'react-router-dom'
import { useAnalysisResult } from '../hooks/useAnalysisResult'
import { ResultCard } from '../components/ResultCard'
import { SuspiciousAPIList } from '../components/SuspiciousAPIList'
import { Spinner } from '@/components/ui/Spinner'
import { Button } from '@/components/ui/Button'

export function ResultPage() {
  const { uploadId } = useParams<{ uploadId: string }>()
  const { data, isLoading, error } = useAnalysisResult(uploadId!)

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[400px] gap-4">
        <Spinner size="lg" />
        <p className="text-terminal-cyan">Analyzing file...</p>
      </div>
    )
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[400px] gap-4 text-center">
        <h2 className="text-2xl font-bold text-terminal-red mb-4">Analysis Failed</h2>
        <p className="text-terminal-muted mb-6">{(error as Error).message}</p>
        <Button asChild><Link to="/">Upload Another File</Link></Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-terminal-green text-glow terminal-prompt">ANALYSIS COMPLETE</h1>
        <Button variant="outline" asChild>
          <Link to="/">Analyze Another File</Link>
        </Button>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <ResultCard result={data!} />
        <SuspiciousAPIList apis={data!.suspiciousAPIs} />
      </div>
    </div>
  )
}
