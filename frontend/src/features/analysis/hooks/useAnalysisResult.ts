// src/features/analysis/hooks/useAnalysisResult.ts
import { useQuery } from '@tanstack/react-query'
import { getAnalysisResult } from '@/services/api/analysis.api'

export function useAnalysisResult(uploadId: string) {
  return useQuery({
    queryKey: ['analysis', uploadId],
    queryFn: () => getAnalysisResult(uploadId),
    retry: 3,
    staleTime: 5 * 60 * 1000,
  })
}
