import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  TextInput,
  ActivityIndicator,
  Alert,
  Image,
  KeyboardAvoidingView,
  Platform,
} from 'react-native';
import { useRouter } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import * as ImagePicker from 'expo-image-picker';
import { useThemeStore } from '../store/themeStore';
import { useAuthStore } from '../store/authStore';
import { ThemeColors, LightColors } from '../constants/colors';
import { userService } from '../services/userService';
import { UserProfile } from '../types';
import { MEDIA_URL } from '../services/api';
import {
  ArrowLeftIcon, CameraIcon, UserIcon,
  PhoneIcon, MapPinIcon, CheckIcon,
} from '../components/icons';
import { useLanguageStore } from '../stores/useLanguageStore';

interface FormData {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  age: string;
  city: string;
  country: string;
}

interface FormErrors {
  firstName?: string;
  lastName?: string;
  age?: string;
}

export default function EditProfileScreen(): React.JSX.Element {
  const router = useRouter();
  const { theme } = useThemeStore();
  const { token } = useAuthStore();
  const { t } = useLanguageStore();
  const colors = ThemeColors[theme];

  const [profile,         setProfile]         = useState<UserProfile | null>(null);
  const [loading,         setLoading]         = useState<boolean>(true);
  const [saving,          setSaving]          = useState<boolean>(false);
  const [uploadingAvatar, setUploadingAvatar] = useState<boolean>(false);
  const [avatarUri,       setAvatarUri]       = useState<string | null>(null);

  const [form, setForm] = useState<FormData>({
    firstName: '', lastName: '', phoneNumber: '', age: '', city: '', country: '',
  });

  const [errors, setErrors] = useState<FormErrors>({});

  useEffect(() => { loadProfile(); }, []);

  const loadProfile = async (): Promise<void> => {
    try {
      setLoading(true);
      const data = await userService.getProfile();
      setProfile(data);
      setForm({
        firstName:   data.firstName   ?? '',
        lastName:    data.lastName    ?? '',
        phoneNumber: data.phoneNumber ?? '',
        age:         data.age ? String(data.age) : '',
        city:        data.city        ?? '',
        country:     data.country     ?? '',
      });
      if (data.profileImageUrl) {
  setAvatarUri(`${MEDIA_URL}${data.profileImageUrl}`);
}
    } catch {
      Alert.alert(t.common.error, t.common.somethingWentWrong);
    } finally {
      setLoading(false);
    }
  };

  const validate = (): boolean => {
    const newErrors: FormErrors = {};
    if (!form.firstName.trim())           newErrors.firstName   = t.auth.firstName;
    else if (form.firstName.trim().length < 2) newErrors.firstName = t.auth.firstName;
    if (!form.lastName.trim())            newErrors.lastName    = t.auth.lastName;
    else if (form.lastName.trim().length < 2)  newErrors.lastName  = t.auth.lastName;
    if (form.age) {
      const ageNum = parseInt(form.age, 10);
      if (isNaN(ageNum) || ageNum < 16 || ageNum > 80)
        newErrors.age = t.common.error;
    }
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async (): Promise<void> => {
    if (!validate()) return;
    try {
      setSaving(true);
      await userService.updateProfile({
        firstName:   form.firstName.trim(),
        lastName:    form.lastName.trim(),
        age:         form.age ? parseInt(form.age, 10) : undefined,
        city:        form.city.trim()    || undefined,
        country:     form.country.trim() || undefined,
      });
      Alert.alert(t.common.success, t.editProfile.successMessage, [
        { text: t.common.ok, onPress: () => router.back() },
      ]);
    } catch {
      Alert.alert(t.common.error, t.common.somethingWentWrong);
    } finally {
      setSaving(false);
    }
  };

  const handlePickAvatar = async (): Promise<void> => {
    const permission = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (!permission.granted) {
      Alert.alert(t.common.error, t.editProfile.avatar);
      return;
    }
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsEditing: true,
      aspect: [1, 1],
      quality: 0.8,
    });
    if (!result.canceled && result.assets[0]) {
      const asset = result.assets[0];
      setAvatarUri(asset.uri);
      await uploadAvatar(asset.uri);
    }
  };

  const uploadAvatar = async (uri: string): Promise<void> => {
    try {
      setUploadingAvatar(true);
      const filename = uri.split('/').pop() ?? 'avatar.jpg';
      const match = /\.(\w+)$/.exec(filename);
      const type = match ? `image/${match[1]}` : 'image/jpeg';
      await userService.uploadAvatar(uri, filename, type);
    } catch {
      Alert.alert(t.common.error, t.common.somethingWentWrong);
      setAvatarUri(profile?.profileImageUrl ?? null);
    } finally {
      setUploadingAvatar(false);
    }
  };

  const handleChange = (field: keyof FormData, value: string): void => {
    setForm(prev => ({ ...prev, [field]: value }));
    if (errors[field as keyof FormErrors])
      setErrors(prev => ({ ...prev, [field]: undefined }));
  };

  const styles = createStyles(colors, theme);

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={colors.primary} />
      </View>
    );
  }

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <LinearGradient
        colors={theme === 'dark' ? ['#14532d', '#166534'] : ['#16A34A', '#15803d']}
        style={styles.header}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
      >
        <TouchableOpacity style={styles.backButton} onPress={() => router.back()} activeOpacity={0.7}>
          <ArrowLeftIcon size={22} color="#ffffff" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>{t.editProfile.title}</Text>
        <TouchableOpacity
          style={[styles.saveButton, saving && styles.saveButtonDisabled]}
          onPress={handleSave}
          disabled={saving}
          activeOpacity={0.8}
        >
          {saving
            ? <ActivityIndicator size="small" color="#ffffff" />
            : <CheckIcon size={20} color="#ffffff" />}
        </TouchableOpacity>
      </LinearGradient>

      <ScrollView
        style={styles.scrollView}
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
        {/* Avatar */}
        <View style={styles.avatarSection}>
          <View style={styles.avatarWrapper}>
            {uploadingAvatar ? (
              <View style={[styles.avatarPlaceholder, styles.avatarLoading]}>
                <ActivityIndicator size="large" color={colors.primary} />
              </View>
            ) : avatarUri ? (
              <Image source={{ uri: avatarUri }} style={styles.avatar} resizeMode="cover" />
            ) : (
              <View style={styles.avatarPlaceholder}>
                <UserIcon size={48} color={colors.textSecondary} />
              </View>
            )}
            <TouchableOpacity
              style={styles.cameraButton}
              onPress={handlePickAvatar}
              activeOpacity={0.85}
              disabled={uploadingAvatar}
            >
              <LinearGradient colors={['#16A34A', '#15803d']} style={styles.cameraGradient}>
                <CameraIcon size={16} color="#ffffff" />
              </LinearGradient>
            </TouchableOpacity>
          </View>
          <Text style={styles.avatarHint}>{t.editProfile.avatar}</Text>
        </View>

        {/* Form */}
        <View style={styles.formSection}>

          {/* Shaxsiy */}
          <View style={styles.sectionHeader}>
            <UserIcon size={16} color={colors.primary} />
            <Text style={styles.sectionTitle}>{t.editProfile.firstName}</Text>
          </View>

          <View style={styles.row}>
            <View style={styles.halfField}>
              <Text style={styles.label}>{t.editProfile.firstName} <Text style={styles.required}>*</Text></Text>
              <TextInput
                style={[styles.input, errors.firstName ? styles.inputError : null]}
                value={form.firstName}
                onChangeText={v => handleChange('firstName', v)}
                placeholder={t.editProfile.firstName}
                placeholderTextColor={colors.textSecondary}
                autoCapitalize="words"
              />
              {errors.firstName ? <Text style={styles.errorText}>{errors.firstName}</Text> : null}
            </View>
            <View style={styles.halfField}>
              <Text style={styles.label}>{t.editProfile.lastName} <Text style={styles.required}>*</Text></Text>
              <TextInput
                style={[styles.input, errors.lastName ? styles.inputError : null]}
                value={form.lastName}
                onChangeText={v => handleChange('lastName', v)}
                placeholder={t.editProfile.lastName}
                placeholderTextColor={colors.textSecondary}
                autoCapitalize="words"
              />
              {errors.lastName ? <Text style={styles.errorText}>{errors.lastName}</Text> : null}
            </View>
          </View>

          <View style={styles.field}>
            <Text style={styles.label}>{t.common.noData}</Text>
            <TextInput
              style={[styles.input, errors.age ? styles.inputError : null]}
              value={form.age}
              onChangeText={v => handleChange('age', v)}
              placeholder="16-80"
              placeholderTextColor={colors.textSecondary}
              keyboardType="number-pad"
              maxLength={2}
            />
            {errors.age ? <Text style={styles.errorText}>{errors.age}</Text> : null}
          </View>

          {/* Aloqa */}
          <View style={[styles.sectionHeader, { marginTop: 20 }]}>
            <PhoneIcon size={16} color={colors.primary} />
            <Text style={styles.sectionTitle}>{t.auth.phone}</Text>
          </View>

          <View style={styles.field}>
            <Text style={styles.label}>{t.auth.phone}</Text>
            <TouchableOpacity
              style={styles.readOnlyField}
              onPress={() => router.push('/change-phone')}
              activeOpacity={0.7}
            >
              <Text style={styles.readOnlyValue}>{form.phoneNumber}</Text>
              <Text style={styles.changeLink}>{t.common.edit}</Text>
            </TouchableOpacity>
          </View>

          {/* Joylashuv */}
          <View style={[styles.sectionHeader, { marginTop: 20 }]}>
            <MapPinIcon size={16} color={colors.primary} />
            <Text style={styles.sectionTitle}>{t.job.location}</Text>
          </View>

          <View style={styles.row}>
            <View style={styles.halfField}>
              <Text style={styles.label}>{t.common.noData}</Text>
              <TextInput
                style={styles.input}
                value={form.city}
                onChangeText={v => handleChange('city', v)}
                placeholder="Toshkent"
                placeholderTextColor={colors.textSecondary}
                autoCapitalize="words"
              />
            </View>
            <View style={styles.halfField}>
              <Text style={styles.label}>{t.common.noData}</Text>
              <TextInput
                style={styles.input}
                value={form.country}
                onChangeText={v => handleChange('country', v)}
                placeholder="O'zbekiston"
                placeholderTextColor={colors.textSecondary}
                autoCapitalize="words"
              />
            </View>
          </View>
        </View>

        {/* Save Button */}
        <TouchableOpacity
          style={[styles.primaryButton, saving && styles.primaryButtonDisabled]}
          onPress={handleSave}
          disabled={saving}
          activeOpacity={0.85}
        >
          <LinearGradient
            colors={saving ? ['#86efac', '#86efac'] : ['#16A34A', '#15803d']}
            style={styles.primaryButtonGradient}
            start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }}
          >
            {saving ? (
              <ActivityIndicator size="small" color="#ffffff" />
            ) : (
              <>
                <CheckIcon size={18} color="#ffffff" />
                <Text style={styles.primaryButtonText}>{t.editProfile.save}</Text>
              </>
            )}
          </LinearGradient>
        </TouchableOpacity>

        <View style={{ height: 40 }} />
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

