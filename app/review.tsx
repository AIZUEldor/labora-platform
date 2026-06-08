import React, { useState } from 'react';
import {
  View, Text, StyleSheet, ScrollView,
  TouchableOpacity, TextInput, ActivityIndicator, Alert,
} from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { FontSize, FontWeight } from '../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../constants/spacing';
import { useThemeStore } from '../store/themeStore';
import { useAuthStore, AuthState } from '../store/authStore';
import { StarIcon, CheckCircleIcon, ArrowLeftIcon } from '../components/icons';
import { reviewService } from '../services/reviewService';
import { UserRole } from '../types';
import { useLanguageStore } from '../stores/useLanguageStore';

function StarRating({ value, onChange, label, colors }: {
  value: number;
  onChange: (v: number) => void;
  label: string;
  colors: any;
}) {
  return (
    <View style={styles.ratingRow}>
      <Text style={[styles.ratingLabel, { color: colors.textSecondary }]}>{label}</Text>
      <View style={styles.starsRow}>
        {[1, 2, 3, 4, 5].map(star => (
          <TouchableOpacity key={star} onPress={() => onChange(star)} activeOpacity={0.7}>
            <StarIcon size={30} color={star <= value ? '#F59E0B' : colors.border} />
          </TouchableOpacity>
        ))}
      </View>
    </View>
  );
}

