import api from './api';
import {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  ForgotPasswordStartRequest,
  ForgotPasswordResendRequest,
  ForgotPasswordVerifyRequest,
  ForgotPasswordCompleteRequest,
  StartOtpResponse,
  ResendOtpResponse,
  VerifyOtpResponse,
  ForgotPasswordCompleteResponse,
} from '../types';

export const authService = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/register', data);
    return response.data;
  },

  forgotPasswordStart: async (data: ForgotPasswordStartRequest): Promise<StartOtpResponse> => {
    const response = await api.post<StartOtpResponse>('/auth/forgot-password/start', data);
    return response.data;
  },

  forgotPasswordResend: async (data: ForgotPasswordResendRequest): Promise<ResendOtpResponse> => {
    const response = await api.post<ResendOtpResponse>('/auth/forgot-password/resend', data);
    return response.data;
  },

  forgotPasswordVerify: async (data: ForgotPasswordVerifyRequest): Promise<VerifyOtpResponse> => {
    const response = await api.post<VerifyOtpResponse>('/auth/forgot-password/verify', data);
    return response.data;
  },

  forgotPasswordComplete: async (data: ForgotPasswordCompleteRequest): Promise<ForgotPasswordCompleteResponse> => {
    const response = await api.post<ForgotPasswordCompleteResponse>('/auth/forgot-password/complete', data);
    return response.data;
  },
};