import { useEffect, useState, useCallback, useMemo, useRef } from 'react';
import {
  View, Text, StyleSheet, ScrollView,
  TouchableOpacity, ActivityIndicator, RefreshControl, TextInput, Image,
} from 'react-native';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { useThemeStore } from '../../store/themeStore';
import { MEDIA_URL } from '../../services/api';
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
import { workerPostService } from '../../services/workerPostService';
import { propertyService } from '../../services/propertyService';
import { Job, Category, WorkerPost, PropertyListing, PropertyType, RentalPeriod } from '../../types';
import { useAuthStore, AuthState } from '../../store/authStore';
import { useLanguageStore } from '../../stores/useLanguageStore';
import { getCategoryLabel } from '../../utils/categoryLocalization';
import { getPropertyTypeLabel, getRentalPeriodLabel } from '../../utils/propertyLocalization';

function CategoryIcon({ name, color }: { name: string; color: string }) {
  const props = { size: 28, color };
  const map: Record<string, React.ReactElement> = {
    construction:   <ConstructionIcon {...props} />,
    it:             <ITIcon {...props} />,
    driver:         <DriverIcon {...props} />,
    chef:           <ChefIcon {...props} />,
    medical:        <MedicalIcon {...props} />,
    education:      <EducationIcon {...props} />,
    finance:        <FinanceIcon {...props} />,
    security:       <SecurityIcon {...props} />,
    cleaning:       <CleaningIcon {...props} />,
    design:         <DesignIcon {...props} />,
    marketing:      <MarketingIcon {...props} />,
    warehouse:      <WarehouseIcon {...props} />,
    daily:          <BriefcaseIcon {...props} />,
    trade:          <TradeIcon {...props} />,
    agriculture:    <AgricultureIcon {...props} />,
    manufacturing:  <ManufacturingIcon {...props} />,
    courier:        <CourierIcon {...props} />,
    legal:          <LegalIcon {...props} />,
    hr:             <HRIcon {...props} />,
    'real estate':  <RealEstateIcon {...props} />,
    beauty:         <BeautyIcon {...props} />,
    'auto service': <AutoServiceIcon {...props} />,
    textile:        <TextileIcon {...props} />,
  };
  return map[name?.toLowerCase()] ?? <BriefcaseIcon {...props} />;
}

