import api from './api';
import { Category } from '../types';

export const categoryService = {
  getCategories: async (): Promise<Category[]> => {
    const response = await api.get<Category[]>('/Category/root');
    return response.data;
  },
};