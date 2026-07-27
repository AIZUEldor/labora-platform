import React, { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, ScrollView, TouchableOpacity,
  ActivityIndicator, Image, Linking, Alert,
} from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { useThemeStore } from '../store/themeStore';
import { useLanguageStore } from '../stores/useLanguageStore';
import { propertyService } from '../services/propertyService';
import { PropertyListing, PropertyType } from '../types';
import { MEDIA_URL } from '../services/api';
import { FontSize, FontWeight } from '../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../constants/spacing';
import {
  BackIcon, MapPinIcon, MoneyIcon, HomeIcon, RealEstateIcon, PhoneIcon,
} from '../components/icons';
import {
  getPropertyTypeLabel,
  getRenovationStatusLabel,
  getRentalPeriodLabel,
} from '../utils/propertyLocalization';

export default function PropertyDetailScreen() {
  const { colors, isDark } = useThemeStore();
  const { language } = useLanguageStore();
  const { id } = useLocalSearchParams<{ id: string }>();
  const propertyId = Array.isArray(id) ? id[0] : id;

  const [property, setProperty] = useState<PropertyListing | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const label = (uz: string, ru: string, en: string) =>
    language === 'uz' ? uz : language === 'ru' ? ru : en;

  useEffect(() => {
    const load = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await propertyService.getPropertyById(propertyId);
        setProperty(data);
      } catch (e: any) {
        setError(e?.message ?? label("Ma'lumotni yuklashda xatolik", 'Ошибка загрузки данных', 'Failed to load data'));
      } finally {
        setLoading(false);
      }
    };
    if (propertyId) {
      load();
    } else {
      setLoading(false);
      setError(label("Ma'lumot topilmadi", 'Данные не найдены', 'No data found'));
    }
  }, [propertyId]);

  const handleCall = () => {
    if (!property?.contactPhoneNumber) return;
    Linking.openURL(`tel:${property.contactPhoneNumber}`).catch(() => {
      Alert.alert('Xato', label("Qo'ng'iroq qilishda xato", 'Ошибка при звонке', 'Error making call'));
    });
  };

  if (loading) {
    return (
      <View style={[styles.container, styles.centerBox, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color={colors.primary} />
      </View>
    );
  }

  if (error || !property) {
    return (
      <View style={[styles.container, styles.centerBox, { backgroundColor: colors.background }]}>
        <Text style={{ color: '#EF4444', fontSize: FontSize.sm }}>
          {error ?? label("Ma'lumot topilmadi", 'Данные не найдены', 'No data found')}
        </Text>
        <TouchableOpacity onPress={() => router.back()} style={[styles.retryBtn, { backgroundColor: colors.primary }]}>
          <Text style={styles.retryText}>{label('Orqaga', 'Назад', 'Back')}</Text>
        </TouchableOpacity>
      </View>
    );
  }

  const priceFormatted = `${property.price.toLocaleString()} ${label("so'm", 'сум', 'UZS')}`;
  const rentalPeriodLabel = getRentalPeriodLabel(property.rentalPeriod, language);
  const propertyTypeLabel = getPropertyTypeLabel(property.propertyType, language);
  const renovationStatusLabel = getRenovationStatusLabel(property.renovationStatus, language);
  // Gallery must come from the ordered images[] (detail fetch), never from coverImageUrl - the
  // latter is only a cheap thumbnail derived for list/marker responses.
  const images = property.images ?? [];

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      {/* Gradient Header */}
      <LinearGradient
        colors={isDark ? ['#14532D', '#15803D'] : ['#15803D', '#22C55E']}
        start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }}
        style={styles.gradientHeader}
      >
        <View style={styles.headerTop}>
          <TouchableOpacity style={styles.headerButton} onPress={() => router.back()}>
            <BackIcon size={22} color="#FFFFFF" />
          </TouchableOpacity>
          <View style={{ width: 40 }} />
        </View>
        <Text style={styles.titleHeader}>{property.title}</Text>
        <View style={styles.headerBadges}>
          <View style={styles.headerBadge}>
            <MapPinIcon size={12} color="#FFFFFF" />
            <Text style={styles.headerBadgeText} numberOfLines={1}>{property.address}</Text>
          </View>
          <View style={styles.headerBadge}>
            <Text style={styles.headerBadgeText}>{propertyTypeLabel}</Text>
          </View>
        </View>
      </LinearGradient>

      <ScrollView showsVerticalScrollIndicator={false}>
        {/* Highlights */}
        <View style={[styles.salaryCard, { backgroundColor: colors.card, ...Shadow.md }]}>
          <View style={styles.salaryItem}>
            <MoneyIcon size={20} color={colors.primary} />
            <Text style={[styles.salaryLabel, { color: colors.textTertiary }]}>
              {label('Narx', 'Цена', 'Price')}
            </Text>
            <Text style={[styles.salaryValue, { color: colors.primary }]} numberOfLines={1}>
              {priceFormatted} / {rentalPeriodLabel}
            </Text>
          </View>
          <View style={[styles.salaryDivider, { backgroundColor: colors.border }]} />
          <View style={styles.salaryItem}>
            <HomeIcon size={20} color={colors.textTertiary} />
            <Text style={[styles.salaryLabel, { color: colors.textTertiary }]}>
              {label('Xonalar', 'Комнаты', 'Rooms')}
            </Text>
            <Text style={[styles.salaryValue, { color: colors.textPrimary }]}>
              {property.roomCount}
            </Text>
          </View>
          <View style={[styles.salaryDivider, { backgroundColor: colors.border }]} />
          <View style={styles.salaryItem}>
            <RealEstateIcon size={20} color={colors.textTertiary} />
            <Text style={[styles.salaryLabel, { color: colors.textTertiary }]}>
              {label('Maydon', 'Площадь', 'Area')}
            </Text>
            <Text style={[styles.salaryValue, { color: colors.textPrimary }]}>
              {property.areaSquareMeters} m²
            </Text>
          </View>
        </View>

        {/* Description */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
            {label('Tavsif', 'Описание', 'Description')}
          </Text>
          <Text style={[styles.description, { color: colors.textSecondary }]}>
            {property.description}
          </Text>
        </View>

        {/* Images */}
        {images.length > 0 && (
          <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
            <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
              {label('Rasmlar', 'Фотографии', 'Photos')}
            </Text>
            <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.imageGalleryRow}>
              {images.map((image) => (
                <Image
                  key={image.id}
                  source={{ uri: `${MEDIA_URL}${image.imageUrl}` }}
                  style={styles.galleryImage}
                />
              ))}
            </ScrollView>
          </View>
        )}

        {/* Details */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
            {label('Tafsilotlar', 'Детали', 'Details')}
          </Text>
          <View style={styles.detailRow}>
            <Text style={[styles.detailLabel, { color: colors.textTertiary }]}>
              {label("Ta'mirlash holati", 'Состояние ремонта', 'Renovation')}
            </Text>
            <Text style={[styles.detailValue, { color: colors.textPrimary }]}>
              {renovationStatusLabel}
            </Text>
          </View>
          {property.propertyType === PropertyType.Apartment && (
            <>
              <View style={styles.detailRow}>
                <Text style={[styles.detailLabel, { color: colors.textTertiary }]}>
                  {label('Qavat', 'Этаж', 'Floor')}
                </Text>
                <Text style={[styles.detailValue, { color: colors.textPrimary }]}>
                  {property.floorNumber}
                </Text>
              </View>
              <View style={styles.detailRow}>
                <Text style={[styles.detailLabel, { color: colors.textTertiary }]}>
                  {label('Binoning umumiy qavatlari', 'Этажность здания', 'Total Floors')}
                </Text>
                <Text style={[styles.detailValue, { color: colors.textPrimary }]}>
                  {property.totalFloors}
                </Text>
              </View>
            </>
          )}
        </View>

        {/* Address */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
            {label('Manzil', 'Адрес', 'Address')}
          </Text>
          <View style={styles.addressRow}>
            <MapPinIcon size={18} color={colors.primary} />
            <Text style={[styles.addressText, { color: colors.textSecondary }]}>
              {property.address}
            </Text>
          </View>
        </View>

        <View style={{ height: 100 }} />
      </ScrollView>

      {/* Call CTA */}
      <View style={[styles.bottomBar, { backgroundColor: colors.surface, ...Shadow.lg }]}>
        <TouchableOpacity
          style={styles.callButtonWrapper}
          activeOpacity={0.85}
          onPress={handleCall}
        >
          <LinearGradient
            colors={['#15803D', '#16A34A']}
            start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }}
            style={styles.callButton}
          >
            <PhoneIcon size={20} color="#FFFFFF" />
            <Text style={styles.callButtonText}>
              {label("Qo'ng'iroq qilish", 'Позвонить', 'Call')}
            </Text>
          </LinearGradient>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  centerBox: { alignItems: 'center', justifyContent: 'center', gap: 12 },
  gradientHeader: {
    paddingTop: 56, paddingBottom: Spacing.xl,
    alignItems: 'center', gap: Spacing.sm,
  },
  headerTop: {
    flexDirection: 'row', justifyContent: 'space-between',
    width: '100%', paddingHorizontal: Spacing.xl, marginBottom: Spacing.md,
  },
  headerButton: {
    width: 40, height: 40, borderRadius: BorderRadius.full,
    backgroundColor: 'rgba(255,255,255,0.2)',
    alignItems: 'center', justifyContent: 'center',
  },
  titleHeader: {
    fontSize: FontSize.xl, fontWeight: FontWeight.bold, color: '#FFFFFF',
    textAlign: 'center', paddingHorizontal: Spacing.xl,
  },
  headerBadges: { flexDirection: 'row', gap: Spacing.md, marginTop: Spacing.sm },
  headerBadge: {
    flexDirection: 'row', alignItems: 'center', gap: 4,
    backgroundColor: 'rgba(255,255,255,0.2)', borderRadius: BorderRadius.full,
    paddingHorizontal: Spacing.md, paddingVertical: Spacing.xs, maxWidth: 200,
  },
  headerBadgeText: { fontSize: FontSize.xs, color: '#FFFFFF', fontWeight: FontWeight.medium },
  salaryCard: {
    flexDirection: 'row', marginHorizontal: Spacing.xl,
    marginTop: Spacing.lg, borderRadius: BorderRadius.xl, padding: Spacing.lg,
  },
  salaryItem: { flex: 1, alignItems: 'center', gap: 4 },
  salaryLabel: { fontSize: FontSize.xs, fontWeight: FontWeight.medium },
  salaryValue: { fontSize: FontSize.sm, fontWeight: FontWeight.bold },
  salaryDivider: { width: 1, marginVertical: 4 },
  section: {
    marginHorizontal: Spacing.xl, marginTop: Spacing.lg,
    borderRadius: BorderRadius.xl, padding: Spacing.lg,
  },
  sectionTitle: { fontSize: FontSize.lg, fontWeight: FontWeight.bold, marginBottom: Spacing.md },
  description: { fontSize: FontSize.md, lineHeight: 24 },
  imageGalleryRow: { marginTop: 4 },
  galleryImage: { width: 120, height: 120, borderRadius: BorderRadius.lg, marginRight: Spacing.sm },
  detailRow: {
    flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center',
    paddingVertical: Spacing.sm,
  },
  detailLabel: { fontSize: FontSize.sm },
  detailValue: { fontSize: FontSize.sm, fontWeight: FontWeight.semiBold },
  addressRow: { flexDirection: 'row', alignItems: 'center', gap: Spacing.sm },
  addressText: { flex: 1, fontSize: FontSize.md, lineHeight: 22 },
  bottomBar: {
    position: 'absolute', bottom: 0, left: 0, right: 0,
    paddingHorizontal: Spacing.xl, paddingVertical: Spacing.lg, paddingBottom: Spacing.xl,
  },
  callButtonWrapper: { borderRadius: BorderRadius.lg, overflow: 'hidden', ...Shadow.md },
  callButton: {
    height: 54, flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    gap: Spacing.sm, borderRadius: BorderRadius.lg,
  },
  callButtonText: { fontSize: FontSize.lg, fontWeight: FontWeight.bold, color: '#FFFFFF' },
  retryBtn: {
    paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md,
    borderRadius: BorderRadius.lg, marginTop: 4,
  },
  retryText: { color: '#fff', fontWeight: FontWeight.semiBold, fontSize: FontSize.sm },
});
