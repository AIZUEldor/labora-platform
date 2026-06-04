import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  ActivityIndicator,
  RefreshControl,
  Platform,
} from 'react-native';
import { useRouter } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { useThemeStore } from '../store/themeStore';
import {
  notificationService,
  NotificationResponse,
} from '../services/notificationService';
import { NotificationType } from '../types';
import {
  BellIcon,
  CheckIcon,
  BriefcaseIcon,
  StarIcon,
  MapPinIcon,
  ArrowLeftIcon,
  CheckCircleIcon,
} from '../components/icons';
import { ThemeColors, LightColors } from '../constants/colors';
import { NotificationListSkeleton } from '../components/SkeletonLoader';
import { useLanguageStore } from '../stores/useLanguageStore';

function getNotificationIcon(type: NotificationType, color: string): React.JSX.Element {
  switch (type) {
    case NotificationType.ApplicationAccepted:
      return <CheckCircleIcon size={20} color={color} />;
    case NotificationType.ApplicationRejected:
      return <BriefcaseIcon size={20} color={color} />;
    case NotificationType.JobCompleted:
      return <CheckIcon size={20} color={color} />;
    case NotificationType.ReviewRequest:
      return <StarIcon size={20} color={color} />;
    case NotificationType.NewJobRecommended:
      return <MapPinIcon size={20} color={color} />;
    default:
      return <BellIcon size={20} color={color} />;
  }
}

function getNotificationColors(type: NotificationType): { bg: string; icon: string } {
  switch (type) {
    case NotificationType.ApplicationAccepted:  return { bg: '#f0fdf4', icon: '#16A34A' };
    case NotificationType.ApplicationRejected:  return { bg: '#fff5f5', icon: '#ef4444' };
    case NotificationType.JobCompleted:         return { bg: '#eff6ff', icon: '#3b82f6' };
    case NotificationType.ReviewRequest:        return { bg: '#fefce8', icon: '#eab308' };
    case NotificationType.NewJobRecommended:    return { bg: '#f0fdf4', icon: '#16A34A' };
    default:                                    return { bg: '#f9fafb', icon: '#6b7280' };
  }
}

function getNotificationColorsDark(type: NotificationType): { bg: string; icon: string } {
  switch (type) {
    case NotificationType.ApplicationAccepted:  return { bg: '#14532d', icon: '#4ade80' };
    case NotificationType.ApplicationRejected:  return { bg: '#450a0a', icon: '#f87171' };
    case NotificationType.JobCompleted:         return { bg: '#1e3a5f', icon: '#60a5fa' };
    case NotificationType.ReviewRequest:        return { bg: '#422006', icon: '#fbbf24' };
    case NotificationType.NewJobRecommended:    return { bg: '#14532d', icon: '#4ade80' };
    default:                                    return { bg: '#1f2937', icon: '#9ca3af' };
  }
}

