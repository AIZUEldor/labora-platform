import React, { useEffect, useState, useCallback } from 'react';
import {
  View, Text, StyleSheet, ScrollView, TouchableOpacity,
  ActivityIndicator, RefreshControl, Alert, Image,
} from 'react-native';
import { FontSize, FontWeight } from '../../constants/typography';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { useThemeStore } from '../../store/themeStore';
import { useAuthStore, AuthState } from '../../store/authStore';
import { router } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import * as DocumentPicker from 'expo-document-picker';
import * as ImagePicker from 'expo-image-picker';
import {
  EditIcon, HeartIcon, StarIcon, BellIcon,
  LockIcon, HelpIcon, LogoutIcon, SunIcon,
  MoonIcon, ChevronRightIcon, CameraIcon,
  ApplicationsIcon, BriefcaseIcon,
} from '../../components/icons';
import { userService } from '../../services/userService';
import { jobApplicationService } from '../../services/jobApplicationService';
import { jobService } from '../../services/jobService';
import { UserProfile, UserRole } from '../../types';
import { ProfileSkeleton } from '../../components/SkeletonLoader';
import { MEDIA_URL } from '../../services/api';

type MenuItem = {
  icon: string;
  label: string;
  route?: string;
};

 const WORKER_MENU: MenuItem[] = [
  { icon: 'edit',         label: 'Profilni tahrirlash', route: '/edit-profile' },
  { icon: 'applications', label: 'Arizalarim',          route: '/(tabs)/applications' },
  { icon: 'heart',        label: 'Saqlangan ishlar' },
  { icon: 'star',         label: 'Baholarim' },
  { icon: 'bell',         label: 'Bildirishnomalar',    route: '/notifications' },
  { icon: 'lock', label: "Parolni o'zgartirish", route: '/change-password' },
  { icon: 'help',         label: 'Yordam' },
];

const EMPLOYER_MENU: MenuItem[] = [
  { icon: 'edit',      label: 'Profilni tahrirlash', route: '/edit-profile' },
  { icon: 'briefcase', label: "E'lonlarim",           route: '/(tabs)/applications' },
  { icon: 'star',      label: 'Baholarim' },
  { icon: 'bell',      label: 'Bildirishnomalar',     route: '/notifications' },
  {  icon: 'lock', label: "Parolni o'zgartirish", route: '/change-password' },
  { icon: 'help',      label: 'Yordam' },
];

function MenuIcon({ name, color }: { name: string; color: string }) {
  const props = { size: 22, color };
  switch (name) {
    case 'edit':         return <EditIcon         {...props} />;
    case 'applications': return <ApplicationsIcon {...props} />;
    case 'briefcase':    return <BriefcaseIcon    {...props} />;
    case 'heart':        return <HeartIcon        {...props} />;
    case 'star':         return <StarIcon         {...props} />;
    case 'bell':         return <BellIcon         {...props} />;
    case 'lock':         return <LockIcon         {...props} />;
    case 'help':         return <HelpIcon         {...props} />;
    default:             return null;
  }
}

