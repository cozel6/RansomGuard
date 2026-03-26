// src/features/analysis/components/SuspiciousAPIList.tsx
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'

interface SuspiciousAPIListProps {
  apis: string[]
}

const API_DESCRIPTIONS: Record<string, string> = {
  CryptEncrypt: 'File encryption capability',
  DeleteFile: 'File deletion capability',
  CreateProcess: 'Process creation/injection',
  RegSetValueEx: 'Registry modification',
  WriteFile: 'File system write access',
  InternetOpen: 'Network communication',
  ShellExecute: 'Command execution',
}

export function SuspiciousAPIList({ apis }: SuspiciousAPIListProps) {
  if (apis.length === 0) {
    return (
      <Card>
        <CardHeader><CardTitle>Suspicious API Calls</CardTitle></CardHeader>
        <CardContent>
          <p className="text-terminal-muted">No suspicious API calls detected</p>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader><CardTitle>Suspicious API Calls ({apis.length})</CardTitle></CardHeader>
      <CardContent>
        <div className="space-y-2">
          {apis.map((api) => (
            <div key={api} className="flex items-center justify-between p-3 rounded-md bg-terminal-gray/50 border border-terminal-light">
              <code className="text-terminal-cyan font-mono">{api}</code>
              <span className="text-xs text-terminal-muted">{API_DESCRIPTIONS[api] || 'Unknown API'}</span>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  )
}
