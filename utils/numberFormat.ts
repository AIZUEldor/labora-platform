// Digit-only sanitizer + thousands-space formatter shared by every salary input.
// The formatted string returned by formatThousands is a display-only view - callers
// must keep storing/submitting the sanitized digits (sanitizeDigits' output), never
// the formatted output, so the numeric payload sent to the backend never changes.

export function sanitizeDigits(value: string): string {
  return value.replace(/\D/g, '');
}

export function formatThousands(value: string): string {
  const digits = sanitizeDigits(value);
  if (!digits) return '';
  return digits.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
}
