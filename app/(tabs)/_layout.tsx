import { Tabs } from 'expo-router';
import { useAuthStore, AuthState } from '../../store/authStore';
import { useThemeStore } from '../../store/themeStore';
import { UserRole } from '../../types';
import Svg, { Path, Circle, Rect } from 'react-native-svg';
import { StyleSheet } from 'react-native';

export default function TabsLayout() {
  const role       = useAuthStore((state: AuthState) => state.role);
  const { colors } = useThemeStore();

  const isEmployer = Number(role) === UserRole.Employer;

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarStyle: [styles.tabBar, { backgroundColor: colors.surface }],
        tabBarShowLabel: false,
        tabBarActiveTintColor: '#16A34A',
        tabBarInactiveTintColor: '#9CA3AF',
      }}
    >
      {/* Bosh sahifa */}
      <Tabs.Screen
        name="index"
        options={{
          tabBarIcon: ({ focused, color }) => (
            <Svg width={26} height={26} viewBox="0 0 24 24" fill="none">
              <Path
                d="M3 9.5L12 3L21 9.5V20C21 20.55 20.55 21 20 21H15V15H9V21H4C3.45 21 3 20.55 3 20V9.5Z"
                fill={focused ? '#16A34A' : 'none'}
                stroke={color} strokeWidth={1.8} strokeLinejoin="round"
              />
            </Svg>
          ),
        }}
      />

      {/* Arizalar — Worker uchun, Employer da yashirin */}
      <Tabs.Screen
        name="applications"
        options={{
          href: isEmployer ? null : undefined,
          tabBarIcon: ({ focused, color }) => (
            <Svg width={26} height={26} viewBox="0 0 24 24" fill="none">
              <Rect
                x="4" y="3" width="16" height="18" rx="2"
                fill={focused ? '#DCFCE7' : 'none'}
                stroke={color} strokeWidth={1.8}
              />
              <Path d="M8 8H16M8 12H16M8 16H12" stroke={color} strokeWidth={1.8} strokeLinecap="round" />
            </Svg>
          ),
        }}
      />

      {/* E'lon qilish — faqat Employer */}
      <Tabs.Screen
        name="create-job"
        options={{
          href: isEmployer ? undefined : null,
          tabBarIcon: ({ focused, color }) => (
            <Svg width={26} height={26} viewBox="0 0 24 24" fill="none">
              <Circle cx="12" cy="12" r="10"
                fill={focused ? '#16A34A' : 'none'}
                stroke={color} strokeWidth={1.8}
              />
              <Path d="M12 8V16M8 12H16"
                stroke={focused ? '#fff' : color} strokeWidth={1.8} strokeLinecap="round"
              />
            </Svg>
          ),
        }}
      />

      {/* Profil */}
      <Tabs.Screen
        name="profile"
        options={{
          tabBarIcon: ({ focused, color }) => (
            <Svg width={26} height={26} viewBox="0 0 24 24" fill="none">
              <Circle cx="12" cy="8" r="4"
                fill={focused ? '#DCFCE7' : 'none'}
                stroke={color} strokeWidth={1.8}
              />
              <Path d="M4 20C4 17 7.58 15 12 15C16.42 15 20 17 20 20"
                stroke={color} strokeWidth={1.8} strokeLinecap="round"
              />
            </Svg>
          ),
        }}
      />
    </Tabs>
  );
}

const styles = StyleSheet.create({
  tabBar: {
    borderTopWidth: 0,
    height: 64,
    paddingBottom: 8,
    paddingTop: 8,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: -2 },
    shadowOpacity: 0.06,
    shadowRadius: 12,
    elevation: 12,
  },
});

