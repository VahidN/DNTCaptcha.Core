import { InvalidApiResponseError, isNonEmptyString, isRecord } from '../../core/http/api-response-validation';
import type { LoginResponse } from '../interface/login-response.interface';

export function parseLoginResponse(value: unknown): LoginResponse {
  if (!isRecord(value) || !isNonEmptyString(value['name'])) {
    throw new InvalidApiResponseError('login');
  }
  return { name: value['name'] };
}
