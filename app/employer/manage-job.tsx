import React, { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, ScrollView, TouchableOpacity, Image,
  ActivityIndicator, Alert,
} from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import * as ImagePicker from 'expo-image-picker';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { useThemeStore } from '../../store/themeStore';
import { MapPinIcon, ClockIcon, MoneyIcon, ApplicationsIcon, PlusIcon, CloseIcon } from '../../components/icons';
import Svg, { Path } from 'react-native-svg';
import { jobService } from '../../services/jobService';
import { Job } from '../../types';
import { JobDetailSkeleton } from '../../components/SkeletonLoader';
import { useLanguageStore } from '../../stores/useLanguageStore';
import { MEDIA_URL } from '../../services/api';
import { getJobTypeLabel } from '../../utils/jobType';
import { getCategoryLabel } from '../../utils/categoryLocalization';

function BackIcon({ size = 24, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M19 12H5M5 12L12 19M5 12L12 5" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

export default function ManageJobScreen() {
  const { colors, isDark } = useThemeStore();
  const { t, language } = useLanguageStore();
  const { jobId } = useLocalSearchParams<{ jobId: string }>();

  const [job,             setJob]             = useState<Job | null>(null);
  const [loading,         setLoading]         = useState(true);
  const [error,           setError]           = useState<string | null>(null);
  const [uploading,       setUploading]       = useState(false);
  const [deletingImageId, setDeletingImageId] = useState<string | null>(null);

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

  // Post-mutation refresh only - the upload/delete already succeeded on the server, so a failure
  // here must never blank the screen or drop the gallery the employer is currently looking at.
  // Unlike the initial load above, this intentionally never touches `loading` or `error`.
  // Returns whether the refetch itself succeeded, so callers can tell "mutation failed" apart from
  // "mutation succeeded but the follow-up refresh didn't" and warn instead of showing a false error.
  const refreshJob = async (): Promise<boolean> => {
    if (!jobId) return false;
    try {
      const data = await jobService.getJobById(jobId);
      setJob(data);
      return true;
    } catch {
      // Keep showing the last known-good job/gallery rather than surface a stale-refresh error.
      return false;
    }
  };

  const handleAddImage = async () => {
    if (!job || uploading) return;
    if ((job.images?.length ?? 0) >= 5) {
      Alert.alert('', t.job.maxImages);
      return;
    }
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      quality: 0.8,
    });
    if (result.canceled || !result.assets[0]) return;

    setUploading(true);
    try {
      await jobService.uploadJobImage(job.id, result.assets[0].uri);
      const refreshed = await refreshJob();
      if (!refreshed) {
        Alert.alert('', t.job.refreshFailed);
      }
    } catch (e: any) {
      Alert.alert(t.common.error, e?.message ?? t.common.somethingWentWrong);
    } finally {
      setUploading(false);
    }
  };

  const handleDeleteImage = (imageId: string) => {
    Alert.alert(
      t.common.delete,
      t.common.confirm,
      [
        { text: t.common.cancel, style: 'cancel' },
        {
          text: t.common.yes,
          style: 'destructive',
          onPress: async () => {
            if (!job) return;
            setDeletingImageId(imageId);
            try {
              await jobService.deleteJobImage(job.id, imageId);
              const refreshed = await refreshJob();
              if (!refreshed) {
                Alert.alert('', t.job.refreshFailed);
              }
            } catch (e: any) {
              Alert.alert(t.common.error, e?.message ?? t.common.somethingWentWrong);
            } finally {
              setDeletingImageId(null);
            }
          },
        },
      ]
    );
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
            {getCategoryLabel(job.categoryName, language)}{job.subCategoryName ? ` › ${getCategoryLabel(job.subCategoryName, language)}` : ''}
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
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>{t.job.images}</Text>
          <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.imageGalleryRow}>
            {(job.images ?? []).map((image) => (
              <View key={image.id} style={styles.imageWrapper}>
                <Image
                  source={{ uri: `${MEDIA_URL}${image.imageUrl}` }}
                  style={styles.galleryImage}
                />
                <TouchableOpacity
                  style={styles.removeImageBtn}
                  onPress={() => handleDeleteImage(image.id)}
                  disabled={deletingImageId === image.id}
                  activeOpacity={0.8}
                >
                  {deletingImageId === image.id ? (
                    <ActivityIndicator size="small" color="#fff" />
                  ) : (
                    <CloseIcon size={14} color="#fff" />
                  )}
                </TouchableOpacity>
              </View>
            ))}
            {(job.images?.length ?? 0) < 5 && (
              <TouchableOpacity
                style={[styles.addImageBtn, { backgroundColor: colors.card, borderColor: colors.border }]}
                onPress={handleAddImage}
                disabled={uploading}
                activeOpacity={0.7}
              >
                {uploading ? (
                  <ActivityIndicator color={colors.primary} />
                ) : (
                  <PlusIcon size={28} color={colors.primary} />
                )}
              </TouchableOpacity>
            )}
          </ScrollView>
        </View>

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
  imageWrapper:    { position: 'relative', marginRight: Spacing.sm },
  galleryImage:    { width: 90, height: 90, borderRadius: BorderRadius.lg },
  removeImageBtn: {
    position: 'absolute', top: 4, right: 4,
    backgroundColor: 'rgba(0,0,0,0.6)', borderRadius: BorderRadius.sm, padding: 3,
  },
  addImageBtn: {
    width: 90, height: 90, borderRadius: BorderRadius.lg,
    borderWidth: 1.5, borderStyle: 'dashed',
    justifyContent: 'center', alignItems: 'center',
  },
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
