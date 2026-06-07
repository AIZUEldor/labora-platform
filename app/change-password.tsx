import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  TextInput,
  ActivityIndicator,
  Alert,
  Platform,
} from 'react-native';
import { useRouter } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { useThemeStore } from '../store/themeStore';
import { ThemeColors, LightColors } from '../constants/colors';
import { ArrowLeftIcon, LockIcon, CheckIcon, EyeIcon, EyeOffIcon } from '../components/icons';
import api from '../services/api';

export default function ChangePasswordScreen(): React.JSX.Element {
  const router = useRouter();
  const { theme } = useThemeStore();
  const colors = ThemeColors[theme];

  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showCurrent, setShowCurrent] = useState(false);
  const [showNew, setShowNew] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (): Promise<void> => {
    if (!currentPassword || !newPassword || !confirmPassword) {
      Alert.alert('Xatolik', 'Barcha maydonlarni to\'ldiring.');
      return;
    }
    if (newPassword.length < 6) {
      Alert.alert('Xatolik', 'Yangi parol kamida 6 ta belgidan iborat bo\'lishi kerak.');
      return;
    }
    if (newPassword !== confirmPassword) {
      Alert.alert('Xatolik', 'Yangi parol va tasdiqlash paroli mos kelmaydi.');
      return;
    }
    if (currentPassword === newPassword) {
      Alert.alert('Xatolik', 'Yangi parol joriy paroldan farq qilishi kerak.');
      return;
    }

    try {
      setLoading(true);
      await api.post('/User/change-password', {
        currentPassword,
        newPassword,
      });
      Alert.alert('Muvaffaqiyat', 'Parol muvaffaqiyatli o\'zgartirildi.', [
        { text: 'OK', onPress: () => router.back() },
      ]);
    } catch {
      Alert.alert('Xatolik', 'Joriy parol noto\'g\'ri yoki xatolik yuz berdi.');
    } finally {
      setLoading(false);
    }
  };

  const styles = createStyles(colors, theme);

  return (
    <View style={styles.container}>
      <LinearGradient
        colors={theme === 'dark' ? ['#14532d', '#166534'] : ['#16A34A', '#15803d']}
        style={styles.header}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
      >
        <TouchableOpacity style={styles.backButton} onPress={() => router.back()} activeOpacity={0.7}>
          <ArrowLeftIcon size={22} color="#ffffff" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Parolni o'zgartirish</Text>
        <View style={{ width: 40 }} />
      </LinearGradient>

      <ScrollView
        style={styles.scrollView}
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
        <View style={styles.iconSection}>
          <View style={styles.iconWrapper}>
            <LockIcon size={40} color={colors.primary} />
          </View>
          <Text style={styles.iconTitle}>Yangi parol o'rnating</Text>
          <Text style={styles.iconSubtitle}>Xavfsizlik uchun kuchli parol tanlang</Text>
        </View>

        <View style={styles.formSection}>
          {/* Joriy parol */}
          <View style={styles.field}>
            <Text style={styles.label}>Joriy parol</Text>
            <View style={styles.inputWrapper}>
              <TextInput
                style={styles.input}
                value={currentPassword}
                onChangeText={setCurrentPassword}
                placeholder="Joriy parolingiz"
                placeholderTextColor={colors.textSecondary}
                secureTextEntry={!showCurrent}
                autoCapitalize="none"
              />
              <TouchableOpacity style={styles.eyeButton} onPress={() => setShowCurrent(p => !p)}>
                {showCurrent
                  ? <EyeOffIcon size={20} color={colors.textSecondary} />
                  : <EyeIcon size={20} color={colors.textSecondary} />
                }
              </TouchableOpacity>
            </View>
          </View>

          {/* Yangi parol */}
          <View style={styles.field}>
            <Text style={styles.label}>Yangi parol</Text>
            <View style={styles.inputWrapper}>
              <TextInput
                style={styles.input}
                value={newPassword}
                onChangeText={setNewPassword}
                placeholder="Kamida 6 ta belgi"
                placeholderTextColor={colors.textSecondary}
                secureTextEntry={!showNew}
                autoCapitalize="none"
              />
              <TouchableOpacity style={styles.eyeButton} onPress={() => setShowNew(p => !p)}>
                {showNew
                  ? <EyeOffIcon size={20} color={colors.textSecondary} />
                  : <EyeIcon size={20} color={colors.textSecondary} />
                }
              </TouchableOpacity>
            </View>
          </View>

          {/* Tasdiqlash */}
          <View style={styles.field}>
            <Text style={styles.label}>Yangi parolni tasdiqlang</Text>
            <View style={styles.inputWrapper}>
              <TextInput
                style={styles.input}
                value={confirmPassword}
                onChangeText={setConfirmPassword}
                placeholder="Parolni qayta kiriting"
                placeholderTextColor={colors.textSecondary}
                secureTextEntry={!showConfirm}
                autoCapitalize="none"
              />
              <TouchableOpacity style={styles.eyeButton} onPress={() => setShowConfirm(p => !p)}>
                {showConfirm
                  ? <EyeOffIcon size={20} color={colors.textSecondary} />
                  : <EyeIcon size={20} color={colors.textSecondary} />
                }
              </TouchableOpacity>
            </View>
          </View>
        </View>

        <TouchableOpacity
          style={[styles.primaryButton, loading && styles.primaryButtonDisabled]}
          onPress={handleSubmit}
          disabled={loading}
          activeOpacity={0.85}
        >
          <LinearGradient
            colors={loading ? ['#86efac', '#86efac'] : ['#16A34A', '#15803d']}
            style={styles.primaryButtonGradient}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 0 }}
          >
            {loading ? (
              <ActivityIndicator size="small" color="#ffffff" />
            ) : (
              <>
                <CheckIcon size={18} color="#ffffff" />
                <Text style={styles.primaryButtonText}>Saqlash</Text>
              </>
            )}
          </LinearGradient>
        </TouchableOpacity>

        <View style={{ height: 40 }} />
      </ScrollView>
    </View>
  );
}

