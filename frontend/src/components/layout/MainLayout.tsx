// src/components/layout/MainLayout.tsx
import { ReactNode } from 'react'
import { Header } from './Header'
import { Container } from './Container'

interface MainLayoutProps {
  children: ReactNode
}

export function MainLayout({ children }: MainLayoutProps) {
  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="flex-1 py-8">
        <Container>{children}</Container>
      </main>
      <footer className="border-t border-terminal-light py-6">
        <Container>
          <p className="text-center text-sm text-terminal-muted">
            RansomGuard © 2026 • Academic Research Project
          </p>
        </Container>
      </footer>
    </div>
  )
}
