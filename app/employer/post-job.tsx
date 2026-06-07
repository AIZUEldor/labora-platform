import React, { useState, useEffect } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity,
  TextInput, ScrollView, ActivityIndicator, Alert, Modal, FlatList, Platform,
} from 'react-native';
import { router } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import DateTimePicker from '@react-native-community/datetimepicker';
import { useThemeStore } from '../../store/themeStore';
import { useLanguageStore } from '../../stores/useLanguageStore';
import { jobService } from '../../services/jobService';
import { Category } from '../../types';
import { Spacing, BorderRadius, Shadow } from '../../constants/spacing';
import { FontSize, FontWeight } from '../../constants/typography';
import Svg, { Path } from 'react-native-svg';
import api from '../../services/api';

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

function CalendarIcon({ size = 20, color = '#000' }: { size?: number; color?: string }) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path d="M8 2v3M16 2v3M3.5 9.09h17M21 8.5V17c0 3-1.5 5-5 5H8c-3.5 0-5-2-5-5V8.5c0-3 1.5-5 5-5h8c3.5 0 5 2 5 5z" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

const JOB_TYPES = [
  { value: 0, label: 'Full-time' },
  { value: 1, label: 'Part-time' },
  { value: 2, label: 'Remote' },
  { value: 3, label: 'Contract' },
  { value: 4, label: 'Internship' },
];

