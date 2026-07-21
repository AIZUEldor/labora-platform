import { useEffect } from 'react';
import { Stack, router } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { pushNotificationService } from '../services/pushNotificationService';
import * as Notifications from 'expo-notifications';
import { useAuthStore, AuthState } from '../store/authStore';
import { useThemeStore } from '../store/themeStore';
import { useLanguageStore } from '../stores/useLanguageStore';


export default function RootLayout() {
  const loadToken = useAuthStore((state: AuthState) => state.loadToken);
  const token = useAuthStore((state: AuthState) => state.token);
  const isLoading = useAuthStore((state: AuthState) => state.isLoading);
  const { isDark, loadTheme } = useThemeStore();
  const { loadLanguage } = useLanguageStore();

  useEffect(() => {
    loadToken();
    loadTheme();
     loadLanguage();
  }, []);

  useEffect(() => {
  if (!isLoading) {
    router.replace('/(tabs)');
  }
}, [isLoading]);

  useEffect(() => {
    if (!token) return;

    const registerPush = async (): Promise<void> => {
  try {
    const pushToken = await pushNotificationService.registerForPushNotifications();

    console.log('DEVICE PUSH TOKEN:', pushToken);

    if (pushToken) {
      await pushNotificationService.registerTokenToServer(pushToken);
      console.log('PUSH TOKEN REGISTERED TO SERVER');
    }
  } catch (error) {
    console.log('PUSH REGISTER ERROR:', error);
  }
};

    registerPush();

    const subscription = Notifications.addNotificationResponseReceivedListener(response => {
      const data = response.notification.request.content.data as any;
      if (data?.referenceId) {
        router.push(`/job-detail?id=${data.referenceId}`);
      }
    });

    return () => subscription.remove();
  }, [token]);

  return (
    <>
      <StatusBar style={isDark ? 'light' : 'dark'} />
      <Stack screenOptions={{ headerShown: false }}>
        <Stack.Screen name="auth" />
        <Stack.Screen name="(tabs)" />
        <Stack.Screen name="post-worker" />
        <Stack.Screen name="employer/post-job" />
        <Stack.Screen name="employer/job-applications" />
        <Stack.Screen name="job-detail" />
        <Stack.Screen name="edit-profile" />
        <Stack.Screen name="notifications" />
        <Stack.Screen name="saved-jobs" />
        <Stack.Screen name="review" />
        <Stack.Screen name="search" />
        <Stack.Screen name="worker-post-detail" />
        <Stack.Screen name="application-detail" />
        <Stack.Screen name="change-password" />
        <Stack.Screen name="change-phone" />
        <Stack.Screen name="change-phone-verify" />
        <Stack.Screen name="forgot-password" />
        <Stack.Screen name="forgot-password-verify" />
        <Stack.Screen name="login-verify" />
        <Stack.Screen name="register-verify" />
      </Stack>
    </>
  );
}