export default function ProfileScreen() {
  const { colors, isDark, toggleTheme } = useThemeStore();
  const { t } = useLanguageStore();
  const logout     = useAuthStore((state: AuthState) => state.logout);
  const role       = useAuthStore((state: AuthState) => state.role);
  const isEmployer = Number(role) === UserRole.Employer;

  const WORKER_MENU = [
    { icon: 'edit',         label: t.profile.editProfile,      route: '/edit-profile' },
    { icon: 'applications', label: t.applications.title,        route: '/(tabs)/applications' },
    { icon: 'heart', label: "Saqlangan ishlar" },
{ icon: 'star',  label: t.profile.reviews },
{ icon: 'bell',  label: t.notifications.title, route: '/notifications' },
{ icon: 'lock',  label: "Parolni o'zgartirish" },
{ icon: 'help',  label: "Yordam" },
  ];

  const EMPLOYER_MENU = [
    { icon: 'edit',      label: t.profile.editProfile,   route: '/edit-profile' },
    { icon: 'briefcase', label: t.employer.myJobs,        route: '/(tabs)/applications' },
    { icon: 'star',      label: t.profile.reviews },
    { icon: 'bell',      label: t.notifications.title,    route: '/notifications' },
    { icon: 'lock', label: "Parolni o'zgartirish" },
{ icon: 'help', label: "Yordam" },
  ];

  const [profile,         setProfile]         = useState<UserProfile | null>(null);
  const [statCount,       setStatCount]       = useState(0);
  const [statCount2,      setStatCount2]      = useState(0);
  const [loading,         setLoading]         = useState(true);
  const [refreshing,      setRefreshing]      = useState(false);
  const [uploadingCv,     setUploadingCv]     = useState(false);
  const [uploadingAvatar, setUploadingAvatar] = useState(false);

  const load = useCallback(async () => {
    try {
      if (isEmployer) {
        const [prof, jobs] = await Promise.all([
          userService.getProfile(),
          jobService.getMyJobs(),
        ]);
        setProfile(prof);
        setStatCount(jobs.length);
        setStatCount2(jobs.filter((j: any) => j.status === 0).length);
      } else {
        const [prof, apps] = await Promise.all([
          userService.getProfile(),
          jobApplicationService.getMyApplications(),
        ]);
        setProfile(prof);
        setStatCount(apps.length);
        setStatCount2(apps.filter((a: any) => a.status === 1).length);
      }
    } catch (_) {
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [isEmployer]);

  useEffect(() => { load(); }, []);

  const onRefresh = () => { setRefreshing(true); load(); };

  const handleLogout = async () => {
    await logout();
    router.replace('/auth/login');
  };

  const handleUploadCv = async () => {
    try {
      const result = await DocumentPicker.getDocumentAsync({
        type: ['application/pdf', 'application/msword',
               'application/vnd.openxmlformats-officedocument.wordprocessingml.document'],
        copyToCacheDirectory: true,
      });
      if (result.canceled) return;
      const file = result.assets[0];
      setUploadingCv(true);
      const url = await userService.uploadCv(file.uri, file.name);
      setProfile(prev => prev ? { ...prev, cvUrl: url } : prev);
      Alert.alert(t.common.success, t.profile.uploadCV);
    } catch (e: any) {
      Alert.alert(t.common.error, e?.message ?? t.common.somethingWentWrong);
    } finally {
      setUploadingCv(false);
    }
  };

  const handleUploadAvatar = async () => {
    try {
      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ImagePicker.MediaTypeOptions.Images,
        allowsEditing: true,
        aspect: [1, 1],
        quality: 0.8,
      });
      if (result.canceled) return;
      const file = result.assets[0];
      const fileName = file.uri.split('/').pop() ?? 'avatar.jpg';
      const type = file.mimeType ?? 'image/jpeg';
      setUploadingAvatar(true);
      const url = await userService.uploadAvatar(file.uri, fileName, type);
      setProfile(prev => prev ? { ...prev, profileImageUrl: url } : prev);
      Alert.alert(t.common.success, t.editProfile.avatar);
    } catch (e: any) {
      Alert.alert(t.common.error, e?.message ?? t.common.somethingWentWrong);
    } finally {
      setUploadingAvatar(false);
    }
  };

  const fullName = profile
    ? [profile.firstName, profile.lastName].filter(Boolean).join(' ') || '...'
    : '...';
  const initials = profile?.firstName
    ? `${profile.firstName[0]}${profile.lastName?.[0] ?? ''}`.toUpperCase()
    : '?';
  const roleLabel  = isEmployer ? t.auth.employer : t.auth.worker;
  const menuItems  = isEmployer ? EMPLOYER_MENU : WORKER_MENU;

  if (loading) {
    return (
      <View style={[styles.container, { backgroundColor: colors.background }]}>
        <ProfileSkeleton />
      </View>
    );
  }

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh}
            tintColor={colors.primary} colors={[colors.primary]} />
        }
      >
        {/* Gradient Header */}
        <LinearGradient
          colors={isDark ? ['#14532D', '#15803D'] : ['#15803D', '#22C55E']}
          start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }}
          style={styles.gradientHeader}
        >
          <View style={styles.avatarContainer}>
            <View style={styles.avatar}>
  {profile?.profileImageUrl ? (
    <Image
      source={{ uri: `${MEDIA_URL}${profile.profileImageUrl}` }}
      style={{ width: 90, height: 90, borderRadius: 45 }}
      resizeMode="cover"
    />
  ) : (
    <Text style={styles.avatarText}>{initials}</Text>
  )}
