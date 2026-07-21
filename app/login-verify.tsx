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
import { useRouter, useLocalSearchParams } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { useThemeStore } from '../store/themeStore';
import { useAuthStore } from '../store/authStore';
import { ThemeColors, LightColors } from '../constants/colors';
import { ArrowLeftIcon, LockIcon, CheckIcon } from '../components/icons';
import { authService } from '../services/authService';
import { useLanguageStore } from '../stores/useLanguageStore';

export default function LoginVerifyScreen(): React.JSX.Element {
  const router = useRouter();
  const { verificationId } = useLocalSearchParams<{ verificationId: string }>();
  const { theme } = useThemeStore();
  const colors = ThemeColors[theme];
  const { t } = useLanguageStore();
  const login = useAuthStore((state) => state.login);

  const [code, setCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [resending, setResending] = useState(false);

  const handleConfirm = async (): Promise<void> => {
    if (loading || resending) return;
    if (!verificationId) {
      Alert.alert(t.common.error, t.loginVerify.genericError);
      return;
    }
    if (!code.trim() || code.trim().length !== 6) {
      Alert.alert(t.common.error, t.loginVerify.invalidCode);
      return;
    }

    try {
      setLoading(true);

      // 1. Kodni tasdiqlash
      const verifyResponse = await authService.loginVerify({
        verificationId,
        code: code.trim(),
      });

      if (!verifyResponse.isVerified || !verifyResponse.operationToken) {
        Alert.alert(t.common.error, t.loginVerify.invalidCode);
        return;
      }

      // 2. Operation token bilan darhol yakunlash
      const completeResponse = await authService.loginComplete({
        verificationId,
        operationToken: verifyResponse.operationToken,
      });

      // 3. Auth holatini saqlash — faqat shundan keyin (tabs) ga o'tish
      await login(
        completeResponse.token,
        completeResponse.role,
        completeResponse.firstName,
        completeResponse.lastName
      );

      router.replace('/(tabs)');
    } catch (error: any) {
      Alert.alert(t.common.error, error?.message || t.loginVerify.genericError);
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async (): Promise<void> => {
    if (resending || loading || !verificationId) return;
    try {
      setResending(true);
      await authService.loginResend({ verificationId });
      Alert.alert(t.common.success, t.loginVerify.resendSuccess);
    } catch (error: any) {
      Alert.alert(t.common.error, error?.message || t.loginVerify.genericError);
    } finally {
      setResending(false);
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
        <Text style={styles.headerTitle}>{t.loginVerify.verifyTitle}</Text>
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
          <Text style={styles.iconTitle}>{t.loginVerify.verifyTitle}</Text>
          <Text style={styles.iconSubtitle}>{t.loginVerify.verifySubtitle}</Text>
        </View>

        <View style={styles.formSection}>
          <View style={styles.field}>
            <Text style={styles.label}>{t.loginVerify.codeLabel}</Text>
            <View style={styles.inputWrapper}>
              <TextInput
                style={styles.input}
                value={code}
                onChangeText={setCode}
                placeholder={t.loginVerify.codePlaceholder}
                placeholderTextColor={colors.textSecondary}
                keyboardType="number-pad"
                maxLength={6}
                editable={!loading}
              />
            </View>
          </View>
        </View>

        <TouchableOpacity
          style={styles.resendButton}
          onPress={handleResend}
          disabled={resending || loading}
          activeOpacity={0.7}
        >
          {resending
            ? <ActivityIndicator size="small" color={colors.primary} />
            : <Text style={styles.resendText}>{t.loginVerify.resendButton}</Text>
          }
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.primaryButton, loading && styles.primaryButtonDisabled]}
          onPress={handleConfirm}
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
                <Text style={styles.primaryButtonText}>{t.loginVerify.confirmButton}</Text>
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
    input: {
      flex: 1,
      height: 48,
      paddingHorizontal: 14,
      fontSize: 15,
      color: colors.textPrimary,
      letterSpacing: 2,
    },
    resendButton: {
      alignSelf: 'center',
      marginTop: 16,
      padding: 8,
    },
    resendText: {
      fontSize: 14,
      fontWeight: '600',
      color: colors.primary,
    },
    primaryButton: {
      marginHorizontal: 16,
      marginTop: 24,
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
