import React, { useEffect, useRef } from 'react';
import { View, Animated, StyleSheet, ViewStyle } from 'react-native';
import { useThemeStore } from '../store/themeStore';
import { ThemeColors } from '../constants/colors';

interface SkeletonProps {
  width?: number | string;
  height?: number;
  borderRadius?: number;
  style?: ViewStyle;
}

export function SkeletonBox({ width = '100%', height = 16, borderRadius = 8, style }: SkeletonProps) {
  const { theme } = useThemeStore();
  const colors = ThemeColors[theme];
  const opacity = useRef(new Animated.Value(0.4)).current;

  useEffect(() => {
    const animation = Animated.loop(
      Animated.sequence([
        Animated.timing(opacity, {
          toValue: 1,
          duration: 800,
          useNativeDriver: true,
        }),
        Animated.timing(opacity, {
          toValue: 0.4,
          duration: 800,
          useNativeDriver: true,
        }),
      ])
    );
    animation.start();
    return () => animation.stop();
  }, []);

  const baseColor = theme === 'dark' ? '#1f2937' : '#e5e7eb';

  return (
    <Animated.View
      style={[
        {
          width: width as any,
          height,
          borderRadius,
          backgroundColor: baseColor,
          opacity,
        },
        style,
      ]}
    />
  );
}

// Job Card Skeleton
export function JobCardSkeleton() {
  const { theme } = useThemeStore();
  const colors = ThemeColors[theme];

  return (
    <View style={[styles.jobCard, { backgroundColor: theme === 'dark' ? '#1f2937' : '#ffffff', borderColor: colors.border }]}>
      <View style={styles.jobCardHeader}>
        <SkeletonBox width={42} height={42} borderRadius={12} />
        <View style={styles.jobCardHeaderText}>
          <SkeletonBox width="70%" height={14} borderRadius={7} />
          <View style={{ height: 6 }} />
          <SkeletonBox width="45%" height={11} borderRadius={6} />
        </View>
        <SkeletonBox width={60} height={24} borderRadius={12} />
      </View>
      <View style={{ height: 12 }} />
      <SkeletonBox width="90%" height={11} borderRadius={6} />
      <View style={{ height: 6 }} />
      <SkeletonBox width="60%" height={11} borderRadius={6} />
      <View style={{ height: 12 }} />
      <View style={styles.jobCardFooter}>
        <SkeletonBox width={80} height={11} borderRadius={6} />
        <SkeletonBox width={60} height={11} borderRadius={6} />
      </View>
    </View>
  );
}

// Profile Skeleton
export function ProfileSkeleton() {
  return (
    <View style={styles.profileContainer}>
      {/* Header */}
      <View style={styles.profileHeader}>
        <SkeletonBox width={90} height={90} borderRadius={45} />
        <View style={{ height: 12 }} />
        <SkeletonBox width={140} height={18} borderRadius={9} />
        <View style={{ height: 8 }} />
        <SkeletonBox width={90} height={13} borderRadius={7} />
        <View style={{ height: 12 }} />
        <View style={styles.profileStats}>
          {[0, 1, 2].map(i => (
            <View key={i} style={styles.profileStatItem}>
              <SkeletonBox width={40} height={18} borderRadius={9} />
              <View style={{ height: 4 }} />
              <SkeletonBox width={55} height={11} borderRadius={6} />
            </View>
          ))}
        </View>
      </View>
      {/* Menu items */}
      <View style={styles.profileMenuContainer}>
        {[0, 1, 2, 3, 4].map(i => (
          <View key={i} style={styles.profileMenuItem}>
            <SkeletonBox width={40} height={40} borderRadius={12} />
            <SkeletonBox width="60%" height={13} borderRadius={7} />
            <SkeletonBox width={20} height={13} borderRadius={7} />
          </View>
        ))}
      </View>
    </View>
  );
}

// Application Card Skeleton
export function ApplicationCardSkeleton() {
  const { theme } = useThemeStore();
  const colors = ThemeColors[theme];

  return (
    <View style={[styles.appCard, { backgroundColor: theme === 'dark' ? '#1f2937' : '#ffffff', borderColor: colors.border }]}>
      <View style={styles.appCardHeader}>
        <SkeletonBox width="65%" height={14} borderRadius={7} />
        <SkeletonBox width={70} height={22} borderRadius={11} />
      </View>
      <View style={{ height: 8 }} />
      <SkeletonBox width="40%" height={11} borderRadius={6} />
      <View style={{ height: 6 }} />
      <SkeletonBox width="55%" height={11} borderRadius={6} />
      <View style={{ height: 12 }} />
      <SkeletonBox width="100%" height={36} borderRadius={10} />
    </View>
  );
}