// ─── WORKER HOME ─────────────────────────────────────────────────────────────
function WorkerHome() {
  const { colors, isDark, toggleTheme } = useThemeStore();
  const { language, t } = useLanguageStore();
  const firstName = useAuthStore((state: AuthState) => state.firstName);

  const [mode, setMode] = useState<'jobs' | 'properties'>('jobs');

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

  const [properties,        setProperties]        = useState<PropertyListing[]>([]);
  const [loadingProperties, setLoadingProperties] = useState(false);
  const [errorProperties,   setErrorProperties]   = useState<string | null>(null);
  const [propertyFilter, setPropertyFilter] = useState<'all' | 'apartment' | 'house' | 'daily' | 'monthly'>('all');

  const PAGE_SIZE = 10;

  const label = (uz: string, ru: string, en: string) =>
    language === 'uz' ? uz : language === 'ru' ? ru : en;

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
    if (mode !== 'jobs') return;
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
  }, [mode, search, selectedCat, selectedSubCat]);

  useEffect(() => {
    if (page === 1) return;
    const load = async () => {
      setLoadingMore(true);
      await fetchJobs(page, false);
      setLoadingMore(false);
    };
    load();
  }, [page]);

  // Property listings are independent of the Jobs feed above: separate state, separate fetch,
  // never merged into `jobs`. Fetched once (raw, unfiltered) when Properties mode is first
  // entered; search filtering below is purely local and never re-triggers a network request.
  const propertiesFetchedRef = useRef(false);

  const fetchProperties = useCallback(async () => {
    setLoadingProperties(true);
    setErrorProperties(null);
    try {
      const data = await propertyService.getProperties();
      setProperties(data);
      propertiesFetchedRef.current = true;
    } catch (e: any) {
      setErrorProperties(e?.message ?? t.common.somethingWentWrong);
    } finally {
      setLoadingProperties(false);
    }
  }, []);

  useEffect(() => {
    if (mode === 'properties' && !propertiesFetchedRef.current) fetchProperties();
  }, [mode, fetchProperties]);

  const PROPERTY_FILTERS: { label: string; value: 'all' | 'apartment' | 'house' | 'daily' | 'monthly' }[] = [
    { label: label('Barchasi', 'Все', 'All'),                        value: 'all' },
    { label: getPropertyTypeLabel(PropertyType.Apartment, language),  value: 'apartment' },
    { label: getPropertyTypeLabel(PropertyType.House, language),      value: 'house' },
    { label: getRentalPeriodLabel(RentalPeriod.Daily, language),      value: 'daily' },
    { label: getRentalPeriodLabel(RentalPeriod.Monthly, language),    value: 'monthly' },
  ];

  const filteredProperties = useMemo(() => {
    let result = properties;
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(p =>
        p.title?.toLowerCase().includes(q) ||
        p.description?.toLowerCase().includes(q) ||
        p.address?.toLowerCase().includes(q)
      );
    }
    switch (propertyFilter) {
      case 'apartment': return result.filter(p => p.propertyType === PropertyType.Apartment);
      case 'house':      return result.filter(p => p.propertyType === PropertyType.House);
      case 'daily':      return result.filter(p => p.rentalPeriod === RentalPeriod.Daily);
      case 'monthly':    return result.filter(p => p.rentalPeriod === RentalPeriod.Monthly);
      default:           return result;
    }
  }, [properties, search, propertyFilter]);

  const onRefresh = () => {
    setRefreshing(true);
    if (mode === 'jobs') {
      setSearch('');
      setSelectedCat(null);
      setSelectedSubCat(null);
      setPage(1);
    } else {
      fetchProperties().finally(() => setRefreshing(false));
    }
  };

  const loadMore = () => {
    if (mode !== 'jobs') return;
    if (loadingMore || jobs.length >= totalCount) return;
    setPage(p => p + 1);
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={[styles.header, { backgroundColor: colors.surface, ...Shadow.sm }]}>
        <View>
          <Text style={[styles.greeting, { color: colors.textSecondary }]}>
            {t.home.greeting}, {firstName || t.common.noData}!
          </Text>
          <Text style={[styles.headerTitle, { color: colors.textPrimary }]}>
            {t.home.findJob}
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
          <TouchableOpacity
            style={[styles.iconButton, { backgroundColor: colors.primaryLight }]}
            onPress={() => router.push('/notifications')}
          >
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
        <View style={[styles.modeToggleContainer, { backgroundColor: colors.card, borderColor: colors.border }]}>
          <TouchableOpacity
            style={[styles.modeToggleBtn, mode === 'jobs' && { backgroundColor: colors.primary }]}
            onPress={() => setMode('jobs')}
            activeOpacity={0.8}
          >
            <Text style={[styles.modeToggleText, { color: mode === 'jobs' ? '#fff' : colors.textSecondary }]}>
              {label('Ishlar', 'Работы', 'Jobs')}
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.modeToggleBtn, mode === 'properties' && { backgroundColor: colors.primary }]}
            onPress={() => setMode('properties')}
            activeOpacity={0.8}
          >
            <Text style={[styles.modeToggleText, { color: mode === 'properties' ? '#fff' : colors.textSecondary }]}>
              {label("Ko'chmas mulk", 'Недвижимость', 'Properties')}
            </Text>
          </TouchableOpacity>
        </View>

        <View style={[styles.searchContainer, { backgroundColor: colors.surface, borderColor: colors.border }]}>
          <SearchIcon size={18} color={colors.textTertiary} />
          <TextInput
            style={[styles.searchInput, { color: colors.textPrimary }]}
            placeholder={mode === 'jobs' ? t.search.placeholder : label('Mulk qidirish...', 'Поиск недвижимости...', 'Search properties...')}
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

        {mode === 'jobs' && (
          <>
            <View style={styles.sectionHeader}>
              <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>{t.home.categories}</Text>
              {selectedCat !== null && (
                <TouchableOpacity onPress={() => { setSelectedCat(null); setSelectedSubCat(null); }}>
                  <Text style={[styles.seeAll, { color: colors.primary }]}>{t.home.seeAll}</Text>
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
                      onPress={() => { setSelectedCat(isSelected ? null : cat.id); setSelectedSubCat(null); }}
                      style={[styles.categoryCard, { backgroundColor: isSelected ? colors.primary : colors.card, ...Shadow.sm }]}
                      activeOpacity={0.8}
                    >
                      <CategoryIcon name={cat.name?.toLowerCase()} color={isSelected ? '#fff' : colors.primary} />
                      <Text style={[styles.categoryName, { color: isSelected ? '#fff' : colors.textSecondary }]}>
                        {getCategoryLabel(cat.name, language)}
                      </Text>
                    </TouchableOpacity>
                  );
                })}
              </ScrollView>
            )}

            {selectedCat && (() => {
              const subs = categories.find(c => c.id === selectedCat)?.subCategories ?? [];
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
                        <Text style={[styles.subCategoryText, { color: isSelected ? '#fff' : colors.textSecondary }]}>
                          {getCategoryLabel(sub.name, language)}
                        </Text>
                      </TouchableOpacity>
                    );
                  })}
                </ScrollView>
              );
            })()}

            <View style={styles.sectionHeader}>
              <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
                {selectedCat
                  ? getCategoryLabel(categories.find(c => c.id === selectedCat)?.name, language)
                  : t.home.recentJobs}
              </Text>
              {!loading && (
                <Text style={[styles.seeAll, { color: colors.textTertiary }]}>{totalCount}</Text>
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
                  <Text style={styles.retryText}>{t.common.retry}</Text>
                </TouchableOpacity>
              </View>
            )}

            {!loading && !error && jobs.length === 0 && (
              <View style={styles.centerBox}>
                <Text style={[styles.stateText, { color: colors.textSecondary }]}>
                  {search ? `"${search}" ${t.search.noResults}` : t.home.noJobs}
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
                    {job.coverImageUrl ? (
                      <Image
                        source={{ uri: `${MEDIA_URL}${job.coverImageUrl}` }}
                        style={styles.companyLogoImage}
                        resizeMode="cover"
                      />
                    ) : (
                      <Text style={[styles.companyLogoText, { color: colors.primary }]}>
                        {(job.employerName ?? job.title ?? '?')[0].toUpperCase()}
                      </Text>
                    )}
                  </View>
                  <View style={styles.jobInfo}>
                    <Text style={[styles.jobTitle, { color: colors.textPrimary }]} numberOfLines={1}>{job.title}</Text>
                    <Text style={[styles.companyName, { color: colors.textSecondary }]} numberOfLines={1}>{job.employerName}</Text>
                  </View>
                  <View style={[styles.salaryBadge, { backgroundColor: colors.primaryLight }]}>
                    <Text style={[styles.salaryText, { color: colors.primary }]}>
                      {job.salary ? `${(job.salary / 1_000_000).toFixed(1)}M ${t.common.currency}` : t.common.noData}
                    </Text>
                  </View>
                </View>
                <View style={styles.jobCardBottom}>
                  {job.location && (
                    <View style={styles.jobMeta}>
                      <LocationIcon size={12} color={colors.textTertiary} />
                      <Text style={[styles.jobMetaText, { color: colors.textTertiary }]} numberOfLines={1}>{job.location}</Text>
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
          </>
        )}

        {mode === 'properties' && (
          <>
            <View style={styles.sectionHeader}>
              <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
                {label("Ko'chmas mulk e'lonlari", 'Объявления о недвижимости', 'Property Listings')}
              </Text>
              {!loadingProperties && (
                <Text style={[styles.seeAll, { color: colors.textTertiary }]}>{filteredProperties.length}</Text>
              )}
            </View>

            <ScrollView horizontal showsHorizontalScrollIndicator={false}
              contentContainerStyle={styles.subCategoriesContainer}>
              {PROPERTY_FILTERS.map(f => {
                const isSelected = propertyFilter === f.value;
                return (
                  <TouchableOpacity
                    key={f.value}
                    onPress={() => setPropertyFilter(f.value)}
                    style={[styles.subCategoryChip, {
                      backgroundColor: isSelected ? colors.primary : colors.card,
                      borderColor: isSelected ? colors.primary : colors.border,
                    }]}
                    activeOpacity={0.8}
                  >
                    <Text style={[styles.subCategoryText, { color: isSelected ? '#fff' : colors.textSecondary }]}>
                      {f.label}
                    </Text>
                  </TouchableOpacity>
                );
              })}
            </ScrollView>

            {loadingProperties && (
              <View style={{ paddingHorizontal: Spacing.xl, paddingTop: Spacing.md }}>
                <JobListSkeleton count={4} />
              </View>
            )}

            {!loadingProperties && errorProperties && (
              <View style={styles.centerBox}>
                <Text style={[styles.stateText, { color: '#EF4444' }]}>{errorProperties}</Text>
                <TouchableOpacity style={[styles.retryBtn, { backgroundColor: colors.primary }]}
                  onPress={() => fetchProperties()} activeOpacity={0.8}>
                  <Text style={styles.retryText}>{t.common.retry}</Text>
                </TouchableOpacity>
              </View>
            )}

            {!loadingProperties && !errorProperties && filteredProperties.length === 0 && (
              <View style={styles.centerBox}>
                <Text style={[styles.stateText, { color: colors.textSecondary }]}>
                  {search
                    ? `"${search}" ${t.search.noResults}`
                    : label("Ko'chmas mulk e'lonlari topilmadi", 'Объявления не найдены', 'No properties found')}
                </Text>
              </View>
            )}

            {!loadingProperties && !errorProperties && filteredProperties.map(property => (
              <TouchableOpacity
                key={property.id}
                style={[styles.jobCard, { backgroundColor: colors.card, ...Shadow.md }]}
                activeOpacity={0.85}
                onPress={() => router.push({ pathname: '/property-detail', params: { id: property.id } })}
              >
                <View style={styles.jobCardTop}>
                  <View style={[styles.companyLogo, { backgroundColor: colors.primaryLight }]}>
                    {property.coverImageUrl ? (
                      <Image
                        source={{ uri: `${MEDIA_URL}${property.coverImageUrl}` }}
                        style={styles.companyLogoImage}
                        resizeMode="cover"
                      />
                    ) : (
                      <RealEstateIcon size={24} color={colors.primary} />
                    )}
                  </View>
                  <View style={styles.jobInfo}>
                    <Text style={[styles.jobTitle, { color: colors.textPrimary }]} numberOfLines={1}>{property.title}</Text>
                    <Text style={[styles.companyName, { color: colors.textSecondary }]} numberOfLines={1}>
                      {getPropertyTypeLabel(property.propertyType, language)}
                    </Text>
                  </View>
                  <View style={[styles.salaryBadge, { backgroundColor: colors.primaryLight, maxWidth: '45%', flexShrink: 1 }]}>
                    <Text style={[styles.salaryText, { color: colors.primary }]} numberOfLines={1}>
                      {`${(property.price / 1_000_000).toFixed(1)}M ${t.common.currency} / ${getRentalPeriodLabel(property.rentalPeriod, language)}`}
                    </Text>
                  </View>
                </View>
                <View style={styles.jobCardBottom}>
                  <View style={styles.jobMeta}>
                    <LocationIcon size={12} color={colors.textTertiary} />
                    <Text style={[styles.jobMetaText, { color: colors.textTertiary }]} numberOfLines={1}>{property.address}</Text>
                  </View>
                </View>
              </TouchableOpacity>
            ))}
          </>
        )}

        <View style={{ height: 24 }} />
      </ScrollView>

      <TouchableOpacity
        style={[styles.fab, { backgroundColor: colors.primary }]}
        onPress={() => router.push('/post-worker')}
        activeOpacity={0.85}
      >
        <Text style={styles.fabText}>+</Text>
      </TouchableOpacity>
    </View>
  );
}

