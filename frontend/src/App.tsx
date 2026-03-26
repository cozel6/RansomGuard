// src/App.tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { QueryProvider } from './app/providers/QueryProvider'
import { MainLayout } from './components/layout/MainLayout'
import { UploadPage } from './features/upload/pages/UploadPage'
import { ResultPage } from './features/analysis/pages/ResultPage'
import { HistoryPage } from './features/history/pages/HistoryPage'

function App() {
  return (
    <QueryProvider>
      <BrowserRouter>
        <MainLayout>
          <Routes>
            <Route path="/" element={<UploadPage />} />
            <Route path="/result/:uploadId" element={<ResultPage />} />
            <Route path="/history" element={<HistoryPage />} />
          </Routes>
        </MainLayout>
      </BrowserRouter>
    </QueryProvider>
  )
}

export default App
