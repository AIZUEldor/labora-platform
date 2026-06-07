import React, { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, ScrollView,
  TouchableOpacity, ActivityIndicator, Alert,
} from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useThemeStore } from '../store/themeStore';
import { jobApplicationService } from '../services/jobApplicationService';
import { JobApplication, ApplicationStatus } from '../types';
import { BackIcon, ClockIcon, BriefcaseIcon, StarIcon } from '../components/icons';
import { BorderRadius, Shadow, Spacing } from '../constants/spacing';
import { FontSize, FontWeight } from '../constants/typography';

const STATUS_COLOR: Record<number, { bg: string; text: string }> = {
  [ApplicationStatus.Pending]:   { bg: '#FEF9C3', text: '#854D0E' },
  [ApplicationStatus.Accepted]:  { bg: '#DCFCE7', text: '#166534' },
  [ApplicationStatus.Rejected]:  { bg: '#FEE2E2', text: '#991B1B' },
  [ApplicationStatus.Cancelled]: { bg: '#F3F4F6', text: '#6B7280' },
  [ApplicationStatus.Completed]: { bg: '#EFF6FF', text: '#1D4ED8' },
};

const STATUS_LABEL: Record<number, string> = {
  [ApplicationStatus.Pending]:   'Kutilmoqda',
  [ApplicationStatus.Accepted]:  'Qabul qilindi',
  [ApplicationStatus.Rejected]:  'Rad etildi',
  [ApplicationStatus.Cancelled]: 'Bekor qilindi',
  [ApplicationStatus.Completed]: 'Yakunlandi',
};

function formatDate(dateStr: string): string {
  const d = new Date(dateStr);
  return `${d.getDate()}.${d.getMonth() + 1}.${d.getFullYear()}`;
}

