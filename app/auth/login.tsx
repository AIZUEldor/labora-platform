import { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  Alert,
  Platform,
  Image,
} from 'react-native';
import { KeyboardAwareScrollView } from 'react-native-keyboard-aware-scroll-view';
import { LinearGradient } from 'expo-linear-gradient';
import { router } from 'expo-router';
import { authService } from '../../services/authService';
import { useAuthStore, AuthState } from '../../store/authStore';
import { Colors } from '../../constants/colors';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius } from '../../constants/spacing';
import { PhoneIcon, LockIcon, EyeIcon, EyeOffIcon } from '../../components/icons';
import { useLanguageStore } from '../../stores/useLanguageStore';

export default function LoginScreen() {
  const [phoneNumber, setPhoneNumber] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [phoneFocused, setPhoneFocused] = useState(false);
  const [passwordFocused, setPasswordFocused] = useState(false);

  const login = useAuthStore((state: AuthState) => state.login);
  const { t } = useLanguageStore();

  const handleLogin = async () => {
    if (!phoneNumber.trim() || !password.trim()) {
      Alert.alert('Xato', t.common.error);
      return;
    }
    setIsLoading(true);
    try {
      const response = await authService.login({
  phoneNumber: phoneNumber.trim(),
  password,
});

await login(response.token, response.role, response.firstName, response.lastName);
router.replace('/(tabs)');
    } catch (error: any) {
  const message =
    error?.response?.data?.message ||
    error?.response?.data?.title ||
    error?.message ||
    'Noma’lum xatolik';

  Alert.alert('Xato', message);
} finally {
      setIsLoading(false);
    }
  };

  return (
    <KeyboardAwareScrollView
      style={{ flex: 1, backgroundColor: Colors.background }}
      contentContainerStyle={{ flexGrow: 1 }}
      keyboardShouldPersistTaps="always"
      enableOnAndroid={true}
      extraScrollHeight={Platform.OS === 'ios' ? 20 : 80}
      showsVerticalScrollIndicator={false}
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
    resizeMode="contain"
  />
</View>
        <Text style={styles.appName}>ALP</Text>
        <Text style={styles.appTagline}>Ishingizni toping, hayotingizni o'zgartiring</Text>
      </LinearGradient>

      <View style={styles.card}>
        <Text style={styles.title}>Xush kelibsiz!</Text>
        <Text style={styles.subtitle}>{t.auth.login}</Text>

        <View style={styles.inputWrapper}>
          <Text style={styles.inputLabel}>{t.auth.phone}</Text>
          <View style={[styles.inputContainer, phoneFocused && styles.inputFocused]}>
            <PhoneIcon size={18} color={phoneFocused ? Colors.primary : Colors.textTertiary} />
            <TextInput
              style={styles.input}
              placeholder={t.auth.phonePlaceholder}
              placeholderTextColor={Colors.textTertiary}
              value={phoneNumber}
              onChangeText={setPhoneNumber}
              keyboardType="phone-pad"
              autoCapitalize="none"
              autoCorrect={false}
              onFocus={() => setPhoneFocused(true)}
              onBlur={() => setPhoneFocused(false)}
            />
          </View>
        </View>

        <View style={styles.inputWrapper}>
          <Text style={styles.inputLabel}>{t.auth.password}</Text>
          <View style={[styles.inputContainer, passwordFocused && styles.inputFocused]}>
            <LockIcon size={18} color={passwordFocused ? Colors.primary : Colors.textTertiary} />
            <TextInput
              style={styles.input}
              placeholder={t.auth.password}
              placeholderTextColor={Colors.textTertiary}
              value={password}
              onChangeText={setPassword}
              secureTextEntry={!showPassword}
              onFocus={() => setPasswordFocused(true)}
              onBlur={() => setPasswordFocused(false)}
            />
            <TouchableOpacity
              onPress={() => setShowPassword(!showPassword)}
              style={styles.eyeButton}
            >
              {showPassword
                ? <EyeOffIcon size={18} color={Colors.textTertiary} />
                : <EyeIcon size={18} color={Colors.textTertiary} />
              }
            </TouchableOpacity>
          </View>
        </View>

        <TouchableOpacity
          style={styles.forgotButton}
          onPress={() => router.push('/forgot-password')}
        >
          <Text style={styles.forgotText}>{t.auth.forgotPassword}</Text>
        </TouchableOpacity>

        <TouchableOpacity
          onPress={handleLogin}
          disabled={isLoading}
          activeOpacity={0.85}
          style={styles.loginButtonWrapper}
        >
          <LinearGradient
            colors={['#15803D', '#16A34A']}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 0 }}
            style={[styles.loginButton, isLoading && styles.loginButtonDisabled]}
          >
            {isLoading
              ? <ActivityIndicator color={Colors.white} size="small" />
              : <Text style={styles.loginButtonText}>{t.auth.loginButton}</Text>
            }
          </LinearGradient>
        </TouchableOpacity>

        <View style={styles.footer}>
          <Text style={styles.footerText}>{t.auth.noAccount} </Text>
          <TouchableOpacity onPress={() => router.push('/auth/register')}>
            <Text style={styles.registerText}>{t.auth.registerButton}</Text>
          </TouchableOpacity>
        </View>
      </View>
    </KeyboardAwareScrollView>
  );
}

const styles = StyleSheet.create({
  gradientHeader: {
    paddingTop: 64,
    paddingBottom: 48,
    alignItems: 'center',
    gap: Spacing.sm,
  },
  logoContainer: {
    width: 72,
    height: 72,
    borderRadius: BorderRadius.xl,
    backgroundColor: 'rgba(255,255,255,0.2)',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: Spacing.xs,
    borderWidth: 2,
    borderColor: 'rgba(255,255,255,0.4)',
  },
  logoText: {
    fontSize: 36,
    fontWeight: FontWeight.bold,
    color: Colors.white,
  },
  logoImage: {
  width: 72,
  height: 72,
},
  appName: {
    fontSize: FontSize.xxxl,
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
  card: {
    backgroundColor: Colors.white,
    borderTopLeftRadius: 32,
    borderTopRightRadius: 32,
    marginTop: -24,
    paddingHorizontal: Spacing.xl,
    paddingTop: Spacing.xxxl,
    paddingBottom: Spacing.xxxl,
    flex: 1,
  },
  title: {
    fontSize: FontSize.xxl,
    fontWeight: FontWeight.bold,
    color: Colors.textPrimary,
    marginBottom: Spacing.xs,
  },
  subtitle: {
    fontSize: FontSize.md,
    color: Colors.textSecondary,
    marginBottom: Spacing.xl,
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
    gap: Spacing.sm,
  },
  inputFocused: {
    borderColor: Colors.primary,
    backgroundColor: Colors.white,
  },
  input: {
    flex: 1,
    fontSize: FontSize.md,
    color: Colors.textPrimary,
    fontWeight: FontWeight.regular,
  },
  eyeButton: {
    padding: Spacing.xs,
  },
  forgotButton: {
    alignSelf: 'flex-end',
    marginBottom: Spacing.xl,
  },
  forgotText: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.semiBold,
    color: Colors.primary,
  },
  loginButtonWrapper: {
    borderRadius: BorderRadius.lg,
    overflow: 'hidden',
  },
  loginButton: {
    height: 54,
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: BorderRadius.lg,
  },
  loginButtonDisabled: {
    opacity: 0.7,
  },
  loginButtonText: {
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
    color: Colors.textSecondary,
  },
  registerText: {
    fontSize: FontSize.md,
    fontWeight: FontWeight.bold,
    color: Colors.primary,
  },
});