function createStyles(colors: typeof LightColors, theme: 'light' | 'dark') {
  return StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.background,
    },
    header: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      paddingTop: Platform.OS === 'ios' ? 56 : 48,
      paddingBottom: 16,
      paddingHorizontal: 16,
    },
    backButton: {
      width: 40,
      height: 40,
      borderRadius: 20,
      backgroundColor: 'rgba(255,255,255,0.15)',
      justifyContent: 'center',
      alignItems: 'center',
    },
    headerTitle: {
      fontSize: 18,
      fontWeight: '700',
      color: '#ffffff',
      flex: 1,
      textAlign: 'center',
      marginHorizontal: 8,
    },
    scrollView: { flex: 1 },
    scrollContent: { paddingBottom: 24 },
    iconSection: {
      alignItems: 'center',
      paddingVertical: 32,
      gap: 8,
    },
    iconWrapper: {
      width: 80,
      height: 80,
      borderRadius: 40,
      backgroundColor: theme === 'dark' ? '#1e3a2a' : '#f0fdf4',
      justifyContent: 'center',
      alignItems: 'center',
      marginBottom: 8,
    },
    iconTitle: {
      fontSize: 18,
      fontWeight: '700',
      color: colors.textPrimary,
    },
    iconSubtitle: {
      fontSize: 13,
      color: colors.textSecondary,
      textAlign: 'center',
    },
    formSection: {
      paddingHorizontal: 16,
      gap: 16,
    },
    field: { gap: 6 },
    label: {
      fontSize: 13,
      fontWeight: '500',
      color: colors.textSecondary,
    },
    inputWrapper: {
      flexDirection: 'row',
      alignItems: 'center',
      borderWidth: 1.5,
      borderColor: colors.border,
      borderRadius: 12,
      backgroundColor: theme === 'dark' ? '#111827' : '#f9fafb',
    },
    input: {
      flex: 1,
      height: 48,
      paddingHorizontal: 14,
      fontSize: 15,
      color: colors.textPrimary,
    },
    eyeButton: {
      paddingHorizontal: 14,
      height: 48,
      justifyContent: 'center',
    },
    primaryButton: {
      marginHorizontal: 16,
      marginTop: 32,
      borderRadius: 14,
      overflow: 'hidden',
    },
    primaryButtonDisabled: { opacity: 0.7 },
    primaryButtonGradient: {
      height: 52,
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 8,
    },
    primaryButtonText: {
      fontSize: 16,
      fontWeight: '700',
      color: '#ffffff',
      letterSpacing: 0.3,
    },
  });
}