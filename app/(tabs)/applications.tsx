import React, { useEffect, useState, useCallback } from 'react';
import {
  View, Text, StyleSheet, ScrollView,
  TouchableOpacity, ActivityIndicator, RefreshControl, FlatList,
} from 'react-native';
import { router } from 'expo-router';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { useThemeStore } from '../../store/themeStore';
import { useAuthStore, AuthState } from '../../store/authStore';
import { ClockIcon, BriefcaseIcon, ChevronRightIcon, StarIcon } from '../../components/icons';
import { jobApplicationService } from '../../services/jobApplicationService';
import { jobService } from '../../services/jobService';
import { JobApplication, ApplicationStatus, UserRole, Job } from '../../types';
import { ApplicationListSkeleton } from '../../components/SkeletonLoader';
import { useLanguageStore } from '../../stores/useLanguageStore';

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

// ── Employer ──────────────────────────────────────────────────────────────────
function EmployerJobsView({ colors }: { colors: any }) {
  const { t } = useLanguageStore();
  const [jobs,       setJobs]       = useState<Job[]>([]);
  const [loading,    setLoading]    = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error,      setError]      = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      const data = await jobService.getMyJobs();
      setJobs(data);
      setError(null);
    } catch (e: any) {
      setError(e?.message ?? t.common.somethingWentWrong);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => { load(); }, []);

  if (loading) return <View style={{ padding: Spacing.xl }}><ApplicationListSkeleton count={4} /></View>;

  if (error) return (
    <View style={styles.centerBox}>
      <Text style={[styles.stateText, { color: '#EF4444' }]}>{error}</Text>
      <TouchableOpacity style={[styles.retryBtn, { backgroundColor: colors.primary }]}
        onPress={() => { setLoading(true); load(); }}>
        <Text style={styles.retryText}>{t.common.retry}</Text>
      </TouchableOpacity>
    </View>
  );

  if (jobs.length === 0) return (
    <View style={styles.centerBox}>
      <Text style={[styles.stateText, { color: colors.textSecondary }]}>{t.employer.myJobs}</Text>
      <TouchableOpacity style={[styles.retryBtn, { backgroundColor: colors.primary }]}
        onPress={() => router.push('/(tabs)/create-job')}>
        <Text style={styles.retryText}>{t.employer.postJob}</Text>
      </TouchableOpacity>
    </View>
  );

  return (
    <FlatList
      data={jobs}
      keyExtractor={item => item.id}
      contentContainerStyle={styles.listContainer}
      showsVerticalScrollIndicator={false}
      refreshControl={
        <RefreshControl refreshing={refreshing} onRefresh={() => { setRefreshing(true); load(); }}
          tintColor={colors.primary} colors={[colors.primary]} />
      }
      renderItem={({ item }) => (
        <TouchableOpacity
          style={[styles.applicationCard, { backgroundColor: colors.card, ...Shadow.md }]}
          activeOpacity={0.85}
          onPress={() => router.push({ pathname: '/employer/job-applications', params: { jobId: item.id, jobTitle: item.title } })}
        >
          <View style={styles.cardHeader}>
            <View style={[styles.companyLogo, { backgroundColor: colors.primaryLight }]}>
              <BriefcaseIcon size={22} color={colors.primary} />
            </View>
            <View style={styles.cardInfo}>
              <Text style={[styles.jobTitle,   { color: colors.textPrimary   }]} numberOfLines={1}>{item.title}</Text>
              <Text style={[styles.companyName, { color: colors.textSecondary }]} numberOfLines={1}>{item.location}</Text>
            </View>
            <ChevronRightIcon size={20} color={colors.textTertiary} />
          </View>
          <View style={[styles.divider, { backgroundColor: colors.border }]} />
          <View style={styles.metaRow}>
            <View style={styles.metaItem}>
              <ClockIcon size={12} color={colors.textTertiary} />
              <Text style={[styles.metaText, { color: colors.textTertiary }]}>{formatDate(item.createdAt)}</Text>
            </View>
            <View style={[styles.statusBadge, { backgroundColor: item.status === 0 ? '#DCFCE7' : '#FEE2E2' }]}>
              <Text style={[styles.statusText, { color: item.status === 0 ? '#166534' : '#991B1B' }]}>
                {item.status === 0 ? t.employer.publish : t.common.noData}
              </Text>
            </View>
          </View>
        </TouchableOpacity>
      )}
    />
  );
}

