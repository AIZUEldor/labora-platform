import React, { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, ScrollView,
  TouchableOpacity, ActivityIndicator, Alert, Image,
  TextInput, Modal,
} from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useThemeStore } from '../store/themeStore';
import { workerPostService } from '../services/workerPostService';
import { WorkerPost, UpdateWorkerPostRequest } from '../types';
import { BackIcon, ClockIcon, EyeIcon, MapPinIcon, MoneyIcon, BriefcaseIcon, EditIcon, CloseIcon, PlusIcon } from '../components/icons';
import { BorderRadius, Shadow, Spacing } from '../constants/spacing';
import { FontSize, FontWeight } from '../constants/typography';
import { MEDIA_URL } from '../services/api';




const WORKER_POST_STATUS_LABEL: Record<number, { label: string; bg: string; text: string }> = {
  1: { label: 'Faol',          bg: '#DCFCE7', text: '#166534' },
  2: { label: 'Nofaol',        bg: '#F3F4F6', text: '#6B7280' },
  3: { label: 'Qabul qilindi', bg: '#EFF6FF', text: '#1D4ED8' },
};

function formatDate(dateStr: string): string {
  const d = new Date(dateStr);
  return `${d.getDate()}.${d.getMonth() + 1}.${d.getFullYear()}`;
}

