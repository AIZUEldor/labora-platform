import React, { useEffect, useState, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  ActivityIndicator,
  RefreshControl,
  Platform,
} from 'react-native';
import { useRouter } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { useThemeStore } from '../store/themeStore';
import { ThemeColors, LightColors } from '../constants/colors';
import { ArrowLeftIcon, BriefcaseIcon, LocationIcon, ClockIcon, HeartIcon } from '../components/icons';
import { savedJobService } from '../services/savedJobService';
import { Job } from '../types';
import { MEDIA_URL } from '../services/api';

export default function SavedJobsScreen(): React.JSX.Element {
  const router = useRouter();
  const { theme } = useThemeStore();
  const colors = ThemeColors[theme];

  const [jobs, setJobs] = useState<Job[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const load = useCallback(async (): Promise<void> => {
    try {
      const data = await savedJobService.getSavedJobs();
      setJobs(data);
    } catch {
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => { load(); }, []);

  const handleUnsave = async (jobId: string): Promise<void> => {
    try {
      await savedJobService.unsaveJob(jobId);
      setJobs(prev => prev.filter(j => j.id !== jobId));
    } catch {}
  };

  const styles = createStyles(colors, theme);

  return (
    <View style={styles.container}>
      <LinearGradient
        colors={theme === 'dark' ? ['#14532d', '#166534'] : ['#16A34A', '#15803d']}
        style={styles.header}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
      >
        <TouchableOpacity style={styles.backButton} onPress={() => router.back()} activeOpacity={0.7}>
          <ArrowLeftIcon size={22} color="#ffffff" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Saqlangan ishlar</Text>
        <View style={{ width: 40 }} />
      </LinearGradient>

      {loading ? (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={colors.primary} />
        </View>
      ) : (
        <ScrollView
          style={styles.scrollView}
          contentContainerStyle={styles.scrollContent}
          showsVerticalScrollIndicator={false}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => { setRefreshing(true); load(); }}
              tintColor={colors.primary}
              colors={[colors.primary]}
            />
          }
        >
          {jobs.length === 0 ? (
            <View style={styles.emptyContainer}>
              <HeartIcon size={56} color={colors.border} />
              <Text style={styles.emptyTitle}>Saqlangan ishlar yo'q</Text>
              <Text style={styles.emptySubtitle}>
                Ish e'lonlarini saqlash uchun yurak tugmasini bosing
              </Text>
              <TouchableOpacity
                style={styles.browseButton}
                onPress={() => router.push('/(tabs)')}
                activeOpacity={0.85}
              >
                <LinearGradient
                  colors={['#16A34A', '#15803d']}
                  style={styles.browseButtonGradient}
                  start={{ x: 0, y: 0 }}
                  end={{ x: 1, y: 0 }}
                >
                  <Text style={styles.browseButtonText}>Ishlarni ko'rish</Text>
                </LinearGradient>
              </TouchableOpacity>
            </View>
          ) : (
            jobs.map(job => (
              <TouchableOpacity
                key={job.id}
                style={styles.jobCard}
                onPress={() => router.push({ pathname: '/job-detail', params: { id: job.id } })}
                activeOpacity={0.85}
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
                  <TouchableOpacity
                    style={styles.unsaveButton}
                    onPress={() => handleUnsave(job.id)}
                    activeOpacity={0.7}
                  >
                    <HeartIcon size={22} color="#ef4444" />
                  </TouchableOpacity>
                </View>

                <View style={[styles.divider, { backgroundColor: colors.border }]} />

                <View style={styles.jobCardBottom}>
                  {job.location ? (
                    <View style={styles.jobMeta}>
                      <LocationIcon size={12} color={colors.textTertiary} />
                      <Text style={[styles.jobMetaText, { color: colors.textTertiary }]} numberOfLines={1}>
                        {job.location}
                      </Text>
                    </View>
                  ) : null}
                  <View style={[styles.salaryBadge, { backgroundColor: colors.primaryLight }]}>
                    <Text style={[styles.salaryText, { color: colors.primary }]}>
                      {job.salary
                        ? `${(job.salary / 1_000_000).toFixed(1)}M so'm`
                        : 'Kelishiladi'}
                    </Text>
                  </View>
                </View>
              </TouchableOpacity>
            ))
          )}
          <View style={{ height: 40 }} />
        </ScrollView>
      )}
    </View>
  );
}

function createStyles(colors: typeof LightColors, theme: 'light' | 'dark') {
  return StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: colors.background,
    },
    header: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      paddingTop: Platform.OS === 'ios' ? 56 : 48,
      paddingBottom: 16,
      paddingHorizontal: 16,
    },
    backButton: {
      width: 40,
      height: 40,
      borderRadius: 20,
      backgroundColor: 'rgba(255,255,255,0.15)',
      justifyContent: 'center',
      alignItems: 'center',
    },
    headerTitle: {
      fontSize: 18,
      fontWeight: '700',
      color: '#ffffff',
      flex: 1,
      textAlign: 'center',
      marginHorizontal: 8,
    },
    loadingContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
    },
    scrollView: { flex: 1 },
    scrollContent: { padding: 16 },
    emptyContainer: {
      alignItems: 'center',
      paddingVertical: 60,
      gap: 12,
    },
    emptyTitle: {
      fontSize: 18,
      fontWeight: '700',
      color: colors.textPrimary,
    },
    emptySubtitle: {
      fontSize: 14,
      color: colors.textSecondary,
      textAlign: 'center',
      paddingHorizontal: 32,
    },
    browseButton: {
      marginTop: 8,
      borderRadius: 14,
      overflow: 'hidden',
      width: 200,
    },
    browseButtonGradient: {
      height: 48,
      alignItems: 'center',
      justifyContent: 'center',
    },
    browseButtonText: {
      fontSize: 15,
      fontWeight: '700',
      color: '#ffffff',
    },
    jobCard: {
      backgroundColor: colors.card,
      borderRadius: 16,
      padding: 16,
      marginBottom: 12,
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 2 },
      shadowOpacity: theme === 'dark' ? 0.3 : 0.08,
      shadowRadius: 8,
      elevation: 3,
    },
    jobCardTop: {
      flexDirection: 'row',
      alignItems: 'center',
    },
    companyLogo: {
      width: 48,
      height: 48,
      borderRadius: 12,
      alignItems: 'center',
      justifyContent: 'center',
      marginRight: 12,
    },
    companyLogoText: {
      fontSize: 18,
      fontWeight: '700',
    },
    jobInfo: { flex: 1 },
    jobTitle: {
      fontSize: 15,
      fontWeight: '700',
      marginBottom: 2,
    },
    companyName: { fontSize: 13 },
    unsaveButton: {
      padding: 8,
    },
    divider: {
      height: 1,
      marginVertical: 12,
    },
    jobCardBottom: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
    },
    jobMeta: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: 4,
    },
    jobMetaText: {
      fontSize: 12,
      fontWeight: '500',
    },
    salaryBadge: {
      borderRadius: 8,
      paddingHorizontal: 10,
      paddingVertical: 4,
    },
    salaryText: {
      fontSize: 12,
      fontWeight: '700',
    },
  });
}