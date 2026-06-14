import { useState } from 'react';
import {
  View,
  Text,
  Image,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  Alert,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { router } from 'expo-router';
import { authService } from '../../services/authService';
import { useAuthStore, AuthState } from '../../store/authStore';
import { Colors } from '../../constants/colors';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { UserRole } from '../../types';
import { useLanguageStore } from '../../stores/useLanguageStore';

export default function RegisterScreen() {
  const [firstName, setFirstName] = useState<string>('');
  const [lastName, setLastName] = useState<string>('');
  const [age, setAge] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [phoneNumber, setPhoneNumber] = useState<string>('');
  const [role, setRole] = useState<UserRole>(UserRole.Worker);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [showPassword, setShowPassword] = useState<boolean>(false);

  const login = useAuthStore((state: AuthState) => state.login);
  const { t } = useLanguageStore();

  const handleRegister = async () => {
    if (!firstName.trim() || !lastName.trim() || !age.trim() || !password.trim() || !phoneNumber.trim()) {
      Alert.alert('Xato', t.common.error);
      return;
    }
    const ageNum = parseInt(age);
    if (isNaN(ageNum) || ageNum < 16 || ageNum > 100) {
      Alert.alert('Xato', t.common.somethingWentWrong);
      return;
    }
    setIsLoading(true);
    try {
      const response = await authService.register({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        age: ageNum,
        phoneNumber: phoneNumber.trim(),
        password,
        role,
      });
      await login(response.token, response.role, response.firstName, response.lastName);
      router.replace('/(tabs)');
    } catch (error: any) {
      const data = error.response?.data;
      const message = Array.isArray(data)
        ? data.join('\n')
        : data?.message ?? error.message ?? t.common.somethingWentWrong;
      Alert.alert('Xato', message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <LinearGradient
        colors={['#15803D', '#16A34A', '#22C55E']}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={styles.gradientHeader}
      >
        <View style={styles.logoContainer}>
  <Image
    source={require('../../assets/icon.png')}
    style={styles.logoImage}
  />
</View>
<Text style={styles.appName}>ALP</Text>
        <Text style={styles.appTagline}>Hisob yarating va ishni boshlang</Text>
      </LinearGradient>

      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
        <View style={styles.card}>
          <Text style={styles.title}>{t.auth.register}</Text>
          <Text style={styles.subtitle}>{t.auth.phone}</Text>

          {/* Role Selector */}
          <View style={styles.roleContainer}>
            <TouchableOpacity
              style={[styles.roleButton, role === UserRole.Worker && styles.roleButtonActive]}
              onPress={() => setRole(UserRole.Worker)}
              activeOpacity={0.8}
            >
              <Text style={styles.roleIcon}>👷</Text>
              <Text style={[styles.roleText, role === UserRole.Worker && styles.roleTextActive]}>
                {t.auth.worker}
              </Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={[styles.roleButton, role === UserRole.Employer && styles.roleButtonActive]}
              onPress={() => setRole(UserRole.Employer)}
              activeOpacity={0.8}
            >
              <Text style={styles.roleIcon}>🏢</Text>
              <Text style={[styles.roleText, role === UserRole.Employer && styles.roleTextActive]}>
                {t.auth.employer}
              </Text>
            </TouchableOpacity>
          </View>

          {/* Ism va Familiya */}
          <View style={styles.row}>
            <View style={[styles.inputWrapper, { flex: 1 }]}>
              <Text style={styles.inputLabel}>{t.auth.firstName}</Text>
              <View style={styles.inputContainer}>
                <TextInput
                  style={styles.input}
                  placeholder={t.auth.firstName}
                  placeholderTextColor={Colors.textTertiary}
                  value={firstName}
                  onChangeText={setFirstName}
                  autoCapitalize="words"
                />
              </View>
            </View>
            <View style={[styles.inputWrapper, { flex: 1 }]}>
              <Text style={styles.inputLabel}>{t.auth.lastName}</Text>
              <View style={styles.inputContainer}>
                <TextInput
                  style={styles.input}
                  placeholder={t.auth.lastName}
                  placeholderTextColor={Colors.textTertiary}
                  value={lastName}
                  onChangeText={setLastName}
                  autoCapitalize="words"
                />
              </View>
            </View>
          </View>

          {/* Yosh */}
          <View style={styles.inputWrapper}>
            <Text style={styles.inputLabel}>Yosh</Text>
            <View style={styles.inputContainer}>
              <Text style={styles.inputIcon}>🎂</Text>
              <TextInput
                style={styles.input}
                placeholder="Yoshingiz (16-100)"
                placeholderTextColor={Colors.textTertiary}
                value={age}
                onChangeText={setAge}
                keyboardType="number-pad"
                maxLength={3}
              />
            </View>
          </View>

          {/* Telefon raqam */}
          <View style={styles.inputWrapper}>
            <Text style={styles.inputLabel}>{t.auth.phone}</Text>
            <View style={styles.inputContainer}>
              <Text style={styles.inputIcon}>📱</Text>
              <TextInput
                style={styles.input}
                placeholder={t.auth.phonePlaceholder}
                placeholderTextColor={Colors.textTertiary}
                value={phoneNumber}
                onChangeText={setPhoneNumber}
                keyboardType="phone-pad"
              />
            </View>
          </View>

          {/* Parol */}
          <View style={styles.inputWrapper}>
            <Text style={styles.inputLabel}>{t.auth.password}</Text>
            <View style={styles.inputContainer}>
              <Text style={styles.inputIcon}>🔒</Text>
              <TextInput
                style={styles.input}
                placeholder={t.auth.password}
                placeholderTextColor={Colors.textTertiary}
                value={password}
                onChangeText={setPassword}
                secureTextEntry={!showPassword}
              />
              <TouchableOpacity
                onPress={() => setShowPassword(!showPassword)}
                style={styles.eyeButton}
              >
                <Text style={styles.eyeIcon}>{showPassword ? '🙈' : '👁️'}</Text>
              </TouchableOpacity>
            </View>
          </View>

          {/* Register Button */}
          <TouchableOpacity
            onPress={handleRegister}
            disabled={isLoading}
            activeOpacity={0.85}
            style={styles.registerButtonWrapper}
          >
            <LinearGradient
              colors={['#15803D', '#16A34A']}
              start={{ x: 0, y: 0 }}
              end={{ x: 1, y: 0 }}
              style={[styles.registerButton, isLoading && styles.buttonDisabled]}
            >
              {isLoading ? (
                <ActivityIndicator color={Colors.white} size="small" />
              ) : (
                <Text style={styles.registerButtonText}>{t.auth.registerButton}</Text>
              )}
            </LinearGradient>
          </TouchableOpacity>

          {/* Footer */}
          <View style={styles.footer}>
            <Text style={styles.footerText}>{t.auth.haveAccount} </Text>
            <TouchableOpacity onPress={() => router.push('/auth/login')}>
              <Text style={styles.loginText}>{t.auth.loginButton}</Text>
            </TouchableOpacity>
          </View>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  gradientHeader: {
    paddingTop: 56,
    paddingBottom: 40,
    alignItems: 'center',
    gap: Spacing.sm,
  },
  logoContainer: {
    width: 64,
    height: 64,
    borderRadius: BorderRadius.xl,
    backgroundColor: 'rgba(255,255,255,0.2)',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: Spacing.xs,
    borderWidth: 2,
    borderColor: 'rgba(255,255,255,0.4)',
  },
  logoImage: {
  width: 56,
  height: 56,
  borderRadius: 16,
},
  appName: {
    fontSize: FontSize.xxl,
    fontWeight: FontWeight.extraBold,
    color: Colors.white,
    letterSpacing: 1,
  },
  appTagline: {
    fontSize: FontSize.sm,
    color: 'rgba(255,255,255,0.85)',
    textAlign: 'center',
    paddingHorizontal: Spacing.xxxl,
  },
  scrollContent: {
    flexGrow: 1,
    paddingBottom: Spacing.xxxl,
  },
  card: {
    backgroundColor: Colors.white,
    borderTopLeftRadius: 32,
    borderTopRightRadius: 32,
    marginTop: -24,
    paddingHorizontal: Spacing.xl,
    paddingTop: Spacing.xxl,
    paddingBottom: Spacing.xl,
    ...Shadow.lg,
  },
  title: {
    fontSize: FontSize.xxl,
    fontWeight: FontWeight.bold,
    color: Colors.textPrimary,
    marginBottom: Spacing.xs,
  },
  subtitle: {
    fontSize: FontSize.md,
    color: '#6B7280',
    marginBottom: Spacing.lg,
  },
  roleContainer: {
    flexDirection: 'row',
    gap: Spacing.md,
    marginBottom: Spacing.lg,
  },
  roleButton: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: Spacing.sm,
    paddingVertical: Spacing.md,
    borderRadius: BorderRadius.lg,
    borderWidth: 1.5,
    borderColor: Colors.border,
    backgroundColor: Colors.background,
  },
  roleButtonActive: {
    borderColor: Colors.primary,
    backgroundColor: Colors.primaryLight,
  },
  roleIcon: {
    fontSize: 18,
  },
  roleText: {
    fontSize: FontSize.md,
    fontWeight: FontWeight.semiBold,
    color: '#6B7280',
  },
  roleTextActive: {
    color: Colors.primary,
  },
  row: {
    flexDirection: 'row',
    gap: Spacing.md,
  },
  inputWrapper: {
    gap: Spacing.xs,
    marginBottom: Spacing.md,
  },
  inputLabel: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.semiBold,
    color: Colors.textPrimary,
    marginLeft: Spacing.xs,
  },
  inputContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: Colors.background,
    borderRadius: BorderRadius.lg,
    borderWidth: 1.5,
    borderColor: Colors.border,
    paddingHorizontal: Spacing.lg,
    height: 54,
  },
  inputIcon: {
    fontSize: 16,
    marginRight: Spacing.sm,
  },
  input: {
    flex: 1,
    fontSize: FontSize.md,
    color: Colors.textPrimary,
  },
  eyeButton: {
    padding: Spacing.xs,
  },
  eyeIcon: {
    fontSize: 16,
  },
  registerButtonWrapper: {
    borderRadius: BorderRadius.lg,
    overflow: 'hidden',
    marginTop: Spacing.sm,
    ...Shadow.md,
  },
  registerButton: {
    height: 54,
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: BorderRadius.lg,
  },
  buttonDisabled: {
    opacity: 0.7,
  },
  registerButtonText: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.bold,
    color: Colors.white,
    letterSpacing: 0.5,
  },
  footer: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: Spacing.xl,
  },
  footerText: {
    fontSize: FontSize.md,
    color: '#6B7280',
  },
  loginText: {
    fontSize: FontSize.md,
    fontWeight: FontWeight.bold,
    color: Colors.primary,
  },
});