export default function WorkerPostDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useThemeStore();

  const [post,          setPost]          = useState<WorkerPost | null>(null);
  const [loading,       setLoading]       = useState(true);
  const [showEditModal, setShowEditModal] = useState(false);
  const [saving,        setSaving]        = useState(false);
  const [form,          setForm]          = useState({
    title: '',
    description: '',
    expectedSalary: 0,
    experienceYears: 0,
    skills: '',
    city: '',
    country: '',
    status: 1,
  });

  useEffect(() => { loadPost(); }, [id]);

  const loadPost = async () => {
    try {
      const data = await workerPostService.getById(id!);
      setPost(data);
      setForm({
        title:           data.title,
        description:     data.description,
        expectedSalary:  data.expectedSalary,
        experienceYears: data.experienceYears,
        skills:          data.skills ?? '',
        city:            data.city,
        country:         data.country,
        status:          data.status,
      });
    } catch {
      Alert.alert('Xato', "E'lonni yuklashda xato");
      router.back();
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = () => {
    Alert.alert(
      "E'lonni o'chirish",
      "Haqiqatan ham bu e'lonni o'chirmoqchimisiz?",
      [
        { text: 'Bekor qilish', style: 'cancel' },
        {
          text: "O'chirish",
          style: 'destructive',
          onPress: async () => {
            try {
              await workerPostService.delete(id!);
              router.back();
            } catch {
              Alert.alert('Xato', "O'chirishda xato yuz berdi");
            }
          },
        },
      ]
    );
  };

  const handleSave = async () => {
    if (!form.title.trim()) { Alert.alert('Xato', 'Lavozim nomini kiriting'); return; }
    if (!form.city.trim())  { Alert.alert('Xato', 'Shahar kiriting'); return; }

    setSaving(true);
    try {
      await workerPostService.update(id!, {
        title:           form.title,
        description:     form.description,
        expectedSalary:  form.expectedSalary,
        experienceYears: form.experienceYears,
        skills:          form.skills,
        city:            form.city,
        country:         form.country,
        status:          form.status,
      });
      await loadPost();
      setShowEditModal(false);
    } catch {
      Alert.alert('Xato', 'Saqlashda xato yuz berdi');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <View style={[styles.container, { backgroundColor: colors.background, justifyContent: 'center', alignItems: 'center' }]}>
        <ActivityIndicator size="large" color="#16A34A" />
      </View>
    );
  }

  if (!post) return null;

  const statusInfo = WORKER_POST_STATUS_LABEL[post.status] ?? WORKER_POST_STATUS_LABEL[1];

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <LinearGradient
        colors={['#16A34A', '#15803D']}
        style={[styles.header, { paddingTop: insets.top + 12 }]}
      >
        <TouchableOpacity onPress={() => router.back()} style={styles.headerBtn}>
          <BackIcon size={24} color="#fff" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>E'lon tafsiloti</Text>
        <TouchableOpacity onPress={handleDelete} style={styles.headerBtn}>
          <CloseIcon size={24} color="#fff" />
        </TouchableOpacity>
      </LinearGradient>

      <ScrollView
        contentContainerStyle={[styles.content, { paddingBottom: insets.bottom + 24 }]}
        showsVerticalScrollIndicator={false}
      >
        {/* Status + Ko'rish + Tahrirlash */}
        <View style={styles.topRow}>
          <View style={[styles.badge, { backgroundColor: statusInfo.bg }]}>
            <Text style={[styles.badgeText, { color: statusInfo.text }]}>{statusInfo.label}</Text>
          </View>
          <View style={styles.rightMeta}>
            <View style={styles.viewRow}>
              <EyeIcon size={14} color={colors.textSecondary} />
              <Text style={[styles.viewText, { color: colors.textSecondary }]}>{post.viewCount ?? 0} ko'rish</Text>
            </View>
            <TouchableOpacity
              style={[styles.editBtn, { backgroundColor: colors.primaryLight }]}
              onPress={() => setShowEditModal(true)}
            >
              <EditIcon size={14} color="#16A34A" />
              <Text style={[styles.editBtnText, { color: '#16A34A' }]}>Tahrirlash</Text>
            </TouchableOpacity>
          </View>
        </View>

        {/* Sarlavha */}
        <Text style={[styles.title, { color: colors.textPrimary }]}>{post.title}</Text>

        {/* Meta */}
        <View style={[styles.metaCard, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <View style={styles.metaRow}>
            <MapPinIcon size={16} color="#16A34A" />
            <Text style={[styles.metaText, { color: colors.textPrimary }]}>{post.city}, {post.country}</Text>
          </View>
          <View style={[styles.divider, { backgroundColor: colors.border }]} />
          <View style={styles.metaRow}>
            <MoneyIcon size={16} color="#16A34A" />
            <Text style={[styles.metaText, { color: colors.textPrimary }]}>
              {post.expectedSalary.toLocaleString()} so'm
            </Text>
          </View>
          <View style={[styles.divider, { backgroundColor: colors.border }]} />
          <View style={styles.metaRow}>
            <BriefcaseIcon size={16} color="#16A34A" />
            <Text style={[styles.metaText, { color: colors.textPrimary }]}>
              {post.experienceYears} yil tajriba
            </Text>
          </View>
          <View style={[styles.divider, { backgroundColor: colors.border }]} />
          <View style={styles.metaRow}>
            <ClockIcon size={16} color="#16A34A" />
            <Text style={[styles.metaText, { color: colors.textPrimary }]}>{formatDate(post.createdAt)}</Text>
          </View>
        </View>

        {/* Kategoriya */}
        {post.categoryName && (
          <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
            <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>Kategoriya</Text>
            <Text style={[styles.sectionText, { color: colors.textSecondary }]}>
              {post.categoryName}{post.subCategoryName ? ` › ${post.subCategoryName}` : ''}
            </Text>
          </View>
        )}

        {/* Tavsif */}
        <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
          <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>O'zim haqimda</Text>
          <Text style={[styles.sectionText, { color: colors.textSecondary }]}>{post.description}</Text>
        </View>

        {/* Ko'nikmalar */}
        {post.skills && (
          <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
            <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>Ko'nikmalar</Text>
            <View style={styles.skillsRow}>
              {post.skills.split(',').map((skill, i) => (
                <View key={i} style={[styles.skillChip, { backgroundColor: colors.primaryLight }]}>
                  <Text style={[styles.skillText, { color: '#16A34A' }]}>{skill.trim()}</Text>
                </View>
              ))}
            </View>
          </View>
        )}

        {/* Portfolio rasmlari */}
        {post.portfolioImages && post.portfolioImages.length > 0 && (
          <View style={[styles.section, { backgroundColor: colors.card, ...Shadow.sm }]}>
            <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>Ishlarim namunasi</Text>
            <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.imageRow}>
              {post.portfolioImages.map((img, i) => (
                <Image
                  key={i}
                  source={{ uri: `${MEDIA_URL}${img.imageUrl}` }}
                  style={styles.portfolioImage}
                />
              ))}
            </ScrollView>
          </View>
        )}

        {/* O'chirish tugmasi */}
        <TouchableOpacity
          style={[styles.deleteBtn, { borderColor: '#EF4444' }]}
          onPress={handleDelete}
          activeOpacity={0.8}
        >
          <Text style={[styles.deleteBtnText, { color: '#EF4444' }]}>E'lonni o'chirish</Text>
        </TouchableOpacity>
      </ScrollView>

      {/* Tahrirlash Modal */}
      <Modal visible={showEditModal} animationType="slide" transparent>
        <View style={styles.modalOverlay}>
          <View style={[styles.modalContent, { backgroundColor: colors.background }]}>
            <View style={[styles.modalHeader, { borderBottomColor: colors.border }]}>
              <Text style={[styles.modalTitle, { color: colors.textPrimary }]}>E'lonni tahrirlash</Text>
              <TouchableOpacity onPress={() => setShowEditModal(false)}>
                <CloseIcon size={24} color={colors.textPrimary} />
              </TouchableOpacity>
            </View>

            <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.modalBody}
              keyboardShouldPersistTaps="handled">

              <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Lavozim nomi</Text>
              <TextInput
                style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
                value={form.title}
                onChangeText={t => setForm(p => ({ ...p, title: t }))}
                placeholderTextColor={colors.textSecondary}
              />

              <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>O'zingiz haqida</Text>
              <TextInput
                style={[styles.input, styles.textArea, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
                value={form.description}
                onChangeText={t => setForm(p => ({ ...p, description: t }))}
                multiline
                numberOfLines={4}
                textAlignVertical="top"
                placeholderTextColor={colors.textSecondary}
              />

              <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Maosh (so'm)</Text>
              <TextInput
                style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
                value={form.expectedSalary > 0 ? String(form.expectedSalary) : ''}
                onChangeText={t => setForm(p => ({ ...p, expectedSalary: Number(t) || 0 }))}
                keyboardType="numeric"
                placeholderTextColor={colors.textSecondary}
              />

              <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Tajriba (yil)</Text>
              <TextInput
                style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
                value={form.experienceYears > 0 ? String(form.experienceYears) : ''}
                onChangeText={t => setForm(p => ({ ...p, experienceYears: Number(t) || 0 }))}
                keyboardType="numeric"
                placeholderTextColor={colors.textSecondary}
              />

              <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Ko'nikmalar</Text>
              <TextInput
                style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
                value={form.skills}
                onChangeText={t => setForm(p => ({ ...p, skills: t }))}
                placeholderTextColor={colors.textSecondary}
              />

              <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Shahar</Text>
              <TextInput
                style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
                value={form.city}
                onChangeText={t => setForm(p => ({ ...p, city: t }))}
                placeholderTextColor={colors.textSecondary}
              />

              <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Davlat</Text>
              <TextInput
                style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
                value={form.country}
                onChangeText={t => setForm(p => ({ ...p, country: t }))}
                placeholderTextColor={colors.textSecondary}
              />

              <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Ishlarim namunasi</Text>
              <ScrollView horizontal showsHorizontalScrollIndicator={false} style={{ marginBottom: 8 }}>
                {post.portfolioImages.map((img) => (
                  <View key={img.id} style={{ position: 'relative', marginRight: 10 }}>
                    <Image
                      source={{ uri: `${MEDIA_URL}${img.imageUrl}` }}
                      style={{ width: 90, height: 90, borderRadius: 10 }}
                    />
                    <TouchableOpacity
                      style={{
                        position: 'absolute', top: 4, right: 4,
                        backgroundColor: 'rgba(0,0,0,0.6)',
                        borderRadius: 10, padding: 3,
                      }}
                      onPress={async () => {
                        try {
                          await workerPostService.deletePortfolioImage(id!, img.id);
                          await loadPost();
                        } catch {
                          Alert.alert('Xato', "Rasmni o'chirishda xato");
                        }
                      }}
                    >
                      <CloseIcon size={12} color="#fff" />
                    </TouchableOpacity>
                  </View>
                ))}
                {post.portfolioImages.length < 5 && (
                  <TouchableOpacity
                    style={{
                      width: 90, height: 90, borderRadius: 10,
                      borderWidth: 1.5, borderStyle: 'dashed',
                      borderColor: colors.border,
                      justifyContent: 'center', alignItems: 'center',
                    }}
                    onPress={async () => {
                      const result = await (await import('expo-image-picker')).launchImageLibraryAsync({
                        mediaTypes: (await import('expo-image-picker')).MediaTypeOptions.Images,
                        quality: 0.8,
                      });
                      if (!result.canceled && result.assets[0]) {
                        try {
                          await workerPostService.uploadPortfolioImage(id!, result.assets[0].uri);
                          await loadPost();
                        } catch {
                          Alert.alert('Xato', 'Rasm yuklashda xato');
                        }
                      }
                    }}
                  >
                    <PlusIcon size={28} color="#16A34A" />
                  </TouchableOpacity>
                )}
              </ScrollView>

              <Text style={[styles.inputLabel, { color: colors.textPrimary }]}>Status</Text>
              <View style={styles.statusRow}>
                {[
                  { value: 1, label: 'Faol' },
                  { value: 2, label: 'Nofaol' },
                ].map(s => (
                  <TouchableOpacity
                    key={s.value}
                    style={[
                      styles.statusBtn,
                      { borderColor: colors.border },
                      form.status === s.value && { backgroundColor: '#16A34A', borderColor: '#16A34A' },
                    ]}
                    onPress={() => setForm(p => ({ ...p, status: s.value }))}
                  >
                    <Text style={[
                      styles.statusBtnText,
                      { color: colors.textSecondary },
                      form.status === s.value && { color: '#fff' },
                    ]}>
                      {s.label}
                    </Text>
                  </TouchableOpacity>
                ))}
              </View>

              <TouchableOpacity
                style={[styles.saveBtn, saving && { opacity: 0.7 }]}
                onPress={handleSave}
                disabled={saving}
              >
                <LinearGradient colors={['#16A34A', '#15803D']} style={styles.saveGradient}>
                  {saving
                    ? <ActivityIndicator color="#fff" />
                    : <Text style={styles.saveBtnText}>Saqlash</Text>
                  }
                </LinearGradient>
              </TouchableOpacity>
            </ScrollView>
          </View>
        </View>
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  container:    { flex: 1 },
  header: {
    flexDirection: 'row', alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16, paddingBottom: 16,
  },
  headerBtn:    { width: 40, height: 40, justifyContent: 'center' },
  headerTitle:  { fontSize: 18, fontWeight: '700', color: '#fff' },
  content:      { padding: 16, gap: 12 },
  topRow: {
    flexDirection: 'row', alignItems: 'flex-start',
    justifyContent: 'space-between', marginBottom: 4,
  },
  badge: {
    borderRadius: BorderRadius.sm,
    paddingHorizontal: Spacing.md, paddingVertical: Spacing.xs,
  },
  badgeText:    { fontSize: FontSize.sm, fontWeight: FontWeight.bold },
  rightMeta:    { alignItems: 'flex-end', gap: 6 },
  viewRow:      { flexDirection: 'row', alignItems: 'center', gap: 4 },
  viewText:     { fontSize: FontSize.sm },
  editBtn: {
    flexDirection: 'row', alignItems: 'center', gap: 4,
    paddingHorizontal: 10, paddingVertical: 5, borderRadius: 20,
  },
  editBtnText:  { fontSize: 12, fontWeight: '600' },
  title:        { fontSize: FontSize.xl, fontWeight: FontWeight.bold, marginBottom: 4 },
  metaCard:     { borderRadius: BorderRadius.xl, padding: Spacing.lg },
  metaRow:      { flexDirection: 'row', alignItems: 'center', gap: Spacing.sm },
  metaText:     { fontSize: FontSize.md },
  divider:      { height: 1, marginVertical: Spacing.md },
  section:      { borderRadius: BorderRadius.xl, padding: Spacing.lg, gap: 8 },
  sectionTitle: { fontSize: FontSize.md, fontWeight: FontWeight.bold },
  sectionText:  { fontSize: FontSize.sm, lineHeight: 22 },
  skillsRow:    { flexDirection: 'row', flexWrap: 'wrap', gap: 8 },
  skillChip: {
    paddingHorizontal: Spacing.md, paddingVertical: Spacing.xs,
    borderRadius: BorderRadius.full,
  },
  skillText:    { fontSize: FontSize.sm, fontWeight: FontWeight.medium },
  imageRow:     { marginTop: 4 },
  portfolioImage: {
    width: 120, height: 120, borderRadius: BorderRadius.lg, marginRight: 10,
  },
  deleteBtn: {
    borderWidth: 1.5, borderRadius: BorderRadius.xl,
    paddingVertical: 14, alignItems: 'center', marginTop: 8,
  },
  deleteBtnText: { fontSize: FontSize.md, fontWeight: FontWeight.semiBold },

  // Modal
  modalOverlay: {
    flex: 1, backgroundColor: 'rgba(0,0,0,0.5)',
    justifyContent: 'flex-end',
  },
  modalContent: {
    borderTopLeftRadius: 24, borderTopRightRadius: 24,
    maxHeight: '90%',
  },
  modalHeader: {
    flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center',
    padding: 16, borderBottomWidth: 1,
  },
  modalTitle:   { fontSize: 16, fontWeight: '700' },
  modalBody:    { padding: 16, gap: 4, paddingBottom: 32 },
  inputLabel:   { fontSize: 13, fontWeight: '600', marginBottom: 4, marginTop: 8 },
  input: {
    borderWidth: 1, borderRadius: 10,
    paddingHorizontal: 14, paddingVertical: 12,
    fontSize: 14, marginBottom: 4,
  },
  textArea:     { height: 100 },
  statusRow:    { flexDirection: 'row', gap: 10, marginTop: 4 },
  statusBtn: {
    flex: 1, paddingVertical: 10, borderRadius: 10,
    borderWidth: 1.5, alignItems: 'center',
  },
  statusBtnText: { fontSize: 14, fontWeight: '600' },
  saveBtn:      { marginTop: 16 },
  saveGradient: { borderRadius: 12, paddingVertical: 14, alignItems: 'center' },
  saveBtnText:  { color: '#fff', fontSize: 16, fontWeight: '700' },
});