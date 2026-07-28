import { Tabs } from 'expo-router';
import { useState } from 'react';
import { useAuthStore, AuthState } from '../../store/authStore';
import { useThemeStore } from '../../store/themeStore';
import { useLanguageStore } from '../../stores/useLanguageStore';
import { UserRole } from '../../types';
import { router } from 'expo-router';
import Svg, { Path, Circle, Rect } from 'react-native-svg';
import { Modal, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

function PlusButton({ colors }: { colors: any }) {
  const role       = useAuthStore((state: AuthState) => state.role);
  const isEmployer = Number(role) === UserRole.Employer;
  const [visible, setVisible] = useState(false);
  const { language } = useLanguageStore();
  const label = (uz: string, ru: string, en: string) =>
    language === 'uz' ? uz : language === 'ru' ? ru : en;

  return (
    <>
      <TouchableOpacity
        onPress={() => setVisible(true)}
        activeOpacity={0.85}
        style={styles.plusWrapper}
      >
        <View style={[styles.plusButton, { backgroundColor: colors.primary }]}>
          <Svg width={28} height={28} viewBox="0 0 24 24" fill="none">
            <Path d="M12 5V19M5 12H19" stroke="#fff" strokeWidth={2.2} strokeLinecap="round" />
          </Svg>
        </View>
      </TouchableOpacity>

      <Modal visible={visible} transparent animationType="fade" onRequestClose={() => setVisible(false)}>
        <TouchableOpacity style={styles.overlay} activeOpacity={1} onPress={() => setVisible(false)}>
          <TouchableOpacity activeOpacity={1} style={[styles.sheet, { backgroundColor: colors.surface }]}>
            {isEmployer ? (
              <>
                <TouchableOpacity
                  style={[styles.option, { borderBottomColor: colors.border }]}
                  onPress={() => { setVisible(false); router.push('/employer/post-job'); }}
                >
                  <Text style={[styles.optionText, { color: colors.textPrimary }]}>
                    {label("Ish e'loni berish", 'Разместить вакансию', 'Create Job Post')}
                  </Text>
                </TouchableOpacity>
                <TouchableOpacity
                  style={[styles.option, { borderBottomColor: colors.border }]}
                  onPress={() => { setVisible(false); router.push('/property/create'); }}
                >
                  <Text style={[styles.optionText, { color: colors.textPrimary }]}>
                    {label('Uy e\'loni berish', 'Разместить объявление о недвижимости', 'Create Property Listing')}
                  </Text>
                </TouchableOpacity>
              </>
            ) : (
              <>
                <TouchableOpacity
                  style={[styles.option, { borderBottomColor: colors.border }]}
                  onPress={() => { setVisible(false); router.push('/post-worker'); }}
                >
                  <Text style={[styles.optionText, { color: colors.textPrimary }]}>
                    {label('Ish qidirish e\'loni berish', 'Разместить объявление о поиске работы', 'Create Worker Post')}
                  </Text>
                </TouchableOpacity>
                <TouchableOpacity
                  style={[styles.option, { borderBottomColor: colors.border }]}
                  onPress={() => { setVisible(false); router.push('/property/create'); }}
                >
                  <Text style={[styles.optionText, { color: colors.textPrimary }]}>
                    {label('Uy e\'loni berish', 'Разместить объявление о недвижимости', 'Create Property Listing')}
                  </Text>
                </TouchableOpacity>
              </>
            )}
            <TouchableOpacity style={styles.cancel} onPress={() => setVisible(false)}>
              <Text style={[styles.cancelText, { color: colors.textSecondary }]}>
                {label('Bekor qilish', 'Отмена', 'Cancel')}
              </Text>
            </TouchableOpacity>
          </TouchableOpacity>
        </TouchableOpacity>
      </Modal>
    </>
  );
}

export default function TabsLayout() {
  const role       = useAuthStore((state: AuthState) => state.role);
  const { colors } = useThemeStore();
  const insets     = useSafeAreaInsets();

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
      {/* Uy */}
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

      {/* Xarita */}
      <Tabs.Screen
        name="map"
        options={{
          tabBarIcon: ({ focused, color }) => (
            <Svg width={26} height={26} viewBox="0 0 24 24" fill="none">
              <Path
                d="M9 20L3 17V4L9 7M9 20L15 17M9 20V7M15 17L21 20V7L15 4M15 17V4M9 7L15 4"
                fill={focused ? '#DCFCE7' : 'none'}
                stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round"
              />
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

      {/* Arizalar */}
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
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.5)',
    justifyContent: 'flex-end',
  },
  sheet: {
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
    paddingHorizontal: 20,
    paddingTop: 12,
    paddingBottom: 32,
  },
  option: {
    paddingVertical: 16,
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  optionText: {
    fontSize: 16,
    fontWeight: '600',
    textAlign: 'center',
  },
  cancel: {
    paddingVertical: 16,
    marginTop: 8,
  },
  cancelText: {
    fontSize: 15,
    fontWeight: '500',
    textAlign: 'center',
  },
});