export default function NotificationsScreen(): React.JSX.Element {
  const router = useRouter();
  const { theme } = useThemeStore();
  const { t } = useLanguageStore();
  const colors = ThemeColors[theme];

  const [notifications, setNotifications] = useState<NotificationResponse[]>([]);
  const [loading,       setLoading]       = useState<boolean>(true);
  const [refreshing,    setRefreshing]    = useState<boolean>(false);
  const [markingAll,    setMarkingAll]    = useState<boolean>(false);

  const unreadCount = notifications.filter(n => !n.isRead).length;

  const formatTime = useCallback((dateStr: string): string => {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    const diffHour = Math.floor(diffMin / 60);
    const diffDay = Math.floor(diffHour / 24);

    if (diffMin < 1)  return t.common.done;
    if (diffMin < 60) return `${diffMin} min`;
    if (diffHour < 24) return `${diffHour} h`;
    if (diffDay < 7)  return `${diffDay} kun`;
    return date.toLocaleDateString('uz-UZ');
  }, [t]);

  const loadNotifications = useCallback(async (): Promise<void> => {
    try {
      const data = await notificationService.getMyNotifications();
      setNotifications(data);
    } catch {
      // silent fail
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => { loadNotifications(); }, [loadNotifications]);

  const handleRefresh = (): void => {
    setRefreshing(true);
    loadNotifications();
  };

  const handleMarkAsRead = async (id: string): Promise<void> => {
    try {
      await notificationService.markAsRead(id);
      setNotifications(prev => prev.map(n => (n.id === id ? { ...n, isRead: true } : n)));
    } catch { }
  };

  const handleMarkAllAsRead = async (): Promise<void> => {
    if (unreadCount === 0) return;
    try {
      setMarkingAll(true);
      await notificationService.markAllAsRead();
      setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
    } catch { } finally {
      setMarkingAll(false);
    }
  };

  const handleNotificationPress = async (item: NotificationResponse): Promise<void> => {
    if (!item.isRead) await handleMarkAsRead(item.id);
    if (!item.referenceId) return;
    switch (item.type) {
      case NotificationType.ApplicationAccepted:
      case NotificationType.ApplicationRejected:
      case NotificationType.JobCompleted:
      case NotificationType.NewJobRecommended:
        router.push(`/job-detail?id=${item.referenceId}`);
        break;
      case NotificationType.ReviewRequest:
        router.push(`/review?applicationId=${item.referenceId}`);
        break;
    }
  };

  const styles = createStyles(colors, theme);

  const renderItem = ({ item }: { item: NotificationResponse }): React.JSX.Element => {
    const typeColors = theme === 'dark'
      ? getNotificationColorsDark(item.type)
      : getNotificationColors(item.type);

    return (
      <TouchableOpacity
        style={[styles.notificationCard, !item.isRead && styles.unreadCard]}
        onPress={() => handleNotificationPress(item)}
        activeOpacity={0.75}
      >
        {!item.isRead && <View style={styles.unreadDot} />}
        <View style={[styles.iconWrapper, { backgroundColor: typeColors.bg }]}>
          {getNotificationIcon(item.type, typeColors.icon)}
        </View>
        <View style={styles.notificationContent}>
          <Text style={[styles.notificationTitle, !item.isRead && styles.unreadTitle]}>
            {item.title}
          </Text>
          <Text style={styles.notificationMessage} numberOfLines={2}>
            {item.message}
          </Text>
          <Text style={styles.notificationTime}>
            {formatTime(item.createdAt)}
          </Text>
        </View>
      </TouchableOpacity>
    );
  };

  const renderEmpty = (): React.JSX.Element => (
    <View style={styles.emptyContainer}>
      <View style={styles.emptyIconWrapper}>
        <BellIcon size={48} color={colors.textSecondary} />
      </View>
      <Text style={styles.emptyTitle}>{t.notifications.noNotifications}</Text>
      <Text style={styles.emptySubtitle}>{t.notifications.title}</Text>
    </View>
  );

  const renderHeader = (): React.JSX.Element | null => {
    if (notifications.length === 0) return null;
    return (
      <View style={styles.listHeader}>
        <Text style={styles.listHeaderText}>
          {unreadCount > 0
            ? `${unreadCount} ta o'qilmagan`
            : t.notifications.markAllRead}
        </Text>
      </View>
    );
  };

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

        <View style={styles.headerCenter}>
          <Text style={styles.headerTitle}>{t.notifications.title}</Text>
          {unreadCount > 0 && (
            <View style={styles.headerBadge}>
              <Text style={styles.headerBadgeText}>{unreadCount}</Text>
            </View>
          )}
        </View>

        <TouchableOpacity
          style={[styles.markAllButton, (unreadCount === 0 || markingAll) && styles.markAllDisabled]}
          onPress={handleMarkAllAsRead}
          disabled={unreadCount === 0 || markingAll}
          activeOpacity={0.7}
        >
          {markingAll
            ? <ActivityIndicator size="small" color="#ffffff" />
            : <CheckIcon size={20} color="#ffffff" />}
        </TouchableOpacity>
      </LinearGradient>

      {loading ? (
        <View style={{ padding: 16 }}>
          <NotificationListSkeleton count={5} />
        </View>
      ) : (
        <FlatList
          data={notifications}
          keyExtractor={item => item.id}
          renderItem={renderItem}
          ListEmptyComponent={renderEmpty}
          ListHeaderComponent={renderHeader}
          contentContainerStyle={[
            styles.listContent,
            notifications.length === 0 && styles.listContentEmpty,
          ]}
          showsVerticalScrollIndicator={false}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={handleRefresh}
              colors={[colors.primary]}
              tintColor={colors.primary}
            />
          }
        />
      )}
    </View>
  );
}

