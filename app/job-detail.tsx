import React, { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, ScrollView, TouchableOpacity,
  ActivityIndicator, Modal, TextInput, Alert,
} from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { FontSize, FontWeight } from '../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../constants/spacing';
import { useThemeStore } from '../store/themeStore';
import { useAuthStore, AuthState } from '../store/authStore';
import {
  HeartIcon, MapPinIcon, ClockIcon,
  InsuranceIcon, TrainingIcon, TransportIcon,
  FoodIcon, CheckCircleIcon, MoneyIcon,
} from '../components/icons';
import Svg, { Path } from 'react-native-svg';
import { jobService } from '../services/jobService';
import { jobApplicationService } from '../services/jobApplicationService';
import { Job, UserRole } from '../types';
import { JobDetailSkeleton } from '../components/SkeletonLoader';
import * as DocumentPicker from 'expo-document-picker';
import { userService } from '../services/userService';

function BackIcon({ size = 24, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M19 12H5M5 12L12 19M5 12L12 5" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

const DEFAULT_REQUIREMENTS = [
  'Kamida 2 yil ish tajribasi',
  'Jamoada ishlash qobiliyati',
  "Mas'uliyatlilik va puxtalik",
  "O'z vaqtida vazifalarni bajarish",
];

const benefits = [
  { icon: 'insurance', label: "Sug'urta" },
  { icon: 'training',  label: "O'qitish" },
  { icon: 'transport', label: 'Transport' },
  { icon: 'food',      label: 'Ovqat' },
];

function BenefitIcon({ name, color }: { name: string; color: string }) {
  const props = { size: 20, color };
  switch (name) {
    case 'insurance': return <InsuranceIcon {...props} />;
    case 'training':  return <TrainingIcon  {...props} />;
    case 'transport': return <TransportIcon {...props} />;
    case 'food':      return <FoodIcon      {...props} />;
    default:          return null;
  }
}

const JOB_TYPE_LABEL: Record<string, string> = {
  FullTime: 'Full-time', PartTime: 'Part-time',
  Remote: 'Remote', Contract: 'Contract', Internship: 'Internship',
};

export default function JobDetailScreen() {
  const { colors, isDark } = useThemeStore();
  const { id } = useLocalSearchParams<{ id: string }>();
  const role       = useAuthStore((state: AuthState) => state.role);
  const isEmployer = Number(role) === UserRole.Employer;

  const [job,          setJob]          = useState<Job | null>(null);
  const [loading,      setLoading]      = useState(true);
  const [error,        setError]        = useState<string | null>(null);
  const [modalVisible, setModalVisible] = useState(false);
  const [applying,     setApplying]     = useState(false);

  // Worker state
  const [coverLetter,  setCoverLetter]  = useState('');
  const [experience,   setExperience]   = useState('');
  const [workerPhone,  setWorkerPhone]  = useState('');

  const [cvUri,        setCvUri]        = useState<string | null>(null);
const [cvName,       setCvName]       = useState<string | null>(null);
const [uploadingCv,  setUploadingCv]  = useState(false);

  // Employer state
  const [budget,   setBudget]   = useState('');
  const [address,  setAddress]  = useState('');
  const [deadline, setDeadline] = useState('');

  useEffect(() => {
    const load = async () => {
      try {
        setLoading(true);
        const data = await jobService.getJobById(id);
        setJob(data);
      } catch (e: any) {
        setError(e?.message ?? 'Xatolik yuz berdi');
      } finally {
        setLoading(false);
      }
    };
    if (id) load();
  }, [id]);

  const resetForm = () => {
    setCoverLetter(''); setExperience(''); setWorkerPhone('');
    setBudget(''); setAddress(''); setDeadline('');
    setCvUri(null); setCvName(null);
  };

  const handlePickCv = async () => {
  const result = await DocumentPicker.getDocumentAsync({
    type: ['application/pdf', 'application/msword',
           'application/vnd.openxmlformats-officedocument.wordprocessingml.document'],
    copyToCacheDirectory: true,
  });
  if (result.canceled) return;
  setCvUri(result.assets[0].uri);
  setCvName(result.assets[0].name);
};

  const handleApply = async () => {
    let fullCoverLetter = '';

    if (isEmployer) {
      if (!address.trim()) {
        Alert.alert('Xatolik', "Manzilni kiriting");
        return;
      }
      fullCoverLetter = `Byudjet: ${budget || 'Kelishiladi'}\nManzil: ${address}\nMuddat: ${deadline || 'Kelishiladi'}`;
    } else {
      if (!coverLetter.trim()) {
        Alert.alert('Xatolik', "O'zingiz haqida yozing");
        return;
      }
      fullCoverLetter = `${coverLetter}\nTajriba: ${experience || 'Kiritilmagan'}\nTelefon: ${workerPhone || 'Kiritilmagan'}`;
    }

    try {
      setApplying(true);
      if (!isEmployer && cvUri && cvName) {
  setUploadingCv(true);
  try {
    await userService.uploadCv(cvUri, cvName);
  } catch (_) {}
  setUploadingCv(false);
}
      await jobApplicationService.apply(id, fullCoverLetter);
      setModalVisible(false);
      resetForm();
      Alert.alert('Muvaffaqiyat', 'Arizangiz yuborildi!');
    } catch (e: any) {
      Alert.alert('Xatolik', e?.message ?? 'Ariza yuborishda xatolik');
    } finally {
      setApplying(false);
    }
  };

  if (loading) {
  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <JobDetailSkeleton />
    </View>
  );
}

  if (error || !job) {
    return (
      <View style={[styles.container, styles.centerBox, { backgroundColor: colors.background }]}>
        <Text style={{ color: '#EF4444', fontSize: FontSize.sm }}>{error ?? 'Ish topilmadi'}</Text>
        <TouchableOpacity onPress={() => router.back()} style={[styles.retryBtn, { backgroundColor: '#16A34A' }]}>
          <Text style={styles.retryText}>Orqaga</Text>
        </TouchableOpacity>
      </View>
    );
  }

  const salary  = job.salary ? `${(job.salary / 1_000_000).toFixed(1)}M so'm` : 'Kelishiladi';
  const jobType = JOB_TYPE_LABEL[job.status as any] ?? 'Full-time';

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>

      {/* Gradient Header */}
      <LinearGradient
        colors={isDark ? ['#14532D', '#15803D'] : ['#15803D', '#22C55E']}
        start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }}
        style={styles.gradientHeader}
      >
        <View style={styles.headerTop}>
          <TouchableOpacity style={styles.headerButton} onPress={() => router.back()}>
            <BackIcon size={22} color="#FFFFFF" />
          </TouchableOpacity>
          <TouchableOpacity style={styles.headerButton}>
            <HeartIcon size={22} color="#FFFFFF" />
          </TouchableOpacity>
        </View>
        <View style={styles.companyLogoLarge}>
          <Text style={styles.companyLogoText}>
            {(job.employerName ?? job.title ?? '?')[0].toUpperCase()}
          </Text>
        </View>
        <Text style={styles.jobTitleHeader}>{job.title}</Text>
        <Text style={styles.companyNameHeader}>{job.employerName}</Text>
        <View style={styles.headerBadges}>
          {job.location && (
            <View style={styles.headerBadge}>
              <MapPinIcon size={12} color="#FFFFFF" />
              <Text style={styles.headerBadgeText}>{job.location}</Text>
            </View>
          )}
          <View style={styles.headerBadge}>
            <ClockIcon size={12} color="#FFFFFF" />
            <Text style={styles.headerBadgeText}>{jobType}</Text>
          </View>
        </View>
      </LinearGradient>

      <ScrollView showsVerticalScrollIndicator={false}>
        {/* Salary Card */}
        <View style={[styles.salaryCard, { backgroundColor: colors.card, ...Shadow.md }]}>
          <View style={styles.salaryItem}>
            <MoneyIcon size={20} color={colors.primary} />
            <Text style={[styles.salaryLabel, { color: colors.textTertiary }]}>Maosh</Text>
            <Text style={[styles.salaryValue, { color: colors.primary }]}>{salary}</Text>
          </View>
          <View style={[styles.salaryDivider, { backgroundColor: colors.border }]} />
          <View style={styles.salaryItem}>
            <ClockIcon size={20} color={colors.textTertiary} />
            <Text style={[styles.salaryLabel, { color: colors.textTertiary }]}>Ish turi</Text>
            <Text style={[styles.salaryValue, { color: colors.textPrimary }]}>{jobType}</Text>
          </View>
          <View style={[styles.salaryDivider, { backgroundColor: colors.border }]} />
          <View style={styles.salaryItem}>
            <MapPinIcon size={20} color={colors.textTertiary} />
            <Text style={[styles.salaryLabel, { color: colors.textTertiary }]}>Joylashuv</Text>
            <Text style={[styles.salaryValue, { color: colors.textPrimary }]} numberOfLines={1}>
              {job.location ?? "Noma'lum"}
            </Text>
          </View>
        </View>

        {/* Description */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>Ish haqida</Text>
          <Text style={[styles.description, { color: colors.textSecondary }]}>
            {job.description ?? "Ma'lumot kiritilmagan."}
          </Text>
        </View>

        {/* Requirements */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>Talablar</Text>
          {DEFAULT_REQUIREMENTS.map((req, index) => (
            <View key={index} style={styles.requirementItem}>
              <CheckCircleIcon size={18} color={colors.primary} />
              <Text style={[styles.requirementText, { color: colors.textSecondary }]}>{req}</Text>
            </View>
          ))}
        </View>

        {/* Benefits */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>Imtiyozlar</Text>
          <View style={styles.benefitsGrid}>
            {benefits.map((benefit, index) => (
              <View key={index} style={[styles.benefitCard, { backgroundColor: colors.primaryLight }]}>
                <BenefitIcon name={benefit.icon} color={colors.primary} />
                <Text style={[styles.benefitText, { color: colors.primary }]}>{benefit.label}</Text>
              </View>
            ))}
          </View>
        </View>

        <View style={{ height: 100 }} />
      </ScrollView>

      {/* Apply Button */}
      <View style={[styles.bottomBar, { backgroundColor: colors.surface, ...Shadow.lg }]}>
        <TouchableOpacity
          style={styles.applyButtonWrapper}
          activeOpacity={0.85}
          onPress={() => setModalVisible(true)}
        >
          <LinearGradient
            colors={['#15803D', '#16A34A']}
            start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }}
            style={styles.applyButton}
          >
            <Text style={styles.applyButtonText}>
              {isEmployer ? 'Ishchi topish' : 'Ariza yuborish'}
            </Text>
          </LinearGradient>
        </TouchableOpacity>
      </View>

      <Modal visible={modalVisible} transparent animationType="slide">
  <View style={styles.modalOverlay}>
    <View style={[styles.modalContent, { backgroundColor: colors.surface }]}>
      {/* Header */}
      <View style={[styles.modalTopBar, { borderBottomColor: colors.border }]}>
        <Text style={[styles.modalTitle, { color: colors.textPrimary }]}>
          {isEmployer ? 'Ishchi topish' : 'Ariza yuborish'}
        </Text>
        <TouchableOpacity onPress={() => { setModalVisible(false); resetForm(); }}>
          <Text style={{ fontSize: 22, color: colors.textSecondary }}>✕</Text>
        </TouchableOpacity>
      </View>

      <Text style={[styles.modalSubtitle, { color: colors.textSecondary }]}>
        {job.title} — {job.employerName}
      </Text>

      <ScrollView showsVerticalScrollIndicator={false} keyboardShouldPersistTaps="handled">
        {isEmployer ? (
          <>
            <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Byudjet (so'm)</Text>
            <TextInput
              style={[styles.coverInput, { backgroundColor: colors.background, color: colors.textPrimary, borderColor: colors.border }]}
              placeholder="Masalan: 5 000 000"
              placeholderTextColor={colors.textTertiary}
              value={budget}
              onChangeText={setBudget}
              keyboardType="numeric"
            />
            <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>
              Manzil <Text style={{ color: '#EF4444' }}>*</Text>
            </Text>
            <TextInput
              style={[styles.coverInput, { backgroundColor: colors.background, color: colors.textPrimary, borderColor: colors.border }]}
              placeholder="Masalan: Toshkent, Chilonzor"
              placeholderTextColor={colors.textTertiary}
              value={address}
              onChangeText={setAddress}
            />
            <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Muddat</Text>
            <TextInput
              style={[styles.coverInput, { backgroundColor: colors.background, color: colors.textPrimary, borderColor: colors.border }]}
              placeholder="Masalan: 2 hafta"
              placeholderTextColor={colors.textTertiary}
              value={deadline}
              onChangeText={setDeadline}
            />
          </>
        ) : (
          <>
            <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>
              O'zingiz haqida <Text style={{ color: '#EF4444' }}>*</Text>
            </Text>
            <TextInput
              style={[styles.coverInput, { backgroundColor: colors.background, color: colors.textPrimary, borderColor: colors.border, minHeight: 100 }]}
              placeholder="Tajribangiz, ko'nikmalaringiz haqida yozing..."
              placeholderTextColor={colors.textTertiary}
              value={coverLetter}
              onChangeText={setCoverLetter}
              multiline
              numberOfLines={4}
              textAlignVertical="top"
            />
            <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Tajriba</Text>
            <TextInput
              style={[styles.coverInput, { backgroundColor: colors.background, color: colors.textPrimary, borderColor: colors.border }]}
              placeholder="Masalan: 2 yil"
              placeholderTextColor={colors.textTertiary}
              value={experience}
              onChangeText={setExperience}
            />
            <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Telefon</Text>
            <TextInput
              style={[styles.coverInput, { backgroundColor: colors.background, color: colors.textPrimary, borderColor: colors.border }]}
              placeholder="+998 xx xxx xx xx"
              placeholderTextColor={colors.textTertiary}
              value={workerPhone}
              onChangeText={setWorkerPhone}
              keyboardType="phone-pad"
            />
            <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>CV yuklash</Text>
            <TouchableOpacity
              style={[styles.cvPickerBtn, { backgroundColor: colors.background, borderColor: cvUri ? colors.primary : colors.border }]}
              onPress={handlePickCv}
              activeOpacity={0.8}
            >
              <Text style={[styles.cvPickerText, { color: cvUri ? colors.primary : colors.textTertiary }]}>
                {cvUri ? cvName : 'PDF yoki Word fayl tanlang'}
              </Text>
            </TouchableOpacity>
          </>
        )}

        <View style={styles.modalButtons}>
          <TouchableOpacity
            style={[styles.modalBtn, { backgroundColor: colors.background, borderColor: colors.border, borderWidth: 1 }]}
            onPress={() => { setModalVisible(false); resetForm(); }}
            activeOpacity={0.8}
          >
            <Text style={[styles.modalBtnText, { color: colors.textSecondary }]}>Bekor qilish</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.modalBtn, { backgroundColor: '#16A34A' }]}
            onPress={handleApply}
            activeOpacity={0.85}
            disabled={applying}
          >
            {applying
              ? <ActivityIndicator size="small" color="#fff" />
              : <Text style={[styles.modalBtnText, { color: '#fff' }]}>Yuborish</Text>
            }
          </TouchableOpacity>
        </View>
        <View style={{ height: 24 }} />
      </ScrollView>
    </View>
  </View>
</Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  container:   { flex: 1 },
  centerBox:   { alignItems: 'center', justifyContent: 'center', gap: 12 },
  gradientHeader: {
    paddingTop: 56, paddingBottom: Spacing.xl,
    alignItems: 'center', gap: Spacing.sm,
  },
  headerTop: {
    flexDirection: 'row', justifyContent: 'space-between',
    width: '100%', paddingHorizontal: Spacing.xl, marginBottom: Spacing.md,
  },
  headerButton: {
    width: 40, height: 40, borderRadius: BorderRadius.full,
    backgroundColor: 'rgba(255,255,255,0.2)',
    alignItems: 'center', justifyContent: 'center',
  },
  companyLogoLarge: {
    width: 72, height: 72, borderRadius: BorderRadius.xl,
    backgroundColor: 'rgba(255,255,255,0.25)',
    alignItems: 'center', justifyContent: 'center',
    borderWidth: 2, borderColor: 'rgba(255,255,255,0.4)',
    marginBottom: Spacing.sm,
  },
  companyLogoText:   { fontSize: 30, fontWeight: FontWeight.bold, color: '#FFFFFF' },
  jobTitleHeader:    { fontSize: FontSize.xl, fontWeight: FontWeight.bold, color: '#FFFFFF' },
  companyNameHeader: { fontSize: FontSize.md, color: 'rgba(255,255,255,0.85)' },
  headerBadges:      { flexDirection: 'row', gap: Spacing.md, marginTop: Spacing.sm },
  headerBadge: {
    flexDirection: 'row', alignItems: 'center', gap: 4,
    backgroundColor: 'rgba(255,255,255,0.2)', borderRadius: BorderRadius.full,
    paddingHorizontal: Spacing.md, paddingVertical: Spacing.xs,
  },
  headerBadgeText: { fontSize: FontSize.xs, color: '#FFFFFF', fontWeight: FontWeight.medium },
  salaryCard: {
    flexDirection: 'row', marginHorizontal: Spacing.xl,
    marginTop: Spacing.lg, borderRadius: BorderRadius.xl, padding: Spacing.lg,
  },
  salaryItem:    { flex: 1, alignItems: 'center', gap: 4 },
  salaryLabel:   { fontSize: FontSize.xs, fontWeight: FontWeight.medium },
  salaryValue:   { fontSize: FontSize.sm, fontWeight: FontWeight.bold },
  salaryDivider: { width: 1, marginVertical: 4 },
  section: {
    marginHorizontal: Spacing.xl, marginTop: Spacing.lg,
    borderRadius: BorderRadius.xl, padding: Spacing.lg,
  },
  sectionTitle:    { fontSize: FontSize.lg, fontWeight: FontWeight.bold, marginBottom: Spacing.md },
  description:     { fontSize: FontSize.md, lineHeight: 24 },
  requirementItem: { flexDirection: 'row', alignItems: 'center', gap: Spacing.sm, marginBottom: Spacing.sm },
  requirementText: { fontSize: FontSize.md, flex: 1 },
  benefitsGrid:    { flexDirection: 'row', flexWrap: 'wrap', gap: Spacing.md },
  benefitCard: {
    flexDirection: 'row', alignItems: 'center', gap: Spacing.sm,
    borderRadius: BorderRadius.md, paddingHorizontal: Spacing.md, paddingVertical: Spacing.sm,
  },
  benefitText: { fontSize: FontSize.sm, fontWeight: FontWeight.semiBold },
  bottomBar: {
    position: 'absolute', bottom: 0, left: 0, right: 0,
    paddingHorizontal: Spacing.xl, paddingVertical: Spacing.lg, paddingBottom: Spacing.xl,
  },
  applyButtonWrapper: { borderRadius: BorderRadius.lg, overflow: 'hidden', ...Shadow.md },
  applyButton: {
    height: 54, alignItems: 'center', justifyContent: 'center', borderRadius: BorderRadius.lg,
  },
  applyButtonText: { fontSize: FontSize.lg, fontWeight: FontWeight.bold, color: '#FFFFFF' },
  retryBtn: {
    paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md,
    borderRadius: BorderRadius.lg, marginTop: 4,
  },
  retryText: { color: '#fff', fontWeight: FontWeight.semiBold, fontSize: FontSize.sm },
  modalOverlay: {
    flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end',
  },
  modalContent: {
  borderTopLeftRadius: 24, borderTopRightRadius: 24,
  padding: Spacing.xl,
},
  modalTitle:    { fontSize: FontSize.xl, fontWeight: FontWeight.bold },
  modalSubtitle: { fontSize: FontSize.sm, marginBottom: Spacing.sm },
  coverInput: {
    borderWidth: 1.5, borderRadius: BorderRadius.lg,
    padding: Spacing.md, fontSize: FontSize.md, minHeight: 52,
  },
  modalButtons: { flexDirection: 'row', gap: Spacing.md, marginTop: Spacing.sm },
  modalBtn: {
    flex: 1, height: 48, borderRadius: BorderRadius.lg,
    alignItems: 'center', justifyContent: 'center',
  },
modalBtnText: { fontSize: FontSize.md, fontWeight: FontWeight.semiBold },
  cvPickerBtn: {
    borderWidth: 1.5, borderRadius: BorderRadius.lg, borderStyle: 'dashed',
    padding: Spacing.md, minHeight: 52, alignItems: 'center', justifyContent: 'center',
  },

  cvPickerText: { fontSize: FontSize.sm, fontWeight: FontWeight.medium },
  modalTopBar: {
    flexDirection: 'row', justifyContent: 'space-between',
    alignItems: 'center', paddingBottom: 12, borderBottomWidth: 1, marginBottom: 8,
  },
  inputLabel: {
    fontSize: 13, fontWeight: '600', marginBottom: 6, marginTop: 10,
  },
});

