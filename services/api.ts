import axios, { AxiosInstance, AxiosResponse, InternalAxiosRequestConfig } from 'axios';
import * as SecureStore from 'expo-secure-store';

const BASE_URL = 'https://labora-api.onrender.com/api';
export const MEDIA_URL = 'https://labora-api.onrender.com';

const api: AxiosInstance = axios.create({


  baseURL: BASE_URL,
  headers: {
  'Content-Type': 'application/json',
  'ngrok-skip-browser-warning': 'true',
},
  timeout: 30000,
});

// Request interceptor — token qo'shish
api.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
    console.log(
      'API REQUEST:',
      config.baseURL,
      config.url,
      config.method
    );

    const token = await SecureStore.getItemAsync('access_token');

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor — error handling
api.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error) => {
    console.log(
      'API ERROR:',
      error?.config?.baseURL,
      error?.config?.url,
      error?.message,
      error?.response?.status,
      error?.response?.data
    );
    if (error.response?.status === 401) {
      await SecureStore.deleteItemAsync('access_token');
      await SecureStore.deleteItemAsync('user_role');
    }
    const message = error.response?.data?.message
      || error.response?.data
      || error.message
      || 'Xatolik yuz berdi';
    return Promise.reject(new Error(typeof message === 'string' ? message : JSON.stringify(message)));
  }
);

export default api;