export default function ApplicationDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useThemeStore();

  const [app,     setApp]     = useState<JobApplication | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => { loadApp(); }, [id]);

  const loadApp = async () => {
    try {
      const data = await jobApplicationService.getById(id!);
      setApp(data);
    } catch {
      Alert.alert('Xato', 'Arizani yuklashda xato');
      router.back();
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    Alert.alert(
      'Arizani bekor qilish',
      'Haqiqatan ham bu arizani bekor qilmoqchimisiz?',
      [
        { text: "Yo'q", style: 'cancel' },
        {
          text: 'Ha, bekor qilish',
          style: 'destructive',
          onPress: async () => {
            try {
              await jobApplicationService.cancel(id!);
              router.back();
            } catch {
              Alert.alert('Xato', 'Bekor qilishda xato yuz berdi');
            }
          },
        },
      ]
    );
  };

  if (loading) {
    return (
      <View style={[styles.container, { backgroundColor: colors.background, justifyContent: 'center', alignItems: 'center' }]}>
        <ActivityIndicator size="large" color="#16A34A" />
      </View>
    );
  }

  if (!app) return null;

  const statusColor = STATUS_COLOR[app.status] ?? STATUS_COLOR[1];

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <LinearGradient
        colors={['#16A34A', '#15803D']}
        style={[styles.header, { paddingTop: insets.top + 12 }]}
      >
        <TouchableOpacity onPress={() => router.back()} style={styles.headerBtn}>
          <BackIcon size={24} color="#fff" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Ariza tafsiloti</Text>
        <View style={{ width: 40 }} />
      </LinearGradient>

      <ScrollView
        contentContainerStyle={[styles.content, { paddingBottom: insets.bottom + 24 }]}
        showsVerticalScrollIndicator={false}
      >
        {/* Status */}
        <View style={styles.topRow}>
          <View style={[styles.badge, { backgroundColor: statusColor.bg }]}>
            <Text style={[styles.badgeText, { color: statusColor.text }]}>
              {STATUS_LABEL[app.status]}
            </Text>
          </View>
          <View style={styles.dateRow}>
            <ClockIcon size={14} color={colors.textSecondary} />
            <Text style={[styles.dateText, { color: colors.textSecondary }]}>
              {formatDate(app.createdAt)}
            </Text>
          </View>
        </View>

        {/* Ish nomi */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <View style={styles.jobRow}>
            <View style={[styles.jobLogo, { backgroundColor: colors.primaryLight }]}>
              <Text style={[styles.jobLogoText, { color: colors.primary }]}>
                {(app.jobTitle ?? '?')[0].toUpperCase()}
              </Text>
            </View>
            <View style={{ flex: 1 }}>
              <Text style={[styles.jobTitle, { color: colors.textPrimary }]}>{app.jobTitle}</Text>
              {app.workerName && (
                <Text style={[styles.subText, { color: colors.textSecondary }]}>{app.workerName}</Text>
              )}
            </View>
          </View>
        </View>

        {/* Cover letter */}
        {app.coverLetter && (
          <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
            <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>Yuborgan xabarim</Text>
            <Text style={[styles.sectionText, { color: colors.textSecondary }]}>{app.coverLetter}</Text>
          </View>
        )}

        {/* Status haqida ma'lumot */}
        <View style={[styles.infoCard, { backgroundColor: statusColor.bg }]}>
          <Text style={[styles.infoText, { color: statusColor.text }]}>
            {app.status === ApplicationStatus.Pending   && "Arizangiz ko'rib chiqilmoqda. Ish beruvchi javob berganda xabardor qilinasiz."}
            {app.status === ApplicationStatus.Accepted  && "Tabriklaymiz! Arizangiz qabul qilindi. Ish beruvchi siz bilan bog'lanadi."}
            {app.status === ApplicationStatus.Rejected  && "Afsuski, arizangiz rad etildi. Boshqa e'lonlarga ariza bering."}
            {app.status === ApplicationStatus.Cancelled && "Bu ariza bekor qilindi."}
            {app.status === ApplicationStatus.Completed && "Ish yakunlandi. Ish beruvchini baholashingiz mumkin."}
          </Text>
        </View>

        {/* Baholash — faqat Completed da */}
        {app.status === ApplicationStatus.Completed && (
          <TouchableOpacity
            style={[styles.reviewBtn, { backgroundColor: colors.primaryLight }]}
            onPress={() => router.push({
              pathname: '/review',
              params: { jobApplicationId: app.id, targetName: app.jobTitle },
            })}
            activeOpacity={0.8}
          >
            <StarIcon size={18} color="#16A34A" />
            <Text style={[styles.reviewBtnText, { color: '#16A34A' }]}>Ish beruvchini baholash</Text>
          </TouchableOpacity>
        )}

        {/* Bekor qilish — faqat Pending da */}
        {app.status === ApplicationStatus.Pending && (
          <TouchableOpacity
            style={[styles.cancelBtn, { borderColor: '#EF4444' }]}
            onPress={handleCancel}
            activeOpacity={0.8}
          >
            <Text style={[styles.cancelBtnText, { color: '#EF4444' }]}>Arizani bekor qilish</Text>
          </TouchableOpacity>
        )}
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container:    { flex: 1 },
  header: {
    flexDirection: 'row', alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16, paddingBottom: 16,
  },
  headerBtn:    { width: 40, height: 40, justifyContent: 'center' },
  headerTitle:  { fontSize: 18, fontWeight: '700', color: '#fff' },
  content:      { padding: 16, gap: 12 },
  topRow: {
    flexDirection: 'row', alignItems: 'center',
    justifyContent: 'space-between',
  },
  badge: {
    borderRadius: BorderRadius.sm,
    paddingHorizontal: Spacing.md, paddingVertical: Spacing.xs,
  },
  badgeText:    { fontSize: FontSize.sm, fontWeight: FontWeight.bold },
  dateRow:      { flexDirection: 'row', alignItems: 'center', gap: 4 },
  dateText:     { fontSize: FontSize.sm },
  section:      { borderRadius: BorderRadius.xl, padding: Spacing.lg, gap: 8 },
  jobRow:       { flexDirection: 'row', alignItems: 'center', gap: 12 },
  jobLogo: {
    width: 48, height: 48, borderRadius: BorderRadius.md,
    alignItems: 'center', justifyContent: 'center',
  },
  jobLogoText:  { fontSize: FontSize.lg, fontWeight: FontWeight.bold },
  jobTitle:     { fontSize: FontSize.md, fontWeight: FontWeight.bold },
  subText:      { fontSize: FontSize.sm },
  sectionTitle: { fontSize: FontSize.md, fontWeight: FontWeight.bold },
  sectionText:  { fontSize: FontSize.sm, lineHeight: 22 },
  infoCard: {
    borderRadius: BorderRadius.xl, padding: Spacing.lg,
  },
  infoText:     { fontSize: FontSize.sm, lineHeight: 20 },
  reviewBtn: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    gap: 8, borderRadius: BorderRadius.xl, paddingVertical: 14,
  },
  reviewBtnText: { fontSize: FontSize.md, fontWeight: FontWeight.semiBold },
  cancelBtn: {
    borderWidth: 1.5, borderRadius: BorderRadius.xl,
    paddingVertical: 14, alignItems: 'center',
  },
  cancelBtnText: { fontSize: FontSize.md, fontWeight: FontWeight.semiBold },
});