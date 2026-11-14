import { InvalidApiResponseError, isNonEmptyString, isRecord } from '../../core/http/api-response-validation';
import type { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';

export function parseDntCaptchaParams(value: unknown): DntCaptchaParams {
  if (!isRecord(value)) throw new InvalidApiResponseError('CAPTCHA');
  const { dntCaptchaImgUrl, dntCaptchaId, dntCaptchaTextValue, dntCaptchaTokenValue } = value;
  if (
    !isNonEmptyString(dntCaptchaImgUrl) ||
    !isNonEmptyString(dntCaptchaId) ||
    !isNonEmptyString(dntCaptchaTextValue) ||
    !isNonEmptyString(dntCaptchaTokenValue) ||
    !isHttpImageUrl(dntCaptchaImgUrl)
  ) {
    throw new InvalidApiResponseError('CAPTCHA');
  }
  // Keep the original strings: encrypted tokens must not be trimmed or transformed.
  return { dntCaptchaImgUrl, dntCaptchaId, dntCaptchaTextValue, dntCaptchaTokenValue };
}

function isHttpImageUrl(value: string): boolean {
  try {
    // A fixed base validates relative URLs without depending on browser globals.
    const url = new URL(value, 'https://captcha.invalid/');
    return url.protocol === 'http:' || url.protocol === 'https:';
  } catch {
    return false;
  }
}
