// scr/services/api/analysis.api.ts

import apiClient from "./client";
import type {
    UploadResponse,
    AnalysisResult,
    AnalysisHistoryItem,
    HistoryParams,
} from '@/types/api.types';
  
export const uploadFile = async (file: File): Promise<UploadResponse> => {
    const formData = new FormData()
    formData.append('file', file)
  
    const response = await apiClient.post<UploadResponse>('/api/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  
    return response.data
}
  
export const getAnalysisResult = async (uploadId: string): Promise<AnalysisResult> => {
    const response = await apiClient.get<AnalysisResult>(`/api/analysis/${uploadId}`)
    return response.data
  }
  
  export const getAnalysisHistory = async (
    params: HistoryParams = {}
  ): Promise<AnalysisHistoryItem[]> => {
    const response = await apiClient.get<AnalysisHistoryItem[]>('/api/analysis/history', {
      params: {
        count: params.count || 10,
        verdictFilter: params.verdictFilter ?? undefined,
      },
    })
    return response.data
  }