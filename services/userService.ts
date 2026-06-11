import api from './api';
import { UserProfile } from '../types';

export const userService = {
  // Profilni olish
  getProfile: async (): Promise<UserProfile> => {
    const response = await api.get<UserProfile>('/User/profile');
    return response.data;
  },

  // Profilni yangilash
  updateProfile: async (data: Partial<UserProfile>): Promise<UserProfile> => {
    const response = await api.put<UserProfile>('/User/profile', data);
    return response.data;
  },

  // CV yuklash
  uploadCv: async (fileUri: string, fileName: string): Promise<string> => {
    const formData = new FormData();
    formData.append('file', {
      uri: fileUri,
      name: fileName,
      type: 'application/pdf',
    } as any);
    const response = await api.post<{ url: string }>('/User/upload-cv', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data.url;
  },

  // Avatar yuklash
  uploadAvatar: async (fileUri: string, fileName: string, type: string): Promise<string> => {
    const formData = new FormData();
    formData.append('file', {
      uri: fileUri,
      name: fileName,
      type,
    } as any);
    const response = await api.post<{ url: string }>('/User/upload-avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data.url;
  },

  // Account o'chirish
deleteAccount: async (): Promise<void> => {
  await api.delete('/User/delete-account');
},
};