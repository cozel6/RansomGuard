// src/features/history/components/FilterBar.tsx
import { Button } from '@/components/ui/Button'
import { Verdict } from '@/types/api.types'

interface FilterBarProps {
  activeFilter: Verdict | null
  onFilterChange: (filter: Verdict | null) => void
}

const filters: { label: string; value: Verdict | null }[] = [
  { label: 'All', value: null },
  { label: 'Safe', value: Verdict.Safe },
  { label: 'Suspicious', value: Verdict.Suspicious },
  { label: 'Ransomware', value: Verdict.Ransomware },
]

export function FilterBar({ activeFilter, onFilterChange }: FilterBarProps) {
  return (
    <div className="flex gap-2">
      {filters.map((filter) => (
        <Button
          key={filter.label}
          variant={activeFilter === filter.value ? 'default' : 'outline'}
          size="sm"
          onClick={() => onFilterChange(filter.value)}
        >
          {filter.label}
        </Button>
      ))}
    </div>
  )
}
