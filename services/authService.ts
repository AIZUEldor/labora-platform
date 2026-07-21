import api from './api';
import {
  LoginRequest,
  LoginResendRequest,
  LoginVerifyRequest,
  LoginCompleteRequest,
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

  // Kirish — OTP oqimi
  loginStart: async (data: LoginRequest): Promise<StartOtpResponse> => {
    const response = await api.post<StartOtpResponse>('/auth/login/start', data);
    return response.data;
  },

  loginResend: async (data: LoginResendRequest): Promise<ResendOtpResponse> => {
    const response = await api.post<ResendOtpResponse>('/auth/login/resend', data);
    return response.data;
  },

  loginVerify: async (data: LoginVerifyRequest): Promise<VerifyOtpResponse> => {
    const response = await api.post<VerifyOtpResponse>('/auth/login/verify', data);
    return response.data;
  },

  loginComplete: async (data: LoginCompleteRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/login/complete', data);
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