</View>
            <TouchableOpacity style={styles.cameraButton} onPress={handleUploadAvatar} disabled={uploadingAvatar}>
              {uploadingAvatar
                ? <ActivityIndicator size="small" color={colors.primary} />
                : <CameraIcon size={14} color={colors.primary} />
              }
            </TouchableOpacity>
          </View>

          <Text style={styles.userName}>{fullName}</Text>
          {profile?.city ? <Text style={styles.userSub}>{profile.city}</Text> : null}

          <View style={styles.roleBadge}>
            <Text style={styles.roleText}>{roleLabel}</Text>
          </View>

          {/* Stats */}
          <View style={styles.statsContainer}>
            {isEmployer ? (
              <>
                <View style={styles.statItem}>
                  <Text style={styles.statValue}>{statCount}</Text>
                  <Text style={styles.statLabel}>{t.employer.myJobs}</Text>
                </View>
                <View style={styles.statDivider} />
                <View style={styles.statItem}>
                  <Text style={styles.statValue}>{statCount2}</Text>
                  <Text style={styles.statLabel}>{t.employer.publish}</Text>
                </View>
                <View style={styles.statDivider} />
                <View style={styles.statItem}>
                  <Text style={styles.statValue}>{profile?.isVerified ? t.common.yes : t.common.no}</Text>
                  <Text style={styles.statLabel}>{t.common.ok}</Text>
                </View>
              </>
            ) : (
              <>
                <View style={styles.statItem}>
                  <Text style={styles.statValue}>{statCount}</Text>
                  <Text style={styles.statLabel}>{t.applications.title}</Text>
                </View>
                <View style={styles.statDivider} />
                <View style={styles.statItem}>
                  <Text style={styles.statValue}>{statCount2}</Text>
                  <Text style={styles.statLabel}>{t.applications.accepted}</Text>
                </View>
                <View style={styles.statDivider} />
                <View style={styles.statItem}>
                  <Text style={styles.statValue}>{profile?.age ?? '—'}</Text>
                  <Text style={styles.statLabel}>{t.common.noData}</Text>
                </View>
              </>
            )}
          </View>
        </LinearGradient>

        {/* Worker: CV yuklash */}
        {!isEmployer && (
          <TouchableOpacity
            style={[styles.cvBtn, { backgroundColor: colors.card, borderColor: colors.border, ...Shadow.sm }]}
            onPress={handleUploadCv}
            activeOpacity={0.85}
            disabled={uploadingCv}
          >
            {uploadingCv ? (
              <ActivityIndicator color={colors.primary} />
            ) : (
              <>
                <Text style={[styles.cvBtnIcon, { color: colors.primary }]}>📄</Text>
                <View style={styles.cvBtnInfo}>
                  <Text style={[styles.cvBtnTitle, { color: colors.textPrimary }]}>
                    {profile?.cvUrl ? t.profile.myCV : t.profile.uploadCV}
                  </Text>
                  <Text style={[styles.cvBtnSub, { color: colors.textTertiary }]}>
                    {profile?.cvUrl ? t.common.success : 'PDF / Word'}
                  </Text>
                </View>
                <View style={[styles.cvBtnBadge, { backgroundColor: profile?.cvUrl ? '#DCFCE7' : colors.primaryLight }]}>
                  <Text style={[styles.cvBtnBadgeText, { color: profile?.cvUrl ? '#166534' : colors.primary }]}>
                    {profile?.cvUrl ? t.common.done : t.profile.uploadCV}
                  </Text>
                </View>
              </>
            )}
          </TouchableOpacity>
        )}

        {/* Employer: E'lon qilish */}
        {isEmployer && (
          <TouchableOpacity
            style={styles.createJobBtn}
            onPress={() => router.push('/(tabs)/create-job')}
            activeOpacity={0.85}
          >
            <LinearGradient
              colors={['#15803D', '#16A34A']}
              start={{ x: 0, y: 0 }} end={{ x: 1, y: 0 }}
              style={styles.createJobGradient}
            >
              <Text style={styles.createJobText}>+ {t.employer.postJob}</Text>
            </LinearGradient>
          </TouchableOpacity>
        )}

        {/* Theme Toggle */}
        <View style={[styles.themeCard, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <View style={styles.themeLeft}>
            {isDark
              ? <MoonIcon size={22} color={colors.primary} />
              : <SunIcon  size={22} color={colors.primary} />
            }
            <Text style={[styles.themeLabel, { color: colors.textPrimary }]}>
              {isDark ? t.profile.darkMode : t.profile.lightMode}
            </Text>
          </View>
          <TouchableOpacity
            style={[styles.toggleButton, { backgroundColor: isDark ? colors.primary : colors.border }]}
            onPress={toggleTheme} activeOpacity={0.8}
          >
            <View style={[styles.toggleCircle, { transform: [{ translateX: isDark ? 20 : 2 }] }]} />
          </TouchableOpacity>
        </View>

        {/* Til tanlash */}
        <View style={[styles.themeCard, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <LanguagePicker />
        </View>

        {/* Menu */}
        <View style={[styles.menuCard, { backgroundColor: colors.card, ...Shadow.sm }]}>
          {menuItems.map((item, index) => (
            <TouchableOpacity
              key={index}
              style={[
                styles.menuItem,
                index < menuItems.length - 1 && { borderBottomWidth: 1, borderBottomColor: colors.border },
              ]}
              activeOpacity={0.7}
              onPress={() => { if (item.route) router.push(item.route as any); }}
            >
              <View style={[styles.menuIconWrapper, { backgroundColor: colors.primaryLight }]}>
                <MenuIcon name={item.icon} color={colors.primary} />
              </View>
              <Text style={[styles.menuLabel, { color: colors.textPrimary }]}>{item.label}</Text>
              <ChevronRightIcon size={20} color={colors.textTertiary} />
            </TouchableOpacity>
          ))}
        </View>

        {/* Logout */}
        <TouchableOpacity style={styles.logoutButton} onPress={handleLogout} activeOpacity={0.85}>
          <LogoutIcon size={22} color="#DC2626" />
          <Text style={styles.logoutText}>{t.profile.logout}</Text>
        </TouchableOpacity>

        <View style={{ height: Spacing.xxxl }} />
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  gradientHeader: {
    paddingTop: 56, paddingBottom: Spacing.xl,
    alignItems: 'center', gap: Spacing.sm,
  },
  avatarContainer: { position: 'relative', marginBottom: Spacing.sm },
  avatar: {
    width: 90, height: 90, borderRadius: BorderRadius.full,
    backgroundColor: 'rgba(255,255,255,0.25)',
    alignItems: 'center', justifyContent: 'center',
    borderWidth: 3, borderColor: 'rgba(255,255,255,0.5)',
  },
  avatarText:   { fontSize: 36, fontWeight: FontWeight.bold, color: '#FFFFFF' },
  cameraButton: {
    position: 'absolute', bottom: 0, right: 0,
    width: 30, height: 30, borderRadius: BorderRadius.full,
    backgroundColor: '#FFFFFF', alignItems: 'center', justifyContent: 'center',
    ...Shadow.sm,
  },
  userName:  { fontSize: FontSize.xl, fontWeight: FontWeight.bold, color: '#FFFFFF' },
  userSub:   { fontSize: FontSize.sm, color: 'rgba(255,255,255,0.75)' },
  roleBadge: {
    backgroundColor: 'rgba(255,255,255,0.2)', borderRadius: BorderRadius.full,
    paddingHorizontal: Spacing.md, paddingVertical: Spacing.xs,
  },
  roleText: { fontSize: FontSize.sm, color: '#FFFFFF', fontWeight: FontWeight.semiBold },
  statsContainer: {
    flexDirection: 'row', alignItems: 'center',
    backgroundColor: 'rgba(255,255,255,0.15)',
    borderRadius: BorderRadius.xl,
    marginHorizontal: Spacing.xl, marginTop: Spacing.md,
    paddingVertical: Spacing.md, paddingHorizontal: Spacing.xl,
  },
  statItem:    { alignItems: 'center', flex: 1 },
  statValue:   { fontSize: FontSize.xl, fontWeight: FontWeight.bold, color: '#FFFFFF' },
  statLabel:   { fontSize: FontSize.xs, color: 'rgba(255,255,255,0.8)', fontWeight: FontWeight.medium },
  statDivider: { width: 1, height: 32, backgroundColor: 'rgba(255,255,255,0.3)' },
  cvBtn: {
    flexDirection: 'row', alignItems: 'center',
    marginHorizontal: Spacing.xl, marginTop: Spacing.lg,
    borderRadius: BorderRadius.xl, padding: Spacing.lg,
    borderWidth: 1.5, gap: Spacing.md,
  },
  cvBtnIcon:      { fontSize: 24 },
  cvBtnInfo:      { flex: 1 },
  cvBtnTitle:     { fontSize: FontSize.md, fontWeight: FontWeight.semiBold },
  cvBtnSub:       { fontSize: FontSize.xs, marginTop: 2 },
  cvBtnBadge: {
    paddingHorizontal: Spacing.sm, paddingVertical: Spacing.xs,
    borderRadius: BorderRadius.md,
  },
  cvBtnBadgeText: { fontSize: FontSize.xs, fontWeight: FontWeight.bold },
  createJobBtn: {
    marginHorizontal: Spacing.xl, marginTop: Spacing.lg,
    borderRadius: BorderRadius.xl, overflow: 'hidden',
  },
  createJobGradient: { height: 52, alignItems: 'center', justifyContent: 'center' },
  createJobText:     { fontSize: FontSize.md, fontWeight: FontWeight.bold, color: '#fff' },
  themeCard: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    marginHorizontal: Spacing.xl, marginTop: Spacing.lg,
    borderRadius: BorderRadius.xl, padding: Spacing.lg,
  },
  themeLeft:    { flexDirection: 'row', alignItems: 'center', gap: Spacing.md },
  themeLabel:   { fontSize: FontSize.md, fontWeight: FontWeight.semiBold },
  toggleButton: { width: 48, height: 28, borderRadius: BorderRadius.full, padding: 3, justifyContent: 'center' },
  toggleCircle: { width: 22, height: 22, borderRadius: BorderRadius.full, backgroundColor: '#FFFFFF' },
  menuCard: {
    marginHorizontal: Spacing.xl, marginTop: Spacing.lg,
    borderRadius: BorderRadius.xl, overflow: 'hidden',
  },
  menuItem: {
    flexDirection: 'row', alignItems: 'center',
    paddingHorizontal: Spacing.lg, paddingVertical: Spacing.md, gap: Spacing.md,
  },
  menuIconWrapper: {
    width: 40, height: 40, borderRadius: BorderRadius.md,
    alignItems: 'center', justifyContent: 'center',
  },
  menuLabel:    { flex: 1, fontSize: FontSize.md, fontWeight: FontWeight.medium },
  logoutButton: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    marginHorizontal: Spacing.xl, marginTop: Spacing.lg,
    borderRadius: BorderRadius.xl, padding: Spacing.lg,
    gap: Spacing.sm, backgroundColor: '#FEE2E2',
  },
  logoutText: { fontSize: FontSize.md, fontWeight: FontWeight.bold, color: '#DC2626' },
});
