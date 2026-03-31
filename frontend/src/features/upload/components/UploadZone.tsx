// src/features/upload/components/UploadZone.tsx
import { useCallback } from 'react'
import { useDropzone } from 'react-dropzone'
import { cn } from '@/utils/cn'
import { Card, CardContent } from '@/components/ui/Card'
import { Spinner } from '@/components/ui/Spinner'
import { ALLOWED_EXTENSIONS, MAX_FILE_SIZE } from '../types/upload.types'

interface UploadZoneProps {
  onFileSelect: (file: File) => void
  isUploading: boolean
  error: string | null
}

export function UploadZone({ onFileSelect, isUploading, error }: UploadZoneProps) {
  const onDrop = useCallback(
    (acceptedFiles: File[]) => {
      if (acceptedFiles.length > 0) {
        onFileSelect(acceptedFiles[0])
      }
    },
    [onFileSelect]
  )

  const { getRootProps, getInputProps, isDragActive, isDragReject } = useDropzone({
    onDrop,
    accept: {
      'application/x-msdownload': ALLOWED_EXTENSIONS,
      'application/x-dosexec': ALLOWED_EXTENSIONS,
    },
    maxSize: MAX_FILE_SIZE,
    maxFiles: 1,
    disabled: isUploading,
  })

  return (
    <Card>
      <CardContent className="p-8">
        <div
          {...getRootProps()}
          className={cn(
            'border-2 border-dashed rounded-lg p-12 text-center cursor-pointer transition-all',
            'hover:border-terminal-cyan hover:bg-terminal-gray/50',
            isDragActive && 'border-terminal-green bg-terminal-green/10 shadow-glow-green',
            isDragReject && 'border-terminal-red bg-terminal-red/10',
            isUploading && 'cursor-not-allowed opacity-50'
          )}
        >
          <input {...getInputProps()} />

          {isUploading ? (
            <div className="flex flex-col items-center gap-4">
              <Spinner size="lg" />
              <p className="text-terminal-cyan">Uploading file...</p>
            </div>
          ) : (
            <div className="flex flex-col items-center gap-4">
              <div className="h-16 w-16 rounded-full bg-terminal-green/20 flex items-center justify-center border border-terminal-green">
                <span className="text-terminal-green text-3xl">↑</span>
              </div>
              <div>
                <p className="text-lg font-medium text-terminal-green mb-2">
                  {isDragActive ? 'Drop file here' : 'Drop PE file here, or click to browse'}
                </p>
                <p className="text-sm text-terminal-muted">
                  Supports .exe and .dll files up to 10MB
                </p>
              </div>
            </div>
          )}
        </div>

        {error && (
          <div className="mt-4 p-4 rounded-md bg-terminal-red/10 border border-terminal-red">
            <p className="text-sm text-terminal-red">{error}</p>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
