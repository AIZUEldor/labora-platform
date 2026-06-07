import React, { useEffect, useState, useCallback } from 'react';
import {
  View, Text, StyleSheet, ScrollView,
  TouchableOpacity, RefreshControl, FlatList,
} from 'react-native';
import { router } from 'expo-router';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { useThemeStore } from '../../store/themeStore';
import { useAuthStore, AuthState } from '../../store/authStore';
import { ClockIcon, BriefcaseIcon, ChevronRightIcon, StarIcon, EyeIcon } from '../../components/icons';
import { jobApplicationService } from '../../services/jobApplicationService';
import { jobService } from '../../services/jobService';
import { workerPostService } from '../../services/workerPostService';
import { JobApplication, ApplicationStatus, UserRole, Job, WorkerPost } from '../../types';
import { ApplicationListSkeleton } from '../../components/SkeletonLoader';
import { useLanguageStore } from '../../stores/useLanguageStore';

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

const WORKER_POST_STATUS_LABEL: Record<number, { label: string; bg: string; text: string }> = {
  1: { label: 'Faol',          bg: '#DCFCE7', text: '#166534' },
  2: { label: 'Nofaol',        bg: '#F3F4F6', text: '#6B7280' },
  3: { label: 'Qabul qilindi', bg: '#EFF6FF', text: '#1D4ED8' },
};

const FILTERS      = ['Barchasi', 'Kutilmoqda', 'Qabul qilindi', 'Rad etildi', 'Yakunlandi'];
const POST_FILTERS = ['Barchasi', 'Faol', 'Nofaol', 'Qabul qilindi'];

function formatDate(dateStr: string): string {
  const d = new Date(dateStr);
  return `${d.getDate()}.${d.getMonth() + 1}.${d.getFullYear()}`;
}

function FilterTabs({ filters, active, onChange, colors }: {
  filters: string[];
  active: string;
  onChange: (f: string) => void;
  colors: any;
}) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={styles.filterContainer}
    >
      {filters.map(filter => (
        <TouchableOpacity
          key={filter}
          style={[
            styles.filterTab,
            { backgroundColor: colors.surface, borderColor: colors.border },
            active === filter && { backgroundColor: colors.primary, borderColor: colors.primary },
          ]}
          onPress={() => onChange(filter)}
          activeOpacity={0.8}
        >
          <Text style={[
            styles.filterText,
            { color: colors.textSecondary },
            active === filter && { color: '#fff' },
          ]}>
            {filter}
          </Text>
        </TouchableOpacity>
      ))}
    </ScrollView>
  );
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
          style={[styles.card, { backgroundColor: colors.card, ...Shadow.md }]}
          activeOpacity={0.85}
          onPress={() => router.push({ pathname: '/employer/job-applications', params: { jobId: item.id, jobTitle: item.title } })}
        >
          <View style={styles.cardHeader}>
            <View style={[styles.logo, { backgroundColor: colors.primaryLight }]}>
              <BriefcaseIcon size={22} color={colors.primary} />
            </View>
            <View style={styles.cardInfo}>
              <Text style={[styles.jobTitle, { color: colors.textPrimary }]} numberOfLines={1}>{item.title}</Text>
              <Text style={[styles.subText, { color: colors.textSecondary }]} numberOfLines={1}>{item.location}</Text>
            </View>
            <ChevronRightIcon size={20} color={colors.textTertiary} />
          </View>
          <View style={[styles.divider, { backgroundColor: colors.border }]} />
          <View style={styles.metaRow}>
            <View style={styles.metaItem}>
              <ClockIcon size={12} color={colors.textTertiary} />
              <Text style={[styles.metaText, { color: colors.textTertiary }]}>{formatDate(item.createdAt)}</Text>
            </View>
            <View style={[styles.badge, { backgroundColor: item.status === 0 ? '#DCFCE7' : '#FEE2E2' }]}>
              <Text style={[styles.badgeText, { color: item.status === 0 ? '#166534' : '#991B1B' }]}>
                {item.status === 0 ? 'Faol' : 'Yopilgan'}
              </Text>
            </View>
          </View>
        </TouchableOpacity>
      )}
    />
  );
}