// ─── EMPLOYER HOME ────────────────────────────────────────────────────────────
function EmployerHome() {
  const { colors, isDark, toggleTheme } = useThemeStore();
  const { language, t } = useLanguageStore();
  const firstName = useAuthStore((state: AuthState) => state.firstName);

  const [mode, setMode] = useState<'workers' | 'properties'>('workers');

  const [posts,      setPosts]      = useState<WorkerPost[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading,    setLoading]    = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error,      setError]      = useState<string | null>(null);
  const [search,     setSearch]     = useState('');
  const [selectedCat, setSelectedCat] = useState<string | null>(null);

  const [properties,        setProperties]        = useState<PropertyListing[]>([]);
  const [loadingProperties, setLoadingProperties] = useState(false);
  const [errorProperties,   setErrorProperties]   = useState<string | null>(null);
  const [propertyFilter, setPropertyFilter] = useState<'all' | 'apartment' | 'house' | 'daily' | 'monthly'>('all');

  const label = (uz: string, ru: string, en: string) =>
    language === 'uz' ? uz : language === 'ru' ? ru : en;

  const fetchPosts = useCallback(async () => {
    try {
      const data = await workerPostService.getAll(selectedCat ?? undefined);
      const filtered = search
        ? data.filter(p =>
            p.title?.toLowerCase().includes(search.toLowerCase()) ||
            p.description?.toLowerCase().includes(search.toLowerCase())
          )
        : data;
      setPosts(filtered);
      setError(null);
    } catch (e: any) {
      setError(e?.message ?? t.common.somethingWentWrong);
    }
  }, [selectedCat, search]);

  useEffect(() => {
    if (mode !== 'workers') return;
    const load = async () => {
      setLoading(true);
      try {
        const [, cats] = await Promise.all([
          fetchPosts(),
          categories.length === 0 ? categoryService.getCategories() : Promise.resolve(null),
        ]);
        if (cats) setCategories(cats as Category[]);
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    };
    load();
  }, [mode, selectedCat, search]);

  // Property listings are independent of the WorkerPost feed above: separate state, separate
  // fetch, never merged into `posts`. Fetched once (raw, unfiltered) when Properties mode is
  // first entered; search filtering below is purely local and never re-triggers a network request.
  const propertiesFetchedRef = useRef(false);

  const fetchProperties = useCallback(async () => {
    setLoadingProperties(true);
    setErrorProperties(null);
    try {
      const data = await propertyService.getProperties();
      setProperties(data);
      propertiesFetchedRef.current = true;
    } catch (e: any) {
      setErrorProperties(e?.message ?? t.common.somethingWentWrong);
    } finally {
      setLoadingProperties(false);
    }
  }, []);

  useEffect(() => {
    if (mode === 'properties' && !propertiesFetchedRef.current) fetchProperties();
  }, [mode, fetchProperties]);

  const PROPERTY_FILTERS: { label: string; value: 'all' | 'apartment' | 'house' | 'daily' | 'monthly' }[] = [
    { label: label('Barchasi', 'Все', 'All'),                        value: 'all' },
    { label: getPropertyTypeLabel(PropertyType.Apartment, language),  value: 'apartment' },
    { label: getPropertyTypeLabel(PropertyType.House, language),      value: 'house' },
    { label: getRentalPeriodLabel(RentalPeriod.Daily, language),      value: 'daily' },
    { label: getRentalPeriodLabel(RentalPeriod.Monthly, language),    value: 'monthly' },
  ];

  const filteredProperties = useMemo(() => {
    let result = properties;
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(p =>
        p.title?.toLowerCase().includes(q) ||
        p.description?.toLowerCase().includes(q) ||
        p.address?.toLowerCase().includes(q)
      );
    }
    switch (propertyFilter) {
      case 'apartment': return result.filter(p => p.propertyType === PropertyType.Apartment);
      case 'house':      return result.filter(p => p.propertyType === PropertyType.House);
      case 'daily':      return result.filter(p => p.rentalPeriod === RentalPeriod.Daily);
      case 'monthly':    return result.filter(p => p.rentalPeriod === RentalPeriod.Monthly);
      default:           return result;
    }
  }, [properties, search, propertyFilter]);

  const onRefresh = () => {
    setRefreshing(true);
    if (mode === 'workers') {
      setSearch('');
      setSelectedCat(null);
    } else {
      fetchProperties().finally(() => setRefreshing(false));
    }
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={[styles.header, { backgroundColor: colors.surface, ...Shadow.sm }]}>
        <View>
          <Text style={[styles.greeting, { color: colors.textSecondary }]}>
            {t.home.greeting}, {firstName || t.common.noData}!
          </Text>
          <Text style={[styles.headerTitle, { color: colors.textPrimary }]}>
            {language === 'uz' ? 'Ishchi toping' : language === 'ru' ? 'Найти работника' : 'Find Workers'}
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
          <TouchableOpacity
            style={[styles.iconButton, { backgroundColor: colors.primaryLight }]}
            onPress={() => router.push('/notifications')}
          >
            <BellIcon size={20} color={colors.primary} />
          </TouchableOpacity>
        </View>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh}
            tintColor={colors.primary} colors={[colors.primary]} />
        }
      >
        <View style={[styles.modeToggleContainer, { backgroundColor: colors.card, borderColor: colors.border }]}>
          <TouchableOpacity
            style={[styles.modeToggleBtn, mode === 'workers' && { backgroundColor: colors.primary }]}
            onPress={() => setMode('workers')}
            activeOpacity={0.8}
          >
            <Text style={[styles.modeToggleText, { color: mode === 'workers' ? '#fff' : colors.textSecondary }]}>
              {language === 'uz' ? 'Ishchilar' : language === 'ru' ? 'Работники' : 'Workers'}
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.modeToggleBtn, mode === 'properties' && { backgroundColor: colors.primary }]}
            onPress={() => setMode('properties')}
            activeOpacity={0.8}
          >
            <Text style={[styles.modeToggleText, { color: mode === 'properties' ? '#fff' : colors.textSecondary }]}>
              {label("Ko'chmas mulk", 'Недвижимость', 'Properties')}
            </Text>
          </TouchableOpacity>
        </View>

        <View style={[styles.searchContainer, { backgroundColor: colors.surface, borderColor: colors.border }]}>
          <SearchIcon size={18} color={colors.textTertiary} />
          <TextInput
            style={[styles.searchInput, { color: colors.textPrimary }]}
            placeholder={
              mode === 'workers'
                ? (language === 'uz' ? 'Ishchi qidiring...' : language === 'ru' ? 'Поиск работника...' : 'Search workers...')
                : label('Mulk qidirish...', 'Поиск недвижимости...', 'Search properties...')
            }
            placeholderTextColor={colors.textTertiary}
            value={search}
            onChangeText={setSearch}
            returnKeyType="search"
            autoCapitalize="none"
          />
          {search.length > 0 && (
            <TouchableOpacity onPress={() => setSearch('')}>
              <Text style={{ color: colors.textTertiary, fontSize: 16, paddingLeft: 4 }}>✕</Text>
            </TouchableOpacity>
          )}
        </View>

        {mode === 'workers' && (
          <>
            <View style={styles.sectionHeader}>
              <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>{t.home.categories}</Text>
              {selectedCat !== null && (
                <TouchableOpacity onPress={() => setSelectedCat(null)}>
                  <Text style={[styles.seeAll, { color: colors.primary }]}>{t.home.seeAll}</Text>
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
                      style={[styles.categoryCard, { backgroundColor: isSelected ? colors.primary : colors.card, ...Shadow.sm }]}
                      activeOpacity={0.8}
                    >
                      <CategoryIcon name={cat.name?.toLowerCase()} color={isSelected ? '#fff' : colors.primary} />
                      <Text style={[styles.categoryName, { color: isSelected ? '#fff' : colors.textSecondary }]}>
                        {getCategoryLabel(cat.name, language)}
                      </Text>
                    </TouchableOpacity>
                  );
                })}
              </ScrollView>
            )}

            <View style={styles.sectionHeader}>
              <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
                {language === 'uz' ? 'Ishchilar' : language === 'ru' ? 'Работники' : 'Workers'}
              </Text>
              {!loading && (
                <Text style={[styles.seeAll, { color: colors.textTertiary }]}>{posts.length}</Text>
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
                  onPress={() => { setError(null); setLoading(true); fetchPosts(); }} activeOpacity={0.8}>
                  <Text style={styles.retryText}>{t.common.retry}</Text>
                </TouchableOpacity>
              </View>
            )}

            {!loading && !error && posts.length === 0 && (
              <View style={styles.centerBox}>
                <Text style={[styles.stateText, { color: colors.textSecondary }]}>
                  {language === 'uz' ? 'Ishchilar topilmadi' : language === 'ru' ? 'Работники не найдены' : 'No workers found'}
                </Text>
              </View>
            )}

            {!loading && !error && posts.map(post => (
              <TouchableOpacity
                key={post.id}
                style={[styles.jobCard, { backgroundColor: colors.card, ...Shadow.md }]}
                activeOpacity={0.85}
                onPress={() => router.push({ pathname: '/worker-post-detail', params: { id: post.id } })}
              >
                <View style={styles.jobCardTop}>
                  <View style={[styles.companyLogo, { backgroundColor: colors.primaryLight }]}>
                    <Text style={[styles.companyLogoText, { color: colors.primary }]}>
                      {(post.workerFirstName ?? post.title ?? '?')[0].toUpperCase()}
                    </Text>
                  </View>
                  <View style={styles.jobInfo}>
                    <Text style={[styles.jobTitle, { color: colors.textPrimary }]} numberOfLines={1}>
                      {post.title}
                    </Text>
                    <Text style={[styles.companyName, { color: colors.textSecondary }]} numberOfLines={1}>
                      {`${post.workerFirstName} ${post.workerLastName}`}
                    </Text>
                  </View>
                  {post.expectedSalary && (
                    <View style={[styles.salaryBadge, { backgroundColor: colors.primaryLight }]}>
                      <Text style={[styles.salaryText, { color: colors.primary }]}>
                        {`${(post.expectedSalary / 1_000_000).toFixed(1)}M ${t.common.currency}`}
                      </Text>
                    </View>
                  )}
                </View>
                {post.city && (
                  <View style={styles.jobCardBottom}>
                    <View style={styles.jobMeta}>
                      <LocationIcon size={12} color={colors.textTertiary} />
                      <Text style={[styles.jobMetaText, { color: colors.textTertiary }]} numberOfLines={1}>
                        {post.city}
                      </Text>
                    </View>
                  </View>
                )}
              </TouchableOpacity>
            ))}
          </>
        )}

        {mode === 'properties' && (
          <>
            <View style={styles.sectionHeader}>
              <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
                {label("Ko'chmas mulk e'lonlari", 'Объявления о недвижимости', 'Property Listings')}
              </Text>
              {!loadingProperties && (
                <Text style={[styles.seeAll, { color: colors.textTertiary }]}>{filteredProperties.length}</Text>
              )}
            </View>

            <ScrollView horizontal showsHorizontalScrollIndicator={false}
              contentContainerStyle={styles.subCategoriesContainer}>
              {PROPERTY_FILTERS.map(f => {
                const isSelected = propertyFilter === f.value;
                return (
                  <TouchableOpacity
                    key={f.value}
                    onPress={() => setPropertyFilter(f.value)}
                    style={[styles.subCategoryChip, {
                      backgroundColor: isSelected ? colors.primary : colors.card,
                      borderColor: isSelected ? colors.primary : colors.border,
                    }]}
                    activeOpacity={0.8}
                  >
                    <Text style={[styles.subCategoryText, { color: isSelected ? '#fff' : colors.textSecondary }]}>
                      {f.label}
                    </Text>
                  </TouchableOpacity>
                );
              })}
            </ScrollView>

            {loadingProperties && (
              <View style={{ paddingHorizontal: Spacing.xl, paddingTop: Spacing.md }}>
                <JobListSkeleton count={4} />
              </View>
            )}

            {!loadingProperties && errorProperties && (
              <View style={styles.centerBox}>
                <Text style={[styles.stateText, { color: '#EF4444' }]}>{errorProperties}</Text>
                <TouchableOpacity style={[styles.retryBtn, { backgroundColor: colors.primary }]}
                  onPress={() => fetchProperties()} activeOpacity={0.8}>
                  <Text style={styles.retryText}>{t.common.retry}</Text>
                </TouchableOpacity>
              </View>
            )}

            {!loadingProperties && !errorProperties && filteredProperties.length === 0 && (
              <View style={styles.centerBox}>
                <Text style={[styles.stateText, { color: colors.textSecondary }]}>
                  {label("Ko'chmas mulk e'lonlari topilmadi", 'Объявления не найдены', 'No properties found')}
                </Text>
              </View>
            )}

            {!loadingProperties && !errorProperties && filteredProperties.map(property => (
              <TouchableOpacity
                key={property.id}
                style={[styles.jobCard, { backgroundColor: colors.card, ...Shadow.md }]}
                activeOpacity={0.85}
                onPress={() => router.push({ pathname: '/property-detail', params: { id: property.id } })}
              >
                <View style={styles.jobCardTop}>
                  <View style={[styles.companyLogo, { backgroundColor: colors.primaryLight }]}>
                    {property.coverImageUrl ? (
                      <Image
                        source={{ uri: `${MEDIA_URL}${property.coverImageUrl}` }}
                        style={styles.companyLogoImage}
                        resizeMode="cover"
                      />
                    ) : (
                      <RealEstateIcon size={24} color={colors.primary} />
                    )}
                  </View>
                  <View style={styles.jobInfo}>
                    <Text style={[styles.jobTitle, { color: colors.textPrimary }]} numberOfLines={1}>{property.title}</Text>
                    <Text style={[styles.companyName, { color: colors.textSecondary }]} numberOfLines={1}>
                      {getPropertyTypeLabel(property.propertyType, language)}
                    </Text>
                  </View>
                  <View style={[styles.salaryBadge, { backgroundColor: colors.primaryLight, maxWidth: '45%', flexShrink: 1 }]}>
                    <Text style={[styles.salaryText, { color: colors.primary }]} numberOfLines={1}>
                      {`${(property.price / 1_000_000).toFixed(1)}M ${t.common.currency} / ${getRentalPeriodLabel(property.rentalPeriod, language)}`}
                    </Text>
                  </View>
                </View>
                <View style={styles.jobCardBottom}>
                  <View style={styles.jobMeta}>
                    <LocationIcon size={12} color={colors.textTertiary} />
                    <Text style={[styles.jobMetaText, { color: colors.textTertiary }]} numberOfLines={1}>{property.address}</Text>
                  </View>
                </View>
              </TouchableOpacity>
            ))}
          </>
        )}

        <View style={{ height: 24 }} />
      </ScrollView>

      <TouchableOpacity
        style={[styles.fab, { backgroundColor: colors.primary }]}
        onPress={() => router.push('/employer/post-job')}
        activeOpacity={0.85}
      >
        <Text style={styles.fabText}>+</Text>
      </TouchableOpacity>
    </View>
  );
}

