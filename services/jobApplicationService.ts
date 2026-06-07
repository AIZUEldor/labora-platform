import api from './api';
import { JobApplication } from '../types';

export const jobApplicationService = {

  // Ishga ariza yuborish
  apply: async (jobId: string, coverLetter: string): Promise<void> => {
    await api.post('/JobApplication', { jobId, coverLetter });
  },

  // Mening arizalarim
  getMyApplications: async (): Promise<JobApplication[]> => {
  const response = await api.get<JobApplication[]>('/JobApplication/my-applications');
  return response.data;
},

  // Arizani bekor qilish
  cancel: async (id: string): Promise<void> => {
  await api.delete(`/JobApplication/${id}/cancel`);
},

  // E'longa kelgan arizalar
getJobApplications: async (jobId: string): Promise<JobApplication[]> => {
  const response = await api.get<JobApplication[]>(`/JobApplication/job/${jobId}`);
  return response.data;
},

// Ariza statusini yangilash
updateStatus: async (id: string, status: string): Promise<void> => {
  await api.put(`/JobApplication/${id}/status?status=${status}`);
},

// Ishni yakunlash
complete: async (id: string): Promise<void> => {
  await api.put(`/JobApplication/${id}/status?status=Completed`);
},

getById: async (id: string): Promise<JobApplication> => {
  const response = await api.get<JobApplication>(`/JobApplication/${id}`);
  return response.data;
},

};


