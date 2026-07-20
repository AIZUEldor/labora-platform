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
import { ArrowLeftIcon, PhoneIcon, CheckIcon, EyeIcon, EyeOffIcon } from '../components/icons';
import { userService } from '../services/userService';
import { useLanguageStore } from '../stores/useLanguageStore';

export default function ChangePhoneScreen(): React.JSX.Element {
  const router = useRouter();
  const { theme } = useThemeStore();
  const colors = ThemeColors[theme];
  const { t } = useLanguageStore();

  const [newPhoneNumber, setNewPhoneNumber] = useState('');
  const [currentPassword, setCurrentPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (): Promise<void> => {
    if (loading) return;
    if (!newPhoneNumber.trim() || !currentPassword) {
      Alert.alert(t.common.error, t.changePhone.fillAllFields);
      return;
    }

    try {
      setLoading(true);
      const response = await userService.changePhoneStart({
        newPhoneNumber: newPhoneNumber.trim(),
        currentPassword,
      });
      router.push(`/change-phone-verify?verificationId=${response.verificationId}`);
    } catch (error: any) {
      Alert.alert(t.common.error, error?.message || t.changePhone.genericError);
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
        <Text style={styles.headerTitle}>{t.changePhone.title}</Text>
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
            <PhoneIcon size={40} color={colors.primary} />
          </View>
          <Text style={styles.iconTitle}>{t.changePhone.title}</Text>
          <Text style={styles.iconSubtitle}>{t.changePhone.subtitle}</Text>
        </View>

        <View style={styles.formSection}>
          {/* Yangi telefon raqam */}
          <View style={styles.field}>
            <Text style={styles.label}>{t.changePhone.newPhoneLabel}</Text>
            <View style={styles.inputWrapper}>
              <View style={styles.phonePrefix}>
                <PhoneIcon size={18} color={colors.textSecondary} />
              </View>
              <TextInput
                style={styles.input}
                value={newPhoneNumber}
                onChangeText={setNewPhoneNumber}
                placeholder={t.changePhone.newPhonePlaceholder}
                placeholderTextColor={colors.textSecondary}
                keyboardType="phone-pad"
                autoCapitalize="none"
                editable={!loading}
              />
            </View>
          </View>

          {/* Joriy parol */}
          <View style={styles.field}>
            <Text style={styles.label}>{t.changePhone.currentPasswordLabel}</Text>
            <View style={styles.inputWrapper}>
              <TextInput
                style={styles.input}
                value={currentPassword}
                onChangeText={setCurrentPassword}
                placeholder={t.changePhone.currentPasswordPlaceholder}
                placeholderTextColor={colors.textSecondary}
                secureTextEntry={!showPassword}
                autoCapitalize="none"
                editable={!loading}
              />
              <TouchableOpacity style={styles.eyeButton} onPress={() => setShowPassword(p => !p)}>
                {showPassword
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
                <Text style={styles.primaryButtonText}>{t.changePhone.sendCodeButton}</Text>
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
      paddingHorizontal: 32,
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
    phonePrefix: {
      paddingHorizontal: 14,
      height: 48,
      justifyContent: 'center',
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
