import React, { useState, useEffect, useCallback } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity,
  TextInput, ScrollView, ActivityIndicator, Alert, Modal, FlatList,
} from 'react-native';
import { router, useFocusEffect } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { useThemeStore } from '../../store/themeStore';
import { useLanguageStore } from '../../stores/useLanguageStore';
import { useMapPickerStore } from '../../stores/useMapPickerStore';
import { jobService } from '../../services/jobService';
import { categoryService } from '../../services/categoryService';
import { Category } from '../../types';
import { Spacing, BorderRadius } from '../../constants/spacing';
import { FontSize, FontWeight } from '../../constants/typography';
import Svg, { Path } from 'react-native-svg';

function BackIcon({ size = 24, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M19 12H5M5 12L12 19M5 12L12 5" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

function ChevronIcon({ size = 20, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M6 9L12 15L18 9" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

function MapPinIcon({ size = 18, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7z" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
      <Path d="M12 11.5a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z" stroke={color} strokeWidth={1.8} />
    </Svg>
  );
}

const JOB_TYPES = [
  { value: 1, labelUz: 'Kunlik',    labelRu: 'Ежедневная',  labelEn: 'Daily' },
  { value: 2, labelUz: 'Mavsumiy',  labelRu: 'Сезонная',    labelEn: 'Seasonal' },
  { value: 3, labelUz: 'Oylik',     labelRu: 'Ежемесячная', labelEn: 'Monthly' },
  { value: 4, labelUz: 'Part-time', labelRu: 'Part-time',   labelEn: 'Part-time' },
  { value: 5, labelUz: 'Full-time', labelRu: 'Full-time',   labelEn: 'Full-time' },
  { value: 6, labelUz: 'Masofaviy', labelRu: 'Удалённая',   labelEn: 'Remote' },
];

function getJobTypeLabel(value: number, language: string): string {
  const found = JOB_TYPES.find(j => j.value === value);
  if (!found) return '';
  return language === 'uz' ? found.labelUz : language === 'ru' ? found.labelRu : found.labelEn;
}

export default function PostJobScreen() {
  const { colors } = useThemeStore();
  const { language } = useLanguageStore();
  const { pickedLat, pickedLng, pickedAddress, clear: clearPicked } = useMapPickerStore();

  const [title,           setTitle]           = useState('');
  const [description,     setDescription]     = useState('');
  const [salary,          setSalary]          = useState('');
  const [city,            setCity]            = useState('');
  const [country,         setCountry]         = useState("O'zbekiston");
  const [experienceYears, setExperienceYears] = useState('');
  const [jobType,         setJobType]         = useState(5);
  const [latitude,        setLatitude]        = useState<number | null>(null);
  const [longitude,       setLongitude]       = useState<number | null>(null);
  const [jobAddress,      setJobAddress]      = useState<string>('');

  const [categories,          setCategories]          = useState<Category[]>([]);
  const [selectedCategory,    setSelectedCategory]    = useState<Category | null>(null);
  const [selectedSubCategory, setSelectedSubCategory] = useState<Category | null>(null);

  const [categoryModal,    setCategoryModal]    = useState(false);
  const [subCategoryModal, setSubCategoryModal] = useState(false);
  const [jobTypeModal,     setJobTypeModal]     = useState(false);

  const [loading,           setLoading]           = useState(false);
  const [loadingCategories, setLoadingCategories] = useState(true);

  useEffect(() => {
    loadCategories();
    return () => clearPicked(); // ekrandan chiqganda tozalanadi
  }, []);

  // Map picker dan qaytganda koordinatalarni olamiz
  useFocusEffect(
    useCallback(() => {
      if (pickedLat !== null && pickedLng !== null) {
        setLatitude(pickedLat);
        setLongitude(pickedLng);
        setJobAddress(pickedAddress);
      }
    }, [pickedLat, pickedLng])
  );

  const loadCategories = async () => {
    try {
      const data = await categoryService.getCategories();
      const other: Category = {
        id: 'other',
        name: language === 'uz' ? 'Boshqa' : language === 'ru' ? 'Другое' : 'Other',
        subCategories: [],
      };
      setCategories([...data, other]);
    } catch {
      Alert.alert('Xato', 'Kategoriyalarni yuklashda xatolik');
    } finally {
      setLoadingCategories(false);
    }
  };

  const handleCategorySelect = (cat: Category) => {
    setSelectedCategory(cat);
    setSelectedSubCategory(null);
    setCategoryModal(false);
    if (cat.subCategories && cat.subCategories.length > 0) {
      setTimeout(() => setSubCategoryModal(true), 300);
    }
  };

  const handleOpenMapPicker = () => {
    if (latitude !== null && longitude !== null) {
      router.push({ pathname: '/map-picker', params: { initLat: latitude.toString(), initLng: longitude.toString() } });
    } else {
      router.push('/map-picker');
    }
  };

  const handleSubmit = async () => {
    if (!title.trim())     { Alert.alert('Xato', "Ish nomi kiritilmagan"); return; }
    if (!description.trim()) { Alert.alert('Xato', 'Tavsif kiritilmagan'); return; }
    if (!salary)           { Alert.alert('Xato', 'Maosh kiritilmagan'); return; }
    if (!city.trim())      { Alert.alert('Xato', 'Shahar kiritilmagan'); return; }
    if (!selectedCategory) { Alert.alert('Xato', 'Kategoriya tanlanmagan'); return; }
    if (latitude === null) { Alert.alert('Xato', label('Joylashuvni xaritada belgilang', 'Укажите местоположение на карте', 'Please mark location on map')); return; }

    setLoading(true);
    try {
      await jobService.createJob({
        title:           title.trim(),
        description:     description.trim(),
        salary:          parseFloat(salary),
        jobType,
        categoryId:      selectedCategory.id === 'other' ? undefined : selectedCategory.id,
        categoryName:    selectedCategory.name,
        subCategoryId:   selectedSubCategory?.id,
        subCategoryName: selectedSubCategory?.name,
        city:            city.trim(),
        country:         country.trim(),
        latitude:        latitude ?? undefined,
        longitude:       longitude ?? undefined,
        experienceYears: experienceYears ? parseInt(experienceYears) : undefined,
      });
      Alert.alert(
        language === 'uz' ? 'Muvaffaqiyat' : language === 'ru' ? 'Успех' : 'Success',
        language === 'uz' ? "Ish e'loni joylashtirildi!" : language === 'ru' ? 'Вакансия размещена!' : 'Job posted!',
        [{ text: 'OK', onPress: () => router.back() }]
      );
    } catch {
      Alert.alert('Xato', "Ish e'lonini joylashtirishda xatolik");
    } finally {
      setLoading(false);
    }
  };

  const renderModal = (
    visible: boolean,
    onClose: () => void,
    modalTitle: string,
    data: any[],
    onSelect: (item: any) => void,
    keyExtractor: (item: any) => string,
    labelExtractor: (item: any) => string,
  ) => (
    <Modal visible={visible} transparent animationType="slide" onRequestClose={onClose}>
      <View style={styles.modalOverlay}>
        <View style={[styles.modalContainer, { backgroundColor: colors.surface }]}>
          <Text style={[styles.modalTitle, { color: colors.textPrimary }]}>{modalTitle}</Text>
          <FlatList
            data={data}
            keyExtractor={keyExtractor}
            renderItem={({ item }) => (
              <TouchableOpacity
                style={[styles.modalItem, { borderBottomColor: colors.border }]}
                onPress={() => onSelect(item)}
                activeOpacity={0.7}
              >
                <Text style={[styles.modalItemText, { color: colors.textPrimary }]}>
                  {labelExtractor(item)}
                </Text>
              </TouchableOpacity>
            )}
          />
          <TouchableOpacity
            style={[styles.modalCancel, { backgroundColor: colors.border }]}
            onPress={onClose}
          >
            <Text style={[styles.modalCancelText, { color: colors.textSecondary }]}>
              {language === 'uz' ? 'Bekor qilish' : language === 'ru' ? 'Отмена' : 'Cancel'}
            </Text>
          </TouchableOpacity>
        </View>
      </View>
    </Modal>
  );

  const label = (uz: string, ru: string, en: string) =>
    language === 'uz' ? uz : language === 'ru' ? ru : en;

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <LinearGradient colors={[colors.primary, '#15803d']} style={styles.header}>
        <TouchableOpacity onPress={() => router.back()} activeOpacity={0.7}>
          <BackIcon size={22} color="#fff" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>
          {label("Ish e'loni joylash", 'Разместить вакансию', 'Post a Job')}
        </Text>
        <View style={{ width: 22 }} />
      </LinearGradient>

      <ScrollView
        style={styles.scroll}
        contentContainerStyle={styles.scrollContent}
        showsVerticalScrollIndicator={false}
        keyboardShouldPersistTaps="always"
      >
        {/* Ish nomi */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Ish nomi', 'Название вакансии', 'Job Title')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder={label('Masalan: Oshpaz', 'Например: Повар', 'e.g. Chef')}
          placeholderTextColor={colors.textTertiary}
          value={title}
          onChangeText={setTitle}
        />

        {/* Tavsif */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Tavsif', 'Описание', 'Description')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TextInput
          style={[styles.input, styles.textArea, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder={label("Ish haqida batafsil...", 'Подробнее о работе...', 'Job details...')}
          placeholderTextColor={colors.textTertiary}
          value={description}
          onChangeText={setDescription}
          multiline
          numberOfLines={4}
          textAlignVertical="top"
        />

        {/* Maosh */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label("Maosh (so'm)", 'Зарплата (сум)', 'Salary (UZS)')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="3 000 000"
          placeholderTextColor={colors.textTertiary}
          value={salary}
          onChangeText={setSalary}
          keyboardType="numeric"
        />

        {/* Ish turi */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Ish turi', 'Тип работы', 'Job Type')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TouchableOpacity
          style={[styles.selector, { backgroundColor: colors.card, borderColor: colors.border }]}
          onPress={() => setJobTypeModal(true)}
          activeOpacity={0.7}
        >
          <Text style={[styles.selectorText, { color: colors.textPrimary }]}>
            {getJobTypeLabel(jobType, language)}
          </Text>
          <ChevronIcon size={18} color={colors.textSecondary} />
        </TouchableOpacity>

        {/* Kategoriya */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Kategoriya', 'Категория', 'Category')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        {loadingCategories ? (
          <ActivityIndicator color={colors.primary} style={{ marginVertical: 8 }} />
        ) : (
          <TouchableOpacity
            style={[styles.selector, { backgroundColor: colors.card, borderColor: colors.border }]}
            onPress={() => setCategoryModal(true)}
            activeOpacity={0.7}
          >
            <Text style={[styles.selectorText, { color: selectedCategory ? colors.textPrimary : colors.textTertiary }]}>
              {selectedCategory?.name ?? label('Kategoriya tanlang', 'Выберите категорию', 'Select category')}
            </Text>
            <ChevronIcon size={18} color={colors.textSecondary} />
          </TouchableOpacity>
        )}

        {/* Sub-kategoriya */}
        {selectedCategory && (selectedCategory.subCategories?.length ?? 0) > 0 && (
          <>
            <Text style={[styles.label, { color: colors.textSecondary }]}>
              {label("Yo'nalish", 'Направление', 'Subcategory')}
              <Text style={[styles.optional, { color: colors.textTertiary }]}>
                {label(' (ixtiyoriy)', ' (необязательно)', ' (optional)')}
              </Text>
            </Text>
            <TouchableOpacity
              style={[styles.selector, { backgroundColor: colors.card, borderColor: colors.border }]}
              onPress={() => setSubCategoryModal(true)}
              activeOpacity={0.7}
            >
              <Text style={[styles.selectorText, { color: selectedSubCategory ? colors.textPrimary : colors.textTertiary }]}>
                {selectedSubCategory?.name ?? label("Yo'nalish tanlang", 'Выберите направление', 'Select subcategory')}
              </Text>
              <ChevronIcon size={18} color={colors.textSecondary} />
            </TouchableOpacity>
          </>
        )}

        {/* Shahar */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Shahar', 'Город', 'City')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder={label('Toshkent', 'Ташкент', 'Tashkent')}
          placeholderTextColor={colors.textTertiary}
          value={city}
          onChangeText={setCity}
        />

        {/* Davlat */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Davlat', 'Страна', 'Country')}
          <Text style={[styles.optional, { color: colors.textTertiary }]}>
            {label(' (ixtiyoriy)', ' (необязательно)', ' (optional)')}
          </Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          value={country}
          onChangeText={setCountry}
          placeholderTextColor={colors.textTertiary}
        />

        {/* Tajriba */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Tajriba (yil)', 'Опыт (лет)', 'Experience (years)')}
          <Text style={[styles.optional, { color: colors.textTertiary }]}>
            {label(' (ixtiyoriy)', ' (необязательно)', ' (optional)')}
          </Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="0"
          placeholderTextColor={colors.textTertiary}
          value={experienceYears}
          onChangeText={setExperienceYears}
          keyboardType="numeric"
        />

        {/* Joylashuv — xaritada belgilash */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Ish joylashuvi', 'Местоположение', 'Job Location')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TouchableOpacity
          style={[
            styles.locationBtn,
            {
              backgroundColor: colors.card,
              borderColor: latitude !== null ? colors.primary : colors.border,
            },
          ]}
          onPress={handleOpenMapPicker}
          activeOpacity={0.7}
        >
          <MapPinIcon size={18} color={latitude !== null ? colors.primary : colors.textTertiary} />
          <Text style={[styles.locationBtnText, { color: latitude !== null ? colors.primary : colors.textTertiary }]}>
            {latitude !== null
              ? (jobAddress || `${latitude.toFixed(4)}, ${longitude!.toFixed(4)}`)
              : label('Xaritada belgilang', 'Указать на карте', 'Mark on map')}
          </Text>
          <ChevronIcon size={18} color={latitude !== null ? colors.primary : colors.textSecondary} />
        </TouchableOpacity>

        {/* Submit */}
        <TouchableOpacity
          style={[styles.submitBtn, { opacity: loading ? 0.7 : 1 }]}
          onPress={handleSubmit}
          activeOpacity={0.85}
          disabled={loading}
        >
          <LinearGradient colors={[colors.primary, '#15803d']} style={styles.submitGradient}>
            {loading ? (
              <ActivityIndicator color="#fff" />
            ) : (
              <Text style={styles.submitText}>
                {label("E'lon joylash", 'Разместить', 'Post Job')}
              </Text>
            )}
          </LinearGradient>
        </TouchableOpacity>
      </ScrollView>

      {renderModal(
        categoryModal,
        () => setCategoryModal(false),
        label('Kategoriya tanlang', 'Выберите категорию', 'Select Category'),
        categories,
        handleCategorySelect,
        (item) => item.id,
        (item) => item.name,
      )}

      {selectedCategory && renderModal(
        subCategoryModal,
        () => setSubCategoryModal(false),
        label("Yo'nalish tanlang", 'Выберите направление', 'Select Subcategory'),
        selectedCategory.subCategories ?? [],
        (item) => { setSelectedSubCategory(item); setSubCategoryModal(false); },
        (item) => item.id,
        (item) => item.name,
      )}

      {renderModal(
        jobTypeModal,
        () => setJobTypeModal(false),
        label('Ish turini tanlang', 'Выберите тип работы', 'Select Job Type'),
        JOB_TYPES,
        (item) => { setJobType(item.value); setJobTypeModal(false); },
        (item) => item.value.toString(),
        (item) => language === 'uz' ? item.labelUz : language === 'ru' ? item.labelRu : item.labelEn,
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container:       { flex: 1 },
  header:          { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: Spacing.xl, paddingTop: 56, paddingBottom: Spacing.lg },
  headerTitle:     { fontSize: FontSize.lg, fontWeight: FontWeight.bold, color: '#fff' },
  scroll:          { flex: 1 },
  scrollContent:   { padding: Spacing.xl, paddingBottom: 40 },
  label:           { fontSize: FontSize.sm, fontWeight: FontWeight.medium, marginBottom: 6, marginTop: Spacing.md },
  optional:        { fontSize: FontSize.xs, fontWeight: FontWeight.regular },
  input:           { borderRadius: BorderRadius.lg, paddingHorizontal: Spacing.md, paddingVertical: 12, fontSize: FontSize.md, borderWidth: 1.5 },
  textArea:        { height: 120, paddingTop: 12 },
  selector:        { borderRadius: BorderRadius.lg, paddingHorizontal: Spacing.md, paddingVertical: 14, borderWidth: 1.5, flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  selectorText:    { fontSize: FontSize.md },
  locationBtn:     { borderRadius: BorderRadius.lg, paddingHorizontal: Spacing.md, paddingVertical: 14, borderWidth: 1.5, flexDirection: 'row', alignItems: 'center', gap: 8 },
  locationBtnText: { flex: 1, fontSize: FontSize.md },
  submitBtn:       { marginTop: Spacing.xl, borderRadius: BorderRadius.xl, overflow: 'hidden' },
  submitGradient:  { paddingVertical: 16, alignItems: 'center' },
  submitText:      { color: '#fff', fontSize: FontSize.md, fontWeight: FontWeight.bold },
  modalOverlay:    { flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'flex-end' },
  modalContainer:  { borderTopLeftRadius: 24, borderTopRightRadius: 24, maxHeight: '70%', padding: Spacing.xl },
  modalTitle:      { fontSize: FontSize.lg, fontWeight: FontWeight.bold, marginBottom: Spacing.md, textAlign: 'center' },
  modalItem:       { paddingVertical: 14, borderBottomWidth: 1 },
  modalItemText:   { fontSize: FontSize.md },
  modalCancel:     { marginTop: Spacing.md, borderRadius: BorderRadius.lg, paddingVertical: 14, alignItems: 'center' },
  modalCancelText: { fontSize: FontSize.md, fontWeight: FontWeight.medium },
});
     