// ── Worker Applications ───────────────────────────────────────────────────────
function WorkerApplicationsList({ colors }: { colors: any }) {
  const [applications, setApplications] = useState<JobApplication[]>([]);
  const [loading,      setLoading]      = useState(true);
  const [refreshing,   setRefreshing]   = useState(false);
  const [error,        setError]        = useState<string | null>(null);
  const [activeFilter, setActiveFilter] = useState(FILTERS[0]);
  const { t } = useLanguageStore();

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
      <FilterTabs filters={FILTERS} active={activeFilter} onChange={setActiveFilter} colors={colors} />

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
              <TouchableOpacity key={app.id} style={[styles.card, { backgroundColor: colors.card, ...Shadow.md }]} activeOpacity={0.85} onPress={() => router.push({ pathname: '/application-detail', params: { id: app.id } })}>
                <View style={styles.cardHeader}>
                  <View style={[styles.logo, { backgroundColor: colors.primaryLight }]}>
                    <Text style={[styles.logoText, { color: colors.primary }]}>
                      {(app.jobTitle ?? '?')[0].toUpperCase()}
                    </Text>
                  </View>
                  <View style={styles.cardInfo}>
                    <Text style={[styles.jobTitle, { color: colors.textPrimary }]} numberOfLines={1}>{app.jobTitle}</Text>
                    <Text style={[styles.subText, { color: colors.textSecondary }]} numberOfLines={1}>{app.workerName}</Text>
                  </View>
                  <View style={[styles.badge, { backgroundColor: statusColor.bg }]}>
                    <Text style={[styles.badgeText, { color: statusColor.text }]}>{STATUS_LABEL[app.status]}</Text>
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
              </TouchableOpacity>
            );
          })}
          <View style={{ height: 24 }} />
        </ScrollView>
      )}
    </>
  );
}

