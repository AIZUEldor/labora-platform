// Shared Job Type definitions - the numeric `value` on each entry must exactly match the
// backend enum (Labora.Domain/Enums/JobType.cs): Daily=1, Seasonal=2, Monthly=3, PartTime=4,
// FullTime=5, Remote=6. Only the rendered label may vary by language; `value` is what gets
// sent to/read from the backend and must never be derived from or replaced by a label.

export interface JobTypeOption {
  value: number;
  labelUz: string;
  labelRu: string;
  labelEn: string;
}

export const JOB_TYPES: JobTypeOption[] = [
  { value: 1, labelUz: 'Kunlik',    labelRu: 'Ежедневная',  labelEn: 'Daily' },
  { value: 2, labelUz: 'Mavsumiy',  labelRu: 'Сезонная',    labelEn: 'Seasonal' },
  { value: 3, labelUz: 'Oylik',     labelRu: 'Ежемесячная', labelEn: 'Monthly' },
  { value: 4, labelUz: 'Part-time', labelRu: 'Part-time',   labelEn: 'Part-time' },
  { value: 5, labelUz: 'Full-time', labelRu: 'Full-time',   labelEn: 'Full-time' },
  { value: 6, labelUz: 'Masofaviy', labelRu: 'Удалённая',   labelEn: 'Remote' },
];

export function getJobTypeLabel(value: number, language: string): string {
  const found = JOB_TYPES.find(j => j.value === value);
  if (!found) return '';
  return language === 'uz' ? found.labelUz : language === 'ru' ? found.labelRu : found.labelEn;
}
