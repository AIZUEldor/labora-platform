export const LightColors = {
  // Primary
  primary: '#16A34A',
  primaryDark: '#15803D',
  primaryLight: '#DCFCE7',

  // Background
  background: '#F0FDF4',
  surface: '#FFFFFF',
  card: '#FFFFFF',

  // Border
  border: '#BBF7D0',
  borderLight: '#DCFCE7',

  // Text
  textPrimary: '#0A0A0A',
  textSecondary: '#374151',
  textTertiary: '#9CA3AF',
  textInverse: '#FFFFFF',

  // Status
  success: '#16A34A',
  warning: '#F59E0B',
  error: '#EF4444',

  // Tab bar
  tabBar: '#FFFFFF',
  tabBarBorder: '#F0FDF4',

  // Overlay
  overlay: 'rgba(0,0,0,0.5)',

  white: '#FFFFFF',
  black: '#0A0A0A',
};

export const DarkColors = {
  // Primary
  primary: '#22C55E',
  primaryDark: '#16A34A',
  primaryLight: '#052E16',

  // Background
  background: '#0A0A0A',
  surface: '#111827',
  card: '#1F2937',

  // Border
  border: '#166534',
  borderLight: '#14532D',

  // Text
  textPrimary: '#F9FAFB',
  textSecondary: '#D1D5DB',
  textTertiary: '#6B7280',
  textInverse: '#0A0A0A',

  // Status
  success: '#22C55E',
  warning: '#F59E0B',
  error: '#EF4444',

  // Tab bar
  tabBar: '#111827',
  tabBarBorder: '#1F2937',

  // Overlay
  overlay: 'rgba(0,0,0,0.7)',

  white: '#FFFFFF',
  black: '#0A0A0A',
};

export type ColorScheme = typeof LightColors;
export const Colors = LightColors;

export const ThemeColors = {
  light: LightColors,
  dark: DarkColors,
};