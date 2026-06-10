import api from './api';
import { NotificationType } from '../types';
import * as Notifications from 'expo-notifications';
import * as Device from 'expo-device';
import { Platform } from 'react-native';

Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowBanner: true,
    shouldShowList: true,
    shouldPlaySound: true,
    shouldSetBadge: true,
  }),
});

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
  registerPushToken: async (): Promise<void> => {
    if (!Device.isDevice) return;

    const { status: existingStatus } = await Notifications.getPermissionsAsync();
    let finalStatus = existingStatus;

    if (existingStatus !== 'granted') {
      const { status } = await Notifications.requestPermissionsAsync();
      finalStatus = status;
    }

    if (finalStatus !== 'granted') return;

    const tokenData = await Notifications.getDevicePushTokenAsync();
    const token = tokenData.data;

    await api.post('/PushToken', {
      token,
      platform: Platform.OS,
    });
  },

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