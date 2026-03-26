// src/features/upload/types/upload.types.ts

export interface FileValidationResult {
  isValid: boolean
  errors: string[]
}

export const ALLOWED_EXTENSIONS = ['.exe', '.dll']
export const MAX_FILE_SIZE = 10 * 1024 * 1024 // 10MB
