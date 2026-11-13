export class InvalidApiResponseError extends Error {
  override readonly name = 'InvalidApiResponseError';

  constructor(responseName: string) {
    // Do not include response bodies, which may contain sensitive values.
    super(`Invalid ${responseName} API response.`);
  }
}

export function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

export function isNonEmptyString(value: unknown): value is string {
  return typeof value === 'string' && value.trim().length > 0;
}
