import { create } from 'zustand';
import * as SecureStore from 'expo-secure-store';
import { ColorScheme, LightColors, DarkColors } from '../constants/colors';

interface ThemeState {
  isDark: boolean;
  theme: 'light' | 'dark';
  colors: ColorScheme;
  toggleTheme: () => void;
  loadTheme: () => Promise<void>;
}

export const useThemeStore = create<ThemeState>((set) => ({
  isDark: false,
  theme: 'light',
  colors: LightColors,

  loadTheme: async () => {
    const saved = await SecureStore.getItemAsync('app_theme');
    if (saved === 'dark') {
      set({ isDark: true, theme: 'dark', colors: DarkColors });
    }
  },

  toggleTheme: async () => {
    set((state) => {
      const isDark = !state.isDark;
      SecureStore.setItemAsync('app_theme', isDark ? 'dark' : 'light');
      return {
        isDark,
        theme: isDark ? 'dark' : 'light',
        colors: isDark ? DarkColors : LightColors,
      };
    });
  },
}));