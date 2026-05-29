import { Platform } from 'react-native';

export const FontFamily = {
  regular: Platform.OS === 'ios' ? 'SF Pro Text' : 'Roboto',
  medium: Platform.OS === 'ios' ? 'SF Pro Text' : 'Roboto-Medium',
  semiBold: Platform.OS === 'ios' ? 'SF Pro Text' : 'Roboto-Medium',
  bold: Platform.OS === 'ios' ? 'SF Pro Display' : 'Roboto-Bold',
};

export const FontSize = {
  xs: 11,
  sm: 13,
  md: 15,
  lg: 17,
  xl: 20,
  xxl: 24,
  xxxl: 30,
  display: 36,
};

export const LineHeight = {
  xs: 16,
  sm: 18,
  md: 22,
  lg: 24,
  xl: 28,
  xxl: 32,
  xxxl: 38,
  display: 44,
};

export const FontWeight = {
  regular: '400' as const,
  medium: '500' as const,
  semiBold: '600' as const,
  bold: '700' as const,
  extraBold: '800' as const,
};