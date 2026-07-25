import React, { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, ScrollView, TouchableOpacity, Image,
} from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { useThemeStore } from '../../store/themeStore';
import { MapPinIcon, ClockIcon, MoneyIcon, ApplicationsIcon } from '../../components/icons';
import Svg, { Path } from 'react-native-svg';
import { jobService } from '../../services/jobService';
import { Job } from '../../types';
import { JobDetailSkeleton } from '../../components/SkeletonLoader';
import { useLanguageStore } from '../../stores/useLanguageStore';
import { MEDIA_URL } from '../../services/api';

function BackIcon({ size = 24, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M19 12H5M5 12L12 19M5 12L12 5" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

const JOB_TYPES = [
  { value: 1, labelUz: 'Kunlik',    labelRu: 'Ежедневная',  labelEn: 'Daily' },
  { value: 2, labelUz: 'Mavsumiy',  labelRu: 'Сезонная',    labelEn: 'Seasonal' },
  { value: 3, labelUz: 'Oylik',     labelRu: 'Ежемесячная', labelEn: 'Monthly' },
  { value: 4, labelUz: 'Part-time', labelRu: 'Part-time',   labelEn: 'Part-time' },
  { value: 5, labelUz: 'Full-time', labelRu: 'Full-time',   labelEn: 'Full-time' },
  { value: 6, labelUz: 'Masofaviy', labelRu: 'Удалённая',   labelEn: 'Remote' },
];

function getJobTypeLabel(value: number, language: string): string {
  const found = JOB_TYPES.find(j => j.value === value);
  if (!found) return '';
  return language === 'uz' ? found.labelUz : language === 'ru' ? found.labelRu : found.labelEn;
}

export default function ManageJobScreen() {
  const { colors, isDark } = useThemeStore();
  const { t, language } = useLanguageStore();
  const { jobId } = useLocalSearchParams<{ jobId: string }>();

  const [job,     setJob]     = useState<Job | null>(null);
  const [loading, setLoading] = useState(true);
  const [error,   setError]   = useState<string | null>(null);

  useEffect(() => {
    if (!jobId) {
      setError(t.common.somethingWentWrong);
      setLoading(false);
      return;
    }
    const load = async () => {
      try {
        setLoading(true);
        const data = await jobService.getJobById(jobId);
        setJob(data);
        setError(null);
      } catch (e: any) {
        setError(e?.message ?? t.common.somethingWentWrong);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [jobId]);

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
        <Text style={{ color: '#EF4444', fontSize: FontSize.sm }}>{error ?? t.common.noData}</Text>
        <TouchableOpacity onPress={() => router.back()} style={[styles.retryBtn, { backgroundColor: '#16A34A' }]}>
          <Text style={styles.retryText}>{t.common.back}</Text>
        </TouchableOpacity>
      </View>
    );
  }

  const salary  = job.salary ? `${(job.salary / 1_000_000).toFixed(1)}M ${t.common.currency}` : t.common.noData;
  const jobType = getJobTypeLabel(job.jobType, language);

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <LinearGradient
        colors={isDark ? ['#14532D', '#15803D'] : ['#15803D', '#22C55E']}
        start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }}
        style={styles.gradientHeader}
      >
        <View style={styles.headerTop}>
          <TouchableOpacity style={styles.headerButton} onPress={() => router.back()} activeOpacity={0.7}>
            <BackIcon size={22} color="#fff" />
          </TouchableOpacity>
          <View style={styles.headerCenter}>
            <Text style={styles.headerTitle} numberOfLines={1}>{job.title}</Text>
          </View>
          <View style={{ width: 40 }} />
        </View>
      </LinearGradient>

      <ScrollView showsVerticalScrollIndicator={false}>
        {/* Salary / Type / Location */}
        <View style={[styles.salaryCard, { backgroundColor: colors.card, ...Shadow.md }]}>
          <View style={styles.salaryItem}>
            <MoneyIcon size={20} color={colors.primary} />
            <Text style={[styles.salaryLabel, { color: colors.textTertiary }]}>{t.job.salary}</Text>
            <Text style={[styles.salaryValue, { color: colors.primary }]}>{salary}</Text>
          </View>
          <View style={[styles.salaryDivider, { backgroundColor: colors.border }]} />
          <View style={styles.salaryItem}>
            <ClockIcon size={20} color={colors.textTertiary} />
            <Text style={[styles.salaryLabel, { color: colors.textTertiary }]}>{t.job.jobType}</Text>
            <Text style={[styles.salaryValue, { color: colors.textPrimary }]}>{jobType}</Text>
          </View>
          <View style={[styles.salaryDivider, { backgroundColor: colors.border }]} />
          <View style={styles.salaryItem}>
            <MapPinIcon size={20} color={colors.textTertiary} />
            <Text style={[styles.salaryLabel, { color: colors.textTertiary }]}>{t.job.location}</Text>
            <Text style={[styles.salaryValue, { color: colors.textPrimary }]} numberOfLines={1}>
              {job.city}, {job.country}
            </Text>
          </View>
        </View>

        {/* Category */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>{t.job.category}</Text>
          <Text style={[styles.description, { color: colors.textSecondary }]}>
            {job.categoryName}{job.subCategoryName ? ` › ${job.subCategoryName}` : ''}
          </Text>
        </View>

        {/* Description */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>{t.job.description}</Text>
          <Text style={[styles.description, { color: colors.textSecondary }]}>
            {job.description ?? t.common.noData}
          </Text>
        </View>

        {/* Images */}
        {job.images && job.images.length > 0 && (
          <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
            <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>{t.job.images}</Text>
            <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.imageGalleryRow}>
              {job.images.map((image) => (
                <Image
                  key={image.id}
                  source={{ uri: `${MEDIA_URL}${image.imageUrl}` }}
                  style={styles.galleryImage}
                />
              ))}
            </ScrollView>
          </View>
        )}

        {/* View Applicants */}
        <TouchableOpacity
          style={[styles.applicantsBtn, { backgroundColor: colors.primaryLight }]}
          onPress={() => router.push({ pathname: '/employer/job-applications', params: { jobId: job.id, jobTitle: job.title } })}
          activeOpacity={0.8}
        >
          <ApplicationsIcon size={20} color={colors.primary} />
          <Text style={[styles.applicantsBtnText, { color: colors.primary }]}>{t.employer.applicants}</Text>
        </TouchableOpacity>

        <View style={{ height: 40 }} />
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container:   { flex: 1 },
  centerBox:   { alignItems: 'center', justifyContent: 'center', gap: 12 },
  gradientHeader: { paddingTop: 56, paddingBottom: Spacing.lg },
  headerTop: {
    flexDirection: 'row', alignItems: 'center',
    justifyContent: 'space-between', paddingHorizontal: Spacing.xl,
  },
  headerButton: {
    width: 40, height: 40, borderRadius: BorderRadius.full,
    backgroundColor: 'rgba(255,255,255,0.2)',
    alignItems: 'center', justifyContent: 'center',
  },
  headerCenter: { flex: 1, alignItems: 'center' },
  headerTitle:  { fontSize: FontSize.lg, fontWeight: FontWeight.bold, color: '#fff' },
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
  imageGalleryRow: { marginTop: 4 },
  galleryImage:    { width: 120, height: 120, borderRadius: BorderRadius.lg, marginRight: Spacing.sm },
  applicantsBtn: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    gap: Spacing.sm, borderRadius: BorderRadius.xl, paddingVertical: 14,
    marginHorizontal: Spacing.xl, marginTop: Spacing.lg,
  },
  applicantsBtnText: { fontSize: FontSize.md, fontWeight: FontWeight.bold },
  retryBtn: {
    paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md,
    borderRadius: BorderRadius.lg, marginTop: 4,
  },
  retryText: { color: '#fff', fontWeight: FontWeight.semiBold, fontSize: FontSize.sm },
});
