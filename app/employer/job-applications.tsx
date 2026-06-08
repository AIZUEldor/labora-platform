import React, { useEffect, useState, useCallback } from 'react';
import {
  View, Text, StyleSheet, ScrollView,
  TouchableOpacity, ActivityIndicator, RefreshControl, Alert, Linking,
} from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { useThemeStore } from '../../store/themeStore';
import { ClockIcon, StarIcon, PhoneIcon, LocationIcon } from '../../components/icons';
import Svg, { Path } from 'react-native-svg';
import { jobApplicationService } from '../../services/jobApplicationService';
import { JobApplication, ApplicationStatus } from '../../types';
import { useLanguageStore } from '../../stores/useLanguageStore';

const BASE_URL = 'http://10.106.130.66:5020';

function BackIcon({ size = 24, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M19 12H5M5 12L12 19M5 12L12 5" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

function ChevronDownIcon({ size = 18, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M6 9L12 15L18 9" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

function ChevronUpIcon({ size = 18, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M18 15L12 9L6 15" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

const STATUS_COLOR: Record<number, { bg: string; text: string }> = {
  [ApplicationStatus.Pending]:   { bg: '#FEF9C3', text: '#854D0E' },
  [ApplicationStatus.Accepted]:  { bg: '#DCFCE7', text: '#166534' },
  [ApplicationStatus.Rejected]:  { bg: '#FEE2E2', text: '#991B1B' },
  [ApplicationStatus.Cancelled]: { bg: '#F3F4F6', text: '#6B7280' },
  [ApplicationStatus.Completed]: { bg: '#EFF6FF', text: '#1D4ED8' },
};

function formatDate(dateStr: string): string {
  const d = new Date(dateStr);
  return `${d.getDate()}.${d.getMonth() + 1}.${d.getFullYear()}`;
}

export default function JobApplicationsScreen() {
  const { colors, isDark } = useThemeStore();
  const { t } = useLanguageStore();
  const { jobId, jobTitle } = useLocalSearchParams<{ jobId: string; jobTitle: string }>();

  const STATUS_LABEL: Record<number, string> = {
    [ApplicationStatus.Pending]:   t.applications.pending,
    [ApplicationStatus.Accepted]:  t.applications.accepted,
    [ApplicationStatus.Rejected]:  t.applications.rejected,
    [ApplicationStatus.Cancelled]: t.common.cancel ?? 'Bekor',
    [ApplicationStatus.Completed]: t.common.done,
  };

  const [applications, setApplications] = useState<JobApplication[]>([]);
  const [loading,      setLoading]      = useState(true);
  const [refreshing,   setRefreshing]   = useState(false);
  const [error,        setError]        = useState<string | null>(null);
  const [updating,     setUpdating]     = useState<string | null>(null);
  const [expandedId,   setExpandedId]   = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      const data = await jobApplicationService.getJobApplications(jobId);
      setApplications(data);
      setError(null);
    } catch (e: any) {
      setError(e?.message ?? t.common.somethingWentWrong);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [jobId]);

  useEffect(() => { load(); }, []);

  const handleUpdateStatus = async (id: string, status: 'Accepted' | 'Rejected') => {
    Alert.alert(
      status === 'Accepted' ? t.employer.acceptApplicant : t.employer.rejectApplicant,
      t.common.confirm,
      [
        { text: t.common.cancel, style: 'cancel' },
        {
          text: t.common.yes,
          onPress: async () => {
            try {
              setUpdating(id);
              await jobApplicationService.updateStatus(id, status);
              setApplications(prev =>
                prev.map(a => a.id === id
                  ? { ...a, status: status === 'Accepted' ? ApplicationStatus.Accepted : ApplicationStatus.Rejected }
                  : a
                )
              );
            } catch (e: any) {
              Alert.alert(t.common.error, e?.message ?? t.common.somethingWentWrong);
            } finally {
              setUpdating(null);
            }
          },
        },
      ]
    );
  };

  const handleComplete = async (id: string) => {
    Alert.alert(
      t.common.done,
      t.common.confirm,
      [
        { text: t.common.cancel, style: 'cancel' },
        {
          text: t.common.yes,
          onPress: async () => {
            try {
              setUpdating(id);
              await jobApplicationService.complete(id);
              setApplications(prev =>
                prev.map(a => a.id === id
                  ? { ...a, status: ApplicationStatus.Completed }
                  : a
                )
              );
            } catch (e: any) {
              Alert.alert(t.common.error, e?.message ?? t.common.somethingWentWrong);
            } finally {
              setUpdating(null);
            }
          },
        },
      ]
    );
  };

  const handleViewCv = (cvUrl: string) => {
    const fullUrl = `${BASE_URL}${cvUrl}`;
    Linking.openURL(fullUrl).catch(() => {
      Alert.alert(t.common.error, t.common.somethingWentWrong);
    });
  };

  const handleCall = (phone: string) => {
    Linking.openURL(`tel:${phone}`).catch(() => {
      Alert.alert(t.common.error, t.common.somethingWentWrong);
    });
  };

  const toggleExpand = (id: string) => {
    setExpandedId(prev => prev === id ? null : id);
  };

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
            <Text style={styles.headerTitle} numberOfLines={1}>{jobTitle}</Text>
            <Text style={styles.headerSubtitle}>
              {applications.length} {t.employer.applicants}
            </Text>
          </View>
          <View style={{ width: 40 }} />
        </View>
      </LinearGradient>

      {loading && (
        <View style={styles.centerBox}>
          <ActivityIndicator size="large" color={colors.primary} />
        </View>
      )}

      {!loading && error && (
        <View style={styles.centerBox}>
          <Text style={[styles.stateText, { color: '#EF4444' }]}>{error}</Text>
          <TouchableOpacity style={[styles.retryBtn, { backgroundColor: colors.primary }]}
            onPress={() => { setLoading(true); load(); }}>
            <Text style={styles.retryText}>{t.common.retry}</Text>
          </TouchableOpacity>
        </View>
      )}

      {!loading && !error && applications.length === 0 && (
        <View style={styles.centerBox}>
          <Text style={[styles.stateText, { color: colors.textSecondary }]}>
            {t.employer.noApplicants}
          </Text>
        </View>
      )}

      {!loading && !error && applications.length > 0 && (
        <ScrollView
          showsVerticalScrollIndicator={false}
          contentContainerStyle={styles.listContainer}
          refreshControl={
            <RefreshControl refreshing={refreshing} onRefresh={() => { setRefreshing(true); load(); }}
              tintColor={colors.primary} colors={[colors.primary]} />
          }
        >
          {applications.map(app => {
            const statusColor = STATUS_COLOR[app.status] ?? STATUS_COLOR[1];
            const isExpanded  = expandedId === app.id;

            return (
              <View key={app.id} style={[styles.appCard, { backgroundColor: colors.card, ...Shadow.md }]}>

                {/* Top — bosish kengaytiradi */}
                <TouchableOpacity
                  style={styles.cardTop}
                  onPress={() => toggleExpand(app.id)}
                  activeOpacity={0.8}
                >
                  <View style={[styles.avatar, { backgroundColor: colors.primaryLight }]}>
                    <Text style={[styles.avatarText, { color: colors.primary }]}>
                      {(app.workerName ?? '?')[0].toUpperCase()}
                    </Text>
                  </View>
                  <View style={styles.cardInfo}>
                    <Text style={[styles.workerName, { color: colors.textPrimary }]} numberOfLines={1}>
                      {app.workerName}
                    </Text>
                    <View style={styles.metaItem}>
                      <ClockIcon size={12} color={colors.textTertiary} />
                      <Text style={[styles.metaText, { color: colors.textTertiary }]}>
                        {formatDate(app.createdAt)}
                      </Text>
                    </View>
                  </View>
                  <View style={styles.cardTopRight}>
                    <View style={[styles.statusBadge, { backgroundColor: statusColor.bg }]}>
                      <Text style={[styles.statusText, { color: statusColor.text }]}>
                        {STATUS_LABEL[app.status]}
                      </Text>
                    </View>
                    <View style={{ marginTop: 4 }}>
                      {isExpanded
                        ? <ChevronUpIcon size={16} color={colors.textTertiary} />
                        : <ChevronDownIcon size={16} color={colors.textTertiary} />
                      }
                    </View>
                  </View>
                </TouchableOpacity>

                {/* Kengaytirilgan qism */}
                {isExpanded && (
                  <>
                    <View style={[styles.divider, { backgroundColor: colors.border }]} />

                    {/* Cover Letter */}
                    {app.coverLetter && (
                      <>
                        <Text style={[styles.detailLabel, { color: colors.textSecondary }]}>
                          {/* Xabar */}
                          {t.applications.title === 'Arizalarim' ? 'Xabar' : t.applications.title === 'Мои заявки' ? 'Сообщение' : 'Message'}
                        </Text>
                        <Text style={[styles.detailValue, { color: colors.textPrimary }]}>
                          {app.coverLetter}
                        </Text>
                        <View style={[styles.divider, { backgroundColor: colors.border }]} />
                      </>
                    )}

                    {/* Telefon */}
                    {app.workerPhone && (
                      <>
                        <TouchableOpacity
                          style={[styles.contactRow, { backgroundColor: colors.primaryLight }]}
                          onPress={() => handleCall(app.workerPhone!)}
                          activeOpacity={0.8}
                        >
                          <PhoneIcon size={16} color={colors.primary} />
                          <Text style={[styles.contactText, { color: colors.primary }]}>
                            {app.workerPhone}
                          </Text>
                        </TouchableOpacity>
                        <View style={[styles.divider, { backgroundColor: colors.border }]} />
                      </>
                    )}

                    {/* CV */}
                    {app.workerCvUrl && (
                      <>
                        <TouchableOpacity
                          style={[styles.cvBtn, { backgroundColor: '#EFF6FF' }]}
                          onPress={() => handleViewCv(app.workerCvUrl!)}
                          activeOpacity={0.8}
                        >
                          <Text style={[styles.actionBtnText, { color: '#1D4ED8' }]}>
                            {t.profile.myCV}
                          </Text>
                        </TouchableOpacity>
                        <View style={[styles.divider, { backgroundColor: colors.border }]} />
                      </>
                    )}

                    {/* Pending: Qabul / Rad */}
                    {app.status === ApplicationStatus.Pending && (
                      <View style={styles.actionRow}>
                        <TouchableOpacity
                          style={[styles.actionBtn, { backgroundColor: '#FEE2E2' }]}
                          onPress={() => handleUpdateStatus(app.id, 'Rejected')}
                          activeOpacity={0.8}
                          disabled={updating === app.id}
                        >
                          {updating === app.id
                            ? <ActivityIndicator size="small" color="#991B1B" />
                            : <Text style={[styles.actionBtnText, { color: '#991B1B' }]}>
                                {t.employer.rejectApplicant}
                              </Text>
                          }
                        </TouchableOpacity>
                        <TouchableOpacity
                          style={[styles.actionBtn, { backgroundColor: '#DCFCE7' }]}
                          onPress={() => handleUpdateStatus(app.id, 'Accepted')}
                          activeOpacity={0.8}
                          disabled={updating === app.id}
                        >
                          {updating === app.id
                            ? <ActivityIndicator size="small" color="#166534" />
                            : <Text style={[styles.actionBtnText, { color: '#166534' }]}>
                                {t.employer.acceptApplicant}
                              </Text>
                          }
                        </TouchableOpacity>
                      </View>
                    )}

                    {/* Accepted: Yakunlash */}
                    {app.status === ApplicationStatus.Accepted && (
                      <TouchableOpacity
                        style={[styles.completeBtn, { backgroundColor: '#EFF6FF' }]}
                        onPress={() => handleComplete(app.id)}
                        activeOpacity={0.8}
                        disabled={updating === app.id}
                      >
                        {updating === app.id
                          ? <ActivityIndicator size="small" color="#1D4ED8" />
                          : <Text style={[styles.actionBtnText, { color: '#1D4ED8' }]}>
                              {t.common.done}
                            </Text>
                        }
                      </TouchableOpacity>
                    )}

                    {/* Completed: Baholash */}
                    {app.status === ApplicationStatus.Completed && (
                      <TouchableOpacity
                        style={[styles.completeBtn, { backgroundColor: colors.primaryLight }]}
                        onPress={() => router.push({
                          pathname: '/review',
                          params: { jobApplicationId: app.id, targetName: app.workerName },
                        })}
                        activeOpacity={0.8}
                      >
                        <StarIcon size={16} color={colors.primary} />
                        <Text style={[styles.actionBtnText, { color: colors.primary }]}>
                          {t.profile.reviews}
                        </Text>
                      </TouchableOpacity>
                    )}
                  </>
                )}
              </View>
            );
          })}
          <View style={{ height: 24 }} />
        </ScrollView>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container:      { flex: 1 },
  centerBox:      { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 12 },
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
  headerCenter:   { flex: 1, alignItems: 'center' },
  headerTitle:    { fontSize: FontSize.lg, fontWeight: FontWeight.bold, color: '#fff' },
  headerSubtitle: { fontSize: FontSize.xs, color: 'rgba(255,255,255,0.8)', marginTop: 2 },
  listContainer:  { padding: Spacing.xl, gap: Spacing.md },
  appCard:        { borderRadius: BorderRadius.xl, padding: Spacing.lg },
  cardTop:        { flexDirection: 'row', alignItems: 'center' },
  cardTopRight:   { alignItems: 'center' },
  avatar: {
    width: 48, height: 48, borderRadius: BorderRadius.full,
    alignItems: 'center', justifyContent: 'center', marginRight: Spacing.md,
  },
  avatarText:    { fontSize: FontSize.lg, fontWeight: FontWeight.bold },
  cardInfo:      { flex: 1 },
  workerName:    { fontSize: FontSize.md, fontWeight: FontWeight.bold, marginBottom: 4 },
  statusBadge:   { borderRadius: BorderRadius.sm, paddingHorizontal: Spacing.sm, paddingVertical: Spacing.xs },
  statusText:    { fontSize: FontSize.xs, fontWeight: FontWeight.bold },
  divider:       { height: 1, marginVertical: Spacing.md },
  detailLabel:   { fontSize: FontSize.xs, fontWeight: FontWeight.medium, marginBottom: 4 },
  detailValue:   { fontSize: FontSize.sm, lineHeight: 20 },
  contactRow: {
    flexDirection: 'row', alignItems: 'center', gap: Spacing.sm,
    paddingHorizontal: Spacing.md, paddingVertical: 10,
    borderRadius: BorderRadius.lg,
  },
  contactText:   { fontSize: FontSize.sm, fontWeight: FontWeight.semiBold },
  metaItem:      { flexDirection: 'row', alignItems: 'center', gap: 4 },
  metaText:      { fontSize: FontSize.xs },
  actionRow:     { flexDirection: 'row', gap: Spacing.md },
  actionBtn: {
    flex: 1, height: 44, borderRadius: BorderRadius.lg,
    alignItems: 'center', justifyContent: 'center',
  },
  cvBtn: {
    height: 44, borderRadius: BorderRadius.lg,
    alignItems: 'center', justifyContent: 'center',
  },
  completeBtn: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    gap: Spacing.sm, height: 44, borderRadius: BorderRadius.lg,
  },
  actionBtnText: { fontSize: FontSize.sm, fontWeight: FontWeight.bold },
  stateText:     { fontSize: FontSize.sm, textAlign: 'center' },
  retryBtn: {
    paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md,
    borderRadius: BorderRadius.lg, marginTop: 4,
  },
  retryText: { color: '#fff', fontWeight: FontWeight.semiBold, fontSize: FontSize.sm },
});