export default function PostJobScreen() {
  const { colors } = useThemeStore();
  const { language } = useLanguageStore();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [salary, setSalary] = useState('');
  const [city, setCity] = useState('');
  const [country, setCountry] = useState("O'zbekiston");
  const [requiredSkills, setRequiredSkills] = useState('');
  const [experienceYears, setExperienceYears] = useState('');
  const [jobType, setJobType] = useState(0);

  const [deadline, setDeadline] = useState<Date | null>(null);
  const [showDatePicker, setShowDatePicker] = useState(false);

  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null);
  const [selectedSubCategory, setSelectedSubCategory] = useState<Category | null>(null);

  const [categoryModal, setCategoryModal] = useState(false);
  const [subCategoryModal, setSubCategoryModal] = useState(false);
  const [jobTypeModal, setJobTypeModal] = useState(false);

  const [loading, setLoading] = useState(false);
  const [loadingCategories, setLoadingCategories] = useState(true);

  useEffect(() => { loadCategories(); }, []);

  const loadCategories = async () => {
    try {
      const res = await api.get<Category[]>('/categories');
      const other: Category = {
        id: 'other',
        name: language === 'uz' ? 'Boshqa' : language === 'ru' ? 'Другое' : 'Other',
        subCategories: [],
      };
      setCategories([...res.data, other]);
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

  const formatDate = (date: Date) => {
    const d = date.getDate().toString().padStart(2, '0');
    const m = (date.getMonth() + 1).toString().padStart(2, '0');
    const y = date.getFullYear();
    return `${d}.${m}.${y}`;
  };

  const handleSubmit = async () => {
    if (!title.trim())          { Alert.alert('Xato', "Ish nomi kiritilmagan"); return; }
    if (!description.trim())    { Alert.alert('Xato', 'Tavsif kiritilmagan'); return; }
    if (!salary)                { Alert.alert('Xato', 'Maosh kiritilmagan'); return; }
    if (!city.trim())           { Alert.alert('Xato', 'Shahar kiritilmagan'); return; }
    if (!selectedCategory)      { Alert.alert('Xato', 'Kategoriya tanlanmagan'); return; }

    try {
      setLoading(true);
      await jobService.createJob({
        title:           title.trim(),
        description:     description.trim(),
        salary:          parseFloat(salary),
        jobType,
        categoryId:      selectedCategory.id === 'other' ? '' : selectedCategory.id,
        subCategoryId:   selectedSubCategory?.id,
        city:            city.trim(),
        country:         country.trim(),
        requiredSkills:  requiredSkills.trim() || undefined,
        experienceYears: experienceYears ? parseInt(experienceYears) : undefined,
        deadline:        deadline ? deadline.toISOString() : undefined,
      });
      Alert.alert('Muvaffaqiyat', "Ish e'loni joylashtirildi!", [
        { text: 'OK', onPress: () => router.back() }
      ]);
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
      {/* Header */}
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
        {/* Ish nomi — majburiy */}
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

        {/* Tavsif — majburiy */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Tavsif', 'Описание', 'Description')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TextInput
          style={[styles.input, styles.textArea, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder={label('Ish haqida batafsil...', 'Подробнее о работе...', 'Job details...')}
          placeholderTextColor={colors.textTertiary}
          value={description}
          onChangeText={setDescription}
          multiline
          numberOfLines={4}
          textAlignVertical="top"
        />

        {/* Maosh — majburiy */}
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

        {/* Ish turi — majburiy */}
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
            {JOB_TYPES.find(j => j.value === jobType)?.label ?? 'Full-time'}
          </Text>
          <ChevronIcon size={18} color={colors.textSecondary} />
        </TouchableOpacity>

        {/* Kategoriya — majburiy */}
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

        {/* Sub-kategoriya — ixtiyoriy */}
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

        {/* Shahar — majburiy */}
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

        {/* Davlat — ixtiyoriy */}
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

        {/* Ko'nikmalar — ixtiyoriy */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label("Ko'nikmalar", 'Навыки', 'Skills')}
          <Text style={[styles.optional, { color: colors.textTertiary }]}>
            {label(' (ixtiyoriy)', ' (необязательно)', ' (optional)')}
          </Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder={label("Masalan: Excel, 1C, haydovchilik guvohnomasi", 'Например: Excel, 1C, права', 'e.g. Excel, 1C, driving license')}
          placeholderTextColor={colors.textTertiary}
          value={requiredSkills}
          onChangeText={setRequiredSkills}
        />

        {/* Tajriba — ixtiyoriy */}
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

        {/* Deadline — ixtiyoriy, DatePicker */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Oxirgi muddati', 'Срок подачи', 'Application Deadline')}
          <Text style={[styles.optional, { color: colors.textTertiary }]}>
            {label(' (ixtiyoriy)', ' (необязательно)', ' (optional)')}
          </Text>
        </Text>
        <TouchableOpacity
          style={[styles.selector, { backgroundColor: colors.card, borderColor: colors.border }]}
          onPress={() => setShowDatePicker(true)}
          activeOpacity={0.7}
        >
          <Text style={[styles.selectorText, { color: deadline ? colors.textPrimary : colors.textTertiary }]}>
            {deadline ? formatDate(deadline) : label('Sana tanlang', 'Выберите дату', 'Select date')}
          </Text>
          <CalendarIcon size={18} color={colors.textSecondary} />
        </TouchableOpacity>
        {deadline && (
          <TouchableOpacity onPress={() => setDeadline(null)} style={{ marginTop: 4 }}>
            <Text style={{ color: colors.textTertiary, fontSize: FontSize.xs }}>
              {label("Sanani o'chirish", 'Очистить дату', 'Clear date')}
            </Text>
          </TouchableOpacity>
        )}

        {/* DatePicker */}
        {showDatePicker && (
          <DateTimePicker
            value={deadline ?? new Date()}
            mode="date"
            display={Platform.OS === 'ios' ? 'spinner' : 'default'}
            minimumDate={new Date()}
            onChange={(event, date) => {
              setShowDatePicker(Platform.OS === 'ios');
              if (event.type === 'set' && date) setDeadline(date);
              else if (Platform.OS !== 'ios') setShowDatePicker(false);
            }}
          />
        )}

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

      {/* Kategoriya modal */}
      {renderModal(
        categoryModal,
        () => setCategoryModal(false),
        label('Kategoriya tanlang', 'Выберите категорию', 'Select Category'),
        categories,
        handleCategorySelect,
        (item) => item.id,
        (item) => item.name,
      )}

      {/* Sub-kategoriya modal */}
      {selectedCategory && renderModal(
        subCategoryModal,
        () => setSubCategoryModal(false),
        label("Yo'nalish tanlang", 'Выберите направление', 'Select Subcategory'),
        selectedCategory.subCategories ?? [],
        (item) => { setSelectedSubCategory(item); setSubCategoryModal(false); },
        (item) => item.id,
        (item) => item.name,
      )}

      {/* Ish turi modal */}
      {renderModal(
        jobTypeModal,
        () => setJobTypeModal(false),
        label('Ish turini tanlang', 'Выберите тип работы', 'Select Job Type'),
        JOB_TYPES,
        (item) => { setJobType(item.value); setJobTypeModal(false); },
        (item) => item.value.toString(),
        (item) => item.label,
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
  textArea:        { height: 100, paddingTop: 12 },
  selector:        { borderRadius: BorderRadius.lg, paddingHorizontal: Spacing.md, paddingVertical: 14, borderWidth: 1.5, flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  selectorText:    { fontSize: FontSize.md },
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