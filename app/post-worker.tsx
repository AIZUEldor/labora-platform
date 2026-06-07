import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TextInput,
  TouchableOpacity,
  Alert,
  Image,
  ActivityIndicator,
} from 'react-native';
import { useRouter } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import * as ImagePicker from 'expo-image-picker';
import * as DocumentPicker from 'expo-document-picker';
import { useThemeStore } from '../store/themeStore';
import { useLanguageStore } from '../stores/useLanguageStore';
import { workerPostService } from '../services/workerPostService';
import { categoryService } from '../services/categoryService';
import { Category, CreateWorkerPostRequest } from '../types';
import { BackIcon, CloseIcon, PlusIcon } from '../components/icons';

export default function PostWorkerScreen() {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useThemeStore();
  const { t } = useLanguageStore();

  const [loading, setLoading] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null);
  const [selectedSubCategory, setSelectedSubCategory] = useState<Category | null>(null);
  const [showCategoryModal, setShowCategoryModal] = useState(false);
  const [showSubCategoryModal, setShowSubCategoryModal] = useState(false);
  const [portfolioImages, setPortfolioImages] = useState<string[]>([]);
  const [cvUri, setCvUri] = useState<string | null>(null);
  const [cvName, setCvName] = useState<string | null>(null);

  const [form, setForm] = useState<CreateWorkerPostRequest>({
    title: '',
    description: '',
    expectedSalary: 0,
    experienceYears: 0,
    skills: '',
    city: '',
    country: '',
    categoryId: undefined,
    subCategoryId: undefined,
  });

  useEffect(() => {
    loadCategories();
  }, []);

  const loadCategories = async () => {
    try {
      const data = await categoryService.getCategories();
      setCategories(data);
    } catch {
      Alert.alert('Xato', 'Kategoriyalarni yuklashda xato');
    }
  };

  const handleSelectCategory = (category: Category) => {
    setSelectedCategory(category);
    setSelectedSubCategory(null);
    setForm(prev => ({ ...prev, categoryId: category.id, subCategoryId: undefined }));
    setShowCategoryModal(false);
  };

  const handleSelectSubCategory = (sub: Category) => {
    setSelectedSubCategory(sub);
    setForm(prev => ({ ...prev, subCategoryId: sub.id }));
    setShowSubCategoryModal(false);
  };

  const pickPortfolioImage = async () => {
    if (portfolioImages.length >= 5) {
      Alert.alert('', 'Maksimal 5 ta rasm yuklash mumkin');
      return;
    }
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      quality: 0.8,
    });
    if (!result.canceled && result.assets[0]) {
      setPortfolioImages(prev => [...prev, result.assets[0].uri]);
    }
  };

  const removePortfolioImage = (index: number) => {
    setPortfolioImages(prev => prev.filter((_, i) => i !== index));
  };

  const pickCV = async () => {
    const result = await DocumentPicker.getDocumentAsync({
      type: 'application/pdf',
    });
    if (!result.canceled && result.assets[0]) {
      setCvUri(result.assets[0].uri);
      setCvName(result.assets[0].name);
    }
  };

  const handleSubmit = async () => {
    if (!form.title.trim()) {
      Alert.alert('Xato', 'Lavozim nomini kiriting');
      return;
    }
    if (!form.description.trim()) {
      Alert.alert('Xato', "O'zingiz haqida yozing");
      return;
    }
    if (!form.city.trim()) {
      Alert.alert('Xato', 'Shahar kiriting');
      return;
    }
    if (!form.country.trim()) {
      Alert.alert('Xato', 'Davlat kiriting');
      return;
    }

    setLoading(true);
    try {
      const post = await workerPostService.create(form);

      for (const uri of portfolioImages) {
        await workerPostService.uploadPortfolioImage(post.id, uri);
      }

      Alert.alert('', "E'lon muvaffaqiyatli joylashdi", [
        { text: 'OK', onPress: () => router.back() },
      ]);
    } catch {
      Alert.alert('Xato', "E'lon joylashda xato yuz berdi");
    } finally {
      setLoading(false);
    }
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <LinearGradient
        colors={['#16A34A', '#15803D']}
        style={[styles.header, { paddingTop: insets.top + 12 }]}
      >
        <TouchableOpacity onPress={() => router.back()} style={styles.backBtn}>
          <BackIcon size={24} color="#fff" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>E'lon berish</Text>
        <View style={{ width: 40 }} />
      </LinearGradient>

      <ScrollView
        contentContainerStyle={[styles.content, { paddingBottom: insets.bottom + 24 }]}
        showsVerticalScrollIndicator={false}
        keyboardShouldPersistTaps="handled"
      >
        {/* Asosiy ma'lumotlar */}
        <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
          Asosiy ma'lumotlar
        </Text>

        <Text style={[styles.label, { color: colors.textPrimary }]}>
          Lavozim nomi <Text style={{ color: '#EF4444' }}>*</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="Masalan: Usta, Dizayner, Haydovchi"
          placeholderTextColor={colors.textSecondary}
          value={form.title}
          onChangeText={text => setForm(prev => ({ ...prev, title: text }))}
        />

        <Text style={[styles.label, { color: colors.textPrimary }]}>
          O'zingiz haqida <Text style={{ color: '#EF4444' }}>*</Text>
        </Text>
        <TextInput
          style={[styles.input, styles.textArea, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="Tajribangiz, ko'nikmalaringiz haqida yozing..."
          placeholderTextColor={colors.textSecondary}
          value={form.description}
          onChangeText={text => setForm(prev => ({ ...prev, description: text }))}
          multiline
          numberOfLines={4}
          textAlignVertical="top"
        />

        <Text style={[styles.label, { color: colors.textPrimary }]}>
          Kutilgan maosh (so'm) <Text style={{ color: '#EF4444' }}>*</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="Masalan: 3000000"
          placeholderTextColor={colors.textSecondary}
          value={form.expectedSalary > 0 ? String(form.expectedSalary) : ''}
          onChangeText={text => setForm(prev => ({ ...prev, expectedSalary: Number(text) || 0 }))}
          keyboardType="numeric"
        />

        <Text style={[styles.label, { color: colors.textPrimary }]}>
          Shahar <Text style={{ color: '#EF4444' }}>*</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="Masalan: Toshkent"
          placeholderTextColor={colors.textSecondary}
          value={form.city}
          onChangeText={text => setForm(prev => ({ ...prev, city: text }))}
        />

        <Text style={[styles.label, { color: colors.textPrimary }]}>
          Davlat <Text style={{ color: '#EF4444' }}>*</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="Masalan: O'zbekiston"
          placeholderTextColor={colors.textSecondary}
          value={form.country}
          onChangeText={text => setForm(prev => ({ ...prev, country: text }))}
        />

        <Text style={[styles.label, { color: colors.textPrimary }]}>
          Tajriba (yil)
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="Masalan: 3"
          placeholderTextColor={colors.textSecondary}
          value={form.experienceYears > 0 ? String(form.experienceYears) : ''}
          onChangeText={text => setForm(prev => ({ ...prev, experienceYears: Number(text) || 0 }))}
          keyboardType="numeric"
        />

        <Text style={[styles.label, { color: colors.textPrimary }]}>
          Ko'nikmalar
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="Masalan: Payvandlash, Elektr, Santexnika"
          placeholderTextColor={colors.textSecondary}
          value={form.skills}
          onChangeText={text => setForm(prev => ({ ...prev, skills: text }))}
        />

        {/* Kategoriya */}
        <Text style={[styles.sectionTitle, { color: colors.textPrimary, marginTop: 24 }]}>
          Kategoriya
        </Text>

        <TouchableOpacity
          style={[styles.selector, { backgroundColor: colors.card, borderColor: colors.border }]}
          onPress={() => setShowCategoryModal(true)}
        >
          <Text style={{ color: selectedCategory ? colors.textPrimary : colors.textSecondary }}>
            {selectedCategory ? selectedCategory.name : 'Kategoriya tanlang'}
          </Text>
        </TouchableOpacity>

        {selectedCategory && selectedCategory.subCategories && selectedCategory.subCategories.length > 0 && (
          <TouchableOpacity
            style={[styles.selector, { backgroundColor: colors.card, borderColor: colors.border }]}
            onPress={() => setShowSubCategoryModal(true)}
          >
            <Text style={{ color: selectedSubCategory ? colors.textPrimary : colors.textSecondary }}>
              {selectedSubCategory ? selectedSubCategory.name : 'Sub-kategoriya tanlang'}
            </Text>
          </TouchableOpacity>
        )}

        {/* CV yuklash */}
        <Text style={[styles.sectionTitle, { color: colors.textPrimary, marginTop: 24 }]}>
          CV yuklash
        </Text>

        <TouchableOpacity
          style={[styles.uploadBtn, { backgroundColor: colors.card, borderColor: colors.border }]}
          onPress={pickCV}
        >
          <PlusIcon size={20} color="#16A34A" />
          <Text style={[styles.uploadBtnText, { color: colors.textPrimary }]}>
            {cvName ? cvName : 'PDF fayl tanlang'}
          </Text>
        </TouchableOpacity>

        {/* Portfolio rasmlari */}
        <Text style={[styles.sectionTitle, { color: colors.textPrimary, marginTop: 24 }]}>
          Qilgan ishlarimdan namuna
        </Text>
        <Text style={[styles.subLabel, { color: colors.textSecondary }]}>
          Maksimal 5 ta rasm
        </Text>

        <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.imageRow}>
          {portfolioImages.map((uri, index) => (
            <View key={index} style={styles.imageWrapper}>
              <Image source={{ uri }} style={styles.portfolioImage} />
              <TouchableOpacity
                style={styles.removeImageBtn}
                onPress={() => removePortfolioImage(index)}
              >
                <CloseIcon size={14} color="#fff" />
              </TouchableOpacity>
            </View>
          ))}
          {portfolioImages.length < 5 && (
            <TouchableOpacity
              style={[styles.addImageBtn, { backgroundColor: colors.card, borderColor: colors.border }]}
              onPress={pickPortfolioImage}
            >
              <PlusIcon size={28} color="#16A34A" />
            </TouchableOpacity>
          )}
        </ScrollView>

        {/* Submit */}
        <TouchableOpacity
          style={[styles.submitBtn, loading && { opacity: 0.7 }]}
          onPress={handleSubmit}
          disabled={loading}
        >
          <LinearGradient colors={['#16A34A', '#15803D']} style={styles.submitGradient}>
            {loading ? (
              <ActivityIndicator color="#fff" />
            ) : (
              <Text style={styles.submitText}>E'lon joylash</Text>
            )}
          </LinearGradient>
        </TouchableOpacity>
      </ScrollView>

      {/* Kategoriya Modal */}
      {showCategoryModal && (
        <View style={[styles.modal, { backgroundColor: colors.background }]}>
          <View style={[styles.modalHeader, { borderBottomColor: colors.border }]}>
            <Text style={[styles.modalTitle, { color: colors.textPrimary }]}>Kategoriya tanlang</Text>
            <TouchableOpacity onPress={() => setShowCategoryModal(false)}>
              <CloseIcon size={24} color={colors.textPrimary} />
            </TouchableOpacity>
          </View>
          <ScrollView>
            {categories.map(cat => (
              <TouchableOpacity
                key={cat.id}
                style={[styles.modalItem, { borderBottomColor: colors.border }]}
                onPress={() => handleSelectCategory(cat)}
              >
                <Text style={[styles.modalItemText, { color: colors.textPrimary }]}>{cat.name}</Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
        </View>
      )}

      {/* Sub-kategoriya Modal */}
      {showSubCategoryModal && selectedCategory && (
        <View style={[styles.modal, { backgroundColor: colors.background }]}>
          <View style={[styles.modalHeader, { borderBottomColor: colors.border }]}>
            <Text style={[styles.modalTitle, { color: colors.textPrimary }]}>Sub-kategoriya tanlang</Text>
            <TouchableOpacity onPress={() => setShowSubCategoryModal(false)}>
              <CloseIcon size={24} color={colors.textPrimary} />
            </TouchableOpacity>
          </View>
          <ScrollView>
            {selectedCategory.subCategories?.map(sub => (
              <TouchableOpacity
                key={sub.id}
                style={[styles.modalItem, { borderBottomColor: colors.border }]}
                onPress={() => handleSelectSubCategory(sub)}
              >
                <Text style={[styles.modalItemText, { color: colors.textPrimary }]}>{sub.name}</Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
        </View>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingBottom: 16,
  },
  backBtn: { width: 40, height: 40, justifyContent: 'center' },
  headerTitle: { fontSize: 18, fontWeight: '700', color: '#fff' },
  content: { padding: 16 },
  sectionTitle: { fontSize: 16, fontWeight: '700', marginBottom: 12 },
  label: { fontSize: 14, fontWeight: '500', marginBottom: 6 },
  subLabel: { fontSize: 12, marginBottom: 10 },
  input: {
    borderWidth: 1,
    borderRadius: 10,
    paddingHorizontal: 14,
    paddingVertical: 12,
    fontSize: 14,
    marginBottom: 14,
  },
  textArea: { height: 100 },
  selector: {
    borderWidth: 1,
    borderRadius: 10,
    paddingHorizontal: 14,
    paddingVertical: 14,
    marginBottom: 12,
  },
  uploadBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    borderWidth: 1,
    borderRadius: 10,
    paddingHorizontal: 14,
    paddingVertical: 14,
    gap: 10,
    marginBottom: 8,
  },
  uploadBtnText: { fontSize: 14 },
  imageRow: { marginBottom: 24 },
  imageWrapper: { position: 'relative', marginRight: 10 },
  portfolioImage: { width: 100, height: 100, borderRadius: 10 },
  removeImageBtn: {
    position: 'absolute',
    top: 4,
    right: 4,
    backgroundColor: 'rgba(0,0,0,0.6)',
    borderRadius: 10,
    padding: 3,
  },
  addImageBtn: {
    width: 100,
    height: 100,
    borderRadius: 10,
    borderWidth: 1,
    borderStyle: 'dashed',
    justifyContent: 'center',
    alignItems: 'center',
  },
  submitBtn: { marginTop: 8 },
  submitGradient: {
    borderRadius: 12,
    paddingVertical: 16,
    alignItems: 'center',
  },
  submitText: { color: '#fff', fontSize: 16, fontWeight: '700' },
  modal: {
    position: 'absolute',
    top: 0, left: 0, right: 0, bottom: 0,
    zIndex: 100,
  },
  modalHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: 16,
    borderBottomWidth: 1,
  },
  modalTitle: { fontSize: 16, fontWeight: '700' },
  modalItem: {
    padding: 16,
    borderBottomWidth: 1,
  },
  modalItemText: { fontSize: 15 },
});