function createStyles(colors: typeof LightColors, theme: 'light' | 'dark') {
  return StyleSheet.create({
    container:        { flex: 1, backgroundColor: colors.background },
    loadingContainer: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.background },
    header: {
      flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
      paddingTop: Platform.OS === 'ios' ? 56 : 48, paddingBottom: 16, paddingHorizontal: 16,
    },
    backButton: {
      width: 40, height: 40, borderRadius: 20,
      backgroundColor: 'rgba(255,255,255,0.15)',
      justifyContent: 'center', alignItems: 'center',
    },
    headerTitle:        { fontSize: 18, fontWeight: '700', color: '#ffffff', flex: 1, textAlign: 'center', marginHorizontal: 8 },
    saveButton: {
      width: 40, height: 40, borderRadius: 20,
      backgroundColor: 'rgba(255,255,255,0.2)',
      justifyContent: 'center', alignItems: 'center',
    },
    saveButtonDisabled: { opacity: 0.6 },
    scrollView:         { flex: 1 },
    scrollContent:      { paddingBottom: 24 },
    avatarSection: {
      alignItems: 'center', paddingVertical: 28,
      borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: colors.border,
    },
    avatarWrapper:      { position: 'relative', marginBottom: 10 },
    avatar:             { width: 100, height: 100, borderRadius: 50, borderWidth: 3, borderColor: colors.primary },
    avatarPlaceholder: {
      width: 100, height: 100, borderRadius: 50,
      backgroundColor: theme === 'dark' ? '#1e3a2a' : '#f0fdf4',
      borderWidth: 2, borderColor: colors.border,
      justifyContent: 'center', alignItems: 'center',
    },
    avatarLoading:      { borderColor: colors.primary },
    cameraButton:       { position: 'absolute', bottom: 0, right: 0 },
    cameraGradient: {
      width: 32, height: 32, borderRadius: 16,
      justifyContent: 'center', alignItems: 'center',
      borderWidth: 2.5, borderColor: colors.background,
    },
    avatarHint:         { fontSize: 13, color: colors.textSecondary },
    formSection:        { paddingHorizontal: 16, paddingTop: 24 },
    sectionHeader:      { flexDirection: 'row', alignItems: 'center', gap: 8, marginBottom: 14 },
    sectionTitle:       { fontSize: 14, fontWeight: '600', color: colors.primary, textTransform: 'uppercase', letterSpacing: 0.8 },
    field:              { marginBottom: 14 },
    row:                { flexDirection: 'row', gap: 12, marginBottom: 14 },
    halfField:          { flex: 1 },
    label:              { fontSize: 13, fontWeight: '500', color: colors.textSecondary, marginBottom: 6 },
    required:           { color: '#ef4444' },
    input: {
      height: 48, borderRadius: 12, borderWidth: 1.5, borderColor: colors.border,
      backgroundColor: theme === 'dark' ? '#111827' : '#f9fafb',
      paddingHorizontal: 14, fontSize: 15, color: colors.textPrimary,
    },
    inputError:         { borderColor: '#ef4444', backgroundColor: theme === 'dark' ? '#1c0a0a' : '#fff5f5' },
    errorText:          { fontSize: 12, color: '#ef4444', marginTop: 4, marginLeft: 2 },
    readOnlyField: {
      height: 48, borderRadius: 12, borderWidth: 1.5, borderColor: colors.border,
      backgroundColor: theme === 'dark' ? '#111827' : '#f9fafb',
      paddingHorizontal: 14, flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    },
    readOnlyValue:      { fontSize: 15, color: colors.textPrimary },
    changeLink:         { fontSize: 13, fontWeight: '600', color: colors.primary },
    primaryButton:      { marginHorizontal: 16, marginTop: 28, borderRadius: 14, overflow: 'hidden' },
    primaryButtonDisabled: { opacity: 0.7 },
    primaryButtonGradient: { height: 52, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: 8 },
    primaryButtonText:  { fontSize: 16, fontWeight: '700', color: '#ffffff', letterSpacing: 0.3 },
  });
}
