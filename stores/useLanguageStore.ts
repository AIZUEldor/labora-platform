import { create } from 'zustand';
import { translations, TranslationType } from '../i18n';

type Language = 'uz' | 'ru' | 'en';

interface LanguageState {
  language: Language;
  t: TranslationType;
  setLanguage: (lang: Language) => void;
}

export const useLanguageStore = create<LanguageState>((set) => ({
  language: 'uz',
  t: translations.uz,
  setLanguage: (lang: Language) => {
    set({ language: lang, t: translations[lang] });
    try {
      const AsyncStorage = require('@react-native-async-storage/async-storage').default;
      AsyncStorage.setItem('language', lang);
    } catch {}
  },
}));

try {
  const AsyncStorage = require('@react-native-async-storage/async-storage').default;
  AsyncStorage.getItem('language').then((lang: string | null) => {
    if (lang === 'uz' || lang === 'ru' || lang === 'en') {
      useLanguageStore.setState({ language: lang, t: translations[lang] });
    }
  });
} catch {}