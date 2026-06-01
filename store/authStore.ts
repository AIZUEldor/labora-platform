import { create } from 'zustand';
import * as SecureStore from 'expo-secure-store';

export interface AuthState {
  token: string | null;
  role: string | null;
  firstName: string | null;
  lastName: string | null;
  isLoading: boolean;
  login: (token: string, role: string, firstName: string, lastName: string) => Promise<void>;
  logout: () => Promise<void>;
  loadToken: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  role: null,
  firstName: null,
  lastName: null,
  isLoading: true,

  login: async (token, role, firstName, lastName) => {
    await SecureStore.setItemAsync('access_token', token);
    await SecureStore.setItemAsync('user_role', String(role));
    await SecureStore.setItemAsync('first_name', firstName);
    await SecureStore.setItemAsync('last_name', lastName);
    set({ token, role, firstName, lastName });
  },

  logout: async () => {
    await SecureStore.deleteItemAsync('access_token');
    await SecureStore.deleteItemAsync('user_role');
    await SecureStore.deleteItemAsync('first_name');
    await SecureStore.deleteItemAsync('last_name');
    set({ token: null, role: null, firstName: null, lastName: null });
  },

  loadToken: async () => {
    const token     = await SecureStore.getItemAsync('access_token');
    const role      = await SecureStore.getItemAsync('user_role');
    const firstName = await SecureStore.getItemAsync('first_name');
    const lastName  = await SecureStore.getItemAsync('last_name');
    set({ token, role, firstName, lastName, isLoading: false });
  },
}));