// ── Worker Posts ──────────────────────────────────────────────────────────────
function WorkerPostsList({ colors }: { colors: any }) {
  const [posts,        setPosts]        = useState<WorkerPost[]>([]);
  const [loading,      setLoading]      = useState(true);
  const [refreshing,   setRefreshing]   = useState(false);
  const [error,        setError]        = useState<string | null>(null);
  const [activeFilter, setActiveFilter] = useState('Barchasi');
  const { t } = useLanguageStore();

  const load = useCallback(async () => {
    try {
      const data = await workerPostService.getMyPosts();
      setPosts(data);
      setError(null);
    } catch (e: any) {
      setError(e?.message ?? 'Xatolik yuz berdi');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => { load(); }, []);

  const filtered = activeFilter === 'Barchasi'
    ? posts
    : posts.filter(p => WORKER_POST_STATUS_LABEL[p.status]?.label === activeFilter);

  if (loading) return <View style={{ padding: Spacing.xl }}><ApplicationListSkeleton count={4} /></View>;

  if (error) return (
    <View style={styles.centerBox}>
      <Text style={[styles.stateText, { color: '#EF4444' }]}>{error}</Text>
      <TouchableOpacity style={[styles.retryBtn, { backgroundColor: colors.primary }]}
        onPress={() => { setLoading(true); load(); }}>
        <Text style={styles.retryText}>Qayta urinish</Text>
      </TouchableOpacity>
    </View>
  );

  return (
    <>
      <FilterTabs filters={POST_FILTERS} active={activeFilter} onChange={setActiveFilter} colors={colors} />

      {filtered.length === 0 ? (
        <View style={styles.centerBox}>
          <Text style={[styles.stateText, { color: colors.textSecondary }]}>
            {activeFilter === 'Barchasi' ? "Hali e'lon joylashtirilmagan" : "Bu bo'limda e'lon yo'q"}
          </Text>
          {activeFilter === 'Barchasi' && (
            <TouchableOpacity style={[styles.retryBtn, { backgroundColor: colors.primary }]}
              onPress={() => router.push('/post-worker')}>
              <Text style={styles.retryText}>E'lon qo'shish</Text>
            </TouchableOpacity>
          )}
        </View>
      ) : (
        <ScrollView
          showsVerticalScrollIndicator={false}
          contentContainerStyle={styles.listContainer}
          refreshControl={
            <RefreshControl refreshing={refreshing} onRefresh={() => { setRefreshing(true); load(); }}
              tintColor={colors.primary} colors={[colors.primary]} />
          }
        >
          {filtered.map(post => {
            const statusInfo = WORKER_POST_STATUS_LABEL[post.status] ?? WORKER_POST_STATUS_LABEL[1];
            return (
              <TouchableOpacity
                key={post.id}
                style={[styles.card, { backgroundColor: colors.card, ...Shadow.md }]}
                activeOpacity={0.85}
                onPress={() => router.push({ pathname: '/worker-post-detail', params: { id: post.id } })}
              >
                <View style={styles.cardHeader}>
                  <View style={[styles.logo, { backgroundColor: colors.primaryLight }]}>
                    <BriefcaseIcon size={22} color={colors.primary} />
                  </View>
                  <View style={styles.cardInfo}>
                    <Text style={[styles.jobTitle, { color: colors.textPrimary }]} numberOfLines={1}>{post.title}</Text>
                    <Text style={[styles.subText, { color: colors.textSecondary }]} numberOfLines={1}>
                      {post.city}, {post.country}
                    </Text>
                  </View>
                  <View style={[styles.badge, { backgroundColor: statusInfo.bg }]}>
                    <Text style={[styles.badgeText, { color: statusInfo.text }]}>{statusInfo.label}</Text>
                  </View>
                </View>
                <View style={[styles.divider, { backgroundColor: colors.border }]} />
                <View style={styles.metaRow}>
                  <View style={styles.metaItem}>
                    <ClockIcon size={12} color={colors.textTertiary} />
                    <Text style={[styles.metaText, { color: colors.textTertiary }]}>{formatDate(post.createdAt)}</Text>
                  </View>
                  <View style={styles.metaItem}>
                    <EyeIcon size={12} color={colors.textTertiary} />
                    <Text style={[styles.metaText, { color: colors.textTertiary }]}>{post.viewCount ?? 0} ko'rish</Text>
                  </View>
                  {post.expectedSalary > 0 && (
                    <Text style={[styles.metaText, { color: colors.primary, fontWeight: FontWeight.semiBold }]}>
                      {post.expectedSalary.toLocaleString()} so'm
                    </Text>
                  )}
                </View>
              </TouchableOpacity>
            );
          })}
          <View style={{ height: 24 }} />
        </ScrollView>
      )}
    </>
  );
}

// ── Worker Main ───────────────────────────────────────────────────────────────
function WorkerView({ colors }: { colors: any }) {
  const [activeTab, setActiveTab] = useState<'applications' | 'posts'>('applications');

  return (
    <View style={{ flex: 1 }}>
      <View style={[styles.tabRow, { borderBottomColor: colors.border }]}>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'applications' && { borderBottomColor: colors.primary, borderBottomWidth: 2 }]}
          onPress={() => setActiveTab('applications')}
        >
          <Text style={[styles.tabText, { color: activeTab === 'applications' ? colors.primary : colors.textSecondary }]}>
            Arizalarim
          </Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'posts' && { borderBottomColor: colors.primary, borderBottomWidth: 2 }]}
          onPress={() => setActiveTab('posts')}
        >
          <Text style={[styles.tabText, { color: activeTab === 'posts' ? colors.primary : colors.textSecondary }]}>
            E'lonlarim
          </Text>
        </TouchableOpacity>
      </View>

      {activeTab === 'applications'
        ? <WorkerApplicationsList colors={colors} />
        : <WorkerPostsList colors={colors} />
      }
    </View>
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
        ? <EmployerJobsView colors={colors} />
        : <WorkerView colors={colors} />
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
  tabRow: {
    flexDirection: 'row',
    borderBottomWidth: 1,
    marginHorizontal: Spacing.xl,
  },
  tab:             { flex: 1, alignItems: 'center', paddingVertical: Spacing.md },
  tabText:         { fontSize: FontSize.md, fontWeight: FontWeight.semiBold },
  filterContainer: {
    paddingHorizontal: 16,
    paddingVertical: 10,
    flexDirection: 'row',
    gap: 8,
  },
  filterTab: {
  paddingHorizontal: 12,
  paddingVertical: 6,
  borderRadius: 20,
  borderWidth: 1.5,
  alignSelf: 'flex-start',
},
  filterText:      { fontSize: 12, fontWeight: '600' },
  listContainer:   { padding: Spacing.xl, gap: Spacing.md },
  card:            { borderRadius: BorderRadius.xl, padding: Spacing.lg },
  cardHeader:      { flexDirection: 'row', alignItems: 'center' },
  logo: {
    width: 48, height: 48, borderRadius: BorderRadius.md,
    alignItems: 'center', justifyContent: 'center', marginRight: Spacing.md,
  },
  logoText:        { fontSize: FontSize.lg, fontWeight: FontWeight.bold },
  cardInfo:        { flex: 1 },
  jobTitle:        { fontSize: FontSize.md, fontWeight: FontWeight.bold, marginBottom: 2 },
  subText:         { fontSize: FontSize.sm },
  badge:           { borderRadius: BorderRadius.sm, paddingHorizontal: Spacing.sm, paddingVertical: Spacing.xs },
  badgeText:       { fontSize: FontSize.xs, fontWeight: FontWeight.bold },
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
  retryText:       { color: '#fff', fontWeight: FontWeight.semiBold, fontSize: FontSize.sm },
});