// src/features/upload/pages/UploadPage.tsx
import { UploadZone } from '../components/UploadZone'
import { useFileUpload } from '../hooks/useFileUpload'

export function UploadPage() {
  const { uploadFile, isUploading, error } = useFileUpload()

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h1 className="text-4xl font-bold text-terminal-green text-glow mb-4">
          Ransomware Detection
        </h1>
        <p className="text-terminal-muted max-w-2xl mx-auto">
          Upload a PE file (.exe or .dll) for static analysis. The system detects ransomware
          characteristics including entropy analysis, suspicious API usage, and PE structure anomalies.
        </p>
      </div>

      <UploadZone
        onFileSelect={uploadFile}
        isUploading={isUploading}
        error={error?.message || null}
      />
    </div>
  )
}
