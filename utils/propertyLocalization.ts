// Shared Property enum label definitions - the numeric `value` on each entry must exactly match
// the backend enums (Labora.Domain/Enums/PropertyType.cs, RenovationStatus.cs, RentalPeriod.cs).
// Only the rendered label may vary by language; `value` is what gets sent to/read from the
// backend and must never be derived from or replaced by a label.

export interface PropertyEnumOption {
  value: number;
  labelUz: string;
  labelRu: string;
  labelEn: string;
}

export const PROPERTY_TYPES: PropertyEnumOption[] = [
  { value: 1, labelUz: 'Uy',       labelRu: 'Дом',       labelEn: 'House' },
  { value: 2, labelUz: 'Kvartira', labelRu: 'Квартира',  labelEn: 'Apartment' },
];

export const RENOVATION_STATUSES: PropertyEnumOption[] = [
  { value: 1, labelUz: "Ta'mirlash talab qiladi", labelRu: 'Требует ремонта', labelEn: 'Needs renovation' },
  { value: 2, labelUz: "O'rtacha",                labelRu: 'Среднее',         labelEn: 'Average' },
  { value: 3, labelUz: 'Yaxshi',                  labelRu: 'Хорошее',         labelEn: 'Good' },
  { value: 4, labelUz: 'Yevro remont',             labelRu: 'Евроремонт',      labelEn: 'Euro-renovated' },
  { value: 5, labelUz: 'Yangi qurilgan',           labelRu: 'Новостройка',     labelEn: 'Newly built' },
];

export const RENTAL_PERIODS: PropertyEnumOption[] = [
  { value: 1, labelUz: 'Oylik',  labelRu: 'Помесячно', labelEn: 'Monthly' },
  { value: 2, labelUz: 'Kunlik', labelRu: 'Посуточно', labelEn: 'Daily' },
];

export const PROPERTY_LISTING_STATUSES: PropertyEnumOption[] = [
  { value: 1, labelUz: 'Nashr etilgan', labelRu: 'Опубликовано', labelEn: 'Published' },
  { value: 2, labelUz: 'Arxivlangan',   labelRu: 'В архиве',     labelEn: 'Archived' },
];

function getLabel(options: PropertyEnumOption[], value: number, language: string): string {
  const found = options.find(o => o.value === value);
  if (!found) return '';
  return language === 'uz' ? found.labelUz : language === 'ru' ? found.labelRu : found.labelEn;
}

export function getPropertyTypeLabel(value: number, language: string): string {
  return getLabel(PROPERTY_TYPES, value, language);
}

export function getRenovationStatusLabel(value: number, language: string): string {
  return getLabel(RENOVATION_STATUSES, value, language);
}

export function getRentalPeriodLabel(value: number, language: string): string {
  return getLabel(RENTAL_PERIODS, value, language);
}

export function getPropertyListingStatusLabel(value: number, language: string): string {
  return getLabel(PROPERTY_LISTING_STATUSES, value, language);
}
