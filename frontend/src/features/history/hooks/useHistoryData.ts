// src/features/history/hooks/useHistoryData.ts
import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getAnalysisHistory } from '@/services/api/analysis.api'
import { Verdict } from '@/types/api.types'

export function useHistoryData() {
  const [verdictFilter, setVerdictFilter] = useState<Verdict | null>(null)
  const [count, setCount] = useState(10)

  const query = useQuery({
    queryKey: ['history', { count, verdictFilter }],
    queryFn: () => getAnalysisHistory({ count, verdictFilter }),
    staleTime: 1 * 60 * 1000,
  })

  return { ...query, verdictFilter, setVerdictFilter, count, setCount }
}
