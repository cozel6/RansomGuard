// src/services/api/client.ts
import axios, { AxiosError, AxiosInstance } from 'axios'
import type { ErrorResponse } from '@/types/api.types'

const apiClient: AxiosInstance = axios.create({
  baseURL: 'http://localhost:5087',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use(
  (config) => {
    if (import.meta.env.DEV) {
      console.log(`[API Request] ${config.method?.toUpperCase()} ${config.url}`)
    }
    return config
  },
  (error) => Promise.reject(error)
)

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ErrorResponse>) => {
    if (!error.response) {
      return Promise.reject({
        error: {
          code: 'NETWORK_ERROR',
          message: 'Unable to connect to the server.',
          timestamp: new Date().toISOString(),
        },
      } as ErrorResponse)
    }

    const errorResponse: ErrorResponse = error.response.data || {
      error: {
        code: 'UNKNOWN_ERROR',
        message: error.message || 'An unexpected error occurred',
        timestamp: new Date().toISOString(),
      },
    }

    return Promise.reject(errorResponse)
  }
)

export default apiClient