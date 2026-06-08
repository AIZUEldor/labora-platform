import api from './api';
import { Job } from '../types';

export const savedJobService = {
  getSavedJobs: async (): Promise<Job[]> => {
    const response = await api.get<Job[]>('/SavedJob');
    return response.data;
  },

  saveJob: async (jobId: string): Promise<void> => {
    await api.post(`/SavedJob/${jobId}`);
  },

  unsaveJob: async (jobId: string): Promise<void> => {
    await api.delete(`/SavedJob/${jobId}`);
  },

  isJobSaved: async (jobId: string): Promise<boolean> => {
    const response = await api.get<{ isSaved: boolean }>(`/SavedJob/${jobId}/check`);
    return response.data.isSaved;
  },
};