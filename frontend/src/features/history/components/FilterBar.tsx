// src/features/history/components/FilterBar.tsx
import { cn } from '@/utils/cn'
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
        <button
          key={filter.label}
          onClick={() => onFilterChange(filter.value)}
          className={cn(
            'btn-terminal-outline text-sm font-medium px-3 py-1.5 rounded cursor-pointer',
            activeFilter === filter.value && 'active'
          )}
        >
          {filter.label}
        </button>
      ))}
    </div>
  )
}
