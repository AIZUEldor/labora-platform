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
  SunIcon, MoonIcon, BellIcon, ClockIcon, SearchIcon, LocationIcon,
  BriefcaseIcon, TradeIcon, AgricultureIcon, ManufacturingIcon, CourierIcon,
  LegalIcon, HRIcon, RealEstateIcon, BeautyIcon, AutoServiceIcon, TextileIcon,
} from '../../components/icons';
import React from 'react';
import { JobListSkeleton } from '../../components/SkeletonLoader';
import { router } from 'expo-router';
import { jobService } from '../../services/jobService';
import { categoryService } from '../../services/categoryService';
import { Job, Category } from '../../types';
import { useAuthStore, AuthState } from '../../store/authStore';
import { useLanguageStore } from '../../stores/useLanguageStore';

const CATEGORY_NAMES: Record<string, { uz: string; ru: string; en: string }> = {
  'Daily':        { uz: 'Kunlik ishlar',        ru: 'Ежедневная работа',   en: 'Daily jobs' },
  'IT':           { uz: 'IT',                   ru: 'ИТ',                  en: 'IT' },
  'Construction': { uz: 'Qurilish',             ru: 'Строительство',       en: 'Construction' },
  'Driver':       { uz: 'Haydovchi',            ru: 'Водитель',            en: 'Driver' },
  'Chef':         { uz: 'Oshpaz',               ru: 'Повар',               en: 'Chef' },
  'Medical':      { uz: 'Tibbiyot',             ru: 'Медицина',            en: 'Medical' },
  'Education':    { uz: "Ta'lim",               ru: 'Образование',         en: 'Education' },
  'Finance':      { uz: 'Moliya',               ru: 'Финансы',             en: 'Finance' },
  'Security':     { uz: 'Qorovul',              ru: 'Охрана',              en: 'Security' },
  'Cleaning':     { uz: 'Tozalik',              ru: 'Уборка',              en: 'Cleaning' },
  'Design':       { uz: 'Dizayn',               ru: 'Дизайн',              en: 'Design' },
  'Marketing':    { uz: 'Marketing',            ru: 'Маркетинг',           en: 'Marketing' },
  'Warehouse':    { uz: 'Ombor',                ru: 'Склад',               en: 'Warehouse' },
  'Trade':        { uz: 'Savdo',                ru: 'Торговля',            en: 'Trade' },
  'Agriculture':  { uz: "Qishloq xo'jaligi",   ru: 'Сельское хозяйство',  en: 'Agriculture' },
  'Manufacturing':{ uz: 'Ishlab chiqarish',     ru: 'Производство',        en: 'Manufacturing' },
  'Courier':      { uz: 'Kuryer',               ru: 'Курьер',              en: 'Courier' },
  'Legal':        { uz: 'Huquq',                ru: 'Юридические услуgi',  en: 'Legal' },
  'HR':           { uz: 'Kadrlar',              ru: 'Кадры',               en: 'HR' },
  'Real Estate':  { uz: "Ko'chmas mulk",        ru: 'Недвижимость',        en: 'Real Estate' },
  'Beauty':       { uz: "Go'zallik",            ru: 'Красота',             en: 'Beauty' },
  'Auto Service': { uz: "Avto ta'mir",          ru: 'Авто сервис',         en: 'Auto Service' },
  'Textile':      { uz: "To'qimachilik",        ru: 'Текстиль',            en: 'Textile' },
};

