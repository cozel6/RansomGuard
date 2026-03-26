// src/features/history/pages/HistoryPage.tsx
import { useHistoryData } from '../hooks/useHistoryData'
import { HistoryTable } from '../components/HistoryTable'
import { FilterBar } from '../components/FilterBar'
import { Spinner } from '@/components/ui/Spinner'

export function HistoryPage() {
  const { data, isLoading, error, verdictFilter, setVerdictFilter } = useHistoryData()

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-terminal-green text-glow terminal-prompt">ANALYSIS HISTORY</h1>
        <FilterBar activeFilter={verdictFilter} onFilterChange={setVerdictFilter} />
      </div>

      {isLoading && (
        <div className="flex justify-center py-12"><Spinner size="lg" /></div>
      )}

      {error && (
        <div className="p-4 rounded-md bg-terminal-red/10 border border-terminal-red">
          <p className="text-terminal-red">{(error as Error).message}</p>
        </div>
      )}

      {data && <HistoryTable data={data} />}
    </div>
  )
}
