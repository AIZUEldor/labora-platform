import React, { useState, useEffect, useCallback } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity,
  TextInput, ScrollView, ActivityIndicator, Alert, Modal, FlatList, Image,
} from 'react-native';
import { router, useFocusEffect } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import * as ImagePicker from 'expo-image-picker';
import { useThemeStore } from '../../store/themeStore';
import { useLanguageStore } from '../../stores/useLanguageStore';
import { useMapPickerStore } from '../../stores/useMapPickerStore';
import { propertyService } from '../../services/propertyService';
import { CreatePropertyRequest, PropertyType, RenovationStatus, RentalPeriod } from '../../types';
import { Spacing, BorderRadius } from '../../constants/spacing';
import { FontSize, FontWeight } from '../../constants/typography';
import { PlusIcon, CloseIcon } from '../../components/icons';
import { sanitizeDigits, formatThousands } from '../../utils/numberFormat';
import {
  PROPERTY_TYPES,
  RENOVATION_STATUSES,
  RENTAL_PERIODS,
  getPropertyTypeLabel,
  getRenovationStatusLabel,
  getRentalPeriodLabel,
} from '../../utils/propertyLocalization';
import Svg, { Path } from 'react-native-svg';

const MAX_IMAGES = 10;
const MIN_IMAGES = 1;

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