function CategoryIcon({ name, color }: { name: string; color: string }) {
  const props = { size: 28, color };
  const map: Record<string, React.ReactElement> = {
    construction:  <ConstructionIcon {...props} />,
    it:            <ITIcon {...props} />,
    driver:        <DriverIcon {...props} />,
    chef:          <ChefIcon {...props} />,
    medical:       <MedicalIcon {...props} />,
    education:     <EducationIcon {...props} />,
    finance:       <FinanceIcon {...props} />,
    security:      <SecurityIcon {...props} />,
    cleaning:      <CleaningIcon {...props} />,
    design:        <DesignIcon {...props} />,
    marketing:     <MarketingIcon {...props} />,
    warehouse:     <WarehouseIcon {...props} />,
    daily:         <BriefcaseIcon {...props} />,
    trade:         <TradeIcon {...props} />,
    agriculture:   <AgricultureIcon {...props} />,
    manufacturing: <ManufacturingIcon {...props} />,
    courier:       <CourierIcon {...props} />,
    legal:         <LegalIcon {...props} />,
    hr:            <HRIcon {...props} />,
    'real estate': <RealEstateIcon {...props} />,
    beauty:        <BeautyIcon {...props} />,
    'auto service':<AutoServiceIcon {...props} />,
    textile:       <TextileIcon {...props} />,
  };
  return map[name?.toLowerCase()] ?? <BriefcaseIcon {...props} />;
}

