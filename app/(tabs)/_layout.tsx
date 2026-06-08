import { Tabs } from 'expo-router';
import { useAuthStore, AuthState } from '../../store/authStore';
import { useThemeStore } from '../../store/themeStore';
import { UserRole } from '../../types';
import { router } from 'expo-router';
import Svg, { Path, Circle, Rect } from 'react-native-svg';
import { StyleSheet, TouchableOpacity, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

function PlusButton({ colors }: { colors: any }) {
  const role       = useAuthStore((state: AuthState) => state.role);
  const isEmployer = Number(role) === UserRole.Employer;

  return (
    <TouchableOpacity
      onPress={() => {
        if (isEmployer) router.push('/employer/post-job');
        else router.push('/post-worker');
      }}
      activeOpacity={0.85}
      style={styles.plusWrapper}
    >
      <View style={[styles.plusButton, { backgroundColor: colors.primary }]}>
        <Svg width={28} height={28} viewBox="0 0 24 24" fill="none">
          <Path d="M12 5V19M5 12H19" stroke="#fff" strokeWidth={2.2} strokeLinecap="round" />
        </Svg>
      </View>
    </TouchableOpacity>
  );
}

export default function TabsLayout() {
  const role       = useAuthStore((state: AuthState) => state.role);
  const { colors } = useThemeStore();
  const insets     = useSafeAreaInsets();
  const isEmployer = Number(role) === UserRole.Employer;

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarStyle: [styles.tabBar, {
          backgroundColor: colors.surface,
          height: 64 + insets.bottom,
          paddingBottom: insets.bottom + 8,
        }],
        tabBarShowLabel: false,
        tabBarActiveTintColor: '#16A34A',
        tabBarInactiveTintColor: '#9CA3AF',
      }}
    >
      {/* Home */}
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

      {/* Worker: Arizalarim | Employer: E'lonlarim */}
      <Tabs.Screen
        name="applications"
        options={{
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

      {/* O'rtadagi + tugma */}
      <Tabs.Screen
        name="create-job"
        options={{
          tabBarButton: () => <PlusButton colors={colors} />,
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
    paddingTop: 8,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: -2 },
    shadowOpacity: 0.06,
    shadowRadius: 12,
    elevation: 12,
  },
  plusWrapper: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  plusButton: {
    width: 52,
    height: 52,
    borderRadius: 26,
    alignItems: 'center',
    justifyContent: 'center',
    elevation: 4,
    shadowColor: '#16A34A',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3,
    shadowRadius: 6,
  },
});
