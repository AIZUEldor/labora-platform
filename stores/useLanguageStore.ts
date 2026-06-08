import { create } from 'zustand';
import * as SecureStore from 'expo-secure-store';
import { translations, TranslationType, Language } from '../i18n';

interface LanguageState {
  language: Language;
  t: TranslationType;
  setLanguage: (lang: Language) => Promise<void>;
  loadLanguage: () => Promise<void>;
}

export const useLanguageStore = create<LanguageState>((set) => ({
  language: 'uz',
  t: translations.uz,

  setLanguage: async (lang: Language) => {
    set({ language: lang, t: translations[lang] });
    await SecureStore.setItemAsync('language', lang);
  },

  loadLanguage: async () => {
    const lang = await SecureStore.getItemAsync('language');
    if (lang === 'uz' || lang === 'ru' || lang === 'en') {
      set({ language: lang, t: translations[lang] });
    }
  },
}));
