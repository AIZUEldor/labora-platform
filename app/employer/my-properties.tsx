import React, { useCallback, useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, FlatList,
  TouchableOpacity, RefreshControl, Image,
} from 'react-native';
import { router } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import Svg, { Path } from 'react-native-svg';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { useThemeStore } from '../../store/themeStore';
import { useLanguageStore } from '../../stores/useLanguageStore';
import { MEDIA_URL } from '../../services/api';
import { RealEstateIcon, LocationIcon, ChevronRightIcon } from '../../components/icons';
import { propertyService } from '../../services/propertyService';
import { PropertyListing, PropertyListingStatus } from '../../types';
import { ApplicationListSkeleton } from '../../components/SkeletonLoader';
import { getPropertyTypeLabel, getRentalPeriodLabel, getPropertyListingStatusLabel } from '../../utils/propertyLocalization';

function BackIcon({ size = 24, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M19 12H5M5 12L12 19M5 12L12 5" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

const STATUS_COLOR: Record<number, { bg: string; text: string }> = {
  [PropertyListingStatus.Published]: { bg: '#DCFCE7', text: '#166534' },
  [PropertyListingStatus.Archived]:  { bg: '#F3F4F6', text: '#6B7280' },
};

export default function MyPropertiesScreen() {
  const { colors, isDark } = useThemeStore();
  const { t, language } = useLanguageStore();

  const [properties, setProperties] = useState<PropertyListing[]>([]);
  const [loading,    setLoading]    = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error,      setError]      = useState<string | null>(null);

  // "My Properties" has no shared i18n keys yet - matches the inline-ternary convention already
  // used for new Property-only strings on Home/Map, rather than adding unrelated i18n file edits.
  const label = (uz: string, ru: string, en: string) =>
    language === 'uz' ? uz : language === 'ru' ? ru : en;

  // Single reusable load function, fetched once on mount; refresh reuses this exact function
  // rather than issuing a separate request, matching EmployerJobsView's own convention.
  const load = useCallback(async () => {
    try {
      const data = await propertyService.getMyProperties();
      setProperties(data);
      setError(null);
    } catch (e: any) {
      setError(e?.message ?? t.common.somethingWentWrong);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => { load(); }, []);

  const onRefresh = () => { setRefreshing(true); load(); };

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
            <Text style={styles.headerTitle} numberOfLines={1}>
              {label('Mening mulklarim', 'Мои объявления', 'My Properties')}
            </Text>
          </View>
          <View style={{ width: 40 }} />
        </View>
      </LinearGradient>

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

      {!loading && !error && properties.length === 0 && (
        <View style={styles.centerBox}>
          <Text style={[styles.stateText, { color: colors.textSecondary }]}>
            {label("Hali ko'chmas mulk e'lonlari yo'q", 'Пока нет объявлений о недвижимости', 'No properties yet')}
          </Text>
          <TouchableOpacity style={[styles.retryBtn, { backgroundColor: colors.primary }]}
            onPress={() => router.push('/property/create')}>
            <Text style={styles.retryText}>{label("Mulk qo'shish", 'Добавить объявление', 'Add Property')}</Text>
          </TouchableOpacity>
        </View>
      )}

      {!loading && !error && properties.length > 0 && (
        <FlatList
          data={properties}
          keyExtractor={item => item.id}
          contentContainerStyle={styles.listContainer}
          showsVerticalScrollIndicator={false}
          refreshControl={
            <RefreshControl refreshing={refreshing} onRefresh={onRefresh}
              tintColor={colors.primary} colors={[colors.primary]} />
          }
          renderItem={({ item }) => {
            const statusInfo = STATUS_COLOR[item.status] ?? STATUS_COLOR[PropertyListingStatus.Published];
            return (
              <TouchableOpacity
                style={[styles.card, { backgroundColor: colors.card, ...Shadow.md }]}
                activeOpacity={0.85}
                onPress={() => router.push({ pathname: '/property-detail', params: { id: item.id } })}
              >
                <View style={styles.cardHeader}>
                  <View style={[styles.logo, { backgroundColor: colors.primaryLight }]}>
                    {item.coverImageUrl ? (
                      <Image
                        source={{ uri: `${MEDIA_URL}${item.coverImageUrl}` }}
                        style={styles.logoImage}
                        resizeMode="cover"
                      />
                    ) : (
                      <RealEstateIcon size={22} color={colors.primary} />
                    )}
                  </View>
                  <View style={styles.cardInfo}>
                    <Text style={[styles.cardTitle, { color: colors.textPrimary }]} numberOfLines={1}>{item.title}</Text>
                    <Text style={[styles.subText, { color: colors.textSecondary }]} numberOfLines={1}>
                      {getPropertyTypeLabel(item.propertyType, language)}
                    </Text>
                  </View>
                  <View style={[styles.badge, { backgroundColor: statusInfo.bg }]}>
                    <Text style={[styles.badgeText, { color: statusInfo.text }]}>
                      {getPropertyListingStatusLabel(item.status, language)}
                    </Text>
                  </View>
                  <ChevronRightIcon size={20} color={colors.textTertiary} />
                </View>
                <View style={[styles.divider, { backgroundColor: colors.border }]} />
                <View style={styles.metaRow}>
                  <LocationIcon size={12} color={colors.textTertiary} />
                  <Text style={[styles.metaText, { color: colors.textTertiary, flex: 1 }]} numberOfLines={1}>
                    {item.address}
                  </Text>
                </View>
                <Text style={[styles.priceText, { color: colors.primary }]} numberOfLines={1}>
                  {`${item.price.toLocaleString()} ${t.common.currency} / ${getRentalPeriodLabel(item.rentalPeriod, language)}`}
                </Text>
              </TouchableOpacity>
            );
          }}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  centerBox: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 12 },
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
  listContainer: { padding: Spacing.xl, gap: Spacing.md },
  card:          { borderRadius: BorderRadius.xl, padding: Spacing.lg },
  cardHeader:    { flexDirection: 'row', alignItems: 'center' },
  logo: {
    width: 48, height: 48, borderRadius: BorderRadius.md,
    alignItems: 'center', justifyContent: 'center', marginRight: Spacing.md,
  },
  logoImage:  { width: 48, height: 48, borderRadius: BorderRadius.md },
  cardInfo:   { flex: 1 },
  cardTitle:  { fontSize: FontSize.md, fontWeight: FontWeight.bold, marginBottom: 2 },
  subText:    { fontSize: FontSize.sm },
  badge: {
    borderRadius: BorderRadius.sm, paddingHorizontal: Spacing.sm,
    paddingVertical: Spacing.xs, marginLeft: Spacing.sm,
  },
  badgeText:  { fontSize: FontSize.xs, fontWeight: FontWeight.bold },
  divider:    { height: 1, marginVertical: Spacing.md },
  metaRow:    { flexDirection: 'row', alignItems: 'center', gap: 4 },
  metaText:   { fontSize: FontSize.xs, fontWeight: FontWeight.medium },
  priceText:  { fontSize: FontSize.sm, fontWeight: FontWeight.bold, marginTop: Spacing.sm },
  stateText:  { fontSize: FontSize.sm, textAlign: 'center' },
  retryBtn:   { paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md, borderRadius: BorderRadius.lg, marginTop: 4 },
  retryText:  { color: '#fff', fontWeight: FontWeight.semiBold, fontSize: FontSize.sm },
});
