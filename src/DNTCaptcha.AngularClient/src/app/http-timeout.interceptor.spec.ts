import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { appConfig } from './app.config';
import { HTTP_REQUEST_TIMEOUT_MS } from './http-timeout.interceptor';

describe('HTTP request timeout configuration', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [...appConfig.providers, provideHttpClientTesting()] });
  });

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it.each(['GET', 'POST'])('applies the default deadline to %s requests', (method) => {
    TestBed.inject(HttpClient).request(method, '/api/example').subscribe();
    const request = TestBed.inject(HttpTestingController).expectOne('/api/example');
    expect(request.request.timeout).toBe(15_000);
    request.flush({});
  });

  it('allows the application to configure a different default', () => {
    TestBed.overrideProvider(HTTP_REQUEST_TIMEOUT_MS, { useValue: 30_000 });
    TestBed.inject(HttpClient).get('/api/example').subscribe();
    const request = TestBed.inject(HttpTestingController).expectOne('/api/example');
    expect(request.request.timeout).toBe(30_000);
    request.flush({});
  });

  it.each([1_000, 60_000])('preserves an explicit per-request timeout of %s', (timeout) => {
    TestBed.inject(HttpClient).get('/api/example', { timeout }).subscribe();
    const request = TestBed.inject(HttpTestingController).expectOne('/api/example');
    expect(request.request.timeout).toBe(timeout);
    request.flush({});
  });
});

describe('HTTP timeout with the real Fetch backend', () => {
  afterEach(() => {
    vi.useRealTimers();
    vi.unstubAllGlobals();
  });

  it.each(['GET', 'POST'])('aborts a stalled %s request without retrying it', async (method) => {
    vi.useFakeTimers();
    let requestSignal: AbortSignal | undefined;
    const fetch = vi.fn<typeof globalThis.fetch>((_url, init) => {
      requestSignal = init?.signal ?? undefined;
      return new Promise((_resolve, reject) => {
        requestSignal!.addEventListener('abort', () => reject(requestSignal!.reason), { once: true });
      });
    });
    vi.stubGlobal('fetch', fetch);
    // Keep the real backend: HttpTestingController does not execute Fetch's abort timer.
    TestBed.configureTestingModule({ providers: appConfig.providers });
    const result = firstValueFrom(TestBed.inject(HttpClient).request(method, '/api/stalled')).catch(
      (error: unknown) => error
    );
    await vi.advanceTimersByTimeAsync(14_999);
    expect(requestSignal?.aborted).toBe(false);
    await vi.advanceTimersByTimeAsync(1);
    const error = await result;
    expect(requestSignal?.aborted).toBe(true);
    expect(error).toBeInstanceOf(HttpErrorResponse);
    expect(error).toMatchObject({ status: 0, error: { name: 'TimeoutError' } });
    expect(fetch).toHaveBeenCalledTimes(1);
  });
});