export default function ReviewScreen() {
  const { colors, isDark } = useThemeStore();
  const { t } = useLanguageStore();
  const role       = useAuthStore((state: AuthState) => state.role);
  const isEmployer = Number(role) === UserRole.Employer;

  const { jobApplicationId, targetName } = useLocalSearchParams<{
    jobApplicationId: string;
    targetName: string;
  }>();

  const [overallRating,               setOverallRating]               = useState(0);
  const [wouldWorkAgain,              setWouldWorkAgain]              = useState<boolean | null>(null);
  const [comment,                     setComment]                     = useState('');
  const [submitting,                  setSubmitting]                  = useState(false);

  // Worker → Employer
  const [paymentRating,               setPaymentRating]               = useState(0);
  const [employerCommunicationRating, setEmployerCommunicationRating] = useState(0);
  const [workConditionRating,         setWorkConditionRating]         = useState(0);

  // Employer → Worker
  const [experienceRating,            setExperienceRating]            = useState(0);
  const [workerCommunicationRating,   setWorkerCommunicationRating]   = useState(0);
  const [workQualityRating,           setWorkQualityRating]           = useState(0);
  const [punctualityRating,           setPunctualityRating]           = useState(0);
  const [responsibilityRating,        setResponsibilityRating]        = useState(0);

  const handleSubmit = async () => {
    if (overallRating === 0) {
      Alert.alert(t.common.error, t.review.rating);
      return;
    }
    if (wouldWorkAgain === null) {
      Alert.alert(t.common.error, t.review.wouldWorkAgain);
      return;
    }
    try {
      setSubmitting(true);
      await reviewService.create({
        jobApplicationId,
        overallRating,
        wouldWorkAgain,
        comment: comment.trim() || undefined,
        ...(isEmployer ? {
          experienceRating:          experienceRating          || undefined,
          workerCommunicationRating: workerCommunicationRating || undefined,
          workQualityRating:         workQualityRating         || undefined,
          punctualityRating:         punctualityRating         || undefined,
          responsibilityRating:      responsibilityRating      || undefined,
        } : {
          paymentRating:               paymentRating               || undefined,
          employerCommunicationRating: employerCommunicationRating || undefined,
          workConditionRating:         workConditionRating         || undefined,
        }),
      });
      Alert.alert(t.common.success, t.review.title, [
        { text: t.common.ok, onPress: () => router.back() },
      ]);
    } catch (e: any) {
      Alert.alert(t.common.error, e?.message ?? t.common.somethingWentWrong);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>

      {/* Header */}
      <LinearGradient
        colors={isDark ? ['#14532D', '#15803D'] : ['#15803D', '#22C55E']}
        start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }}
        style={styles.gradientHeader}
      >
        <View style={styles.headerTop}>
          <TouchableOpacity style={styles.headerButton} onPress={() => router.back()} activeOpacity={0.7}>
            <ArrowLeftIcon size={22} color="#fff" />
          </TouchableOpacity>
          <View style={styles.headerCenter}>
            <Text style={styles.headerTitle}>{t.review.title}</Text>
            {targetName ? <Text style={styles.headerSubtitle}>{targetName}</Text> : null}
          </View>
          <View style={{ width: 40 }} />
        </View>
      </LinearGradient>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>

        {/* Umumiy baho */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
            {t.review.rating} *
          </Text>
          <View style={styles.starsRowCenter}>
            {[1, 2, 3, 4, 5].map(star => (
              <TouchableOpacity key={star} onPress={() => setOverallRating(star)} activeOpacity={0.7}>
                <StarIcon size={40} color={star <= overallRating ? '#F59E0B' : colors.border} />
              </TouchableOpacity>
            ))}
          </View>
          {overallRating > 0 && (
            <Text style={[styles.ratingValue, { color: '#F59E0B' }]}>
              {overallRating}/5
            </Text>
          )}
        </View>

        {/* Batafsil baholash */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
            {t.review.categories}
          </Text>

          {isEmployer ? (
            // Employer → Worker
            <>
              <StarRating value={responsibilityRating}      onChange={setResponsibilityRating}      label={t.review.responsibility}       colors={colors} />
              <View style={[styles.divider, { backgroundColor: colors.border }]} />
              <StarRating value={workQualityRating}         onChange={setWorkQualityRating}          label={t.review.workQuality}           colors={colors} />
              <View style={[styles.divider, { backgroundColor: colors.border }]} />
              <StarRating value={punctualityRating}         onChange={setPunctualityRating}          label={t.review.punctuality}           colors={colors} />
              <View style={[styles.divider, { backgroundColor: colors.border }]} />
              <StarRating value={workerCommunicationRating} onChange={setWorkerCommunicationRating}  label={t.review.workerCommunication}   colors={colors} />
              <View style={[styles.divider, { backgroundColor: colors.border }]} />
              <StarRating value={experienceRating}          onChange={setExperienceRating}           label={t.editProfile.experience}       colors={colors} />
            </>
          ) : (
            // Worker → Employer
            <>
              <StarRating value={paymentRating}               onChange={setPaymentRating}               label={t.review.paymentOnTime}          colors={colors} />
              <View style={[styles.divider, { backgroundColor: colors.border }]} />
              <StarRating value={employerCommunicationRating} onChange={setEmployerCommunicationRating} label={t.review.employerCommunication}  colors={colors} />
              <View style={[styles.divider, { backgroundColor: colors.border }]} />
              <StarRating value={workConditionRating}         onChange={setWorkConditionRating}         label={t.review.workCondition}          colors={colors} />
            </>
          )}
        </View>

        {/* Qayta ishlash */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
            {t.review.wouldWorkAgain}
          </Text>
          <View style={styles.yesNoRow}>
            <TouchableOpacity
              style={[
                styles.yesNoBtn,
                { borderColor: colors.border, backgroundColor: colors.background },
                wouldWorkAgain === true && { backgroundColor: '#DCFCE7', borderColor: '#16A34A' },
              ]}
              onPress={() => setWouldWorkAgain(true)}
              activeOpacity={0.8}
            >
              <CheckCircleIcon size={20} color={wouldWorkAgain === true ? '#16A34A' : colors.textTertiary} />
              <Text style={[styles.yesNoText, { color: wouldWorkAgain === true ? '#16A34A' : colors.textSecondary }]}>
                {t.common.yes}
              </Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={[
                styles.yesNoBtn,
                { borderColor: colors.border, backgroundColor: colors.background },
                wouldWorkAgain === false && { backgroundColor: '#FEE2E2', borderColor: '#EF4444' },
              ]}
              onPress={() => setWouldWorkAgain(false)}
              activeOpacity={0.8}
            >
              <CheckCircleIcon size={20} color={wouldWorkAgain === false ? '#EF4444' : colors.textTertiary} />
              <Text style={[styles.yesNoText, { color: wouldWorkAgain === false ? '#EF4444' : colors.textSecondary }]}>
                {t.common.no}
              </Text>
            </TouchableOpacity>
          </View>
        </View>

        {/* Izoh */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
            {t.review.comment}
          </Text>
          <TextInput
            style={[styles.commentInput, { backgroundColor: colors.background, color: colors.textPrimary, borderColor: colors.border }]}
            placeholder={t.review.placeholder}
            placeholderTextColor={colors.textTertiary}
            value={comment}
            onChangeText={setComment}
            multiline
            numberOfLines={4}
            textAlignVertical="top"
          />
        </View>

        <View style={{ height: 120 }} />
      </ScrollView>

      {/* Submit */}
      <View style={[styles.bottomBar, { backgroundColor: colors.surface, ...Shadow.lg }]}>
        <TouchableOpacity
          style={[styles.submitWrapper, { opacity: submitting ? 0.7 : 1 }]}
          onPress={handleSubmit}
          activeOpacity={0.85}
          disabled={submitting}
        >
          <LinearGradient
            colors={['#15803D', '#16A34A']}
            start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }}
            style={styles.submitButton}
          >
            {submitting
              ? <ActivityIndicator color="#fff" />
              : <Text style={styles.submitText}>{t.review.submit}</Text>
            }
          </LinearGradient>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container:       { flex: 1 },
  gradientHeader:  { paddingTop: 56, paddingBottom: Spacing.lg },
  headerTop: {
    flexDirection: 'row', alignItems: 'center',
    justifyContent: 'space-between', paddingHorizontal: Spacing.xl,
  },
  headerButton: {
    width: 40, height: 40, borderRadius: BorderRadius.full,
    backgroundColor: 'rgba(255,255,255,0.2)',
    alignItems: 'center', justifyContent: 'center',
  },
  headerCenter:    { flex: 1, alignItems: 'center' },
  headerTitle:     { fontSize: FontSize.lg, fontWeight: FontWeight.bold, color: '#fff' },
  headerSubtitle:  { fontSize: FontSize.xs, color: 'rgba(255,255,255,0.8)', marginTop: 2 },
  content:         { padding: Spacing.xl, gap: Spacing.lg },
  section:         { borderRadius: BorderRadius.xl, padding: Spacing.lg, gap: Spacing.md },
  sectionTitle:    { fontSize: FontSize.md, fontWeight: FontWeight.bold },
  starsRowCenter:  { flexDirection: 'row', justifyContent: 'center', gap: Spacing.sm },
  ratingValue:     { textAlign: 'center', fontSize: FontSize.md, fontWeight: FontWeight.bold },
  ratingRow:       { gap: Spacing.xs },
  ratingLabel:     { fontSize: FontSize.sm, fontWeight: FontWeight.medium },
  starsRow:        { flexDirection: 'row', gap: Spacing.sm },
  divider:         { height: 1 },
  yesNoRow:        { flexDirection: 'row', gap: Spacing.md },
  yesNoBtn: {
    flex: 1, flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    gap: Spacing.sm, height: 48, borderRadius: BorderRadius.lg, borderWidth: 1.5,
  },
  yesNoText:    { fontSize: FontSize.md, fontWeight: FontWeight.semiBold },
  commentInput: {
    borderWidth: 1.5, borderRadius: BorderRadius.lg,
    padding: Spacing.md, fontSize: FontSize.md, minHeight: 100,
  },
  bottomBar: {
    position: 'absolute', bottom: 0, left: 0, right: 0,
    paddingHorizontal: Spacing.xl, paddingVertical: Spacing.lg, paddingBottom: Spacing.xl,
  },
  submitWrapper: { borderRadius: BorderRadius.lg, overflow: 'hidden' },
  submitButton:  { height: 54, alignItems: 'center', justifyContent: 'center', borderRadius: BorderRadius.lg },
  submitText:    { fontSize: FontSize.lg, fontWeight: FontWeight.bold, color: '#fff' },
});
