import { inject, InjectionToken } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';

export const HTTP_REQUEST_TIMEOUT_MS = new InjectionToken<number>('HTTP_REQUEST_TIMEOUT_MS', {
  providedIn: 'root',
  factory: () => 15_000
});

export const requestTimeoutInterceptor: HttpInterceptorFn = (request, next) => {
  const timeout = request.timeout ?? inject(HTTP_REQUEST_TIMEOUT_MS);
  return next(request.clone({ timeout }));
};