// ── Worker ────────────────────────────────────────────────────────────────────
function WorkerApplicationsView({ colors }: { colors: any }) {
  const { t } = useLanguageStore();

  const STATUS_LABEL: Record<number, string> = {
    [ApplicationStatus.Pending]:   t.applications.pending,
    [ApplicationStatus.Accepted]:  t.applications.accepted,
    [ApplicationStatus.Rejected]:  t.applications.rejected,
    [ApplicationStatus.Cancelled]: t.common.cancel ?? 'Bekor qilindi',
    [ApplicationStatus.Completed]: t.common.done,
  };

  const FILTERS = [
    t.home.seeAll,
    t.applications.pending,
    t.applications.accepted,
    t.applications.rejected,
    t.common.done,
  ];

  const [applications, setApplications] = useState<JobApplication[]>([]);
  const [loading,      setLoading]      = useState(true);
  const [refreshing,   setRefreshing]   = useState(false);
  const [error,        setError]        = useState<string | null>(null);
  const [activeFilter, setActiveFilter] = useState(FILTERS[0]);

  const load = useCallback(async () => {
    try {
      const data = await jobApplicationService.getMyApplications();
      setApplications(data);
      setError(null);
    } catch (e: any) {
      setError(e?.message ?? t.common.somethingWentWrong);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => { load(); }, []);

  const filtered = activeFilter === FILTERS[0]
    ? applications
    : applications.filter(a => STATUS_LABEL[a.status] === activeFilter);

  return (
    <>
      {/* Filter tabs */}
      <ScrollView horizontal showsHorizontalScrollIndicator={false}
        contentContainerStyle={styles.filterContainer}>
        {FILTERS.map(filter => (
          <TouchableOpacity
            key={filter}
            style={[
              styles.filterTab,
              { backgroundColor: colors.surface, borderColor: colors.border },
              activeFilter === filter && { backgroundColor: colors.primary, borderColor: colors.primary },
            ]}
            onPress={() => setActiveFilter(filter)}
            activeOpacity={0.8}
          >
            <Text style={[
              styles.filterText,
              { color: colors.textSecondary },
              activeFilter === filter && { color: '#fff' },
            ]}>
              {filter}
            </Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {loading && <View style={{ padding: Spacing.xl }}><ApplicationListSkeleton count={4} /></View>}

      {!loading && error && (
        <View style={styles.centerBox}>
          <Text style={[styles.stateText, { color: '#EF4444' }]}>{error}</Text>
          <TouchableOpacity style={[styles.retryBtn, { backgroundColor: colors.primary }]}
            onPress={() => { setLoading(true); load(); }}>
            <Text style={styles.retryText}>{t.common.retry}</Text>
          </TouchableOpacity>
        </View>
      )}

      {!loading && !error && filtered.length === 0 && (
        <View style={styles.centerBox}>
          <Text style={[styles.stateText, { color: colors.textSecondary }]}>
            {t.applications.noApplications}
          </Text>
        </View>
      )}

      {!loading && !error && filtered.length > 0 && (
        <ScrollView
          showsVerticalScrollIndicator={false}
          contentContainerStyle={styles.listContainer}
          refreshControl={
            <RefreshControl refreshing={refreshing} onRefresh={() => { setRefreshing(true); load(); }}
              tintColor={colors.primary} colors={[colors.primary]} />
          }
        >
          {filtered.map(app => {
            const statusColor = STATUS_COLOR[app.status] ?? STATUS_COLOR[1];
            return (
              <View
                key={app.id}
                style={[styles.applicationCard, { backgroundColor: colors.card, ...Shadow.md }]}
              >
                <View style={styles.cardHeader}>
                  <View style={[styles.companyLogo, { backgroundColor: colors.primaryLight }]}>
                    <Text style={[styles.companyLogoText, { color: colors.primary }]}>
                      {(app.jobTitle ?? '?')[0].toUpperCase()}
                    </Text>
                  </View>
                  <View style={styles.cardInfo}>
                    <Text style={[styles.jobTitle,   { color: colors.textPrimary   }]} numberOfLines={1}>{app.jobTitle}</Text>
                    <Text style={[styles.companyName, { color: colors.textSecondary }]} numberOfLines={1}>{app.workerName}</Text>
                  </View>
                  <View style={[styles.statusBadge, { backgroundColor: statusColor.bg }]}>
                    <Text style={[styles.statusText, { color: statusColor.text }]}>
                      {STATUS_LABEL[app.status]}
                    </Text>
                  </View>
                </View>

                <View style={[styles.divider, { backgroundColor: colors.border }]} />

                <View style={styles.cardFooter}>
                  {app.coverLetter ? (
                    <Text style={[styles.coverLetter, { color: colors.textTertiary }]} numberOfLines={1}>
                      {app.coverLetter}
                    </Text>
                  ) : null}
                  <View style={styles.metaRow}>
                    <View style={styles.metaItem}>
                      <ClockIcon size={12} color={colors.textTertiary} />
                      <Text style={[styles.metaText, { color: colors.textTertiary }]}>{formatDate(app.createdAt)}</Text>
                    </View>
                  </View>
                </View>

                {app.status === ApplicationStatus.Completed && (
                  <>
                    <View style={[styles.divider, { backgroundColor: colors.border }]} />
                    <TouchableOpacity
                      style={[styles.reviewBtn, { backgroundColor: colors.primaryLight }]}
                      onPress={() => router.push({
                        pathname: '/review',
                        params: { jobApplicationId: app.id, targetName: app.jobTitle },
                      })}
                      activeOpacity={0.8}
                    >
                      <StarIcon size={16} color={colors.primary} />
                      <Text style={[styles.reviewBtnText, { color: colors.primary }]}>
                        {t.profile.reviews}
                      </Text>
                    </TouchableOpacity>
                  </>
                )}
              </View>
            );
          })}
          <View style={{ height: 24 }} />
        </ScrollView>
      )}
    </>
  );
}

// ── Main ──────────────────────────────────────────────────────────────────────
export default function ApplicationsScreen() {
  const { colors } = useThemeStore();
  const { t } = useLanguageStore();
  const role       = useAuthStore((state: AuthState) => state.role);
  const isEmployer = Number(role) === UserRole.Employer;

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={[styles.header, { backgroundColor: colors.surface }]}>
        <Text style={[styles.headerTitle, { color: colors.textPrimary }]}>
          {isEmployer ? t.employer.myJobs : t.applications.title}
        </Text>
      </View>
      {isEmployer
        ? <EmployerJobsView      colors={colors} />
        : <WorkerApplicationsView colors={colors} />
      }
    </View>
  );
}

const styles = StyleSheet.create({
  container:       { flex: 1 },
  centerBox:       { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 12 },
  header: {
    flexDirection: 'row', alignItems: 'center', gap: Spacing.sm,
    paddingHorizontal: Spacing.xl, paddingTop: 56, paddingBottom: Spacing.lg,
  },
  headerTitle:     { fontSize: FontSize.xxl, fontWeight: FontWeight.bold },
  filterContainer: { paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md, gap: Spacing.sm },
  filterTab: {
  paddingHorizontal: Spacing.lg, paddingVertical: Spacing.sm,
  borderRadius: BorderRadius.full, borderWidth: 1.5,
  height: 36, alignItems: 'center', justifyContent: 'center',
},
  filterText:      { fontSize: FontSize.sm, fontWeight: FontWeight.semiBold },
  listContainer:   { padding: Spacing.xl, gap: Spacing.md },
  applicationCard: { borderRadius: BorderRadius.xl, padding: Spacing.lg },
  cardHeader:      { flexDirection: 'row', alignItems: 'center' },
  companyLogo: {
    width: 48, height: 48, borderRadius: BorderRadius.md,
    alignItems: 'center', justifyContent: 'center', marginRight: Spacing.md,
  },
  companyLogoText: { fontSize: FontSize.lg, fontWeight: FontWeight.bold },
  cardInfo:        { flex: 1 },
  jobTitle:        { fontSize: FontSize.md, fontWeight: FontWeight.bold, marginBottom: 2 },
  companyName:     { fontSize: FontSize.sm },
  statusBadge:     { borderRadius: BorderRadius.sm, paddingHorizontal: Spacing.sm, paddingVertical: Spacing.xs },
  statusText:      { fontSize: FontSize.xs, fontWeight: FontWeight.bold },
  divider:         { height: 1, marginVertical: Spacing.md },
  cardFooter:      { gap: 6 },
  coverLetter:     { fontSize: FontSize.xs, fontStyle: 'italic' },
  metaRow:         { flexDirection: 'row', gap: Spacing.md, alignItems: 'center' },
  metaItem:        { flexDirection: 'row', alignItems: 'center', gap: 4 },
  metaText:        { fontSize: FontSize.xs, fontWeight: FontWeight.medium },
  reviewBtn: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    gap: Spacing.sm, height: 44, borderRadius: BorderRadius.lg,
  },
  reviewBtnText:   { fontSize: FontSize.sm, fontWeight: FontWeight.semiBold },
  stateText:       { fontSize: FontSize.sm, textAlign: 'center' },
  retryBtn: {
    paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md,
    borderRadius: BorderRadius.lg, marginTop: 4,
  },
  retryText: { color: '#fff', fontWeight: FontWeight.semiBold, fontSize: FontSize.sm },
});