export default function HomeScreen() {
  const { colors, isDark, toggleTheme } = useThemeStore();
 const { language } = useLanguageStore();

  const firstName = useAuthStore((state: AuthState) => state.firstName);
  const role = useAuthStore((state: AuthState) => state.role);

  const [jobs,           setJobs]           = useState<Job[]>([]);
  const [categories,     setCategories]     = useState<Category[]>([]);
  const [loading,        setLoading]        = useState(true);
  const [refreshing,     setRefreshing]     = useState(false);
  const [loadingMore,    setLoadingMore]    = useState(false);
  const [error,          setError]          = useState<string | null>(null);
  const [search,         setSearch]         = useState('');
  const [selectedCat,    setSelectedCat]    = useState<string | null>(null);
  const [selectedSubCat, setSelectedSubCat] = useState<string | null>(null);
  const [page,           setPage]           = useState(1);
  const [totalCount,     setTotalCount]     = useState(0);

  const PAGE_SIZE = 10;

  const fetchJobs = useCallback(async (p: number, reset: boolean) => {
    try {
      let res: any;
      if (search)              res = await jobService.searchJobs(search, p);
      else if (selectedSubCat) res = await jobService.getJobsByCategory(selectedSubCat, p);
      else if (selectedCat)    res = await jobService.getJobsByCategory(selectedCat, p);
      else                     res = await jobService.getJobs(p, PAGE_SIZE);

      setTotalCount(res.totalCount ?? 0);
      setJobs(prev => reset ? res.items : [...prev, ...res.items]);
    } catch (e: any) {
      setError(e?.message ?? t.common.somethingWentWrong);
    }
  }, [search, selectedCat, selectedSubCat]);

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
  }, [search, selectedCat, selectedSubCat]);

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
    setSelectedSubCat(null);
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
          <Text style={[styles.greeting, { color: colors.textSecondary }]}>
            Salom, {firstName || 'Foydalanuvchi'}!
          </Text>
          <Text style={[styles.headerTitle, { color: colors.textPrimary }]}>
            Ishlarni toping
          </Text>
        </View>
        <View style={styles.headerRight}>
          <TouchableOpacity
            style={[styles.iconButton, { backgroundColor: colors.primaryLight }]}
            onPress={toggleTheme} activeOpacity={0.8}
          >
            {isDark
              ? <SunIcon size={20} color={colors.primary} />
              : <MoonIcon size={20} color={colors.primary} />}
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
            placeholder={t.search.placeholder}
            placeholderTextColor={colors.textTertiary}
            value={search}
            onChangeText={v => { setSearch(v); setPage(1); }}
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
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
            {t.home.categories}
          </Text>
          {selectedCat !== null && (
            <TouchableOpacity onPress={() => { setSelectedCat(null); setSelectedSubCat(null); }}>
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
                  onPress={() => {
                    const newCat = isSelected ? null : cat.id;
                    setSelectedCat(newCat);
                    setSelectedSubCat(null);
                  }}
                  style={[styles.categoryCard, {
                    backgroundColor: isSelected ? colors.primary : colors.card, ...Shadow.sm,
                  }]}
                  activeOpacity={0.8}
                >
                  <CategoryIcon name={cat.name?.toLowerCase()} color={isSelected ? '#fff' : colors.primary} />
                  <Text style={[styles.categoryName, { color: isSelected ? '#fff' : colors.textSecondary }]}>
                    {CATEGORY_NAMES[cat.name]?.[language as 'uz' | 'ru' | 'en'] ?? cat.name}
                  </Text>
                </TouchableOpacity>
              );
            })}
          </ScrollView>
        )}

        {/* Sub-kategoriyalar */}
        {selectedCat && (() => {
          const cat = categories.find(c => c.id === selectedCat);
          const subs = cat?.subCategories ?? [];
          if (subs.length === 0) return null;
          return (
            <ScrollView horizontal showsHorizontalScrollIndicator={false}
              contentContainerStyle={styles.subCategoriesContainer}>
              {subs.map(sub => {
                const isSelected = selectedSubCat === sub.id;
                return (
                  <TouchableOpacity
                    key={sub.id}
                    onPress={() => setSelectedSubCat(isSelected ? null : sub.id)}
                    style={[styles.subCategoryChip, {
                      backgroundColor: isSelected ? colors.primary : colors.card,
                      borderColor: isSelected ? colors.primary : colors.border,
                    }]}
                    activeOpacity={0.8}
                  >
                    <Text style={[styles.subCategoryText, {
                      color: isSelected ? '#fff' : colors.textSecondary,
                    }]}>
                      {sub.name}
                    </Text>
                  </TouchableOpacity>
                );
              })}
            </ScrollView>
          );
        })()}

        {/* Ishlar */}
        <View style={styles.sectionHeader}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
            {selectedCat
              ? categories.find(c => c.id === selectedCat)?.name ?? 'Ishlar'
              : 'Yangi ishlar'}
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
            <TouchableOpacity
              style={[styles.retryBtn, { backgroundColor: colors.primary }]}
              onPress={() => { setError(null); setPage(1); }}
              activeOpacity={0.8}
            >
              <Text style={styles.retryText}>Qayta urinish</Text>
            </TouchableOpacity>
          </View>
        )}

        {!loading && !error && jobs.length === 0 && (
          <View style={styles.centerBox}>
            <Text style={[styles.stateText, { color: colors.textSecondary }]}>
              {search
                ? `"${search}" bo'yicha natija topilmadi`
                : "Hozircha ish e'lonlari yo'q"}
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
                <Text style={[styles.jobTitle, { color: colors.textPrimary }]} numberOfLines={1}>
                  {job.title}
                </Text>
                <Text style={[styles.companyName, { color: colors.textSecondary }]} numberOfLines={1}>
                  {job.employerName}
                </Text>
              </View>
              <View style={[styles.salaryBadge, { backgroundColor: colors.primaryLight }]}>
                <Text style={[styles.salaryText, { color: colors.primary }]}>
                  {job.salary
                    ? `${(job.salary / 1_000_000).toFixed(1)}M so'm`
                    : 'Kelishiladi'}
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

      {/* Floating + tugma */}
      <TouchableOpacity
        style={[styles.fab, { backgroundColor: colors.primary }]}
        onPress={() => {
          if (Number(role) === 2) router.push('/employer/post-job');
else router.push('/post-worker');
        }}
        activeOpacity={0.85}
      >
        <Text style={styles.fabText}>+</Text>
      </TouchableOpacity>

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
  subCategoriesContainer: {
    paddingHorizontal: Spacing.xl,
    paddingVertical: Spacing.sm,
    gap: Spacing.sm,
  },
  subCategoryChip: {
    paddingHorizontal: Spacing.md,
    paddingVertical: 8,
    borderRadius: BorderRadius.full,
    borderWidth: 1.5,
  },
  subCategoryText: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.medium,
  },
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
  fab: {
    position: 'absolute',
    bottom: 32,
    right: 24,
    width: 56,
    height: 56,
    borderRadius: 28,
    alignItems: 'center',
    justifyContent: 'center',
    elevation: 6,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 3 },
    shadowOpacity: 0.2,
    shadowRadius: 4,
  },
  fabText: {
    color: '#fff',
    fontSize: 32,
    lineHeight: 36,
    fontWeight: '400',
  },
}); 