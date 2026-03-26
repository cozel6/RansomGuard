// src/features/upload/hooks/useFileUpload.ts
import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { uploadFile } from '@/services/api/analysis.api'
import { validateFile } from '../utils/validateFile'

export function useFileUpload() {
  const navigate = useNavigate()

  const mutation = useMutation({
    mutationFn: uploadFile,
    onSuccess: (data) => {
      navigate(`/result/${data.uploadId}`)
    },
  })

  const handleUpload = (file: File) => {
    const validation = validateFile(file)
    if (!validation.isValid) {
      throw new Error(validation.errors.join(', '))
    }
    mutation.mutate(file)
  }

  return {
    uploadFile: handleUpload,
    isUploading: mutation.isPending,
    error: mutation.error,
    reset: mutation.reset,
  }
}
