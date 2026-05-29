import { useEffect, useState, useCallback } from 'react';
import {
  View, Text, StyleSheet, ScrollView,
  TouchableOpacity, ActivityIndicator, RefreshControl, TextInput,
} from 'react-native';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { useThemeStore } from '../../store/themeStore';
import {
  ConstructionIcon, ITIcon, DriverIcon, ChefIcon,
  MedicalIcon, EducationIcon, FinanceIcon, SecurityIcon,
  CleaningIcon, DesignIcon, MarketingIcon, WarehouseIcon,
  SunIcon, MoonIcon, BellIcon, MapPinIcon, ClockIcon,SearchIcon,LocationIcon,
} from '../../components/icons';
import React from 'react';
import { JobListSkeleton } from '../../components/SkeletonLoader';
import { router } from 'expo-router';
import { jobService } from '../../services/jobService';
import { categoryService } from '../../services/categoryService';
import { Job, Category } from '../../types';

function CategoryIcon({ name, color }: { name: string; color: string }) {
  const props = { size: 28, color };
  const map: Record<string, React.ReactElement> = {
    construction: <ConstructionIcon {...props} />,
    it:           <ITIcon {...props} />,
    driver:       <DriverIcon {...props} />,
    chef:         <ChefIcon {...props} />,
    medical:      <MedicalIcon {...props} />,
    education:    <EducationIcon {...props} />,
    finance:      <FinanceIcon {...props} />,
    security:     <SecurityIcon {...props} />,
    cleaning:     <CleaningIcon {...props} />,
    design:       <DesignIcon {...props} />,
    marketing:    <MarketingIcon {...props} />,
    warehouse:    <WarehouseIcon {...props} />,
  };
  return map[name?.toLowerCase()] ?? <ITIcon {...props} />;
}

