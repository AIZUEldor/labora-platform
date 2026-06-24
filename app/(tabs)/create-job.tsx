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
import Svg, { Path } from 'react-native-svg';
import { jobService } from '../../services/jobService';
import { categoryService } from '../../services/categoryService';
import { Category } from '../../types';
import { useAuthStore, AuthState } from '../../store/authStore';

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
  
  const token = useAuthStore((state: AuthState) => state.token);
  const [title,       setTitle]       = useState('');
  const [description, setDescription] = useState('');
  const [salary,      setSalary]      = useState('');
  const [location,    setLocation]    = useState('');
  const [categoryId,  setCategoryId]  = useState('');
  const [jobType,     setJobType]     = useState('FullTime');
  const [categories,  setCategories]  = useState<Category[]>([]);
  const [loading,     setLoading]     = useState(false);
  const [catLoading,  setCatLoading]  = useState(true);

  useEffect(() => {
    const load = async () => {

      try {
        const cats = await categoryService.getCategories();
        setCategories(cats);
        if (cats.length > 0) setCategoryId(cats[0].id);
      } finally {
        setCatLoading(false);
      }
    };
    load();
  }, []);

  const handleCreate = async () => {
    if (!title.trim() || !description.trim() || !location.trim()) {
      Alert.alert('Xatolik', "Barcha majburiy maydonlarni to'ldiring");
      return;
    }
    try {
      setLoading(true);
      await jobService.createJob({
  title: title.trim(),
  description: description.trim(),
  salary: salary ? Number(salary) : 0,
  jobType: 0,
  categoryId,
  city: location.trim(),
  country: 'Uzbekistan',
});
      Alert.alert('Muvaffaqiyat', "Ish e'loni joylashtirildi!", [
        { text: 'OK', onPress: () => { setTitle(''); setDescription(''); setSalary(''); setLocation(''); } },
      ]);
    } catch (e: any) {
      Alert.alert('Xatolik', e?.message ?? 'Xatolik yuz berdi');
    } finally {
      setLoading(false);
    }
  };

  if (!token) {
  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center', padding: Spacing.xl }}>
        <Text style={{ fontSize: FontSize.xl, fontWeight: FontWeight.bold, color: colors.textPrimary, marginBottom: Spacing.sm }}>
          Login Required
        </Text>

        <Text style={{ fontSize: FontSize.md, color: colors.textSecondary, textAlign: 'center', marginBottom: Spacing.xl }}>
          Please login to post a job vacancy.
        </Text>

        <TouchableOpacity
          style={{ backgroundColor: colors.primary, paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md, borderRadius: BorderRadius.lg }}
          onPress={() => router.push('/auth/login')}
          activeOpacity={0.85}
        >
          <Text style={{ color: '#FFFFFF', fontSize: FontSize.md, fontWeight: FontWeight.bold }}>
            Login
          </Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

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

        {/* Location */}
        <View style={styles.fieldGroup}>
          <Text style={[styles.label, { color: colors.textPrimary }]}>Joylashuv *</Text>
          <TextInput
            style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
            placeholder="Masalan: Toshkent"
            placeholderTextColor={colors.textTertiary}
            value={location}
            onChangeText={setLocation}
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
                    onPress={() => setCategoryId(cat.id)}
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
});