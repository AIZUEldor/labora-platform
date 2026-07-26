// TEMPORARY: Eskiz SMS provider is not enabled in production yet (see Labora.Infrastructure
// Eskiz:Enabled config). EXPO_PUBLIC_OTP_ENABLED is inlined into the JS bundle at Expo build
// time (see eas.json build profiles) - any missing or non-"true" value safely defaults to
// false, so OTP-gated flows fall back to their legacy direct equivalents (login/register) or
// show a temporary-unavailable message (forgot-password/change-phone). Set it to "true" only
// once Eskiz SMS is enabled in production.
export const Features = {
  otpEnabled: process.env.EXPO_PUBLIC_OTP_ENABLED === 'true',
} as const;