// ─── MAIN ─────────────────────────────────────────────────────────────────────
export default function HomeScreen() {
  const role = useAuthStore((state: AuthState) => state.role);
  return Number(role) === 2 ? <EmployerHome /> : <WorkerHome />;
}

const styles = StyleSheet.create({
  container:    { flex: 1 },
  header: {
    flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center',
    paddingHorizontal: Spacing.xl, paddingTop: 56, paddingBottom: Spacing.lg,
  },
  greeting:     { fontSize: FontSize.sm,  fontWeight: FontWeight.medium },
  headerTitle:  { fontSize: FontSize.xxl, fontWeight: FontWeight.bold },
  headerRight:  { flexDirection: 'row', gap: Spacing.sm },
  iconButton: {
    width: 44, height: 44, borderRadius: BorderRadius.full,
    alignItems: 'center', justifyContent: 'center',
  },
  modeToggleContainer: {
    flexDirection: 'row', marginHorizontal: Spacing.xl, marginTop: Spacing.lg,
    borderRadius: BorderRadius.lg, borderWidth: 1.5, padding: 4, gap: 4,
  },
  modeToggleBtn: {
    flex: 1, borderRadius: BorderRadius.md, paddingVertical: Spacing.sm, alignItems: 'center',
  },
  modeToggleText: { fontSize: FontSize.sm, fontWeight: FontWeight.semiBold },
  searchContainer: {
    flexDirection: 'row', alignItems: 'center',
    marginHorizontal: Spacing.xl, marginTop: Spacing.lg, marginBottom: Spacing.sm,
    borderRadius: BorderRadius.lg, paddingHorizontal: Spacing.lg,
    height: 52, borderWidth: 1.5, gap: Spacing.sm,
  },
  searchInput:  { flex: 1, fontSize: FontSize.md, paddingVertical: 0 },
  sectionHeader: {
    flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center',
    paddingHorizontal: Spacing.xl, marginTop: Spacing.xl, marginBottom: Spacing.md,
  },
  sectionTitle: { fontSize: FontSize.lg, fontWeight: FontWeight.bold },
  seeAll:       { fontSize: FontSize.sm, fontWeight: FontWeight.semiBold },
  categoriesContainer:    { paddingHorizontal: Spacing.xl, gap: Spacing.md },
  categoryCard: {
    alignItems: 'center', justifyContent: 'center', borderRadius: BorderRadius.lg,
    paddingVertical: Spacing.md, paddingHorizontal: Spacing.lg, gap: Spacing.xs, minWidth: 80,
  },
  categoryName: { fontSize: FontSize.xs, fontWeight: FontWeight.semiBold },
  subCategoriesContainer: { paddingHorizontal: Spacing.xl, paddingVertical: Spacing.sm, gap: Spacing.sm },
  subCategoryChip: { paddingHorizontal: Spacing.md, paddingVertical: 8, borderRadius: BorderRadius.full, borderWidth: 1.5 },
  subCategoryText: { fontSize: FontSize.sm, fontWeight: FontWeight.medium },
  jobCard: { borderRadius: BorderRadius.xl, marginHorizontal: Spacing.xl, marginBottom: Spacing.md, padding: Spacing.lg },
  jobCardTop:      { flexDirection: 'row', alignItems: 'center', marginBottom: Spacing.md },
  companyLogo: {
    width: 48, height: 48, borderRadius: BorderRadius.md,
    alignItems: 'center', justifyContent: 'center', marginRight: Spacing.md,
  },
  companyLogoText: { fontSize: FontSize.lg, fontWeight: FontWeight.bold },
  companyLogoImage: { width: 48, height: 48, borderRadius: BorderRadius.md },
  jobInfo:         { flex: 1 },
  jobTitle:        { fontSize: FontSize.md, fontWeight: FontWeight.bold, marginBottom: 2 },
  companyName:     { fontSize: FontSize.sm },
  salaryBadge: { borderRadius: BorderRadius.sm, paddingHorizontal: Spacing.sm, paddingVertical: Spacing.xs },
  salaryText:    { fontSize: FontSize.xs, fontWeight: FontWeight.bold },
  jobCardBottom: { flexDirection: 'row', gap: Spacing.lg },
  jobMeta:       { flexDirection: 'row', alignItems: 'center', gap: 4 },
  jobMetaText:   { fontSize: FontSize.xs, fontWeight: FontWeight.medium },
  centerBox: {
    alignItems: 'center', justifyContent: 'center',
    paddingVertical: 40, paddingHorizontal: Spacing.xl, gap: 12,
  },
  stateText:  { fontSize: FontSize.sm, textAlign: 'center' },
  retryBtn: { paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md, borderRadius: BorderRadius.lg, marginTop: 4 },
  retryText:  { color: '#fff', fontWeight: FontWeight.semiBold, fontSize: FontSize.sm },
  fab: {
    position: 'absolute', bottom: 32, right: 24,
    width: 56, height: 56, borderRadius: 28,
    alignItems: 'center', justifyContent: 'center',
    elevation: 6, shadowColor: '#000',
    shadowOffset: { width: 0, height: 3 }, shadowOpacity: 0.2, shadowRadius: 4,
  },
  fabText: { color: '#fff', fontSize: 32, lineHeight: 36, fontWeight: '400' },
});
