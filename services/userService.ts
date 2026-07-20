import api from './api';
import {
  UserProfile,
  ChangePhoneStartRequest,
  ChangePhoneResendRequest,
  ChangePhoneVerifyRequest,
  ChangePhoneCompleteRequest,
  StartOtpResponse,
  ResendOtpResponse,
  VerifyOtpResponse,
  AuthResponse,
} from '../types';

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

  // Telefon raqamni almashtirish — OTP oqimi
  changePhoneStart: async (data: ChangePhoneStartRequest): Promise<StartOtpResponse> => {
    const response = await api.post<StartOtpResponse>('/User/change-phone/start', data);
    return response.data;
  },

  changePhoneResend: async (data: ChangePhoneResendRequest): Promise<ResendOtpResponse> => {
    const response = await api.post<ResendOtpResponse>('/User/change-phone/resend', data);
    return response.data;
  },

  changePhoneVerify: async (data: ChangePhoneVerifyRequest): Promise<VerifyOtpResponse> => {
    const response = await api.post<VerifyOtpResponse>('/User/change-phone/verify', data);
    return response.data;
  },

  changePhoneComplete: async (data: ChangePhoneCompleteRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/User/change-phone/complete', data);
    return response.data;
  },
};