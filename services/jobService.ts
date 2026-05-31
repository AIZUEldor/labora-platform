import api from './api';
import { Job, JobListResponse } from '../types';

export const jobService = {
  getJobs: async (page: number = 1, pageSize: number = 10): Promise<JobListResponse> => {
    const response = await api.get<JobListResponse>('/Job', {
      params: { pageNumber: page, pageSize },
    });
    return response.data;
  },

  getJobById: async (id: string): Promise<Job> => {
    const response = await api.get<Job>(`/Job/${id}`);
    return response.data;
  },

  getNearbyJobs: async (latitude: number, longitude: number, radiusKm: number = 10): Promise<Job[]> => {
    const response = await api.get<Job[]>('/Job/nearby', {
      params: { latitude, longitude, radiusKm },
    });
    return response.data;
  },

  searchJobs: async (keyword: string, page: number = 1): Promise<JobListResponse> => {
    const response = await api.get<JobListResponse>('/Job', {
      params: { keyword, pageNumber: page, pageSize: 10 },
    });
    return response.data;
  },

  getJobsByCategory: async (categoryId: string, page: number = 1): Promise<JobListResponse> => {
    const response = await api.get<JobListResponse>('/Job', {
      params: { categoryId, pageNumber: page, pageSize: 10 },
    });
    return response.data;
  },

  createJob: async (data: {
  title: string;
  description: string;
  salary: number;
  location: string;
  categoryId: string;
  latitude?: number;
  longitude?: number;
}): Promise<Job> => {
  const response = await api.post<Job>('/Job', data);
  return response.data;
},
getMyJobs: async (): Promise<Job[]> => {
  const response = await api.get<Job[]>('/Job/my-jobs');
  return response.data;
},
};

