import React, { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, ScrollView,
  TouchableOpacity, TextInput, ActivityIndicator, Alert,
} from 'react-native';
import { router } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { useThemeStore } from '../../store/themeStore';
import { useAuthStore, AuthState } from '../../store/authStore';
import { useLanguageStore } from '../../stores/useLanguageStore';
import Svg, { Path } from 'react-native-svg';
import { jobService } from '../../services/jobService';
import { categoryService } from '../../services/categoryService';
import { Category } from '../../types';

function BackIcon({ size = 24, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M19 12H5M5 12L12 19M5 12L12 5" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

const JOB_TYPES = ['FullTime', 'PartTime', 'Remote', 'Contract', 'Internship'];
const JOB_TYPE_LABEL: Record<string, string> = {
  FullTime: 'Full-time', PartTime: 'Part-time',
  Remote: 'Remote', Contract: 'Contract', Internship: 'Internship',
};

export default function CreateJobScreen() {
  const { colors, isDark } = useThemeStore();
  const { t }              = useLanguageStore();
  const token              = useAuthStore((state: AuthState) => state.token);

  const [title,       setTitle]       = useState('');
  const [description, setDescription] = useState('');
  const [salary,      setSalary]      = useState('');
  const [city,        setCity]        = useState('');
  const [country,     setCountry]     = useState('');
  const [categoryId,  setCategoryId]  = useState('');
  const [categoryName,setCategoryName]= useState('');
  const [jobType,     setJobType]     = useState('FullTime');
  const [categories,  setCategories]  = useState<Category[]>([]);
  const [loading,     setLoading]     = useState(false);
  const [catLoading,  setCatLoading]  = useState(true);

  useEffect(() => {
    if (!token) return;
    const load = async () => {
      try {
        const cats = await categoryService.getCategories();
        setCategories(cats);
        if (cats.length > 0) {
          setCategoryId(cats[0].id);
          setCategoryName(cats[0].name);
        }
      } finally {
        setCatLoading(false);
      }
    };
    load();
  }, [token]);

  // ── Guest mode ──────────────────────────────────────────────
  if (!token) {
    return (
      <View style={[styles.container, { backgroundColor: colors.background }]}>
        <LinearGradient
          colors={isDark ? ['#14532D', '#15803D'] : ['#15803D', '#22C55E']}
          start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }}
          style={styles.gradientHeader}
        >
          <View style={styles.headerTop}>
            <View style={{ width: 40 }} />
            <Text style={styles.headerTitle}>{t.employer.postJob}</Text>
            <View style={{ width: 40 }} />
          </View>
        </LinearGradient>

        <View style={styles.guestContainer}>
          <Text style={[styles.guestTitle, { color: colors.textPrimary }]}>
            {t.profile.loginRequired}
          </Text>
          <Text style={[styles.guestSubtitle, { color: colors.textSecondary }]}>
            {t.profile.loginToSeeProfile}
          </Text>
          <TouchableOpacity
            style={styles.loginBtn}
            onPress={() => router.push('/auth/login')}
            activeOpacity={0.85}
          >
            <LinearGradient
              colors={['#15803D', '#16A34A']}
              start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }}
              style={styles.loginBtnGradient}
            >
              <Text style={styles.loginBtnText}>{t.auth.login}</Text>
            </LinearGradient>
          </TouchableOpacity>
        </View>
      </View>
    );
  }
  // ────────────────────────────────────────────────────────────

  const handleCreate = async () => {
    if (!title.trim() || !description.trim() || !city.trim()) {
      Alert.alert(t.common.error, "Barcha majburiy maydonlarni to'ldiring");
      return;
    }
    try {
      setLoading(true);
      await jobService.createJob({
        title:        title.trim(),
        description:  description.trim(),
        salary:       salary ? Number(salary) : 0,
        jobType:      JOB_TYPES.indexOf(jobType),
        categoryId,
        categoryName,
        city:         city.trim(),
        country:      country.trim() || 'Uzbekistan',
      });
      Alert.alert(t.common.success, "Ish e'loni joylashtirildi!", [
        { text: t.common.ok, onPress: () => {
          setTitle(''); setDescription(''); setSalary('');
          setCity(''); setCountry('');
        }},
      ]);
    } catch (e: any) {
      Alert.alert(t.common.error, e?.message ?? t.common.somethingWentWrong);
    } finally {
      setLoading(false);
    }
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>

      {/* Header */}
      <LinearGradient
        colors={isDark ? ['#14532D', '#15803D'] : ['#15803D', '#22C55E']}
        start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }}
        style={styles.gradientHeader}
      >
        <View style={styles.headerTop}>
          <View style={{ width: 40 }} />
          <Text style={styles.headerTitle}>Ish e'lon qilish</Text>
          <View style={{ width: 40 }} />
        </View>
      </LinearGradient>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>

        {/* Title */}
        <View style={styles.fieldGroup}>
          <Text style={[styles.label, { color: colors.textPrimary }]}>Ish nomi *</Text>
          <TextInput
            style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
            placeholder="Masalan: Frontend Developer"
            placeholderTextColor={colors.textTertiary}
            value={title}
            onChangeText={setTitle}
          />
        </View>

        {/* Description */}
        <View style={styles.fieldGroup}>
          <Text style={[styles.label, { color: colors.textPrimary }]}>Tavsif *</Text>
          <TextInput
            style={[styles.textarea, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
            placeholder="Ish haqida batafsil yozing..."
            placeholderTextColor={colors.textTertiary}
            value={description}
            onChangeText={setDescription}
            multiline
            numberOfLines={5}
            textAlignVertical="top"
          />
        </View>

        {/* Salary */}
        <View style={styles.fieldGroup}>
          <Text style={[styles.label, { color: colors.textPrimary }]}>Maosh (so'm)</Text>
          <TextInput
            style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
            placeholder="Masalan: 3000000"
            placeholderTextColor={colors.textTertiary}
            value={salary}
            onChangeText={setSalary}
            keyboardType="numeric"
          />
        </View>

        {/* City */}
        <View style={styles.fieldGroup}>
          <Text style={[styles.label, { color: colors.textPrimary }]}>Shahar *</Text>
          <TextInput
            style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
            placeholder="Masalan: Toshkent"
            placeholderTextColor={colors.textTertiary}
            value={city}
            onChangeText={setCity}
          />
        </View>

        {/* Country */}
        <View style={styles.fieldGroup}>
          <Text style={[styles.label, { color: colors.textPrimary }]}>Mamlakat</Text>
          <TextInput
            style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
            placeholder="Masalan: Uzbekistan"
            placeholderTextColor={colors.textTertiary}
            value={country}
            onChangeText={setCountry}
          />
        </View>

        {/* Job Type */}
        <View style={styles.fieldGroup}>
          <Text style={[styles.label, { color: colors.textPrimary }]}>Ish turi</Text>
          <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.chipList}>
            {JOB_TYPES.map(type => {
              const isSelected = jobType === type;
              return (
                <TouchableOpacity
                  key={type}
                  onPress={() => setJobType(type)}
                  style={[
                    styles.chip,
                    { borderColor: colors.border, backgroundColor: colors.card },
                    isSelected && { backgroundColor: '#16A34A', borderColor: '#16A34A' },
                  ]}
                  activeOpacity={0.8}
                >
                  <Text style={[styles.chipText, { color: colors.textSecondary }, isSelected && { color: '#fff' }]}>
                    {JOB_TYPE_LABEL[type]}
                  </Text>
                </TouchableOpacity>
              );
            })}
          </ScrollView>
        </View>

        {/* Category */}
        <View style={styles.fieldGroup}>
          <Text style={[styles.label, { color: colors.textPrimary }]}>Kategoriya</Text>
          {catLoading ? (
            <ActivityIndicator color="#16A34A" />
          ) : (
            <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.chipList}>
              {categories.map(cat => {
                const isSelected = categoryId === cat.id;
                return (
                  <TouchableOpacity
                    key={cat.id}
                    onPress={() => { setCategoryId(cat.id); setCategoryName(cat.name); }}
                    style={[
                      styles.chip,
                      { borderColor: colors.border, backgroundColor: colors.card },
                      isSelected && { backgroundColor: '#16A34A', borderColor: '#16A34A' },
                    ]}
                    activeOpacity={0.8}
                  >
                    <Text style={[styles.chipText, { color: colors.textSecondary }, isSelected && { color: '#fff' }]}>
                      {cat.name}
                    </Text>
                  </TouchableOpacity>
                );
              })}
            </ScrollView>
          )}
        </View>

        <View style={{ height: 100 }} />
      </ScrollView>

      {/* Submit */}
      <View style={[styles.bottomBar, { backgroundColor: colors.surface, ...Shadow.lg }]}>
        <TouchableOpacity
          style={styles.submitWrapper}
          onPress={handleCreate}
          activeOpacity={0.85}
          disabled={loading}
        >
          <LinearGradient
            colors={['#15803D', '#16A34A']}
            start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }}
            style={styles.submitButton}
          >
            {loading
              ? <ActivityIndicator color="#fff" />
              : <Text style={styles.submitText}>E'lon qilish</Text>
            }
          </LinearGradient>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container:      { flex: 1 },
  gradientHeader: { paddingTop: 56, paddingBottom: Spacing.lg },
  headerTop: {
    flexDirection: 'row', alignItems: 'center',
    justifyContent: 'space-between', paddingHorizontal: Spacing.xl,
  },
  headerTitle:   { fontSize: FontSize.lg, fontWeight: FontWeight.bold, color: '#fff' },
  content:       { padding: Spacing.xl, gap: Spacing.lg },
  fieldGroup:    { gap: Spacing.sm },
  label:         { fontSize: FontSize.sm, fontWeight: FontWeight.semiBold },
  input: {
    borderWidth: 1.5, borderRadius: BorderRadius.lg,
    paddingHorizontal: Spacing.md, height: 52, fontSize: FontSize.md,
  },
  textarea: {
    borderWidth: 1.5, borderRadius: BorderRadius.lg,
    paddingHorizontal: Spacing.md, paddingVertical: Spacing.md,
    fontSize: FontSize.md, minHeight: 120,
  },
  chipList:      { gap: Spacing.sm, paddingVertical: 2 },
  chip: {
    paddingHorizontal: Spacing.lg, paddingVertical: Spacing.sm,
    borderRadius: BorderRadius.full, borderWidth: 1.5,
  },
  chipText:      { fontSize: FontSize.sm, fontWeight: FontWeight.medium },
  bottomBar: {
    position: 'absolute', bottom: 0, left: 0, right: 0,
    paddingHorizontal: Spacing.xl, paddingVertical: Spacing.lg, paddingBottom: Spacing.xl,
  },
  submitWrapper: { borderRadius: BorderRadius.lg, overflow: 'hidden', ...Shadow.md },
  submitButton:  { height: 54, alignItems: 'center', justifyContent: 'center', borderRadius: BorderRadius.lg },
  submitText:    { fontSize: FontSize.lg, fontWeight: FontWeight.bold, color: '#fff' },
  // Guest styles
  guestContainer: {
    flex: 1, alignItems: 'center', justifyContent: 'center',
    paddingHorizontal: Spacing.xl, gap: Spacing.lg,
  },
  guestTitle:       { fontSize: FontSize.xl, fontWeight: FontWeight.bold, textAlign: 'center' },
  guestSubtitle:    { fontSize: FontSize.md, textAlign: 'center', lineHeight: 22 },
  loginBtn:         { width: '100%', borderRadius: BorderRadius.xl, overflow: 'hidden', marginTop: Spacing.sm },
  loginBtnGradient: { height: 52, alignItems: 'center', justifyContent: 'center' },
  loginBtnText:     { fontSize: FontSize.md, fontWeight: FontWeight.bold, color: '#FFFFFF' },
});