function createStyles(colors: typeof LightColors, theme: 'light' | 'dark') {
  return StyleSheet.create({
    container:          { flex: 1, backgroundColor: colors.background },
    header: {
      flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
      paddingTop: Platform.OS === 'ios' ? 56 : 48, paddingBottom: 16, paddingHorizontal: 16,
    },
    backButton: {
      width: 40, height: 40, borderRadius: 20,
      backgroundColor: 'rgba(255,255,255,0.15)',
      justifyContent: 'center', alignItems: 'center',
    },
    headerCenter: {
      flex: 1, flexDirection: 'row', alignItems: 'center',
      justifyContent: 'center', gap: 8, marginHorizontal: 8,
    },
    headerTitle:        { fontSize: 18, fontWeight: '700', color: '#ffffff' },
    headerBadge: {
      backgroundColor: '#ef4444', borderRadius: 10,
      minWidth: 20, height: 20, justifyContent: 'center',
      alignItems: 'center', paddingHorizontal: 5,
    },
    headerBadgeText:    { fontSize: 11, fontWeight: '700', color: '#ffffff' },
    markAllButton: {
      width: 40, height: 40, borderRadius: 20,
      backgroundColor: 'rgba(255,255,255,0.2)',
      justifyContent: 'center', alignItems: 'center',
    },
    markAllDisabled:    { opacity: 0.4 },
    listContent:        { paddingHorizontal: 16, paddingTop: 12, paddingBottom: 24 },
    listContentEmpty:   { flex: 1 },
    listHeader:         { marginBottom: 12 },
    listHeaderText:     { fontSize: 13, color: colors.textSecondary, fontWeight: '500' },
    notificationCard: {
      flexDirection: 'row', alignItems: 'flex-start',
      backgroundColor: theme === 'dark' ? '#1f2937' : '#ffffff',
      borderRadius: 14, padding: 14, marginBottom: 10,
      borderWidth: 1, borderColor: colors.border, position: 'relative',
    },
    unreadCard: {
      borderColor: theme === 'dark' ? '#166534' : '#bbf7d0',
      backgroundColor: theme === 'dark' ? '#1a2e1a' : '#f0fdf4',
    },
    unreadDot: {
      position: 'absolute', top: 14, right: 14,
      width: 8, height: 8, borderRadius: 4, backgroundColor: '#16A34A',
    },
    iconWrapper: {
      width: 42, height: 42, borderRadius: 21,
      justifyContent: 'center', alignItems: 'center', marginRight: 12,
    },
    notificationContent: { flex: 1, paddingRight: 16 },
    notificationTitle:   { fontSize: 14, fontWeight: '500', color: colors.textPrimary, marginBottom: 3 },
    unreadTitle:         { fontWeight: '700' },
    notificationMessage: { fontSize: 13, color: colors.textSecondary, lineHeight: 18, marginBottom: 6 },
    notificationTime:    { fontSize: 11, color: colors.textTertiary },
    emptyContainer: {
      flex: 1, justifyContent: 'center', alignItems: 'center', paddingBottom: 80,
    },
    emptyIconWrapper: {
      width: 88, height: 88, borderRadius: 44,
      backgroundColor: theme === 'dark' ? '#1f2937' : '#f3f4f6',
      justifyContent: 'center', alignItems: 'center', marginBottom: 16,
    },
    emptyTitle:    { fontSize: 18, fontWeight: '700', color: colors.textPrimary, marginBottom: 8 },
    emptySubtitle: { fontSize: 14, color: colors.textSecondary, textAlign: 'center' },
  });
}
