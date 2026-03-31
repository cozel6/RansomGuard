// src/features/analysis/components/RiskScoreMeter.tsx
import { getRiskScoreColor } from '@/utils/formatting'
import { cn } from '@/utils/cn'

interface RiskScoreMeterProps {
  score: number
}

export function RiskScoreMeter({ score }: RiskScoreMeterProps) {
  const circumference = 2 * Math.PI * 45
  const offset = circumference - (score / 100) * circumference

  return (
    <div className="flex flex-col items-center gap-4">
      <div className="relative h-32 w-32">
        <svg className="transform -rotate-90 h-32 w-32">
          <circle cx="64" cy="64" r="45" stroke="currentColor" strokeWidth="8" fill="transparent" className="text-terminal-gray" />
          <circle
            cx="64" cy="64" r="45" stroke="currentColor" strokeWidth="8" fill="transparent"
            strokeDasharray={circumference}
            strokeDashoffset={offset}
            className={cn('transition-all duration-1000 ease-out', getRiskScoreColor(score))}
          />
        </svg>
        <div className="absolute inset-0 flex flex-col items-center justify-center">
          <span className={cn('text-3xl font-bold', getRiskScoreColor(score))}>{score}</span>
          <span className="text-xs text-terminal-muted">RISK</span>
        </div>
      </div>
      <p className="text-sm text-terminal-muted">
        {score < 30 && 'Low Risk'}
        {score >= 30 && score < 70 && 'Medium Risk'}
        {score >= 70 && 'High Risk'}
      </p>
    </div>
  )
}
