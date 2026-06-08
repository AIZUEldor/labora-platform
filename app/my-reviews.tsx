import React, { useEffect, useState, useCallback } from 'react';
import {
  View, Text, StyleSheet, ScrollView,
  TouchableOpacity, ActivityIndicator, RefreshControl,
} from 'react-native';
import { router } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { FontSize, FontWeight } from '../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../constants/spacing';
import { useThemeStore } from '../store/themeStore';
import { useAuthStore, AuthState } from '../store/authStore';
import { reviewService } from '../services/reviewService';
import { Review } from '../types';
import { StarIcon, ArrowLeftIcon, ClockIcon } from '../components/icons';
import { useLanguageStore } from '../stores/useLanguageStore';
import { userService } from '../services/userService';

function formatDate(dateStr: string): string {
  const d = new Date(dateStr);
  return `${d.getDate()}.${d.getMonth() + 1}.${d.getFullYear()}`;
}

function StarDisplay({ rating, size = 16 }: { rating: number; size?: number }) {
  const { colors } = useThemeStore();
  return (
    <View style={{ flexDirection: 'row', gap: 2 }}>
      {[1, 2, 3, 4, 5].map(star => (
        <StarIcon key={star} size={size} color={star <= rating ? '#F59E0B' : colors.border} />
      ))}
    </View>
  );
}

export default function MyReviewsScreen() {
  const { colors, isDark } = useThemeStore();
  const { t, language } = useLanguageStore();

  const [reviews,    setReviews]    = useState<Review[]>([]);
  const [average,    setAverage]    = useState<number>(0);
  const [loading,    setLoading]    = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error,      setError]      = useState<string | null>(null);

  const load = useCallback(async () => {
  try {
    const profile = await userService.getProfile();
    const [data, avg] = await Promise.all([
      reviewService.getUserReviews(profile.id),
      reviewService.getAverageRating(profile.id),
    ]);
    setReviews(data);
    setAverage(avg);
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
        style={styles.header}
      >
        <View style={styles.headerTop}>
          <TouchableOpacity style={styles.headerButton} onPress={() => router.back()} activeOpacity={0.7}>
            <ArrowLeftIcon size={22} color="#fff" />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>{t.profile.reviews}</Text>
          <View style={{ width: 40 }} />
        </View>

        {/* O'rtacha reyting */}
        {!loading && reviews.length > 0 && (
          <View style={styles.averageBox}>
            <Text style={styles.averageValue}>{average.toFixed(1)}</Text>
            <StarDisplay rating={Math.round(average)} size={20} />
            <Text style={styles.averageCount}>
              {reviews.length} {language === 'uz' ? 'ta baho' : language === 'ru' ? 'отзывов' : 'reviews'}
            </Text>
          </View>
        )}
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

      {!loading && !error && reviews.length === 0 && (
        <View style={styles.centerBox}>
          <StarIcon size={48} color={colors.border} />
          <Text style={[styles.stateText, { color: colors.textSecondary, marginTop: Spacing.md }]}>
            {language === 'uz' ? 'Hozircha baholar yo\'q' : language === 'ru' ? 'Отзывов пока нет' : 'No reviews yet'}
          </Text>
        </View>
      )}

      {!loading && !error && reviews.length > 0 && (
        <ScrollView
          showsVerticalScrollIndicator={false}
          contentContainerStyle={styles.listContainer}
          refreshControl={
            <RefreshControl refreshing={refreshing} onRefresh={onRefresh}
              tintColor={colors.primary} colors={[colors.primary]} />
          }
        >
          {reviews.map(review => (
            <View key={review.id} style={[styles.card, { backgroundColor: colors.card, ...Shadow.md }]}>
              <View style={styles.cardTop}>
                <View style={[styles.avatar, { backgroundColor: colors.primaryLight }]}>
                  <Text style={[styles.avatarText, { color: colors.primary }]}>
                    {(review.reviewerName ?? '?')[0].toUpperCase()}
                  </Text>
                </View>
                <View style={styles.cardInfo}>
                  <Text style={[styles.reviewerName, { color: colors.textPrimary }]} numberOfLines={1}>
                    {review.reviewerName}
                  </Text>
                  <StarDisplay rating={review.rating} size={14} />
                </View>
                <View style={styles.dateRow}>
                  <ClockIcon size={12} color={colors.textTertiary} />
                  <Text style={[styles.dateText, { color: colors.textTertiary }]}>
                    {formatDate(review.createdAt)}
                  </Text>
                </View>
              </View>

              {review.comment ? (
                <>
                  <View style={[styles.divider, { backgroundColor: colors.border }]} />
                  <Text style={[styles.comment, { color: colors.textSecondary }]}>
                    {review.comment}
                  </Text>
                </>
              ) : null}
            </View>
          ))}
          <View style={{ height: 24 }} />
        </ScrollView>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container:    { flex: 1 },
  header:       { paddingTop: 56, paddingBottom: Spacing.xl },
  headerTop: {
    flexDirection: 'row', alignItems: 'center',
    justifyContent: 'space-between', paddingHorizontal: Spacing.xl,
  },
  headerButton: {
    width: 40, height: 40, borderRadius: BorderRadius.full,
    backgroundColor: 'rgba(255,255,255,0.2)',
    alignItems: 'center', justifyContent: 'center',
  },
  headerTitle:  { fontSize: FontSize.lg, fontWeight: FontWeight.bold, color: '#fff' },
  averageBox: {
    alignItems: 'center', marginTop: Spacing.lg, gap: Spacing.xs,
  },
  averageValue: { fontSize: 48, fontWeight: FontWeight.bold, color: '#fff', lineHeight: 56 },
  averageCount: { fontSize: FontSize.sm, color: 'rgba(255,255,255,0.8)' },
  centerBox:    { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 12 },
  listContainer: { padding: Spacing.xl, gap: Spacing.md },
  card:         { borderRadius: BorderRadius.xl, padding: Spacing.lg },
  cardTop:      { flexDirection: 'row', alignItems: 'center' },
  avatar: {
    width: 44, height: 44, borderRadius: BorderRadius.full,
    alignItems: 'center', justifyContent: 'center', marginRight: Spacing.md,
  },
  avatarText:   { fontSize: FontSize.lg, fontWeight: FontWeight.bold },
  cardInfo:     { flex: 1, gap: 4 },
  reviewerName: { fontSize: FontSize.md, fontWeight: FontWeight.bold },
  dateRow:      { flexDirection: 'row', alignItems: 'center', gap: 4 },
  dateText:     { fontSize: FontSize.xs },
  divider:      { height: 1, marginVertical: Spacing.md },
  comment:      { fontSize: FontSize.sm, lineHeight: 20 },
  stateText:    { fontSize: FontSize.sm, textAlign: 'center' },
  retryBtn:     { paddingHorizontal: Spacing.xl, paddingVertical: Spacing.md, borderRadius: BorderRadius.lg },
  retryText:    { color: '#fff', fontWeight: FontWeight.semiBold, fontSize: FontSize.sm },
});
