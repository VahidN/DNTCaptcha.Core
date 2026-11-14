import { InvalidApiResponseError, isNonEmptyString, isRecord } from '../../core/http/api-response-validation';
import type { DntCaptchaParams } from '../interfaces/dnt-captcha-params.interface';

export function parseDntCaptchaParams(value: unknown, mode: 'image' | 'audio' = 'image'): DntCaptchaParams {
  if (!isRecord(value)) throw new InvalidApiResponseError('CAPTCHA');
  const { dntCaptchaImgUrl, dntCaptchaAudioUrl, dntCaptchaId, dntCaptchaTextValue, dntCaptchaTokenValue } = value;
  if (
    !isNonEmptyString(dntCaptchaId) ||
    !isNonEmptyString(dntCaptchaTextValue) ||
    !isNonEmptyString(dntCaptchaTokenValue)
  ) {
    throw new InvalidApiResponseError('CAPTCHA');
  }
  // Keep the original strings: encrypted tokens must not be trimmed or transformed.
  const common = { dntCaptchaId, dntCaptchaTextValue, dntCaptchaTokenValue };
  if (mode === 'audio') {
    if (dntCaptchaImgUrl !== '' || !isNonEmptyString(dntCaptchaAudioUrl) || !isHttpUrl(dntCaptchaAudioUrl)) {
      throw new InvalidApiResponseError('CAPTCHA');
    }
    return { ...common, dntCaptchaImgUrl, dntCaptchaAudioUrl };
  }
  if (!isNonEmptyString(dntCaptchaImgUrl) || !isHttpUrl(dntCaptchaImgUrl)) {
    throw new InvalidApiResponseError('CAPTCHA');
  }
  return { ...common, dntCaptchaImgUrl };
}

function isHttpUrl(value: string): boolean {
  try {
    // A fixed base validates relative URLs without depending on browser globals.
    const url = new URL(value, 'https://captcha.invalid/');
    return url.protocol === 'http:' || url.protocol === 'https:';
  } catch {
    return false;
  }
}
