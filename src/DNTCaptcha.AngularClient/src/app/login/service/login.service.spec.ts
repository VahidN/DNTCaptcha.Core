import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { LoginService } from './login.service';
import { LoginDto } from '../interface/login.dto';
import { InvalidApiResponseError } from '../../core/http/api-response-validation';

const loginData: LoginDto = {
  dntCaptchaText: 'text',
  dntCaptchaToken: 'token',
  dntCaptchaInputText: '42',
  username: 'user+name@example.test',
  password: 'p&ss=word+value'
};

describe('LoginService', () => {
  let service: LoginService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(LoginService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('posts credentials and CAPTCHA fields as form-encoded data', () => {
    const next = vi.fn();
    service.login(loginData).subscribe(next);
    const request = http.expectOne('api/account/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.detectContentTypeHeader()).toBe('application/x-www-form-urlencoded;charset=UTF-8');
    const body = request.request.serializeBody();
    expect(typeof body).toBe('string');
    expect(Object.fromEntries(new URLSearchParams(String(body)))).toEqual(loginData);
    request.flush({ name: 'test' });
    expect(next).toHaveBeenCalledExactlyOnceWith({ name: 'test' });
  });

  it.each(
    [null, [], {}, 'unexpected', 42, { name: null }, { name: 42 }, { name: '' }, { name: ' \t ' }].map((body) => ({
      body
    }))
  )('rejects malformed successful login response %#', ({ body }) => {
    const next = vi.fn();
    const error = vi.fn();
    service.login(loginData).subscribe({ next, error });
    http.expectOne('api/account/login').flush(body);
    expect(next).not.toHaveBeenCalled();
    expect(error).toHaveBeenCalledExactlyOnceWith(expect.any(InvalidApiResponseError));
  });

  it('rejects an empty HTTP 204 response', () => {
    const error = vi.fn();
    service.login(loginData).subscribe({ error });
    http.expectOne('api/account/login').flush(null, { status: 204, statusText: 'No Content' });
    expect(error).toHaveBeenCalledExactlyOnceWith(expect.any(InvalidApiResponseError));
  });

  it('accepts additional fields and preserves the name without coercing it', () => {
    const next = vi.fn();
    service.login(loginData).subscribe(next);
    http.expectOne('api/account/login').flush({ name: ' Test User ', additionalField: 'ignored' });
    expect(next).toHaveBeenCalledExactlyOnceWith({ name: ' Test User ' });
  });

  it('preserves HTTP failure status and headers', () => {
    const error = vi.fn();
    service.login(loginData).subscribe({ error });
    http.expectOne('api/account/login').flush(
      {},
      {
        status: 429,
        statusText: 'Too Many Requests',
        headers: { 'Retry-After': '3' }
      }
    );
    expect(error.mock.calls[0]?.[0].status).toBe(429);
    expect(error.mock.calls[0]?.[0].headers.get('Retry-After')).toBe('3');
  });
});