export default function CreatePropertyScreen() {
  const { colors } = useThemeStore();
  const { language } = useLanguageStore();
  const { pickedLat, pickedLng, pickedAddress, clear: clearPicked } = useMapPickerStore();

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [propertyType, setPropertyType] = useState<PropertyType>(PropertyType.House);
  const [roomCount, setRoomCount] = useState('');
  const [areaSquareMeters, setAreaSquareMeters] = useState('');
  const [floorNumber, setFloorNumber] = useState('');
  const [totalFloors, setTotalFloors] = useState('');
  const [renovationStatus, setRenovationStatus] = useState<RenovationStatus>(RenovationStatus.Average);
  const [price, setPrice] = useState('');
  const [rentalPeriod, setRentalPeriod] = useState<RentalPeriod>(RentalPeriod.Monthly);
  const [address, setAddress] = useState('');
  const [latitude, setLatitude] = useState<number | null>(null);
  const [longitude, setLongitude] = useState<number | null>(null);
  const [contactPhoneNumber, setContactPhoneNumber] = useState('');

  const [propertyTypeModal, setPropertyTypeModal] = useState(false);
  const [renovationModal, setRenovationModal] = useState(false);
  const [rentalPeriodModal, setRentalPeriodModal] = useState(false);

  const [loading, setLoading] = useState(false);
  const [images, setImages] = useState<string[]>([]);

  const label = (uz: string, ru: string, en: string) =>
    language === 'uz' ? uz : language === 'ru' ? ru : en;

  useEffect(() => {
    return () => clearPicked(); // ekrandan chiqganda tozalanadi
  }, []);

  // Map picker dan qaytganda koordinatalar va manzilni olamiz
  useFocusEffect(
    useCallback(() => {
      if (pickedLat !== null && pickedLng !== null) {
        setLatitude(pickedLat);
        setLongitude(pickedLng);
        if (pickedAddress) setAddress(pickedAddress);
      }
    }, [pickedLat, pickedLng])
  );

  const handleOpenMapPicker = () => {
    if (latitude !== null && longitude !== null) {
      router.push({ pathname: '/map-picker', params: { initLat: latitude.toString(), initLng: longitude.toString() } });
    } else {
      router.push('/map-picker');
    }
  };

  const handleSelectPropertyType = (value: number) => {
    setPropertyType(value);
    setPropertyTypeModal(false);
    // House never carries floor fields - clear them so a stale Apartment value can't linger.
    if (value === PropertyType.House) {
      setFloorNumber('');
      setTotalFloors('');
    }
  };

  const pickImage = async () => {
    if (images.length >= MAX_IMAGES) {
      Alert.alert('', label(`Maksimal ${MAX_IMAGES} ta rasm yuklash mumkin`, `Максимум ${MAX_IMAGES} фотографий`, `Maximum ${MAX_IMAGES} images allowed`));
      return;
    }
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      quality: 0.8,
    });
    if (!result.canceled && result.assets[0]) {
      setImages(prev => [...prev, result.assets[0].uri]);
    }
  };

  const removeImage = (index: number) => {
    setImages(prev => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async () => {
    if (loading) return;

    if (!title.trim()) { Alert.alert('Xato', label('Sarlavha kiritilmagan', 'Заголовок не указан', 'Title is required')); return; }
    if (!description.trim()) { Alert.alert('Xato', label('Tavsif kiritilmagan', 'Описание не указано', 'Description is required')); return; }
    if (!roomCount || isNaN(parseInt(roomCount, 10)) || parseInt(roomCount, 10) <= 0) { Alert.alert('Xato', label('Xonalar sonini kiriting', 'Укажите количество комнат', 'Enter room count')); return; }
    if (!areaSquareMeters || isNaN(parseFloat(areaSquareMeters)) || parseFloat(areaSquareMeters) <= 0) { Alert.alert('Xato', label('Maydonni kiriting', 'Укажите площадь', 'Enter area')); return; }
    if (!price || parseFloat(sanitizeDigits(price)) <= 0) { Alert.alert('Xato', label('Narxni kiriting', 'Укажите цену', 'Enter price')); return; }
    if (!address.trim()) { Alert.alert('Xato', label('Manzil kiritilmagan', 'Адрес не указан', 'Address is required')); return; }

    if (propertyType === PropertyType.Apartment) {
      const floorNum = parseInt(floorNumber, 10);
      const totalNum = parseInt(totalFloors, 10);
      if (!floorNumber || isNaN(floorNum) || floorNum <= 0) { Alert.alert('Xato', label('Qavatni kiriting', 'Укажите этаж', 'Enter floor number')); return; }
      if (!totalFloors || isNaN(totalNum) || totalNum <= 0) { Alert.alert('Xato', label('Binoning umumiy qavatlar sonini kiriting', 'Укажите этажность здания', 'Enter total floors')); return; }
      if (floorNum > totalNum) { Alert.alert('Xato', label('Qavat raqami umumiy qavatlar sonidan oshmasligi kerak', 'Этаж не может превышать этажность здания', 'Floor cannot exceed total floors')); return; }
    }

    if (!contactPhoneNumber.trim()) { Alert.alert('Xato', label('Telefon raqami kiritilmagan', 'Номер телефона не указан', 'Phone number is required')); return; }

    if (latitude === null || longitude === null) { Alert.alert('Xato', label('Joylashuvni xaritada belgilang', 'Укажите местоположение на карте', 'Please mark location on map')); return; }

    if (images.length < MIN_IMAGES) { Alert.alert('Xato', label(`Kamida ${MIN_IMAGES} ta rasm yuklang`, `Загрузите минимум ${MIN_IMAGES} фото`, `Upload at least ${MIN_IMAGES} image`)); return; }
    if (images.length > MAX_IMAGES) { Alert.alert('Xato', label(`Maksimal ${MAX_IMAGES} ta rasm yuklash mumkin`, `Максимум ${MAX_IMAGES} фотографий`, `Maximum ${MAX_IMAGES} images allowed`)); return; }

    setLoading(true);
    try {
      const data: CreatePropertyRequest = {
        title: title.trim(),
        description: description.trim(),
        propertyType,
        roomCount: parseInt(roomCount, 10),
        areaSquareMeters: parseFloat(areaSquareMeters),
        floorNumber: propertyType === PropertyType.Apartment ? parseInt(floorNumber, 10) : null,
        totalFloors: propertyType === PropertyType.Apartment ? parseInt(totalFloors, 10) : null,
        renovationStatus,
        price: parseFloat(sanitizeDigits(price)),
        rentalPeriod,
        address: address.trim(),
        latitude,
        longitude,
        contactPhoneNumber: contactPhoneNumber.trim(),
      };

      // Single combined multipart request - the listing and all its images are created together
      // in one call, unlike Job/WorkerPost's create-then-loop-upload flow.
      await propertyService.createProperty(data, images);

      Alert.alert(
        language === 'uz' ? 'Muvaffaqiyat' : language === 'ru' ? 'Успех' : 'Success',
        label("E'lon joylashtirildi!", 'Объявление размещено!', 'Listing posted!'),
        [{ text: 'OK', onPress: () => router.back() }]
      );
    } catch (error: any) {
      Alert.alert('Xato', error?.message || label("E'lonni joylashtirishda xatolik", 'Ошибка при размещении объявления', 'Error posting listing'));
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
              {label('Bekor qilish', 'Отмена', 'Cancel')}
            </Text>
          </TouchableOpacity>
        </View>
      </View>
    </Modal>
  );

  return (
    <View style={[styles.container, { backgroundColor: colors.background }]}>
      <LinearGradient colors={[colors.primary, '#15803d']} style={styles.header}>
        <TouchableOpacity onPress={() => router.back()} activeOpacity={0.7}>
          <BackIcon size={22} color="#fff" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>
          {label("Ko'chmas mulk e'loni", 'Объявление о недвижимости', 'Property Listing')}
        </Text>
        <View style={{ width: 22 }} />
      </LinearGradient>

      <ScrollView
        style={styles.scroll}
        contentContainerStyle={styles.scrollContent}
        showsVerticalScrollIndicator={false}
        keyboardShouldPersistTaps="always"
      >
        {/* Sarlavha */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Sarlavha', 'Заголовок', 'Title')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder={label('Masalan: 2 xonali kvartira ijaraga', 'Например: 2-комнатная квартира в аренду', 'e.g. 2-room apartment for rent')}
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
          placeholder={label('Mulk haqida batafsil...', 'Подробнее о недвижимости...', 'Property details...')}
          placeholderTextColor={colors.textTertiary}
          value={description}
          onChangeText={setDescription}
          multiline
          numberOfLines={4}
          textAlignVertical="top"
        />

        {/* Mulk turi */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Mulk turi', 'Тип недвижимости', 'Property Type')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TouchableOpacity
          style={[styles.selector, { backgroundColor: colors.card, borderColor: colors.border }]}
          onPress={() => setPropertyTypeModal(true)}
          activeOpacity={0.7}
        >
          <Text style={[styles.selectorText, { color: colors.textPrimary }]}>
            {getPropertyTypeLabel(propertyType, language)}
          </Text>
          <ChevronIcon size={18} color={colors.textSecondary} />
        </TouchableOpacity>

        {/* Xonalar soni */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Xonalar soni', 'Количество комнат', 'Room Count')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="2"
          placeholderTextColor={colors.textTertiary}
          value={roomCount}
          onChangeText={setRoomCount}
          keyboardType="numeric"
        />

        {/* Maydon */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Maydon (m²)', 'Площадь (м²)', 'Area (m²)')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="45.5"
          placeholderTextColor={colors.textTertiary}
          value={areaSquareMeters}
          onChangeText={setAreaSquareMeters}
          keyboardType="numeric"
        />

        {/* Qavat / Umumiy qavatlar - faqat Kvartira uchun */}
        {propertyType === PropertyType.Apartment && (
          <>
            <Text style={[styles.label, { color: colors.textSecondary }]}>
              {label('Qavat', 'Этаж', 'Floor')}
              <Text style={{ color: colors.primary }}> *</Text>
            </Text>
            <TextInput
              style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
              placeholder="3"
              placeholderTextColor={colors.textTertiary}
              value={floorNumber}
              onChangeText={setFloorNumber}
              keyboardType="numeric"
            />

            <Text style={[styles.label, { color: colors.textSecondary }]}>
              {label('Binoning umumiy qavatlari', 'Этажность здания', 'Total Building Floors')}
              <Text style={{ color: colors.primary }}> *</Text>
            </Text>
            <TextInput
              style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
              placeholder="9"
              placeholderTextColor={colors.textTertiary}
              value={totalFloors}
              onChangeText={setTotalFloors}
              keyboardType="numeric"
            />
          </>
        )}

        {/* Ta'mirlash holati */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label("Ta'mirlash holati", 'Состояние ремонта', 'Renovation Status')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TouchableOpacity
          style={[styles.selector, { backgroundColor: colors.card, borderColor: colors.border }]}
          onPress={() => setRenovationModal(true)}
          activeOpacity={0.7}
        >
          <Text style={[styles.selectorText, { color: colors.textPrimary }]}>
            {getRenovationStatusLabel(renovationStatus, language)}
          </Text>
          <ChevronIcon size={18} color={colors.textSecondary} />
        </TouchableOpacity>

        {/* Narx */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label("Narx (so'm)", 'Цена (сум)', 'Price (UZS)')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="3 000 000"
          placeholderTextColor={colors.textTertiary}
          value={formatThousands(price)}
          onChangeText={text => setPrice(sanitizeDigits(text))}
          keyboardType="numeric"
        />

        {/* Ijara muddati */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Ijara muddati', 'Период аренды', 'Rental Period')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TouchableOpacity
          style={[styles.selector, { backgroundColor: colors.card, borderColor: colors.border }]}
          onPress={() => setRentalPeriodModal(true)}
          activeOpacity={0.7}
        >
          <Text style={[styles.selectorText, { color: colors.textPrimary }]}>
            {getRentalPeriodLabel(rentalPeriod, language)}
          </Text>
          <ChevronIcon size={18} color={colors.textSecondary} />
        </TouchableOpacity>

        {/* Manzil */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Manzil', 'Адрес', 'Address')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder={label("Ko'cha, uy raqami", 'Улица, номер дома', 'Street, house number')}
          placeholderTextColor={colors.textTertiary}
          value={address}
          onChangeText={setAddress}
        />

        {/* Joylashuv — xaritada belgilash */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Joylashuv', 'Местоположение', 'Location')}
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
              ? `${latitude.toFixed(4)}, ${longitude!.toFixed(4)}`
              : label('Xaritada belgilang', 'Указать на карте', 'Mark on map')}
          </Text>
          <ChevronIcon size={18} color={latitude !== null ? colors.primary : colors.textSecondary} />
        </TouchableOpacity>

        {/* Aloqa uchun telefon raqami */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Aloqa uchun telefon', 'Контактный телефон', 'Contact Phone')}
          <Text style={{ color: colors.primary }}> *</Text>
        </Text>
        <TextInput
          style={[styles.input, { backgroundColor: colors.card, color: colors.textPrimary, borderColor: colors.border }]}
          placeholder="+998901234567"
          placeholderTextColor={colors.textTertiary}
          value={contactPhoneNumber}
          onChangeText={setContactPhoneNumber}
          keyboardType="phone-pad"
        />

        {/* Rasmlar */}
        <Text style={[styles.label, { color: colors.textSecondary }]}>
          {label('Rasmlar', 'Фотографии', 'Photos')}
          <Text style={[styles.optional, { color: colors.textTertiary }]}>
            {label(` (${MIN_IMAGES}-${MAX_IMAGES} ta)`, ` (${MIN_IMAGES}-${MAX_IMAGES})`, ` (${MIN_IMAGES}-${MAX_IMAGES})`)}
          </Text>
        </Text>
        <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.imageRow}>
          {images.map((uri, index) => (
            <View key={index} style={styles.imageWrapper}>
              <Image source={{ uri }} style={styles.previewImage} />
              <TouchableOpacity
                style={styles.removeImageBtn}
                onPress={() => removeImage(index)}
                activeOpacity={0.8}
              >
                <CloseIcon size={14} color="#fff" />
              </TouchableOpacity>
            </View>
          ))}
          {images.length < MAX_IMAGES && (
            <TouchableOpacity
              style={[styles.addImageBtn, { backgroundColor: colors.card, borderColor: colors.border }]}
              onPress={pickImage}
              activeOpacity={0.7}
            >
              <PlusIcon size={28} color={colors.primary} />
            </TouchableOpacity>
          )}
        </ScrollView>

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
                {label("E'lon joylash", 'Разместить', 'Post Listing')}
              </Text>
            )}
          </LinearGradient>
        </TouchableOpacity>
      </ScrollView>

      {renderModal(
        propertyTypeModal,
        () => setPropertyTypeModal(false),
        label('Mulk turini tanlang', 'Выберите тип недвижимости', 'Select Property Type'),
        PROPERTY_TYPES,
        (item) => handleSelectPropertyType(item.value),
        (item) => item.value.toString(),
        (item) => language === 'uz' ? item.labelUz : language === 'ru' ? item.labelRu : item.labelEn,
      )}

      {renderModal(
        renovationModal,
        () => setRenovationModal(false),
        label("Ta'mirlash holatini tanlang", 'Выберите состояние ремонта', 'Select Renovation Status'),
        RENOVATION_STATUSES,
        (item) => { setRenovationStatus(item.value); setRenovationModal(false); },
        (item) => item.value.toString(),
        (item) => language === 'uz' ? item.labelUz : language === 'ru' ? item.labelRu : item.labelEn,
      )}

      {renderModal(
        rentalPeriodModal,
        () => setRentalPeriodModal(false),
        label('Ijara muddatini tanlang', 'Выберите период аренды', 'Select Rental Period'),
        RENTAL_PERIODS,
        (item) => { setRentalPeriod(item.value); setRentalPeriodModal(false); },
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
  imageRow:        { marginTop: 4 },
  imageWrapper:    { position: 'relative', marginRight: Spacing.sm },
  previewImage:    { width: 90, height: 90, borderRadius: BorderRadius.lg },
  removeImageBtn:  { position: 'absolute', top: 4, right: 4, backgroundColor: 'rgba(0,0,0,0.6)', borderRadius: BorderRadius.sm, padding: 3 },
  addImageBtn:     { width: 90, height: 90, borderRadius: BorderRadius.lg, borderWidth: 1.5, borderStyle: 'dashed', justifyContent: 'center', alignItems: 'center' },
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
