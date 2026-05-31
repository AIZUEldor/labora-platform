import api from './api';
import { NotificationType } from '../types';

export interface NotificationResponse {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  referenceId: string | null;
  createdAt: string;
}

export interface UserPreferenceRequest {
  preferredCategoryIds: string[];
  preferredJobType: number | null;
  maxDistanceKm: number | null;
}

export const notificationService = {
  getMyNotifications: async (): Promise<NotificationResponse[]> => {
    const response = await api.get('/notifications');
    return response.data;
  },

  getUnreadCount: async (): Promise<number> => {
    const response = await api.get('/notifications/unread-count');
    return response.data.count;
  },

  markAsRead: async (id: string): Promise<void> => {
    await api.put(`/notifications/${id}/read`);
  },

  markAllAsRead: async (): Promise<void> => {
    await api.put('/notifications/read-all');
  },

  savePreferences: async (dto: UserPreferenceRequest): Promise<void> => {
    await api.post('/notifications/preferences', dto);
  },

  getPreferences: async (): Promise<UserPreferenceRequest[]> => {
    const response = await api.get('/notifications/preferences');
    return response.data;
  },
};