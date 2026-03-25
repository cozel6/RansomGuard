function App() {
  return (
    <div className="min-h-screen bg-terminal-black flex items-center justify-center">
      <div className="terminal-card p-8 max-w-md w-full text-center">
        <h1 className="text-2xl font-bold text-terminal-green text-glow mb-2">
          RansomGuard
        </h1>
        <p className="text-terminal-muted text-sm mb-6">
          Sistema de detecție ransomware
        </p>
        <div className="flex gap-3 justify-center">
          <button className="btn-terminal">Upload fișier</button>
          <button className="btn-terminal-outline">Istoric</button>
        </div>
      </div>
    </div>
  )
}

export default App
