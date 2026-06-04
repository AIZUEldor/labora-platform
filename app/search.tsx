import React, { useState, useCallback } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity,
  ActivityIndicator, TextInput, FlatList,
} from 'react-native';
import { router } from 'expo-router';
import { FontSize, FontWeight } from '../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../constants/spacing';
import { useThemeStore } from '../store/themeStore';
import { SearchIcon, LocationIcon, ClockIcon, MapPinIcon } from '../components/icons';
import Svg, { Path } from 'react-native-svg';
import { jobService } from '../services/jobService';
import { Job } from '../types';
import { useLanguageStore } from '../stores/useLanguageStore';

function BackIcon({ size = 24, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M19 12H5M5 12L12 19M5 12L12 5" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

const JOB_TYPE_LABEL: Record<string, string> = {
  FullTime: 'Full-time', PartTime: 'Part-time',
  Remote: 'Remote', Contract: 'Contract', Internship: 'Internship',
};

export default function SearchScreen() {
  const { colors } = useThemeStore();
  const { t } = useLanguageStore();

  const [query,      setQuery]      = useState('');
  const [jobs,       setJobs]       = useState<Job[]>([]);
  const [loading,    setLoading]    = useState(false);
  const [searched,   setSearched]   = useState(false);
  const [totalCount, setTotalCount] = useState(0);

  const search = useCallback(async (text: string) => {
    if (!text.trim()) { setJobs([]); setSearched(false); return; }
    try {
      setLoading(true);
      const res = await jobService.searchJobs(text, 1);
      setJobs(res.items ?? []);
      setTotalCount(res.totalCount ?? 0);
      setSearched(true);
    } catch (_) {
      setJobs([]);
    } finally {
      setLoading(false);
    }
  }, []);

  const renderJob = ({ item }: { item: Job }) => (
    <TouchableOpacity
      style={[styles.jobCard, { backgroundColor: colors.card, ...Shadow.md }]}
      activeOpacity={0.85}
      onPress={() => router.push({ pathname: '/job-detail', params: { id: item.id } })}
    >
      <View style={styles.jobCardTop}>
        <View style={[styles.companyLogo, { backgroundColor: colors.primaryLight }]}>
          <Text style={[styles.companyLogoText, { color: colors.primary }]}>
            {(item.employerName ?? item.title ?? '?')[0].toUpperCase()}
          </Text>
        </View>
        <View style={styles.jobInfo}>
          <Text style={[styles.jobTitle,   { color: colors.textPrimary   }]} numberOfLines={1}>{item.title}</Text>
          <Text style={[styles.companyName, { color: colors.textSecondary }]} numberOfLines={1}>{item.employerName}</Text>
        </View>
        <View style={[styles.salaryBadge, { backgroundColor: colors.primaryLight }]}>
          <Text style={[styles.salaryText, { color: colors.primary }]}>
            {item.salary ? `${(item.salary / 1_000_000).toFixed(1)}M` : t.common.noData}
          </Text>
        </View>
      </View>
      <View style={styles.jobCardBottom}>
        {item.location && (
          <View style={styles.jobMeta}>
            <MapPinIcon size={12} color={colors.textTertiary} />
            <Text style={[styles.metaText, { color: colors.textTertiary }]} numberOfLines={1}>{item.location}</Text>
          </View>
        )}
        <View style={styles.jobMeta}>
          <ClockIcon size={12} color={colors.textTertiary} />
          <Text style={[styles.metaText, { color: colors.textTertiary }]}>
            {JOB_TYPE_LABEL[item.status as any] ?? 'Full-time'}
          </Text>
        </View>
      </View>
    </TouchableOpacity>
  );

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>

      {/* Header */}
      <View style={[styles.header, { backgroundColor: colors.surface }]}>
        <TouchableOpacity style={styles.backBtn} onPress={() => router.back()} activeOpacity={0.7}>
          <BackIcon size={22} color={colors.textPrimary} />
        </TouchableOpacity>
        <View style={[styles.searchBar, { backgroundColor: colors.background, borderColor: colors.border }]}>
          <SearchIcon size={18} color={colors.textTertiary} />
          <TextInput
            style={[styles.searchInput, { color: colors.textPrimary }]}
            placeholder={t.search.placeholder}
            placeholderTextColor={colors.textTertiary}
            value={query}
            onChangeText={text => { setQuery(text); search(text); }}
            autoFocus
            returnKeyType="search"
            onSubmitEditing={() => search(query)}
            autoCapitalize="none"
          />
          {query.length > 0 && (
            <TouchableOpacity onPress={() => { setQuery(''); setJobs([]); setSearched(false); }}>
              <Text style={{ color: colors.textTertiary, fontSize: 16, paddingLeft: 4 }}>✕</Text>
            </TouchableOpacity>
          )}
        </View>
      </View>

      {/* Loading */}
      {loading && (
        <View style={styles.centerBox}>
          <ActivityIndicator size="large" color={colors.primary} />
        </View>
      )}

      {/* Empty state */}
      {!loading && searched && jobs.length === 0 && (
        <View style={styles.centerBox}>
          <Text style={[styles.stateText, { color: colors.textSecondary }]}>
            "{query}" {t.search.noResults}
          </Text>
        </View>
      )}

      {/* Initial state */}
      {!loading && !searched && (
        <View style={styles.centerBox}>
          <SearchIcon size={48} color={colors.border} />
          <Text style={[styles.stateText, { color: colors.textSecondary }]}>
            {t.search.placeholder}
          </Text>
        </View>
      )}

      {/* Results */}
      {!loading && jobs.length > 0 && (
        <>
          <Text style={[styles.resultCount, { color: colors.textSecondary }]}>
            {totalCount} {t.search.results}
          </Text>
          <FlatList
            data={jobs}
            keyExtractor={item => item.id}
            renderItem={renderJob}
            contentContainerStyle={styles.listContainer}
            showsVerticalScrollIndicator={false}
          />
        </>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: {
    flexDirection: 'row', alignItems: 'center',
    paddingHorizontal: Spacing.xl, paddingTop: 56, paddingBottom: Spacing.lg,
    gap: Spacing.md,
  },
  backBtn: { padding: 4 },
  searchBar: {
    flex: 1, flexDirection: 'row', alignItems: 'center',
    borderRadius: BorderRadius.lg, paddingHorizontal: Spacing.md,
    height: 48, borderWidth: 1.5, gap: Spacing.sm,
  },
  searchInput: { flex: 1, fontSize: FontSize.md, paddingVertical: 0 },
  centerBox: {
    flex: 1, alignItems: 'center', justifyContent: 'center', gap: 12,
    paddingHorizontal: Spacing.xl,
  },
  stateText:   { fontSize: FontSize.sm, textAlign: 'center' },
  resultCount: { fontSize: FontSize.sm, paddingHorizontal: Spacing.xl, paddingVertical: Spacing.sm },
  listContainer: { paddingHorizontal: Spacing.xl, paddingBottom: 24, gap: Spacing.md },
  jobCard:     { borderRadius: BorderRadius.xl, padding: Spacing.lg },
  jobCardTop:  { flexDirection: 'row', alignItems: 'center', marginBottom: Spacing.md },
  companyLogo: {
    width: 48, height: 48, borderRadius: BorderRadius.md,
    alignItems: 'center', justifyContent: 'center', marginRight: Spacing.md,
  },
  companyLogoText: { fontSize: FontSize.lg, fontWeight: FontWeight.bold },
  jobInfo:     { flex: 1 },
  jobTitle:    { fontSize: FontSize.md, fontWeight: FontWeight.bold, marginBottom: 2 },
  companyName: { fontSize: FontSize.sm },
  salaryBadge: { borderRadius: BorderRadius.sm, paddingHorizontal: Spacing.sm, paddingVertical: Spacing.xs },
  salaryText:  { fontSize: FontSize.xs, fontWeight: FontWeight.bold },
  jobCardBottom: { flexDirection: 'row', gap: Spacing.lg },
  jobMeta:     { flexDirection: 'row', alignItems: 'center', gap: 4 },
  metaText:    { fontSize: FontSize.xs, fontWeight: FontWeight.medium },
});
