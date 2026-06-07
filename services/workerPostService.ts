import api from './api';
import { WorkerPost, CreateWorkerPostRequest } from '../types';

export const workerPostService = {
  getAll: async (categoryId?: string, subCategoryId?: string, city?: string): Promise<WorkerPost[]> => {
    const params: Record<string, string> = {};
    if (categoryId) params.categoryId = categoryId;
    if (subCategoryId) params.subCategoryId = subCategoryId;
    if (city) params.city = city;
    const response = await api.get('/WorkerPost', { params });
    return response.data;
  },

  getById: async (id: string): Promise<WorkerPost> => {
    const response = await api.get(`/WorkerPost/${id}`);
    return response.data;
  },

  getMyPosts: async (): Promise<WorkerPost[]> => {
    const response = await api.get('/WorkerPost/my');
    return response.data;
  },

  create: async (data: CreateWorkerPostRequest): Promise<WorkerPost> => {
    const response = await api.post('/WorkerPost', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateWorkerPostRequest> & { status?: number }): Promise<WorkerPost> => {
    const response = await api.put(`/WorkerPost/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/WorkerPost/${id}`);
  },

  uploadPortfolioImage: async (postId: string, imageUri: string, caption?: string): Promise<void> => {
    const formData = new FormData();
    formData.append('file', {
      uri: imageUri,
      name: 'portfolio.jpg',
      type: 'image/jpeg',
    } as any);
    if (caption) formData.append('caption', caption);
    await api.post(`/WorkerPost/${postId}/images`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },

  deletePortfolioImage: async (postId: string, imageId: string): Promise<void> => {
    await api.delete(`/WorkerPost/${postId}/images/${imageId}`);
  },

  incrementView: async (postId: string): Promise<void> => {
  await api.patch(`/WorkerPost/${postId}/view`);
},

contactWorker: async (postId: string): Promise<void> => {
  await api.post(`/WorkerPost/${postId}/contact`);
},
};