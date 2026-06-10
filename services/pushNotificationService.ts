import * as Notifications from 'expo-notifications';
import * as Device from 'expo-device';
import { Platform } from 'react-native';
import api from './api';

Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowBanner: true,
    shouldShowList: true,
    shouldPlaySound: true,
    shouldSetBadge: true,
  }),
});

export const pushNotificationService = {
  registerForPushNotifications: async (): Promise<string | null> => {
    if (!Device.isDevice) return null;

    const { status: existingStatus } = await Notifications.getPermissionsAsync();
    let finalStatus = existingStatus;

    if (existingStatus !== 'granted') {
      const { status } = await Notifications.requestPermissionsAsync();
      finalStatus = status;
    }

    if (finalStatus !== 'granted') return null;

    if (Platform.OS === 'android') {
      await Notifications.setNotificationChannelAsync('default', {
        name: 'default',
        importance: Notifications.AndroidImportance.MAX,
        vibrationPattern: [0, 250, 250, 250],
        lightColor: '#16A34A',
      });
    }

    const token = await Notifications.getDevicePushTokenAsync();
return token.data as string;
  },

  registerTokenToServer: async (token: string): Promise<void> => {
    const deviceType = Platform.OS === 'ios' ? 'ios' : 'android';
    await api.post('/pushnotifications/register', { token, deviceType });
  },

  removeTokenFromServer: async (): Promise<void> => {
    await api.delete('/pushnotifications/remove');
  },
};