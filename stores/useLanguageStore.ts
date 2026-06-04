import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { uz, ru, en, TranslationKeys, Language } from "../i18n";

const translations: Record<Language, TranslationKeys> = { uz, ru, en };

interface LanguageState {
  language: Language;
  t: TranslationKeys;
  setLanguage: (lang: Language) => void;
}

export const useLanguageStore = create<LanguageState>()(
  persist(
    (set) => ({
      language: "uz",
      t: uz,
      setLanguage: (lang: Language) => {
        set({ language: lang, t: translations[lang] });
      },
    }),
    {
      name: "labora-language",
      storage: createJSONStorage(() => AsyncStorage),
      // Only persist the language key, not the whole translations object
      partialize: (state) => ({ language: state.language }),
      // Rehydrate translations after loading from storage
      onRehydrateStorage: () => (state) => {
        if (state) {
          state.t = translations[state.language];
        }
      },
    }
  )
);
