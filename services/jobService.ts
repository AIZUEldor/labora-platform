import api from './api';
import { Job, JobImage, JobListResponse, NearbyJob } from '../types';

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

  getNearbyJobs: async (latitude: number, longitude: number, radiusKm: number = 10): Promise<NearbyJob[]> => {
    const response = await api.get<NearbyJob[]>('/Job/nearby', {
      params: { latitude, longitude, radiusKm },
    });
    return response.data;
  },

  getAllActiveJobs: async (latitude: number = 0, longitude: number = 0): Promise<NearbyJob[]> => {
    const response = await api.get<NearbyJob[]>('/Job/all-active', {
      params: { latitude, longitude },
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
    jobType: number;
    categoryId: string | undefined;
    subCategoryId?: string;
    city: string;
    country: string;
    latitude?: number;
    longitude?: number;
    requiredSkills?: string;
    experienceYears?: number;
    deadline?: string;
    categoryName: string;
    subCategoryName?: string;
  }): Promise<Job> => {
    const response = await api.post<Job>('/Job', data);
    return response.data;
  },

  getMyJobs: async (): Promise<Job[]> => {
    const response = await api.get<Job[]>('/Job/my-jobs');
    return response.data;
  },

  getAllJobs: async (): Promise<Job[]> => {
    const response = await api.get<JobListResponse>('/Job', {
      params: { pageNumber: 1, pageSize: 1000 },
    });
    return response.data.items;
  },

  uploadJobImage: async (jobId: string, imageUri: string, caption?: string): Promise<JobImage> => {
    const formData = new FormData();
    formData.append('file', {
      uri: imageUri,
      name: 'job.jpg',
      type: 'image/jpeg',
    } as any);
    if (caption) formData.append('caption', caption);
    const response = await api.post<JobImage>(`/Job/${jobId}/images`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  deleteJobImage: async (jobId: string, imageId: string): Promise<void> => {
    await api.delete(`/Job/${jobId}/images/${imageId}`);
  },
};
