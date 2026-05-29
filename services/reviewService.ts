import api from './api';
import { Review } from '../types';

export const reviewService = {
  // Baholash yaratish
  create: async (data: {
    jobApplicationId: string;
    overallRating: number;
    paymentRating?: number;
    employerCommunicationRating?: number;
    experienceRating?: number;
    workerCommunicationRating?: number;
    workQualityRating?: number;
    punctualityRating?: number;
    responsibilityRating?: number;
    wouldWorkAgain: boolean;
    comment?: string;
  }): Promise<void> => {
    await api.post('/Review', data);
  },

  // Foydalanuvchi baholarini olish
  getUserReviews: async (userId: string): Promise<Review[]> => {
    const response = await api.get<Review[]>(`/Review/user/${userId}`);
    return response.data;
  },

  // O'rtacha reyting
  getAverageRating: async (userId: string): Promise<number> => {
    const response = await api.get<{ averageRating: number }>(`/Review/user/${userId}/average`);
    return response.data.averageRating;
  },
};