export default function HomeScreen() {
  const { colors, isDark, toggleTheme } = useThemeStore();

  const [jobs,        setJobs]        = useState<Job[]>([]);
  const [categories,  setCategories]  = useState<Category[]>([]);
  const [loading,     setLoading]     = useState(true);
  const [refreshing,  setRefreshing]  = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error,       setError]       = useState<string | null>(null);
  const [search,      setSearch]      = useState('');
  const [selectedCat, setSelectedCat] = useState<string | null>(null);
  const [page,        setPage]        = useState(1);
  const [totalCount,  setTotalCount]  = useState(0);

  const PAGE_SIZE = 10;

  const fetchJobs = useCallback(async (p: number, reset: boolean) => {
    try {
      let res: any;
      if (search)       res = await jobService.searchJobs(search, p);
      else if (selectedCat) res = await jobService.getJobsByCategory(selectedCat, p);
      else              res = await jobService.getJobs(p, PAGE_SIZE);

      setTotalCount(res.totalCount ?? 0);
      setJobs(prev => reset ? res.items : [...prev, ...res.items]);
    } catch (e: any) {
      setError(e?.message ?? 'Xatolik yuz berdi');
    }
  }, [search, selectedCat]);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError(null);
      setPage(1);
      try {
        const [, cats] = await Promise.all([
          fetchJobs(1, true),
          categories.length === 0 ? categoryService.getCategories() : Promise.resolve(null),
        ]);
        if (cats) setCategories(cats as Category[]);
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    };
    load();
  }, [search, selectedCat]);

  useEffect(() => {
    if (page === 1) return;
    const load = async () => {
      setLoadingMore(true);
      await fetchJobs(page, false);
      setLoadingMore(false);
    };
    load();
  }, [page]);

  const onRefresh = () => {
    setRefreshing(true);
    setSearch('');
    setSelectedCat(null);
    setPage(1);
  };

  const loadMore = () => {
    if (loadingMore || jobs.length >= totalCount) return;
    setPage(p => p + 1);
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>

      {/* Header */}
      <View style={[styles.header, { backgroundColor: colors.surface, ...Shadow.sm }]}>
        <View>
          <Text style={[styles.greeting,   { color: colors.textSecondary }]}>Salom! 👋</Text>
          <Text style={[styles.headerTitle, { color: colors.textPrimary   }]}>Ishlarni toping</Text>
        </View>
        <View style={styles.headerRight}>
          <TouchableOpacity
            style={[styles.iconButton, { backgroundColor: colors.primaryLight }]}
            onPress={toggleTheme} activeOpacity={0.8}
          >
            {isDark ? <SunIcon size={20} color={colors.primary} /> : <MoonIcon size={20} color={colors.primary} />}
          </TouchableOpacity>
          <TouchableOpacity style={[styles.iconButton, { backgroundColor: colors.primaryLight }]}>
            <BellIcon size={20} color={colors.primary} />
          </TouchableOpacity>
        </View>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        onScroll={({ nativeEvent: { layoutMeasurement, contentOffset, contentSize } }) => {
          if (layoutMeasurement.height + contentOffset.y >= contentSize.height - 80) loadMore();
        }}
        scrollEventThrottle={400}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh}
            tintColor={colors.primary} colors={[colors.primary]} />
        }
      >
        {/* Search */}
        <View style={[styles.searchContainer, { backgroundColor: colors.surface, borderColor: colors.border }]}>
          <SearchIcon size={18} color={colors.textTertiary} />
          <TextInput
            style={[styles.searchInput, { color: colors.textPrimary }]}
            placeholder="Ish qidiring..."
            placeholderTextColor={colors.textTertiary}
            value={search}
            onChangeText={t => { setSearch(t); setPage(1); }}
            returnKeyType="search"
            autoCapitalize="none"
          />
          {search.length > 0 && (
            <TouchableOpacity onPress={() => setSearch('')}>
              <Text style={{ color: colors.textTertiary, fontSize: 16, paddingLeft: 4 }}>✕</Text>
            </TouchableOpacity>
          )}
        </View>

        {/* Kategoriyalar */}
        <View style={styles.sectionHeader}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>Kategoriyalar</Text>
          {selectedCat !== null && (
            <TouchableOpacity onPress={() => setSelectedCat(null)}>
              <Text style={[styles.seeAll, { color: colors.primary }]}>Barchasi</Text>
            </TouchableOpacity>
          )}
        </View>

        {categories.length > 0 && (
          <ScrollView horizontal showsHorizontalScrollIndicator={false}
            contentContainerStyle={styles.categoriesContainer}>
            {categories.map(cat => {
              const isSelected = selectedCat === cat.id;
              return (
                <TouchableOpacity
                  key={cat.id}
                  onPress={() => setSelectedCat(isSelected ? null : cat.id)}
                  style={[styles.categoryCard, {
                    backgroundColor: isSelected ? colors.primary : colors.card, ...Shadow.sm,
                  }]}
                  activeOpacity={0.8}
                >
                  <CategoryIcon name={cat.name?.toLowerCase()} color={isSelected ? '#fff' : colors.primary} />
                  <Text style={[styles.categoryName, { color: isSelected ? '#fff' : colors.textSecondary }]}>
                    {cat.name}
                  </Text>
                </TouchableOpacity>
              );
            })}
          </ScrollView>
        )}

        {/* Ishlar */}
        <View style={styles.sectionHeader}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
            {selectedCat ? categories.find(c => c.id === selectedCat)?.name ?? 'Ishlar' : 'Yangi ishlar'}
          </Text>
          {!loading && (
            <Text style={[styles.seeAll, { color: colors.textTertiary }]}>{totalCount} ta</Text>
          )}
        </View>

        {loading && (
  <View style={{ paddingHorizontal: Spacing.xl, paddingTop: Spacing.md }}>
    <JobListSkeleton count={4} />
  </View>
)}

        {!loading && error && (
          <View style={styles.centerBox}>
            <Text style={[styles.stateText, { color: '#EF4444' }]}>{error}</Text>
            <TouchableOpacity style={[styles.retryBtn, { backgroundColor: colors.primary }]}
              onPress={() => { setError(null); setPage(1); }} activeOpacity={0.8}>
              <Text style={styles.retryText}>Qayta urinish</Text>
            </TouchableOpacity>
          </View>
        )}

        {!loading && !error && jobs.length === 0 && (
          <View style={styles.centerBox}>
            <Text style={[styles.stateText, { color: colors.textSecondary }]}>
              {search ? `"${search}" bo'yicha natija topilmadi` : "Hozircha ish e'lonlari yo'q"}
            </Text>
          </View>
        )}

        {!loading && !error && jobs.map(job => (
          <TouchableOpacity
            key={job.id}
            style={[styles.jobCard, { backgroundColor: colors.card, ...Shadow.md }]}
            activeOpacity={0.85}
            onPress={() => router.push({ pathname: '/job-detail', params: { id: job.id } })}
          >
            <View style={styles.jobCardTop}>
              <View style={[styles.companyLogo, { backgroundColor: colors.primaryLight }]}>
                <Text style={[styles.companyLogoText, { color: colors.primary }]}>
                  {(job.employerName ?? job.title ?? '?')[0].toUpperCase()}
                </Text>
              </View>
              <View style={styles.jobInfo}>
                <Text style={[styles.jobTitle,   { color: colors.textPrimary   }]} numberOfLines={1}>{job.title}</Text>
                <Text style={[styles.companyName, { color: colors.textSecondary }]} numberOfLines={1}>{job.employerName}</Text>
              </View>
              <View style={[styles.salaryBadge, { backgroundColor: colors.primaryLight }]}>
                <Text style={[styles.salaryText, { color: colors.primary }]}>
                  {job.salary ? `${(job.salary / 1_000_000).toFixed(1)}M so'm` : 'Kelishiladi'}
                </Text>
              </View>
            </View>
            <View style={styles.jobCardBottom}>
              {job.location && (
                <View style={styles.jobMeta}>
                  <LocationIcon size={12} color={colors.textTertiary} />
                  <Text style={[styles.jobMetaText, { color: colors.textTertiary }]} numberOfLines={1}>
                    {job.location}
                  </Text>
                </View>
              )}
              {job.distance !== undefined && (
                <View style={styles.jobMeta}>
                  <ClockIcon size={12} color={colors.textTertiary} />
                  <Text style={[styles.jobMetaText, { color: colors.textTertiary }]}>
                    {job.distance.toFixed(1)} km
                  </Text>
                </View>
              )}
            </View>
          </TouchableOpacity>
        ))}

        {loadingMore && (
          <View style={{ paddingVertical: 16, alignItems: 'center' }}>
            <ActivityIndicator size="small" color={colors.primary} />
          </View>
        )}

        <View style={{ height: 24 }} />
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: {
    flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center',
    paddingHorizontal: Spacing.xl, paddingTop: 56, paddingBottom: Spacing.lg,
  },
  greeting:    { fontSize: FontSize.sm,  fontWeight: FontWeight.medium },
  headerTitle: { fontSize: FontSize.xxl, fontWeight: FontWeight.bold   },
  headerRight: { flexDirection: 'row', gap: Spacing.sm },
  iconButton: {
    width: 44, height: 44, borderRadius: BorderRadius.full,
    alignItems: 'center', justifyContent: 'center',
  },
  searchContainer: {
    flexDirection: 'row', alignItems: 'center',
    marginHorizontal: Spacing.xl, marginTop: Spacing.lg, marginBottom: Spacing.sm,
    borderRadius: BorderRadius.lg, paddingHorizontal: Spacing.lg,
    height: 52, borderWidth: 1.5, gap: Spacing.sm,
  },
  searchInput: { flex: 1, fontSize: FontSize.md, paddingVertical: 0 },
  sectionHeader: {
    flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center',
    paddingHorizontal: Spacing.xl, marginTop: Spacing.xl, marginBottom: Spacing.md,
  },
  sectionTitle: { fontSize: FontSize.lg, fontWeight: FontWeight.bold    },
  seeAll:       { fontSize: FontSize.sm, fontWeight: FontWeight.semiBold },
  categoriesContainer: { paddingHorizontal: Spacing.xl, gap: Spacing.md },
  categoryCard: {
    alignItems: 'center', justifyContent: 'center', borderRadius: BorderRadius.lg,
    paddingVertical: Spacing.md, paddingHorizontal: Spacing.lg, gap: Spacing.xs, minWidth: 80,
  },
  categoryName: { fontSize: FontSize.xs, fontWeight: FontWeight.semiBold },
  jobCard: {
    borderRadius: BorderRadius.xl,
    marginHorizontal: Spacing.xl, marginBottom: Spacing.md, padding: Spacing.lg,
  },
  jobCardTop:      { flexDirection: 'row', alignItems: 'center', marginBottom: Spacing.md },
  companyLogo: {
    width: 48, height: 48, borderRadius: BorderRadius.md,
    alignItems: 'center', justifyContent: 'center', marginRight: Spacing.md,
  },
  companyLogoText: { fontSize: FontSize.lg, fontWeight: FontWeight.bold },
  jobInfo:         { flex: 1 },
  jobTitle:        { fontSize: FontSize.md, fontWeight: FontWeight.bold, marginBottom: 2 },
  companyName:     { fontSize: FontSize.sm },
  salaryBadge: {
    borderRadius: BorderRadius.sm,
    paddingHorizontal: Spacing.sm, paddingVertical: Spacing.xs,
  },
  salaryText:    { fontSize: FontSize.xs, fontWeight: FontWeight.bold },
  jobCardBottom: { flexDirection: 'row', gap: Spacing.lg },
  jobMeta:       { flexDirection: 'row', alignItems: 'center', gap: 4 },
  jobMetaText:   { fontSize: FontSize.xs, fontWeight: FontWeight.medium },
  centerBox: {
    alignItems: 'center', justifyContent: 'center',
    paddingVertical: 40, paddingHorizontal: Spacing.xl, gap: 12,
  },
  stateText: { fontSize: FontSize.sm, textAlign: 'center' },
  retryBtn: {
    paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md,
    borderRadius: BorderRadius.lg, marginTop: 4,
  },
  retryText: { color: '#fff', fontWeight: FontWeight.semiBold, fontSize: FontSize.sm },
});