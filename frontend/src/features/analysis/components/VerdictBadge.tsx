// src/features/analysis/components/VerdictBadge.tsx
import { Badge } from '@/components/ui/Badge'
import { Verdict } from '@/types/api.types'
import { getVerdictLabel, getVerdictVariant } from '@/utils/formatting'

interface VerdictBadgeProps {
  verdict: Verdict
}

export function VerdictBadge({ verdict }: VerdictBadgeProps) {
  return (
    <Badge variant={getVerdictVariant(verdict)} className="text-base px-4 py-1">
      {getVerdictLabel(verdict)}
    </Badge>
  )
}
