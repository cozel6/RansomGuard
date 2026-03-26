// src/features/upload/utils/validateFile.ts
import { ALLOWED_EXTENSIONS, MAX_FILE_SIZE, FileValidationResult } from '../types/upload.types'

export function validateFile(file: File): FileValidationResult {
  const errors: string[] = []

  if (file.size > MAX_FILE_SIZE) {
    errors.push(`File size must be less than ${MAX_FILE_SIZE / 1024 / 1024}MB`)
  }

  const extension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase()
  if (!ALLOWED_EXTENSIONS.includes(extension)) {
    errors.push(`Only ${ALLOWED_EXTENSIONS.join(', ')} files are allowed`)
  }

  if (file.name.includes('\0') || file.name.includes('..') || file.name.includes('/') || file.name.includes('\\')) {
    errors.push('Invalid filename')
  }

  return { isValid: errors.length === 0, errors }
}
