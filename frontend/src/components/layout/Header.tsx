// src/components/layout/Header.tsx
import { Link, useLocation } from 'react-router-dom'
import { cn } from '@/utils/cn'

export function Header() {
  const location = useLocation()
  const isActive = (path: string) => location.pathname === path

  return (
    <header className="sticky top-0 z-40 w-full border-b border-terminal-light bg-terminal-dark/95 backdrop-blur">
      <div className="container flex h-16 items-center justify-between px-4">
        <Link to="/" className="flex items-center gap-2 hover-glow">
          <div className="h-8 w-8 rounded bg-terminal-green/20 flex items-center justify-center border border-terminal-green">
            <span className="text-terminal-green font-bold text-lg">R</span>
          </div>
          <span className="text-xl font-bold text-terminal-green text-glow">RansomGuard</span>
        </Link>

        <nav className="flex items-center gap-6">
          <Link
            to="/"
            className={cn(
              'text-sm font-medium transition-colors hover:text-terminal-cyan',
              isActive('/') ? 'text-terminal-green' : 'text-terminal-muted'
            )}
          >
            Upload
          </Link>
          <Link
            to="/history"
            className={cn(
              'text-sm font-medium transition-colors hover:text-terminal-cyan',
              isActive('/history') ? 'text-terminal-green' : 'text-terminal-muted'
            )}
          >
            History
          </Link>
        </nav>
      </div>
    </header>
  )
}
