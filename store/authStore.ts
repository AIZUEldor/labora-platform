import { create } from 'zustand';
import * as SecureStore from 'expo-secure-store';

export interface AuthState {
  token: string | null;
  role: string | null;
  isLoading: boolean;
  login: (token: string, role: string) => Promise<void>;
  logout: () => Promise<void>;
  loadToken: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  role: null,
  isLoading: true,

  login: async (token: string, role: string) => {
    await SecureStore.setItemAsync('access_token', token);
    await SecureStore.setItemAsync('user_role', String(role));
    set({ token, role });
  },

  logout: async () => {
    await SecureStore.deleteItemAsync('access_token');
    await SecureStore.deleteItemAsync('user_role');
    set({ token: null, role: null });
  },

  loadToken: async () => {
    const token = await SecureStore.getItemAsync('access_token');
    const role = await SecureStore.getItemAsync('user_role');
    set({ token, role, isLoading: false });
  },
}));