// Job Detail Skeleton
export function JobDetailSkeleton() {
  const { theme } = useThemeStore();
  const colors = ThemeColors[theme];

  return (
    <View style={[styles.detailContainer, { backgroundColor: colors.background }]}>
      {/* Header image area */}
      <SkeletonBox width="100%" height={200} borderRadius={0} />
      <View style={styles.detailContent}>
        {/* Title */}
        <SkeletonBox width="80%" height={22} borderRadius={11} />
        <View style={{ height: 10 }} />
        <SkeletonBox width="50%" height={14} borderRadius={7} />
        <View style={{ height: 20 }} />
        {/* Info chips */}
        <View style={styles.detailChips}>
          {[0, 1, 2].map(i => (
            <SkeletonBox key={i} width={90} height={32} borderRadius={16} />
          ))}
        </View>
        <View style={{ height: 20 }} />
        {/* Description */}
        <SkeletonBox width="40%" height={14} borderRadius={7} />
        <View style={{ height: 10 }} />
        <SkeletonBox width="100%" height={11} borderRadius={6} />
        <View style={{ height: 6 }} />
        <SkeletonBox width="95%" height={11} borderRadius={6} />
        <View style={{ height: 6 }} />
        <SkeletonBox width="85%" height={11} borderRadius={6} />
        <View style={{ height: 6 }} />
        <SkeletonBox width="90%" height={11} borderRadius={6} />
        <View style={{ height: 6 }} />
        <SkeletonBox width="70%" height={11} borderRadius={6} />
      </View>
    </View>
  );
}

// Notification Skeleton
export function NotificationSkeleton() {
  const { theme } = useThemeStore();
  const colors = ThemeColors[theme];

  return (
    <View style={[styles.notifCard, { backgroundColor: theme === 'dark' ? '#1f2937' : '#ffffff', borderColor: colors.border }]}>
      <SkeletonBox width={42} height={42} borderRadius={21} />
      <View style={styles.notifContent}>
        <SkeletonBox width="70%" height={13} borderRadius={7} />
        <View style={{ height: 6 }} />
        <SkeletonBox width="90%" height={11} borderRadius={6} />
        <View style={{ height: 6 }} />
        <SkeletonBox width="30%" height={10} borderRadius={5} />
      </View>
    </View>
  );
}

// List skeletons
export function JobListSkeleton({ count = 4 }: { count?: number }) {
  return (
    <>
      {Array.from({ length: count }).map((_, i) => (
        <JobCardSkeleton key={i} />
      ))}
    </>
  );
}

export function ApplicationListSkeleton({ count = 4 }: { count?: number }) {
  return (
    <>
      {Array.from({ length: count }).map((_, i) => (
        <ApplicationCardSkeleton key={i} />
      ))}
    </>
  );
}

export function NotificationListSkeleton({ count = 5 }: { count?: number }) {
  return (
    <>
      {Array.from({ length: count }).map((_, i) => (
        <NotificationSkeleton key={i} />
      ))}
    </>
  );
}

const styles = StyleSheet.create({
  // Job card
  jobCard: {
    borderRadius: 14,
    padding: 14,
    marginBottom: 12,
    borderWidth: 1,
  },
  jobCardHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
  },
  jobCardHeaderText: {
    flex: 1,
  },
  jobCardFooter: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },

  // Profile
  profileContainer: {
    flex: 1,
  },
  profileHeader: {
    alignItems: 'center',
    paddingVertical: 36,
    paddingHorizontal: 24,
  },
  profileStats: {
    flexDirection: 'row',
    gap: 32,
    marginTop: 8,
  },
  profileStatItem: {
    alignItems: 'center',
  },
  profileMenuContainer: {
    marginHorizontal: 20,
    borderRadius: 14,
    overflow: 'hidden',
    gap: 2,
  },
  profileMenuItem: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 14,
    gap: 12,
  },

  // Application card
  appCard: {
    borderRadius: 14,
    padding: 14,
    marginBottom: 12,
    borderWidth: 1,
  },
  appCardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },

  // Job detail
  detailContainer: {
    flex: 1,
  },
  detailContent: {
    padding: 20,
  },
  detailChips: {
    flexDirection: 'row',
    gap: 8,
  },

  // Notification
  notifCard: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    borderRadius: 14,
    padding: 14,
    marginBottom: 10,
    borderWidth: 1,
    gap: 12,
  },
  notifContent: {
    flex: 1,
  },
});
