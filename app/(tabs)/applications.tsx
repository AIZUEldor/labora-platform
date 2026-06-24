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
import { LinearGradient } from 'expo-linear-gradient';

const STATUS_COLOR: Record<number, { bg: string; text: string }> = {
  [ApplicationStatus.Pending]:   { bg: '#FEF9C3', text: '#854D0E' },
  [ApplicationStatus.Accepted]:  { bg: '#DCFCE7', text: '#166534' },
  [ApplicationStatus.Rejected]:  { bg: '#FEE2E2', text: '#991B1B' },
  [ApplicationStatus.Cancelled]: { bg: '#F3F4F6', text: '#6B7280' },
  [ApplicationStatus.Completed]: { bg: '#EFF6FF', text: '#1D4ED8' },
};

const WORKER_POST_STATUS: Record<number, { bg: string; text: string }> = {
  1: { bg: '#DCFCE7', text: '#166534' },
  2: { bg: '#F3F4F6', text: '#6B7280' },
  3: { bg: '#EFF6FF', text: '#1D4ED8' },
};

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
    <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.filterContainer}>
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
          <Text style={[styles.filterText, { color: colors.textSecondary }, active === filter && { color: '#fff' }]}>
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
        onPress={() => router.push('/employer/post-job')}>
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
              <Text style={[styles.subText,  { color: colors.textSecondary }]} numberOfLines={1}>{item.location}</Text>
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
                {item.status === 0 ? t.employer.publish : t.common.done}
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
  const { t } = useLanguageStore();
  const [applications, setApplications] = useState<JobApplication[]>([]);
  const [loading,      setLoading]      = useState(true);
  const [refreshing,   setRefreshing]   = useState(false);
  const [error,        setError]        = useState<string | null>(null);
  const [activeFilter, setActiveFilter] = useState('all');

  const STATUS_LABEL: Record<number, string> = {
    [ApplicationStatus.Pending]:   t.applications.pending,
    [ApplicationStatus.Accepted]:  t.applications.accepted,
    [ApplicationStatus.Rejected]:  t.applications.rejected,
    [ApplicationStatus.Cancelled]: t.applications.cancelled,
    [ApplicationStatus.Completed]: t.applications.completed,
  };

  const FILTERS = [
    { key: 'all',       label: t.home.seeAll },
    { key: 'pending',   label: t.applications.pending },
    { key: 'accepted',  label: t.applications.accepted },
    { key: 'rejected',  label: t.applications.rejected },
    { key: 'completed', label: t.applications.completed },
  ];

  const STATUS_KEY: Record<number, string> = {
    [ApplicationStatus.Pending]:   'pending',
    [ApplicationStatus.Accepted]:  'accepted',
    [ApplicationStatus.Rejected]:  'rejected',
    [ApplicationStatus.Cancelled]: 'cancelled',
    [ApplicationStatus.Completed]: 'completed',
  };

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

  const filtered = activeFilter === 'all'
    ? applications
    : applications.filter(a => STATUS_KEY[a.status] === activeFilter);

  return (
    <>
      <FilterTabs
        filters={FILTERS.map(f => f.label)}
        active={FILTERS.find(f => f.key === activeFilter)?.label ?? FILTERS[0].label}
        onChange={label => {
          const found = FILTERS.find(f => f.label === label);
          if (found) setActiveFilter(found.key);
        }}
        colors={colors}
      />

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
          <Text style={[styles.stateText, { color: colors.textSecondary }]}>{t.applications.noApplications}</Text>
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
              <TouchableOpacity
                key={app.id}
                style={[styles.card, { backgroundColor: colors.card, ...Shadow.md }]}
                activeOpacity={0.85}
                onPress={() => router.push({ pathname: '/application-detail', params: { id: app.id } })}
              >
                <View style={styles.cardHeader}>
                  <View style={[styles.logo, { backgroundColor: colors.primaryLight }]}>
                    <Text style={[styles.logoText, { color: colors.primary }]}>
                      {(app.jobTitle ?? '?')[0].toUpperCase()}
                    </Text>
                  </View>
                  <View style={styles.cardInfo}>
                    <Text style={[styles.jobTitle, { color: colors.textPrimary }]} numberOfLines={1}>{app.jobTitle}</Text>
                    <Text style={[styles.subText,  { color: colors.textSecondary }]} numberOfLines={1}>{app.workerName}</Text>
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
                      <Text style={[styles.reviewBtnText, { color: colors.primary }]}>{t.profile.reviews}</Text>
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
  const { t } = useLanguageStore();
  const [posts,        setPosts]        = useState<WorkerPost[]>([]);
  const [loading,      setLoading]      = useState(true);
  const [refreshing,   setRefreshing]   = useState(false);
  const [error,        setError]        = useState<string | null>(null);
  const [activeFilter, setActiveFilter] = useState('all');

  const POST_STATUS_LABEL: Record<number, string> = {
    1: t.employer.publish,
    2: t.common.no,
    3: t.applications.accepted,
  };

  const POST_STATUS_KEY: Record<number, string> = { 1: 'active', 2: 'inactive', 3: 'accepted' };

  const POST_FILTERS = [
    { key: 'all',      label: t.home.seeAll },
    { key: 'active',   label: t.employer.publish },
    { key: 'inactive', label: t.common.no },
    { key: 'accepted', label: t.applications.accepted },
  ];

  const load = useCallback(async () => {
    try {
      const data = await workerPostService.getMyPosts();
      setPosts(data);
      setError(null);
    } catch (e: any) {
      setError(e?.message ?? t.common.somethingWentWrong);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => { load(); }, []);

  const filtered = activeFilter === 'all'
    ? posts
    : posts.filter(p => POST_STATUS_KEY[p.status] === activeFilter);

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

  return (
    <>
      <FilterTabs
        filters={POST_FILTERS.map(f => f.label)}
        active={POST_FILTERS.find(f => f.key === activeFilter)?.label ?? POST_FILTERS[0].label}
        onChange={label => {
          const found = POST_FILTERS.find(f => f.label === label);
          if (found) setActiveFilter(found.key);
        }}
        colors={colors}
      />

      {filtered.length === 0 ? (
        <View style={styles.centerBox}>
          <Text style={[styles.stateText, { color: colors.textSecondary }]}>
            {activeFilter === 'all' ? t.home.noJobs : t.applications.noApplications}
          </Text>
          {activeFilter === 'all' && (
            <TouchableOpacity style={[styles.retryBtn, { backgroundColor: colors.primary }]}
              onPress={() => router.push('/post-worker')}>
              <Text style={styles.retryText}>{t.employer.postJob}</Text>
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
            const statusInfo = WORKER_POST_STATUS[post.status] ?? WORKER_POST_STATUS[1];
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
                    <Text style={[styles.subText,  { color: colors.textSecondary }]} numberOfLines={1}>
                      {post.city}, {post.country}
                    </Text>
                  </View>
                  <View style={[styles.badge, { backgroundColor: statusInfo.bg }]}>
                    <Text style={[styles.badgeText, { color: statusInfo.text }]}>{POST_STATUS_LABEL[post.status]}</Text>
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
                    <Text style={[styles.metaText, { color: colors.textTertiary }]}>{post.viewCount ?? 0}</Text>
                  </View>
                  {post.expectedSalary > 0 && (
                    <Text style={[styles.metaText, { color: colors.primary, fontWeight: FontWeight.semiBold }]}>
                      {post.expectedSalary.toLocaleString()} {t.common.currency}
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
  const { t } = useLanguageStore();
  const [activeTab, setActiveTab] = useState<'applications' | 'posts'>('applications');

  return (
    <View style={{ flex: 1 }}>
      <View style={[styles.tabRow, { borderBottomColor: colors.border }]}>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'applications' && { borderBottomColor: colors.primary, borderBottomWidth: 2 }]}
          onPress={() => setActiveTab('applications')}
        >
          <Text style={[styles.tabText, { color: activeTab === 'applications' ? colors.primary : colors.textSecondary }]}>
            {t.applications.title}
          </Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'posts' && { borderBottomColor: colors.primary, borderBottomWidth: 2 }]}
          onPress={() => setActiveTab('posts')}
        >
          <Text style={[styles.tabText, { color: activeTab === 'posts' ? colors.primary : colors.textSecondary }]}>
            {t.employer.myJobs}
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

// ── Guest View ────────────────────────────────────────────────────────────────
function GuestView({ colors, isDark }: { colors: any; isDark: boolean }) {
  const { t } = useLanguageStore();
  return (
    <View style={styles.guestContainer}>
      <View style={[styles.guestIconBox, { backgroundColor: colors.primaryLight }]}>
        <BriefcaseIcon size={40} color={colors.primary} />
      </View>
      <Text style={[styles.guestTitle, { color: colors.textPrimary }]}>
        {t.profile.loginRequired}
      </Text>
      <Text style={[styles.guestSubtitle, { color: colors.textSecondary }]}>
        {t.applications.noApplications}
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
  );
}

// ── Main ──────────────────────────────────────────────────────────────────────
export default function ApplicationsScreen() {
  const { colors, isDark } = useThemeStore();
  const { t }              = useLanguageStore();
  const role               = useAuthStore((state: AuthState) => state.role);
  const token              = useAuthStore((state: AuthState) => state.token);
  const isEmployer         = Number(role) === UserRole.Employer;

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={[styles.header, { backgroundColor: colors.surface }]}>
        <Text style={[styles.headerTitle, { color: colors.textPrimary }]}>
          {isEmployer ? t.employer.myJobs : t.applications.title}
        </Text>
      </View>

      {!token
        ? <GuestView colors={colors} isDark={isDark} />
        : isEmployer
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
  tabRow:          { flexDirection: 'row', borderBottomWidth: 1, marginHorizontal: Spacing.xl },
  tab:             { flex: 1, alignItems: 'center', paddingVertical: Spacing.md },
  tabText:         { fontSize: FontSize.md, fontWeight: FontWeight.semiBold },
  filterContainer: { paddingHorizontal: 16, paddingVertical: 10, flexDirection: 'row', gap: 8 },
  filterTab:       { paddingHorizontal: 12, paddingVertical: 6, borderRadius: 20, borderWidth: 1.5, alignSelf: 'flex-start' },
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
  retryBtn:        { paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md, borderRadius: BorderRadius.lg, marginTop: 4 },
  retryText:       { color: '#fff', fontWeight: FontWeight.semiBold, fontSize: FontSize.sm },
  // Guest styles
  guestContainer: {
    flex: 1, alignItems: 'center', justifyContent: 'center',
    paddingHorizontal: Spacing.xl, gap: Spacing.lg,
  },
  guestIconBox: {
    width: 80, height: 80, borderRadius: BorderRadius.full,
    alignItems: 'center', justifyContent: 'center',
  },
  guestTitle:    { fontSize: FontSize.xl, fontWeight: FontWeight.bold, textAlign: 'center' },
  guestSubtitle: { fontSize: FontSize.md, textAlign: 'center', lineHeight: 22 },
  loginBtn:      { width: '100%', borderRadius: BorderRadius.xl, overflow: 'hidden', marginTop: Spacing.sm },
  loginBtnGradient: { height: 52, alignItems: 'center', justifyContent: 'center' },
  loginBtnText:  { fontSize: FontSize.md, fontWeight: FontWeight.bold, color: '#FFFFFF' },
});
