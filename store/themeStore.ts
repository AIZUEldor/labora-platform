import { create } from 'zustand';
import { ColorScheme, LightColors, DarkColors } from '../constants/colors';

interface ThemeState {
  isDark: boolean;
  theme: 'light' | 'dark';
  colors: ColorScheme;
  toggleTheme: () => void;
}

export const useThemeStore = create<ThemeState>((set) => ({
  isDark: false,
  theme: 'light',
  colors: LightColors,

  toggleTheme: () => {
    set((state) => ({
      isDark: !state.isDark,
      theme: !state.isDark ? 'dark' : 'light',
      colors: !state.isDark ? DarkColors : LightColors,
    }));
  },
}));